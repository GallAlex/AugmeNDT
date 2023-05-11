using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleColor
{
    public ScaleColor()
    {
    }

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

        double ratio = (value - minValue) / (maxValue - minValue);

        if (double.IsNaN(ratio))
        {
            Debug.LogError("Calculation yielded NaN: Check Results");
            ratio = 0;
        }

        if (range.Length > 2)
        {
            int colorIndex = Convert.ToInt32(ratio * (range.Length - 1));

            // clamp the color index to ensure it's within range
            colorIndex = Math.Min(Math.Max(colorIndex, 0), range.Length - 1);

            //Check that StartColor is not EndColor
            if(colorIndex == range.Length - 1) colorIndex = range.Length - 2;

            startColor = range[colorIndex];
            endColor = range[Math.Min(colorIndex + 1, range.Length - 1)];
        }

        // interpolate the color
        Color interpolatedColor = Color.Lerp(startColor, endColor, (float)ratio);
		
        return interpolatedColor;
    }

    public static Color GetCategoricalColor(double value, double minValue, double maxValue, Color[] range)
    {

        double ratio = (value - minValue) / (maxValue - minValue);
        if (double.IsNaN(ratio))
        {
            Debug.LogError("Calculation yielded NaN: Check Results");
            ratio = 0;
        }
        int colorIndex = Convert.ToInt32(ratio * (range.Length - 1));

        // clamp the color index to ensure it's within range
        colorIndex = Math.Min(Math.Max(colorIndex, 0), range.Length - 1);

        Color selectedColor = range[colorIndex];

        return selectedColor;
    }

    
}
