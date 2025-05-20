// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace AugmeNDT{
    public class ScaleColor
    {
        public ScaleColor()
        {
        }

        /// <summary>
        /// This method interpolates (mix) colors based on a given input value's proportion between a minimum and maximum value.
        /// The colors to be interpolated will be taken from the array range.
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Color GetInterpolatedColor(double value, double minValue, double maxValue, Color[] range)
        {
            //If only one Color
            if (range.Length == 1)
            {
                Debug.LogWarning("Only one Color for interploation assigned");
                return range[0];
            }

            Color startColor = range[0];
            Color endColor = range[1];

            if (value < minValue || value > maxValue)
            {
                Debug.LogError("Value is greater/smaller than max/min value! Expect wrong Colors!");
            }

            double ratio = (value - minValue) / (maxValue - minValue);

            if (double.IsNaN(ratio))
            {
                Debug.LogError("Calculation yielded NaN: Check Results");
                ratio = 0;
            }

            if (range.Length > 2)
            {
                int colorIndex = Convert.ToInt32(ratio * (range.Length - 1));
                colorIndex = Math.Abs(colorIndex);

                // clamp the color index to ensure it's within range
                //colorIndex = Math.Min(Math.Max(colorIndex, 0), range.Length - 1);

                //Check that StartColor is not EndColor
                if(colorIndex == range.Length - 1) colorIndex = range.Length - 2;

                startColor = range[colorIndex];
                endColor = range[Math.Min(colorIndex + 1, range.Length - 1)];
            }

            // interpolate the color
            Color interpolatedColor = Color.Lerp(startColor, endColor, (float)ratio);
		
            return interpolatedColor;
        }

        /// <summary>
        /// The method calculates and assigns a specific categorical color from a predefined color range array based on the given value within a specified min-max value interval.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Color GetCategoricalColor(double value, double minValue, double maxValue, Color[] range)
        {
            if (value < minValue || value > maxValue)
            {
                Debug.LogError("Value is greater/smaller than max/min value! Expect wrong Colors!");
            }

            double ratio = (value - minValue) / (maxValue - minValue);

            if (double.IsNaN(ratio))
            {
                Debug.LogError("Calculation yielded NaN: Check Results");
                ratio = 0;
            }

            int colorIndex = Convert.ToInt32(ratio * (range.Length - 1));
            colorIndex = Math.Abs(colorIndex);

            // clamp the color index to ensure it's within range
            //colorIndex = Math.Min(Math.Max(colorIndex, 0), range.Length - 1);

            //Debug.Log("colorIndex: " + colorIndex + " | ratio: " + ratio+ " | value: " + value + " | minValue: " + minValue + " | maxValue: " + maxValue);


            Color selectedColor = range[colorIndex];

            return selectedColor;
        }

        /// <summary>
        /// The method calculates and assigns a specific categorical color from a predefined color range array based on the given value within a specified min-max value interval.
        /// The color array has to be an uneven number of colors so that a specific midValue is always assigned to the middle color.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="midValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Color GetCategoricalColor(double value, double minValue, double midValue, double maxValue, Color[] range)
        {
            if (range.Length % 2 == 0)
            {
                Debug.LogError("Not an uneven number of colors specified! MidValue will be ignored");
                return GetCategoricalColor(value, minValue, maxValue, range);
            }
            if (midValue > maxValue || midValue < minValue)
            {
                Debug.LogError("MidValue is greater/smaller than max/min value! MidValue will be ignored!");
                return GetCategoricalColor(value, minValue, maxValue, range);
            }
            if (value < minValue || value > maxValue)
            {
                Debug.LogError("Value is greater/smaller than max/min value! Expect wrong Colors!");
            }

            // Calculate midvalue color index - int is truncated
            int centerColorIndex = (range.Length / 2);
            
            // Select midvalue color
            if (value == midValue) return range[centerColorIndex];

            Color[] subColors = new Color[range.Length / 2];    // Create Array with only upper/lower part of the colors

            // Select color for values bigger than midValue
            if (value > midValue)
            {
                Array.Copy(range, centerColorIndex + 1, subColors, 0, subColors.Length);
                return GetCategoricalColor(value, midValue, maxValue, subColors);
            }
            if (value < midValue)
            {
                Array.Copy(range, 0, subColors, 0, subColors.Length);
                return GetCategoricalColor(value, minValue, midValue, subColors);
            }

            return Color.green; //Error Color
        }

        /// <summary>
        /// Method returns the index of the color which would be returned by GetCategoricalColor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="colors"></param>
        /// <returns></returns>
        public static int GetCategoricalColorIndex(double value, double minValue, double maxValue, int numberOfColors)
        {
            double ratio = (value - minValue) / (maxValue - minValue);
            if (double.IsNaN(ratio))
            {
                Debug.LogError("Calculation yielded NaN: Check Results");
                ratio = 0;
            }
            int colorIndex = Convert.ToInt32(ratio * (numberOfColors - 1));
            colorIndex = Math.Abs(colorIndex);

            return colorIndex;
        }

        /// <summary>
        /// Method returns the minimal & maximal value which returns the specified color by GetCategoricalColor
        /// </summary>
        /// <param name="colorIndex"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static double[] GetCategoricalColorValueRange(int colorIndex, double minValue, double maxValue, int numberOfColors)
        {
            if (colorIndex < 0 || colorIndex >= numberOfColors)
            {
                throw new ArgumentOutOfRangeException(nameof(colorIndex), "Invalid color index.");
            }

            double rangeLength = maxValue - minValue;
            double rangeStart = minValue + (rangeLength / numberOfColors) * colorIndex;
            double rangeEnd = minValue + (rangeLength / numberOfColors) * (colorIndex + 1);

            return new double[] { rangeStart, rangeEnd };
        }

        public static double[] GetCategoricalColorValueRange(int colorIndex, double minValue, double midValue, double maxValue, int numberOfColors)
        {
            if (numberOfColors % 2 == 0)
            {
                Debug.LogError("Not an uneven number of colors specified!");
            }

            if (colorIndex < 0 || colorIndex >= numberOfColors)
            {
                throw new ArgumentOutOfRangeException(nameof(colorIndex), "Invalid color index.");
            }

            if(colorIndex == numberOfColors / 2) return new double[] { midValue, midValue }; //Return midValue for midColor

            int remainingColors = numberOfColors / 2;

            double rangeStart = 0;
            double rangeEnd = 0;

            if (colorIndex < remainingColors)
            {
                return GetCategoricalColorValueRange(colorIndex, minValue, midValue, remainingColors);
            }
            else // (colorIndex > remainingColors)
            {
                // Change the index to be between 0 to remainingColors
                int changedColorIndex = colorIndex - remainingColors - 1;
                return GetCategoricalColorValueRange(changedColorIndex, midValue, maxValue, remainingColors);
            }

        }

    }
}
