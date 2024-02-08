// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Linq;
using UnityEngine;

namespace MagicLeap.MRTK.Utilities
{
    public class EuroFilter
    {
        public float[] DerivativeCutoff { get; set; }
        public float[] MinCutoff { get; set; }
        public float[] Beta { get; set; }

        private double _deltaTime;

        private float[] _delta;
        private float[] _result;

        private float[] _previousData;
        private float[] _previousDelta;
        private double _previousTimestamp;

        private bool _initialized;
        private bool _resetOnZero;
        private int _order;

        public EuroFilter(int order, float[] minCutoff, float[] beta, float[] derivativeCutoff, bool resetOnZero = true)
        {
            _resetOnZero = resetOnZero;
            _deltaTime = 0;
            _order = order;
            _initialized = false;

            DerivativeCutoff = derivativeCutoff;
            MinCutoff = minCutoff;
            Beta = beta;

            _delta = new float[order];
            _result = new float[order];

            _previousData = new float[order];
            _previousDelta = new float[order];
        }

        public EuroFilter(int order, float minCutoff, float beta, float derivativeCutoff, bool resetOnZero = true)
        {
            _resetOnZero = resetOnZero;
            _deltaTime = 0;
            _order = order;
            _initialized = false;

            DerivativeCutoff = Enumerable.Repeat(derivativeCutoff, order).ToArray();
            MinCutoff = Enumerable.Repeat(minCutoff, order).ToArray();
            Beta = Enumerable.Repeat(beta, order).ToArray();

            _delta = new float[order];
            _result = new float[order];

            _previousData = new float[order];
            _previousDelta = new float[order];
        }

        public void UpdateFilter(int order, float[] minCutoff, float[] beta, float[] derivativeCutoff)
        {
            _order = order;
            MinCutoff = minCutoff;
            Beta = beta;
            DerivativeCutoff = derivativeCutoff;
        }

        public void UpdateFilter(int order, float minCutoff, float beta, float derivativeCutoff)
        {
            float[] minCutoffs = Enumerable.Repeat(minCutoff, order).ToArray();
            float[] betas = Enumerable.Repeat(beta, order).ToArray();
            float[] derivativeCutoffs = Enumerable.Repeat(derivativeCutoff, order).ToArray();
            UpdateFilter(order, minCutoffs, betas, derivativeCutoffs);
        }

        public Vector3 Filter(double timestamp, Vector3 vector)
        {
            float[] filteredVector = Filter(timestamp, new float[] {vector.x, vector.y, vector.z});
            return new Vector3(filteredVector[0], filteredVector[1], filteredVector[2]);
        }

        public Quaternion Filter(double timestamp, Quaternion quaternion)
        {
            if(_initialized)
            {
                // Protect against rotation polarity flip to avoid filtering the rotation around the long way
                FullRangeRotation(ref quaternion);
            }

            float[] filteredQuaternion =
                Filter(timestamp, new float[] {quaternion.x, quaternion.y, quaternion.z, quaternion.w});
            return new Quaternion(filteredQuaternion[0], filteredQuaternion[1], filteredQuaternion[2],
                filteredQuaternion[3]);
        }

        public float[] Filter(double timestamp, float[] dataPoint)
        {
            if (_resetOnZero && dataPoint.All(x => x == 0f))
            {
                Reset();
                return dataPoint;
            }

            if (IsValidTimestamp(timestamp))
            {
                _deltaTime = timestamp - _previousTimestamp;
            }

            _previousTimestamp = timestamp;

            if (!_initialized)
            {
                _previousData = dataPoint;
            }

            for (int i = 0; i < _order; ++i)
            {
                _delta[i] = _initialized ? (float)((dataPoint[i] - _previousData[i]) / _deltaTime) : 0f;

                float derivativeSmoothFactor = GetSmoothingFactor(DerivativeCutoff[i]);
                float dxHat = ExponentialSmoothing(derivativeSmoothFactor, _delta[i], _previousDelta[i]);

                float cutoff = MinCutoff[i] + Beta[i] * Mathf.Abs(dxHat);
                float smoothFactor = GetSmoothingFactor(cutoff);
                _result[i] = ExponentialSmoothing(smoothFactor, dataPoint[i], _previousData[i]);
                _previousData[i] = _result[i];
                _previousDelta[i] = dxHat;
            }
    
            _initialized = true;
            return _result;
        }

        private float GetSmoothingFactor(float cutoff)
        {
            double result = 2.0 * Math.PI * cutoff * _deltaTime;
            return (float)(result / (result + 1.0));
        }

        private float ExponentialSmoothing(float smoothFactor, float value, float previousValue)
        {
            return smoothFactor * value + (1f - smoothFactor) * previousValue;
        }

        /// <summary>
        ///     Validates whether time has initialized or time has changed
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        private bool IsValidTimestamp(double timestamp)
        {
            if(_deltaTime == 0)
            {
                return _previousTimestamp != 0 && timestamp != 0;
            }
            return _previousTimestamp != timestamp;
        }

        /// <summary>
        ///     We know that a quaternion (x, y, z, w) represents the same rotation as (-x, -y, -z, -w).
        ///     So an input device has to send only one of the two quaternions (assuming only normalized quaternions).
        ///     ML only provides quaternions with a positive y value causing the w, x, and z components to jump to an opposite value
        ///     when y reaches 0. The resulting behaviour is a quick 360 degree rotation.
        ///
        ///     This solution detects if the input quaternion is in the opposite part of the space of possible values from the current
        ///     filtered quaternion. If the distance between the 2 quaternions is above sqrt(2), the quaternion is reversed to maintain
        ///     a continuous rotation. 
        /// </summary>
        /// <remarks>
        ///     Solution from Dario Mazzanti's One Euro filter implementation https://github.com/DarioMazzanti/OneEuroFilterUnity/commit/8f0ea49e0071a97f493accbe151ffb03d6f0a5e8
        /// </remarks>
        private void FullRangeRotation(ref Quaternion rot)
        {
            Vector4 normalizedCurrent = new Vector4(_previousData[0], _previousData[1], _previousData[2], _previousData[3]).normalized;
            Vector4 normalizedNew = new Vector4(rot.x, rot.y, rot.z, rot.w).normalized;
            if (Vector4.SqrMagnitude(normalizedCurrent - normalizedNew) > 2)
            {
                rot = new Quaternion(-rot.x, -rot.y, -rot.z, -rot.w);
            }
        }

        public void Reset()
        {
            _initialized = false;
            _delta = new float[_order];
            _result = new float[_order];

            _previousData = new float[_order];
            _previousDelta = new float[_order];
            _previousTimestamp = 0;
        }
    }
}