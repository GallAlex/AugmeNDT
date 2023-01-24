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
       
        double ratio = (value - minValue) / (maxValue - minValue);
        int colorIndex = Convert.ToInt32(ratio * (range.Length - 1));

        // clamp the color index to ensure it's within range
        colorIndex = Math.Min(Math.Max(colorIndex, 0), range.Length - 1);

        Color startColor = range[colorIndex];
        Color endColor = range[Math.Min(colorIndex + 1, range.Length - 1)];

        // interpolate the color
        Color interplotatedColor = Color.Lerp(startColor, endColor, (float)ratio);

        return interplotatedColor;
    }

    public static Color GetCategoricalColor(double value, double minValue, double maxValue, Color[] range)
    {

        double ratio = (value - minValue) / (maxValue - minValue);
        int colorIndex = Convert.ToInt32(ratio * (range.Length - 1));

        // clamp the color index to ensure it's within range
        colorIndex = Math.Min(Math.Max(colorIndex, 0), range.Length - 1);

        Color selectedColor = range[colorIndex];

        Debug.Log("selectedColor Nr. " + colorIndex + " is " + selectedColor);

        return selectedColor;
    }



}
