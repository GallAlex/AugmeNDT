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
    public class RollingMeanFilter
    {
        public int[] FrameCount { get; private set; }
        private int[] _currentFrameCount;
        private float[][] _data;
        private float[] _sum;
        private float[] _average;
        private float[] _helperArray;
        private const int DefaultFrameCount = 5;
        private bool _resetOnZero;
        private int _order;

        public RollingMeanFilter(int order, int[] frameCount, bool resetOnZero = true)
        {
            _resetOnZero = resetOnZero;
            _order = order;
            CheckFrameCounts(ref frameCount);
            FrameCount = frameCount;
            _data = new float[order][];
            _currentFrameCount = new int[order];
            _sum = new float[order];
            _average = new float[order];
            _helperArray = new float[order];
            for (int i = 0; i < order; ++i)
            {
                int count = frameCount == null || frameCount.Length - 1 < i ? DefaultFrameCount : frameCount[i];
                _data[i] = new float[count];
            }
        }

        public RollingMeanFilter(int order, int frameCount, bool resetOnZero = true)
        {
            int[] frames = Enumerable.Repeat(frameCount, order).ToArray();
            _resetOnZero = resetOnZero;
            _order = order;
            CheckFrameCounts(ref frames);
            FrameCount = frames;
            _data = new float[order][];
            _currentFrameCount = new int[order];
            _sum = new float[order];
            _average = new float[order];
            _helperArray = new float[order];
            for (int i = 0; i < order; ++i)
            {
                int count = frames == null || frames.Length - 1 < i ? DefaultFrameCount : frames[i];
                _data[i] = new float[count];
            }
        }

        /// <summary>
        ///		Ensure that we have at least one frame for the rolling mean
        /// </summary>
        private void CheckFrameCounts(ref int[] frameCount)
        {
            if (frameCount.Any(count => count == 0))
            {
                Debug.Log(
                    "A rolling mean filter has been configured to have a window size of zero. Raw data will be reported back.");
                for (int i = 0; i < frameCount.Length; ++i)
                {
                    frameCount[i] = frameCount[i] == 0 ? 1 : frameCount[i];
                }
            }
        }

        public float[] Filter(float[] dataPoint, float timestamp)
        {
            if (!IsValid(dataPoint))
            {
                Reset();
                return dataPoint;
            }

            if (_resetOnZero)
            {
                if (dataPoint.All(x => x == 0f))
                {
                    Reset();
                    return dataPoint;
                }
            }
            for (int i = 0; i < _order; ++i)
            {
                if (_currentFrameCount[i] == FrameCount[i])
                {
                    _sum[i] = _sum[i] - _data[i][0];
                    int lastIndex = _data[i].Length - 1;
                    Array.Copy(_data[i], 1, _data[i], 0, lastIndex);
                    _data[i][lastIndex] = dataPoint[i];
                }
                else
                {
                    _currentFrameCount[i] = _currentFrameCount[i] + 1;
                    _data[i][_currentFrameCount[i] - 1] = dataPoint[i];
                }
                _sum[i] = _sum[i] + dataPoint[i];
                _average[i] = _sum[i] / _currentFrameCount[i];
            }
            return _average;
        }

        public void UpdateFrameCount(int[] frameCount)
        {
            FrameCount = frameCount;
            Resize();
        }

        public float Filter(float dataPoint, float timestamp)
        {
            if (_order != 1)
            {
                return 0f;
            }
            _helperArray[0] = dataPoint;
            return Filter(_helperArray, timestamp)[0];
        }

        public Vector2 Filter(Vector2 dataPoint, float timestamp)
        {
            if (_order != 2)
            {
                return Vector2.zero;
            }
            _helperArray[0] = dataPoint.x;
            _helperArray[1] = dataPoint.y;
            float[] average = Filter(_helperArray, timestamp);
            return new Vector2
            {
                x = average[0],
                y = average[1]
            };
        }

        public Vector3 Filter(Vector3 dataPoint, float timestamp)
        {
            if (_order != 3)
            {
                return Vector3.zero;
            }
            _helperArray[0] = dataPoint.x;
            _helperArray[1] = dataPoint.y;
            _helperArray[2] = dataPoint.z;
            float[] average = Filter(_helperArray, timestamp);
            return new Vector3
            {
                x = average[0],
                y = average[1],
                z = average[2]
            };
        }

        public Quaternion Filter(Quaternion dataPoint, float timestamp)
        {
            if (_order != 4)
            {
                return Quaternion.identity;
            }
            _helperArray[0] = dataPoint.x;
            _helperArray[1] = dataPoint.y;
            _helperArray[2] = dataPoint.z;
            _helperArray[3] = dataPoint.w;
            float[] average = Filter(_helperArray, timestamp);
            return new Quaternion
            {
                x = average[0],
                y = average[1],
                z = average[2],
                w = average[3]
            };
        }

        private bool IsValid(float[] dataPoint)
        {
            for (int i = 0; i < dataPoint.Length; i++)
            {
                if (double.IsNaN(dataPoint[i]) || double.IsInfinity(dataPoint[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public void Reset()
        {
            _data = new float[_order][];
            _currentFrameCount = new int[_order];
            _sum = new float[_order];
            _average = new float[_order];
            for (int i = 0; i < _order; ++i)
            {
                int count = FrameCount == null || FrameCount.Length - 1 < i ? DefaultFrameCount : FrameCount[i];
                _data[i] = new float[count];
            }
        }

        public void Resize()
        {
            var newData = new float[_order][];
            var newSum = new float[_order];
            var newAverage = new float[_order];
            for (int i = 0; i < _order; ++i)
            {
                int count = FrameCount == null ? DefaultFrameCount : FrameCount[i];
                newData[i] = new float[count];
                if (newData[i].Length >= _data[i].Length)
                {
                    Array.Copy(_data[i], 0, newData[i], 0, _data[i].Length);
                    for (int frameCount = _data[i].Length - 1; frameCount < newData[i].Length; ++frameCount)
                    {
                        newData[i][frameCount] = _data[i].Last();
                    }
                }
                else if (newData[i].Length < _data[i].Length)
                {
                    Array.Copy(_data[i], _currentFrameCount[i] - (newData[i].Length + 1), newData[i], 0,
                        newData[i].Length);
                }
                _currentFrameCount[i] =
                    newData[i].Length >= _data[i].Length ? _currentFrameCount[i] : newData[i].Length;

                // Fill the rest of the array up with the latest value
                for (int sIndex = 0; sIndex < _currentFrameCount[i]; ++sIndex)
                {
                    newSum[i] += newData[i][sIndex];
                }
                newAverage[i] = newSum[i] / _currentFrameCount[i];
            }
            _sum = newSum;
            _average = newAverage;
            _data = newData;
        }
    }
}
