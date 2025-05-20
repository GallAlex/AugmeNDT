// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System.Collections.Generic;
using System.Linq;

namespace AugmeNDT{
    /// <summary>
    /// Helper class to preprocess abstract datasets (i.e., csv files).
    /// Class calculates the minimal and maximal values for each attribute, normalizes a datasets and
    /// calculates the statistic Values of a dataset.
    /// </summary>
    public class DataPreparation
    {

        /// <summary>
        /// Methods returns the min and max value for each attribute in the provided dataset
        /// Returns a Dictionary<AttributeName, [0]min value, [1] max value>.
        /// </summary>
        /// <param name="dataValues"></param>
        /// <returns>Dataset with only min max value</returns>
        public Dictionary<string, double[]> GetMinMaxValueDataset(Dictionary<string, double[]> dataValues)
        {
            Dictionary<string, double[]> minMaxDataValues = new Dictionary<string, double[]>(dataValues.Keys.Count);

            //Calculate Min Max for every Attribute
            foreach (var dataValue in dataValues)
            {
                double[] minMax = new[] { dataValue.Value.Min(), dataValue.Value.Max() };
                minMaxDataValues.Add(dataValue.Key, minMax);
            }

            return minMaxDataValues;
        }

        /*
    /// <summary>
    /// Methods returns the min and max value for each value in the provided statistic dataset
    /// Returns a List<DistributionValues>[0] min values and List<DistributionValues>[1] max values.
    /// </summary>
    /// <param name="dataValues"></param>
    /// <returns>Min max values of each statistics</returns>
    public List<Distribution.DistributionValues> GetMinMaxStatisticValueDataset(Dictionary<string, Distribution.DistributionValues> statisticValues)
    {
        List<Distribution.DistributionValues> minMaxStatisticValues = new List<Distribution.DistributionValues>(2);

        //Calculate Min Max for every Attribute
        foreach (var statisticValue in statisticValues)
        {
            foreach (var value in )
            {
                
            }

            double[] minMax = new[] { dataValue.Value.Min(), dataValue.Value.Max() };
            minMaxDataValues.Add(dataValue.Key, minMax);
        }

        return minMaxStatisticValues;
    }
    
    
    public Dictionary<string, double[]> GetNormalizedDataset(Dictionary<string, double[]> dataValues)
    {
        // Get Min Max for every Attribute
        Dictionary<string, double[]> minMaxDataValues = GetMinMaxValueDataset(dataValues);

        // Normalize Values between 0 to 1 and calculate statistic info
        List<Scale> normalizeScales = new List<Scale>(dataValues.Count);
        Dictionary<string, double[]> normalizedDataValues = new Dictionary<string, double[]>();
        
       
        //Calculate Min Max for every Attribute
        foreach (var dataValue in dataValues)
        {

            List<double> domain = new List<double>(2)
            {
                minMaxDataValues[dataValue.Key][0],
                minMaxDataValues[dataValue.Key][1]
            };
            ScaleLinear currentNormScale = new ScaleLinear(domain);
            normalizeScales.Add(currentNormScale);

            // Normalize Values
            normalizedDataValues.Add(dataValue.Key, currentNormScale.GetNormalizedArray(dataValue.Value));
        }

        return normalizedDataValues;
    }


    /// <summary>
    /// Calculates the statistical Values used in the Chart
    /// </summary>
    public void GetStatisticalDataset(Dictionary<string, double[]> dataValues)
    {
        Dictionary<string, double[]> normalizedDataValues = GetNormalizedDataset(dataValues);
        Dictionary<string, Distribution.DistributionValues> statisticValues = new Dictionary<string, Distribution.DistributionValues>();

        //Calculate Min Max for every Attribute
        foreach (var dataValue in dataValues)
        {
            // Calculate statistic data from dataset
            statisticValues.Add(dataValue.Key, Distribution.GetDescriptiveStatisticValues(normalizedDataValues[dataValue.Key]));
        }

        // Output statisticValues for Debug Purposes
        foreach (var statisticValue in statisticValues)
        {
            string output = ">> " + statisticValue.Key + ": \n";

            output = output + ("largestElement" + ": " + statisticValue.Value.largestElement) + "\n";
            output = output + ("smallestElement" + ": " + statisticValue.Value.smallestElement) + "\n";
            output = output + ("median" + ": " + statisticValue.Value.median) + "\n";
            output = output + ("mean" + ": " + statisticValue.Value.mean) + "\n";
            output = output + ("variance" + ": " + statisticValue.Value.variance) + "\n";
            output = output + ("stdDev" + ": " + statisticValue.Value.stdDev) + "\n";
            output = output + ("iqr" + ": " + statisticValue.Value.iqr) + "\n";
            output = output + ("upperQuartile" + ": " + statisticValue.Value.upperQuartile) + "\n";
            output = output + ("lowerQuartile" + ": " + statisticValue.Value.lowerQuartile) + "\n";
            output = output + ("kurtosis" + ": " + statisticValue.Value.kurtosis) + "\n";
            output = output + ("skewness" + ": " + statisticValue.Value.skewness) + "\n";
            output = output + ("modality" + ": " + statisticValue.Value.modality) + "\n";

            Debug.Log(output);
        }

        double minMValue = double.MaxValue;
        double maxMValue = double.MinValue;

        //Check Number of Modes
        foreach (var normAttributes in normalizedDataValues)
        {
            double mValue = Distribution.GetModalityValueIncremental(normAttributes.Value);

            //Store min and max mValue
            if (mValue < minMValue)
            {
                minMValue = mValue;
            }

            if (mValue > maxMValue)
            {
                maxMValue = mValue;
            }
        }

        // Fill mValuesExtend
        mValuesExtend = new[] { minMValue, maxMValue };


        // Calculate min and max skewness and kurtosis
        minMaxSkewnessValue = new[] { double.MaxValue, double.MinValue };
        minMaxKurtosisValue = new[] { double.MaxValue, double.MinValue };

        foreach (var statisticValue in statisticValues)
        {
            //Calculate Skewness
            double skewness = statisticValue.Value.skewness;
            //Calculate Kurtosis
            double kurtosis = statisticValue.Value.kurtosis;

            //Store min and max skewness 
            if (skewness < minMaxSkewnessValue[0]) minMaxSkewnessValue[0] = skewness;
            if (skewness > minMaxSkewnessValue[1]) minMaxSkewnessValue[1] = skewness;
            //Store min and max kurtosis
            if (kurtosis < minMaxKurtosisValue[0]) minMaxKurtosisValue[0] = kurtosis;
            if (kurtosis > minMaxKurtosisValue[1]) minMaxKurtosisValue[1] = kurtosis;
        }
    }
    */
    }
}
