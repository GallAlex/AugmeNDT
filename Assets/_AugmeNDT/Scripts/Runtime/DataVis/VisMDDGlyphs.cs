using MathNet.Numerics.Statistics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// This class is used to create a multidimensional distribution Glyph chart visualization.
/// </summary>
public class VisMDDGlyphs : Vis
{
    // Normalized Values
    public Dictionary<string, double[]> normalizedDataValues;
    
    // Calculate statistic data from dataset
    public Dictionary<string, Distribution.DistributionValues> statisticValues;

    // min/max mValues
    public double[] mValuesExtend;

    // min/max values
    public double[] minMaxSkewnessValue;
    public double[] minMaxKurtosisValue;

    public VisMDDGlyphs()
    {
        title = "MDD-Glyphs Chart";
        axes = 3;

        //Initialize dataScales
        //dataScaleTypes = new List<Scale.DataScale>()
        //{
        //    Scale.DataScale.Nominal,   // X
        //    Scale.DataScale.Linear,    // Y
        //    Scale.DataScale.Linear,   // Z
        //};

        dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
        tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
    }

    public override GameObject CreateVis()
    {
        xyzTicks = new int[] {dataValues.Keys.Count, 10, 10 };
        Debug.Log("dataValues.Keys.Count: " + dataValues.Keys.Count);
        base.CreateVis();
        

        List<Scale> normalizedScales = DataPreparation();

        
        //## 01: Create Data Scales for Axes
        List<Scale> scale = new List<Scale>(axes);

        Debug.Log("MDDGlyph");

        // X Axis
        List<double> domain = new List<double>(2)
        {
            0,
            statisticValues.Keys.Count-1
        };
        scale.Add(new ScaleNominal(domain, dataValues.Keys.ToList()));

        // Y Axis
        domain = new List<double>(2)
        {
            statisticValues.ElementAt(1).Value.smallestElement,
            statisticValues.ElementAt(1).Value.largestElement
        };
        scale.Add(new ScaleLinear(domain));

        // Z Axis
        domain = new List<double>(2)
        {
            mValuesExtend[0], 
            mValuesExtend[1]
        };
        scale.Add(new ScaleLinear(domain));


        //## 02: Create Axes and Grids

        // X Axis
        encodedAttribute.Add(-1);
        visContainer.CreateAxis("Attributes", (Direction)0, scale[0]);
        visContainer.CreateGrid((Direction)0, (Direction)1);

        // Y Axis
        encodedAttribute.Add(1);
        visContainer.CreateAxis("Attributes Value", (Direction)1, scale[1]);
        visContainer.CreateGrid((Direction)1, (Direction)2);

        // Z Axis
        encodedAttribute.Add(2);
        visContainer.CreateAxis("Modality", (Direction)2, scale[2]);
        visContainer.CreateGrid((Direction)2, (Direction)0);

        //Output min/max values
        Debug.Log("Min skewness: " + minMaxSkewnessValue[0] + " Max skewness: " + minMaxSkewnessValue[1]);
        Debug.Log("Min kurtosis: " + minMaxKurtosisValue[0] + " Max kurtosis: " + minMaxKurtosisValue[1]);
        
        //## 03: Create Data Points

        // For every Attribute one Data Mark
        for (int value = 0; value < statisticValues.Count; value++)
        {
            Debug.Log("Attribute Nr" + value + ": " + statisticValues.ElementAt(value).Key);
            
            //Default:
            DataMark.Channel channel = DataMark.DefaultDataChannel();

            //X Axis (Attributes)
            var xCoordinate = scale[0].GetScaledValue(value);
            channel.position[0] = (float)xCoordinate;
            //Debug.Log("xPos Nr" + value + ": " + xCoordinate);

            //Size - Distance between lower and upper quartiles
            var q1 = statisticValues.ElementAt(value).Value.lowerQuartile;
            var q2 = statisticValues.ElementAt(value).Value.upperQuartile;

            var barHeight = q2-q1;
            channel.size[1] = (float)barHeight;

            //Y Axis (Mean)
            var yCoordinate = q1;
            channel.position[1] = (float)yCoordinate;

            //Z Axis (Modality)
            var zCoordinate = scale[2].GetScaledValue(statisticValues.ElementAt(value).Value.modality);
            channel.position[2] = (float)zCoordinate;

            //Color (Skewness + Kurtosis)
            Color c = GetShapeColor(statisticValues.ElementAt(value).Value.skewness, statisticValues.ElementAt(value).Value.kurtosis);
            Debug.Log("Final Color: " + c);
            channel.color = c;


            visContainer.CreateDataMark(dataMarkPrefab, channel);
        }


        //## 04: Rescale
        visContainerObject.transform.localScale = new Vector3(width, height, depth);

        return visContainerObject;
    }

