using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class stores the calculated statistical Values for one Distribution
/// </summary>
public class DistributionValues
{

    private double largestElement;
    private double smallestElement;

    private double median;
    private double mean;

    private double variance;
    private double stdDev;
    private double iqr;
    private double upperQuartile;
    private double lowerQuartile;

    private double kurtosis;
    private double skewness;

    private double modality; //peaks

    #region Getter/Setter

    public double LargestElement => largestElement;

    public double SmallestElement => smallestElement;

    public double Median => median;

    public double Mean => mean;

    public double Variance => variance;

    public double StdDev => stdDev;

    public double Iqr => iqr;

    public double UpperQuartile => upperQuartile;

    public double LowerQuartile => lowerQuartile;

    public double Kurtosis => kurtosis;

    public double Skewness => skewness;

    public double Modality => modality;

    #endregion

    public DistributionValues()
    {
    }

    /// <summary>
    /// Constructor fills class automatically with the statistical values
    /// </summary>
    /// <param name="data"></param>
    public DistributionValues(double[] data)
    {
        GetDescriptiveStatisticValues(data);
    }

    /// <summary>
    /// Method calculates all statistical characteristics in one pass
    /// </summary>
    /// <param name="data"></param>
    public void GetDescriptiveStatisticValues(double[] data)
    {
        largestElement = DistributionCalc.GetMaximumValue(data);
        smallestElement = DistributionCalc.GetMinimumValue(data); ;

        mean = DistributionCalc.GetMeanValue(data);
        variance = DistributionCalc.GetVarianceValue(data);
        stdDev = DistributionCalc.GetStandardDeviationValue(data);

        kurtosis = DistributionCalc.GetKurtosisValue(data);
        skewness = DistributionCalc.GetSkewnessValue(data);

        median = DistributionCalc.GetMedianValue(data);
        iqr = DistributionCalc.GetIQRValue(data);
        upperQuartile = DistributionCalc.GetUpperQuartileValue(data);
        lowerQuartile = DistributionCalc.GetLowerQuartileValue(data);

        modality = DistributionCalc.GetModalityValueIncremental(data);
    }

    /// <summary>
    /// Outputs a string summarizing all statistical Values of the Distribution
    /// </summary>
    /// <returns></returns>
    public string PrintDistributionValues()
    {
        string output = "Statistical Values :\n";

        output = output + ("largestElement" + ": " + LargestElement) + "\n";
        output = output + ("smallestElement" + ": " + SmallestElement) + "\n";
        output = output + ("median" + ": " + Median) + "\n";
        output = output + ("mean" + ": " + Mean) + "\n";
        output = output + ("variance" + ": " + Variance) + "\n";
        output = output + ("stdDev" + ": " + StdDev) + "\n";
        output = output + ("iqr" + ": " + Iqr) + "\n";
        output = output + ("upperQuartile" + ": " + UpperQuartile) + "\n";
        output = output + ("lowerQuartile" + ": " + LowerQuartile) + "\n";
        output = output + ("kurtosis" + ": " + Kurtosis) + "\n";
        output = output + ("skewness" + ": " + Skewness) + "\n";
        output = output + ("modality" + ": " + Modality) + "\n";

        return output;
    }

    /// <summary>
    /// Methods compares multiple (>1) DistributionValues and returns a List of DistributionValues with the min[0] and max[1] value for each statistical value.
    /// </summary>
    /// <param name="distributionValues"></param>
    /// <returns></returns>
    public static List<DistributionValues> GetMinMaxDistValues(List<DistributionValues> distributionValues)
    {
        if (distributionValues.Count < 2)
        {
            Debug.LogError("DistributionValues list has less then 2 entries or is empty");
            return null;
        }

        //Classes to save min/max values
        DistributionValues minDistValues = FillWithValues(Double.MaxValue);
        DistributionValues maxDistValues = FillWithValues(Double.MinValue);

        // Check for min/max values between the same members
        foreach (var distValue in distributionValues)
        {
            minDistValues.largestElement = Math.Min(distValue.largestElement, minDistValues.largestElement);
            minDistValues.smallestElement = Math.Min(distValue.smallestElement, minDistValues.smallestElement);
            minDistValues.median = Math.Min(distValue.median, minDistValues.median);
            minDistValues.mean = Math.Min(distValue.mean, minDistValues.mean);
            minDistValues.variance = Math.Min(distValue.variance, minDistValues.variance);
            minDistValues.stdDev = Math.Min(distValue.stdDev, minDistValues.stdDev);
            minDistValues.iqr = Math.Min(distValue.iqr, minDistValues.iqr);
            minDistValues.upperQuartile = Math.Min(distValue.upperQuartile, minDistValues.upperQuartile);
            minDistValues.lowerQuartile = Math.Min(distValue.lowerQuartile, minDistValues.lowerQuartile);
            minDistValues.kurtosis = Math.Min(distValue.kurtosis, minDistValues.kurtosis);
            minDistValues.skewness = Math.Min(distValue.skewness, minDistValues.skewness);
            minDistValues.modality = Math.Min(distValue.modality, minDistValues.modality);

            maxDistValues.largestElement = Math.Max(distValue.largestElement, maxDistValues.largestElement);
            maxDistValues.smallestElement = Math.Max(distValue.smallestElement, maxDistValues.smallestElement);
            maxDistValues.median = Math.Max(distValue.median, maxDistValues.median);
            maxDistValues.mean = Math.Max(distValue.mean, maxDistValues.mean);
            maxDistValues.variance = Math.Max(distValue.variance, maxDistValues.variance);
            maxDistValues.stdDev = Math.Max(distValue.stdDev, maxDistValues.stdDev);
            maxDistValues.iqr = Math.Max(distValue.iqr, maxDistValues.iqr);
            maxDistValues.upperQuartile = Math.Max(distValue.upperQuartile, maxDistValues.upperQuartile);
            maxDistValues.lowerQuartile = Math.Max(distValue.lowerQuartile, maxDistValues.lowerQuartile);
            maxDistValues.kurtosis = Math.Max(distValue.kurtosis, maxDistValues.kurtosis);
            maxDistValues.skewness = Math.Max(distValue.skewness, maxDistValues.skewness);
            maxDistValues.modality = Math.Max(distValue.modality, maxDistValues.modality);
        }

        return new List<DistributionValues> { minDistValues, maxDistValues };
    }

    /// <summary>
    /// Helper function fills all values with min/max values based on fillValue.
    /// </summary>
    /// <param name="fillValue"></param>
    /// <returns></returns>
    private static DistributionValues FillWithValues(double fillValue)
    {
        DistributionValues filledDistributionValues = new DistributionValues();

        filledDistributionValues.largestElement = fillValue;
        filledDistributionValues.smallestElement = fillValue;
        filledDistributionValues.median = fillValue;
        filledDistributionValues.mean = fillValue;
        filledDistributionValues.variance = fillValue;
        filledDistributionValues.stdDev = fillValue;
        filledDistributionValues.iqr = fillValue;
        filledDistributionValues.upperQuartile = fillValue;
        filledDistributionValues.lowerQuartile = fillValue;
        filledDistributionValues.kurtosis = fillValue;
        filledDistributionValues.skewness = fillValue;
        filledDistributionValues.modality = fillValue;

        return filledDistributionValues;
    }
}
