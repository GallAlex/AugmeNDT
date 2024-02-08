// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;

namespace MagicLeap.MRTK.Utilities
{

    public class KeyPointMotionFilter
    {
        private EuroFilter _euroFilter;
        private RollingMeanFilter _rollingMeanFilter;
        private Vector3 _keyPointPosition;
        private float _keyPointSpeed;
        private float _highSpeed;
        private float _lowMotionMinCutoff;
        private float _lowMotionBeta;
        private float _highMotionMinCutoff;
        private float _highMotionBeta;

        private double TimeStamp => Time.timeSinceLevelLoadAsDouble;

        private Vector3 KeyPointPosition
        {
            get => _keyPointPosition;
            set
            {
                _keyPointSpeed = _keyPointPosition == Vector3.zero ? 0 : Vector3.Distance(value, _keyPointPosition) / Time.deltaTime;
                _keyPointPosition = value;
            }
        }

        public KeyPointMotionFilter(float highSpeed, float lowMotionMinCutoff, float lowMotionBeta, float highMotionMinCutoff, float highMotionBeta)
        {
            _rollingMeanFilter = new RollingMeanFilter(3, 5);
            _highSpeed = highSpeed;
            _lowMotionMinCutoff = lowMotionMinCutoff;
            _lowMotionBeta = lowMotionBeta;
            _highMotionMinCutoff = highMotionMinCutoff;
            _highMotionBeta = highMotionBeta;

            _euroFilter = new EuroFilter(3, _highMotionMinCutoff, _highMotionBeta, 1);
        }

        public Vector3 Filter(Vector3 _rawKeyPointPosition)
        {
            // moderately smoothed keypoint position to evaluate speed variable
            KeyPointPosition = _rollingMeanFilter.Filter(_rawKeyPointPosition, (float)TimeStamp);

            // interpolate between high and low motion filter parameters
            float interpolant = Mathf.Clamp01(_keyPointSpeed / _highSpeed);
            float minCutoff = Mathf.Lerp(_lowMotionMinCutoff, _highMotionMinCutoff, interpolant);
            float beta = Mathf.Lerp(_lowMotionBeta, _highMotionBeta, interpolant);

            // apply filter
            _euroFilter.UpdateFilter(3, minCutoff, beta, 1);
            return _euroFilter.Filter(TimeStamp, _rawKeyPointPosition);
        }

        public void Reset()
        {
            _keyPointPosition = Vector3.zero;
            _euroFilter.Reset();
        }
    }
}