    /// <summary>
    /// Calculates the statistical Values used in the Chart
    /// </summary>
    private List<Scale> DataPreparation()
    {
        // Normalize Values between 0 to 1 and calculate statistic info
        List<Scale> normalizeScales = new List<Scale>(dataValues.Count);
        normalizedDataValues = new Dictionary<string, double[]>();

        statisticValues = new Dictionary<string, Distribution.DistributionValues>();

        //Calculate Min Max for every Attribute
        foreach (var dataValue in dataValues)
        {
            double currentMin = dataValue.Value.Min();
            double currentMax = dataValue.Value.Max();

            List<double> domain = new List<double>(2)
            {
                currentMin,
                currentMax
            };
            ScaleLinear currentNormScale = new ScaleLinear(domain);
            normalizeScales.Add(currentNormScale);

            // Normalize Values
            normalizedDataValues.Add(dataValue.Key, currentNormScale.GetNormalizedArray(dataValue.Value));

            // Calculate statistic data from dataset
            statisticValues.Add(dataValue.Key, Distribution.GetDescriptiveStatisticValues(currentNormScale.GetNormalizedArray(dataValue.Value)));
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
        minMaxSkewnessValue = new[]{ double.MaxValue, double.MinValue };
        minMaxKurtosisValue = new[]{ double.MaxValue, double.MinValue };

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


        return normalizeScales;
    }


    // Method Calculates the resulting Color based on Skewness and Kurtosis
    private Color GetShapeColor(double skewness, double kurtosis)
    {
        Debug.Log("skewness: " + skewness);
        Debug.Log("kurtosis: " + kurtosis);
        
        // Base Colors go from blue to gray to red
        int baseColors = 3;

        //Decide base color by checking Skewness Value
        double ratio = (skewness - minMaxSkewnessValue[0]) / (minMaxSkewnessValue[1] - minMaxSkewnessValue[0]);
        int colorIndex = Convert.ToInt32(ratio * (baseColors - 1));

        // clamp the color index to ensure it's within range
        colorIndex = Math.Min(Math.Max(colorIndex, 0), baseColors - 1);
        
        // Define final color by checking kurtosis Value (Error Value Green)
        Color finalColor = new Color(0, 1.0f, 0);

        Debug.Log("Used SingleHue Range: " + colorIndex);

        switch (colorIndex)
        {
            case 0:
                finalColor = ScaleColor.GetCategoricalColor(kurtosis, minMaxKurtosisValue[0], minMaxKurtosisValue[1], ColorHelper.blueHueValues);
                break;
            case 1:
                finalColor = ScaleColor.GetCategoricalColor(kurtosis, minMaxKurtosisValue[0], minMaxKurtosisValue[1], ColorHelper.yellowHueValues);
                break;
            case 2:
                finalColor = ScaleColor.GetCategoricalColor(kurtosis, minMaxKurtosisValue[0], minMaxKurtosisValue[1], ColorHelper.orangeHueValues);
                break;
        }

        return finalColor;

    }
}

