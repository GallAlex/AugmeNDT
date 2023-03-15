using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class is used to create a multidimensional distribution Glyph chart visualization.
/// </summary>
public class VisMDDGlyphs : Vis
{
    // Stores the Normalized Values of each dataset
    public List<Dictionary<string, double[]>> normalizedDataSets;

    // Stores the calculated statistic data of each dataset
    public List<Dictionary<string, DistributionValues>> statisticDataSets;

    // Stores the minimum and maximum for each statistic value based on all dataset
    private List<DistributionValues> minMaxStatisticValues;

    // If more than one dataset is loaded, should the z-Axis be for the other Datasets?
    private bool use4DData = false;

    private List<Scale> scale;

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

        // Create Interactor
        visInteractor = new VisMDDGlyphInteractor(this);

    }

    public override GameObject CreateVis(GameObject container)
    {
        if (dataSets.Count > 1) use4DData = true;
        
        if(!use4DData) xyzTicks = new int[] { dataSets[0].Keys.Count, 10, 10 };
        else xyzTicks = new int[] { dataSets[0].Keys.Count, 10, dataSets.Count };

        xyzOffset = new float[] { 0.05f, 0.05f, 0.05f };
        
        base.CreateVis(container);

        DataPreparation();

        Debug.Log("Create MDDGlyph");

        //## 01: Create Data Scales for Axes
        scale = new List<Scale>(axes);

        List<double> range = new List<double>(2)
        {
            0.0d + xyzOffset[0],
            1.0d - xyzOffset[0]
        };

        List<double> specialYRange = new List<double>(2)
        {
            0.0d + xyzOffset[2],
            1.0d - xyzOffset[2]
        };

        // X Axis
        List<double> domain = new List<double>(2)
        {
            0,
            dataSets[0].Keys.Count-1
        };
        scale.Add(new ScaleNominal(domain, range, dataSets[0].Keys.ToList()));

        // Y Axis
        domain = new List<double>(2)
        {
            minMaxStatisticValues[0].SmallestElement,
            minMaxStatisticValues[1].LargestElement
        };
        scale.Add(new ScaleLinear(domain, specialYRange));

        if (!use4DData)
        {
            // Z Axis
            domain = new List<double>(2)
            {
                minMaxStatisticValues[0].Modality,
                minMaxStatisticValues[1].Modality
            };
            scale.Add(new ScaleLinear(domain, range));
        }
        else
        {
            // Z Axis
            domain = new List<double>(2)
            {
                0,
                dataSets.Count-1
            };

            //TODO: Use real dataset names
            List<string> names = new List<string>(dataSets.Count - 1);
            for (int i = 0; i < dataSets.Count; i++)
            {
                names.Add("Dataset " + i);
            }
            scale.Add(new ScaleNominal(domain, range, names));
        }

        //## 02: Create Axes and Grids

        // X Axis
        encodedAttribute.Add(-1);
        visContainer.CreateAxis("Attributes", (Direction)0, scale[0]);
        visContainer.CreateGrid((Direction)0, (Direction)1);

        // Y Axis
        encodedAttribute.Add(1);
        visContainer.CreateAxis("Attributes Value", (Direction)1, scale[1]);
        visContainer.CreateGrid((Direction)1, (Direction)2);

        if (!use4DData)
        {
            // Z Axis
            encodedAttribute.Add(2);
            visContainer.CreateAxis("Modality", (Direction)2, scale[2]);
            visContainer.CreateGrid((Direction)2, (Direction)0); 
        }
        else
        {
            // Z Axis
            encodedAttribute.Add(2);
            visContainer.CreateAxis("Timestep", (Direction)2, scale[2]);
            visContainer.CreateGrid((Direction)2, (Direction)0);
        }

        //## 03: Create Data Points
        int currentDataSet = 0;
        // For every Attribute one Data Mark
        //Loop through every dataset statisticValues[] and create a Data Mark for every Attribute in every dataset
        foreach (var statisticValue in statisticDataSets)
        {
            for (int value = 0; value < statisticValue.Count; value++)
            {
                //Debug.Log("Attribute Nr" + value + ": " + statisticValues.ElementAt(value).Key);

                //Default:
                DataMark.Channel channel = DataMark.DefaultDataChannel();

                //X Axis (Attributes)
                var xCoordinate = scale[0].GetScaledValue(value);
                channel.position[0] = (float)xCoordinate;
                
                //Debug.Log("xPos Nr" + value + ": " + xCoordinate);

                //Size - Distance between lower and upper quartiles
                var q1 = scale[1].GetScaledValue(statisticValue.ElementAt(value).Value.LowerQuartile);
                var q2 = scale[1].GetScaledValue(statisticValue.ElementAt(value).Value.UpperQuartile);

                var barHeight = q2 - q1;
                channel.size[1] = (float)barHeight;

                //Y Axis (Mean)
                var yCoordinate = q1;
                channel.position[1] = (float)yCoordinate;

                if (!use4DData)
                {
                    //Z Axis (Modality)
                    var zCoordinate = scale[2].GetScaledValue(statisticValue.ElementAt(value).Value.Modality);
                    channel.position[2] = (float)zCoordinate;
                }
                else
                {
                    //Z Axis (Timesteps)
                    var zCoordinate = scale[2].GetScaledValue(currentDataSet);
                    channel.position[2] = (float)zCoordinate;
                }


                //Color (Skewness + Kurtosis)
                Color c = GetShapeColor(statisticValue.ElementAt(value).Value.Skewness, statisticValue.ElementAt(value).Value.Kurtosis);

                channel.color = c;


                visContainer.CreateDataMark(dataMarkPrefab, channel);
            }

            currentDataSet++;
        }

        //## 04: Rescale
        visContainerObject.transform.localScale = new Vector3(width, height, depth);

        return visContainerObject;
    }

    /// <summary>
    /// Calculates the statistical Values used in the Chart
    /// </summary>
    private void DataPreparation()
    {
        int currentDataSet = 0;
        normalizedDataSets = new List<Dictionary<string, double[]>>(dataSets.Count);
        statisticDataSets = new List<Dictionary<string, DistributionValues>>(dataSets.Count);

        // Gather all statisticValues from all datasets in one List
        List<DistributionValues> aggregatedStatisticValues = new List<DistributionValues>(statisticDataSets.Count);
        
        //TODO: Calculate statistic data from every dataset by looping through dataValues[]
        //TODO: Calculate global Min/Max over every dataset by looking at local min/max for every Attribute in a Dataset
        foreach (var data in dataSets)
        {

            // Normalize Values between 0 to 1 and calculate statistic info
            List<Scale> normalizeScales = new List<Scale>(data.Count);
            Dictionary<string, double[]> normalizedValues = new Dictionary<string, double[]>(data.Count);
            Dictionary<string, DistributionValues>  statisticValues = new Dictionary<string, DistributionValues>(data.Count);
            
            //Calculate Min Max for every Attribute
            foreach (var dataValue in data)
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
                normalizedValues.Add(dataValue.Key, currentNormScale.GetNormalizedArray(dataValue.Value));

                // Calculate statistic values from dataset
                DistributionValues distVal = new DistributionValues();
                distVal.GetDescriptiveStatisticValues(currentNormScale.GetNormalizedArray(dataValue.Value));
                statisticValues.Add(dataValue.Key, distVal);
            }

            //Normalized Dataset
            normalizedDataSets.Add(normalizedValues);
            //Normalized Dataset
            statisticDataSets.Add(statisticValues);
            // Collected all statisticValues from all datasets in one List
            aggregatedStatisticValues.AddRange(statisticValues.Values.ToList());

            //#### PRINT STATISTIC VALUES ####
            //string statisticValuesString = ">> DataSet [" + currentDataSet + "]: ";
            //// Output statisticValues for Debug Purposes
            //foreach (var statisticValue in statisticValues)
            //{
            //    statisticValuesString += statisticValue.Key + ": \n" + statisticValue.Value.PrintDistributionValues();
            //    statisticValuesString += "\n";
            //}
            //Debug.Log(statisticValuesString);
            //#### END PRINT STATISTIC VALUES ####

            currentDataSet++;
        }

        // Calculate min/max values of statisticValues over all Datasets
        minMaxStatisticValues = DistributionValues.GetMinMaxDistValues(aggregatedStatisticValues);
    }



    // Method Calculates the resulting Color based on Skewness and Kurtosis
    private Color GetShapeColor(double skewness, double kurtosis)
    {
        // Base Colors go from blue to gray to red
        int baseColors = 3;

        //Decide base color by checking Skewness Value
        double ratio = (skewness - minMaxStatisticValues[0].Skewness) / (minMaxStatisticValues[1].Skewness - minMaxStatisticValues[0].Skewness);
        int colorIndex = Convert.ToInt32(ratio * (baseColors - 1));

        // clamp the color index to ensure it's within range
        colorIndex = Math.Min(Math.Max(colorIndex, 0), baseColors - 1);

        // Define final color by checking kurtosis Value (Error Value Green)
        Color finalColor = new Color(0, 1.0f, 0);

        switch (colorIndex)
        {
            case 0:
                finalColor = ScaleColor.GetCategoricalColor(kurtosis, minMaxStatisticValues[0].Kurtosis, minMaxStatisticValues[1].Kurtosis, ColorHelper.blueHueValues);
                break;
            case 1:
                finalColor = ScaleColor.GetCategoricalColor(kurtosis, minMaxStatisticValues[0].Kurtosis, minMaxStatisticValues[1].Kurtosis, ColorHelper.yellowHueValues);
                break;
            case 2:
                finalColor = ScaleColor.GetCategoricalColor(kurtosis, minMaxStatisticValues[0].Kurtosis, minMaxStatisticValues[1].Kurtosis, ColorHelper.orangeHueValues);
                break;
        }

        return finalColor;

    }

    public List<int> GetFiberIDsFromIQRRange(int attribute, int axis)
    {
        int datasetNumber = 0;
        
        var q1 = scale[axis].GetScaledValue(statisticDataSets[datasetNumber].ElementAt(attribute).Value.LowerQuartile);
        var q2 = scale[axis].GetScaledValue(statisticDataSets[datasetNumber].ElementAt(attribute).Value.UpperQuartile);

        // Go through every fiber and check if it is in the IQR Range (in the normalizedDataSets)
        List<int> fiberIDs = new List<int>();
        
        for (int i = 0; i < normalizedDataSets[datasetNumber].ElementAt(attribute).Value.Length; i++)
        {
            var value = scale[axis].GetScaledValue(normalizedDataSets[datasetNumber].ElementAt(attribute).Value[i]);
            if (value >= q1 && value <= q2)
            {
                fiberIDs.Add(i);
            }
        }

        return fiberIDs;
    }
}

