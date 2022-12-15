using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class is used to create a multidimensional distribution Glyph chart visualization.
/// </summary>
public class VisMDDGlyphs : Vis
{
    // Normalized Values
    public Dictionary<string, double[]> normalizedDataValues;
    
    // Calculate statistic data from dataset
    public Dictionary<string, Distribution.DistributionValues> statisticValues;
    
    public VisMDDGlyphs()
    {
        title = "MDD-Glyphs Chart";
        axes = 3;

        //Initialize dataScales
        dataScales = new List<Scale.DataScale>()
        {
            Scale.DataScale.Linear,   // X
            Scale.DataScale.Linear,    // Y
            Scale.DataScale.Linear,   // Z
            Scale.DataScale.Linear     // Color

        };

        dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
        tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
    }

    public override GameObject CreateVis()
    {
        xyzTicks = new int[] {dataValues.Keys.Count, 5, 5 };
        
        base.CreateVis();

        List<Scale> normalizedScales = DataPreparation();

        
        //## 01: Create Data Scales for Axes
        List<Scale> scale = new List<Scale>(axes);
        // Range from 0 to 1
        List<double> range = new List<double>(2);
        range.Add(0);
        range.Add(1);

        Debug.Log("MDDGlyph");

        // X Axis
        List<double> domain = new List<double>(2)
        {
            0,
            statisticValues.Keys.Count - 1
        };
        scale.Add(CreateScale(dataScales[0], domain, range));

        /*

        // Y Axis
        domain = new List<double>(2)
        {
            statisticValues.ElementAt(1).Value.smallestElement,
            statisticValues.ElementAt(1).Value.largestElement
        };
        scale.Add(CreateScale(dataScales[1], domain, range));

        // Z Axis
        domain = new List<double>(2)
        {
            statisticValues.ElementAt(2).Value.smallestElement,
            statisticValues.ElementAt(2).Value.largestElement
        };
        scale.Add(CreateScale(dataScales[2], domain, range));
        */
        
        // Color
        //domain = new List<double>(2);
        //domain.Add(dataValues.ElementAt(2).Value.Min());
        //domain.Add(dataValues.ElementAt(2).Value.Max());
        //scale.Add(CreateScale(dataScales[3], domain, range));


        //## 02: Create Axes and Grids

        // X Axis
        encodedAttribute.Add(-1);
        visContainer.CreateAxis("Attributes", (Direction)0, scale[0]);
        visContainer.CreateGrid((Direction)0, (Direction)1);

        // Y Axis
        encodedAttribute.Add(1);
        visContainer.CreateAxis("Mean", (Direction)1, scale[0]);
        visContainer.CreateGrid((Direction)1, (Direction)2);

        // Z Axis
        encodedAttribute.Add(2);
        visContainer.CreateAxis("Median", (Direction)2, scale[0]);
        visContainer.CreateGrid((Direction)2, (Direction)0);

        //## 03: Create Data Points

        // For every Attribute one Data Mark
        for (int value = 0; value < statisticValues.Count; value++)
        {
            //Default:
            DataMark.Channel channel = DataMark.DefaultDataChannel();

            //X Axis (Attributes)
            var xCoordinate = scale[0].GetScaledValue(value);
            channel.position[0] = (float)xCoordinate;
            //Debug.Log("xPos Nr" + value + ": " + xCoordinate);

            //Y Axis (Mean)
            var barHeight = statisticValues.ElementAt(value).Value.mean;
            channel.size[1] = (float)barHeight;
            
            //Z Axis (median)
            var zCoordinate = statisticValues.ElementAt(value).Value.median;
            channel.position[2] = (float)zCoordinate;

            //Debug.Log("## Attr Z: " + statisticValues.ElementAt(value).Key);
            //Debug.Log("Scale domain " + scale[2].domain[0] + " " + scale[2].domain[1]);
            //Debug.Log("Scale range " + scale[2].range[0] + " " + scale[2].range[1]);
            //Debug.Log("Value (Median) " + statisticValues.ElementAt(value).Value.median);
            //Debug.Log("Scaled Value (Median)" + zCoordinate);


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
            output = output + ("kurtosis" + ": " + statisticValue.Value.kurtosis) + "\n";
            output = output + ("skewness" + ": " + statisticValue.Value.skewness) + "\n";

            Debug.Log(output);
        }

        return normalizeScales;
    }
}

