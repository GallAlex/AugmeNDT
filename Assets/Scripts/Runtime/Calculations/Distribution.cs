using MathNet.Numerics.Statistics;
using System;
using UnityEngine;

/// <summary>
/// Class calculates descriptive statistics of distributions
/// Uses intern MathNet.Numerics.Statistics library
/// </summary>
public class Distribution
{
    public struct DistributionValues
    {
        public double largestElement;
        public double smallestElement;

        public double median;
        public double mean;

        public double variance;
        public double stdDev;
        public double iqr;

        public double kurtosis;
        public double skewness;
    }

    public double GetQuickSymmetryValue()
    {
        // if(Mean < Median < Mode) return Skewed left;
        // if(Mean == Median == Mode) return Symmetric;
        // if(Mode < Median < Mean) return Skewed rigth;
        throw new NotImplementedException();
    }

    public double GetSymmetryValue()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculates the skewness value of the input data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Skewness double value</returns>
    public double GetSkewnessValue(double[] data)
    {
        var value = Statistics.Skewness(data);
        if (double.IsNaN(value)) Debug.LogError("Skewness Value is NaN: Data has less than three entries or an entry is NaN");

        return value;
    }

    /// <summary>
    /// Calculates the kurtosis value of the input data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Kurtosis double value</returns>
    public double GetKurtosisValue(double[] data)
    {
        var value = Statistics.Kurtosis(data);
        if (double.IsNaN(value)) Debug.LogError("Skewness Value is NaN: Data has less than four entries or an entry is NaN");

        return value;
    }

    public double GetModalityValue()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculates the arithmetic mean or average of the input data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Mean double value</returns>
    public double GetMeanValue(double[] data)
    {
        return ArrayStatistics.Mean(data);
    }

    /// <summary>
    /// Calculates the Median value (value in the center) of the input data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Median double value</returns>
    public double GetMedianValue(double[] data)
    {
        var clonedArray = new double[data.Length];
        data.CopyTo(clonedArray, 0);

        return ArrayStatistics.MedianInplace(clonedArray);
    }

    /// <summary>
    /// Calculates the Standard Deviation of the input data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Standard Deviation double value</returns>
    public double GetStandardDeviationValue(double[] data)
    {
        var value = ArrayStatistics.StandardDeviation(data);
        if (double.IsNaN(value)) Debug.LogError("Standard Deviation is NaN: Data has less than two entries or an entry is NaN");

        return value;
    }

    /// <summary>
    /// Calculates the Variance of the input data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Variance double value</returns>
    public double GetVarianceValue(double[] data)
    {
        var value = ArrayStatistics.Variance(data);
        if (double.IsNaN(value)) Debug.LogError("Standard Deviation is NaN: Data has less than two entries or an entry is NaN");

        return value;
    }

    /// <summary>
    /// Calculates the inter-quartile range of the input data
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Inter-quartile range (IQR) double value</returns>
    public double GetIQRValue(double[] data)
    {
        var clonedArray = new double[data.Length];
        data.CopyTo(clonedArray, 0);

        return ArrayStatistics.InterquartileRangeInplace(clonedArray);
    }

    /// <summary>
    /// Method calculates a whole set of statistical characteristics in one pass
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Struct of statistical characteristics of input dataset</returns>
    public DistributionValues GetDescriptiveStatisticValues(double[] data)
    {
        var statistics = new DescriptiveStatistics(data);
        DistributionValues values = new DistributionValues();

        values.largestElement = statistics.Maximum;
        values.smallestElement = statistics.Minimum;

        values.mean = statistics.Mean;
        values.variance = statistics.Variance;
        values.stdDev = statistics.StandardDeviation;

        values.kurtosis = statistics.Kurtosis;
        values.skewness = statistics.Skewness;

        values.median = GetMedianValue(data);
        values.iqr = GetIQRValue(data);

        return values;
    }

}
