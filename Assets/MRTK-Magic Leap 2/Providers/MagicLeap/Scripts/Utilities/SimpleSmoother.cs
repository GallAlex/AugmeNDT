// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeap.MRTK.Utilities
{
    public class SimpleSmoother
    {
        public float MaxDistance = 0.0254f;
        public float SmoothTime = .1f;
        public Vector3 PositionFiltered;

        public float Stability
        {
            get { return _stability; }

            set
            {
                //slight filtering:
                _stability = Mathf.Lerp(_stability, value, Time.deltaTime * 5);
            }
        }

        private float _stability;

        //Public Variables:
        public Vector3 velocity;
        public List<Vector3> locationHistory = new List<Vector3>();
        public Vector3 target;
        public Quaternion Rotation;
        private float _rotationalVelocity;
        private float _smoothTime = .1f;

        public Vector3 UpdatePosition(Vector3 position)
        {
            locationHistory.Add(position);
            //only need 3 in our history:
            if (locationHistory.Count > 3)
            {
                locationHistory.RemoveAt(0);
            }

            if (locationHistory.Count == 3)
            {
                //movement intent stats:
                Vector3 vectorA = locationHistory[locationHistory.Count - 2] -
                                  locationHistory[locationHistory.Count - 3];
                Vector3 vectorB = locationHistory[locationHistory.Count - 1] -
                                  locationHistory[locationHistory.Count - 2];
                float delta = Vector3.Distance(locationHistory[locationHistory.Count - 3],
                    locationHistory[locationHistory.Count - 1]);
                float angle = Vector3.Angle(vectorA, vectorB);
                Stability = 1 - Mathf.Clamp01(delta / MaxDistance);
                if (float.IsNaN(Stability))
                {
                    return position;
                }

                //moving in a constant direction?
                if (angle < 90)
                {
                    target = locationHistory[locationHistory.Count - 1];
                }

                //snap or smooth:
                if (Stability == 0)
                {
                    PositionFiltered = target;
                }
                else
                {
                    PositionFiltered = Vector3.SmoothDamp(PositionFiltered, target,
                        ref velocity, SmoothTime * Stability);
                }
            }
            else
            {
                PositionFiltered = position;
            }

            return PositionFiltered;
        }

        public Quaternion UpdateRotation(Quaternion targetRotation)
        {
            if (Stability == 0)
            {
                Rotation = targetRotation;
            }
            else
            {
                float delta = Quaternion.Angle(Rotation, targetRotation);
                if (delta > 0f)
                {
                    float t = Mathf.SmoothDampAngle(delta, 0.0f, ref _rotationalVelocity, _smoothTime * Stability);
                    t = 1.0f - (t / delta);
                    Rotation = Quaternion.Slerp(Rotation, targetRotation, t);
                }
            }

            return Rotation;
        }

        public void Reset()
        {
          locationHistory.Clear();
        }
    }
}