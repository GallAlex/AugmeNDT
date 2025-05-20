// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System;
using MathNet.Numerics.Statistics;
using UnityEngine;

namespace AugmeNDT{
    /// <summary>
    /// Class calculates descriptive statistics of distributions
    /// Uses intern MathNet.Numerics.Statistics library when possible
    /// </summary>
    public static class DistributionCalc
    {

        public static double GetQuickSymmetryValue()
        {
            // if(Mean < Median < Mode) return Skewed left;
            // if(Mean == Median == Mode) return Symmetric;
            // if(Mode < Median < Mean) return Skewed rigth;
            throw new NotImplementedException();
        }

        public static double GetSymmetryValue()
        {
            throw new NotImplementedException();
        }

        public static double GetMinimumValue(double[] data)
        {
            var value = Statistics.Minimum(data);
            if (double.IsNaN(value)) Debug.LogError("Minimum Value is NaN: Data has less than one entry or an entry is NaN");

            return value;
        }

        public static double GetMaximumValue(double[] data)
        {
            var value = Statistics.Maximum(data);
            if (double.IsNaN(value)) Debug.LogError("Maximum Value is NaN: Data has less than one entry or an entry is NaN");

            return value;
        }

        /// <summary>
        /// Calculates the skewness value of the input data
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Skewness double value</returns>
        public static double GetSkewnessValue(double[] data)
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
        public static double GetKurtosisValue(double[] data)
        {
            var value = Statistics.Kurtosis(data);
            if (double.IsNaN(value)) Debug.LogError("Kurtosis Value is NaN: Data has less than four entries or an entry is NaN");

            return value;
        }

        /// <summary>
        /// Method returns the number of modes based on a histogram.
        /// Currently uses the Sturge's Rule for the bin number estimation.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Estimated number of modes</returns>
        public static double GetModalityValue(double[] data)
        {
            int numberOfBins = GetNumberOfBins(data.Length);
            double mValue = CalculateModalityValue(data, numberOfBins);

            return mValue / 2.0d;
        }

        /// <summary>
        /// Method returns the number of modes based on multiple histograms (with varying bin numbers).
        /// Methods calculates multiple mValues by calculating the modality value with histograms of decreasing bin numbers.
        /// The higest mValue is used do calculate the number of modes.
        /// Currently, four times the estimated bin count of Sturge's Rule is used.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Estimated number of modes</returns>
        public static double GetModalityValueIncremental(double[] data)
        {
            int numberOfBins = GetNumberOfBins(data.Length) * 4;
            int remainingBins = numberOfBins;
            double max_mValue = 0.0d;

            while (remainingBins > 1)
            {
                double mValue = CalculateModalityValue(data, remainingBins);

                //Save largest mValue found
                if (mValue > max_mValue)
                {
                    max_mValue = mValue;
                }

                //restart with less bins
                remainingBins = remainingBins / 2;
            }

            return max_mValue / 2.0d;
        }

        /// <summary>
        /// Method calculates an mvalue (modal value) based on the frequency diferences of the bins.
        /// The mvalue sums the absolute difference in elevation, normalized for the height of the plot
        /// Can be compared to a threshold of 2,4 to determine if the distribution is multimodal.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="numberOfBins"></param>
        /// <returns></returns>
        private static double CalculateModalityValue(double[] data, int numberOfBins)
        {
            Histogram hist = new Histogram(data, numberOfBins);

            // Calculate for the histogram hist, with maximum bin value maxBinFrequency, the mValue which gets compared to a threshold:
            double maxBinFrequency = hist[0].Count;
            double mValue = 0.0d;

            for (int i = 1; i < numberOfBins; i++)
            {
                double binFrequency = hist[i].Count;

                //if new binFrequency is greater as maxBinFrequency
                if (binFrequency > maxBinFrequency)
                {
                    maxBinFrequency = binFrequency;
                }

                //Sum up Difference of the frequency in the bins
                double binDifference = Math.Abs(hist[i].Count - hist[i - 1].Count);
                mValue += binDifference;
            }

            mValue = mValue / maxBinFrequency;

            double threshold = 2.4d;
            if (mValue > threshold)
            {
                //Debug.Log("mValue indicates that multiple modes may be present");
            }

            return mValue;
        }

        /// <summary>
        /// Calculates the arithmetic mean or average of the input data
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Mean double value</returns>
        public static double GetMeanValue(double[] data)
        {
            return ArrayStatistics.Mean(data);
        }

        /// <summary>
        /// Calculates the Median value (value in the center) of the input data
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Median double value</returns>
        public static double GetMedianValue(double[] data)
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
        public static double GetStandardDeviationValue(double[] data)
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
        public static double GetVarianceValue(double[] data)
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
        public static double GetIQRValue(double[] data)
        {
            var clonedArray = new double[data.Length];
            data.CopyTo(clonedArray, 0);

            return ArrayStatistics.InterquartileRangeInplace(clonedArray);
        }

        public static double GetUpperQuartileValue(double[] data)
        {
            var clonedArray = new double[data.Length];
            data.CopyTo(clonedArray, 0);

            return ArrayStatistics.UpperQuartileInplace(clonedArray);
        }

        public static double GetLowerQuartileValue(double[] data)
        {
            var clonedArray = new double[data.Length];
            data.CopyTo(clonedArray, 0);

            return ArrayStatistics.LowerQuartileInplace(clonedArray);
        }

        /// <summary>
        /// Calculates the number of needed bins through Sturge's Rule
        /// Calculates the number of observations in the given dataset
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetNumberOfBins(int observations)
        {
            return (int)Math.Ceiling(1 + 3.322 * Math.Log10(observations));
        }

        /// <summary>
        /// The chi-square goodness-of-fit test is applied to binned data and is testing the similarity between two distributions
        /// https://www.itl.nist.gov/div898/handbook/eda/section3/eda35f.htm
        /// </summary>
        /// <param name="dist1"></param>
        /// <param name="dist2"></param>
        /// <returns></returns>
        public static double GetChiSquaredMetric(double[] dist1, double[] dist2)
        {
            int binCount = DistributionCalc.GetNumberOfBins(Math.Max(dist1.Length, dist2.Length));
            double chiSquare = 0.0;

            Histogram hist1 = new Histogram(dist1, binCount);
            Histogram hist2 = new Histogram(dist2, binCount);

            for (int i = 0; i < binCount; i++)
            {//for each bin

                if (hist2[i].Count != 0)
                {
                    int diff = ((int)hist1[i].Count) - ((int)hist2[i].Count);
                    chiSquare += Math.Pow(((double)diff), 2) / (hist2[i].Count);
                }
                else
                {
                    int diff = ((int)hist1[i].Count);
                    chiSquare += Math.Pow(((double)diff), 2);
                }
            }

            return chiSquare;
        }

        //public static double CalculateChiSquaredMetric(double[] dist1, double[] dist2)
        //{
        //    if (dist1.Length != dist2.Length) { throw new ArgumentException("Distributions must have the same length"); }

        //    return GoodnessOfFit.RSquared(dist1, dist2);
        //}
    }
}
