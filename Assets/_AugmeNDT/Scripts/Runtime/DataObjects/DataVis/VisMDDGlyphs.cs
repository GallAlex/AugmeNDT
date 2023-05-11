using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// This class is used to create a multidimensional distribution Glyph chart visualization.
/// </summary>
public class VisMDDGlyphs : Vis
{
    // Stores the Normalized Values of each dataset
    public List<Dictionary<string, double[]>> normalizedDataSets;

    // Stores the calculated statistic data of each normalized dataset
    public List<Dictionary<string, DistributionValues>> statisticDataSets;

    // Stores the minimum and maximum for each statistic value based on all dataset
    private List<DistributionValues> minMaxStatisticValues;

    // If more than one dataset is loaded, should the z-Axis be for the other Datasets?
    private bool use4DData = false;

    public VisMDDGlyphs()
    {
        title = "MDD-Glyphs Chart";
        axes = 3;

        dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
        tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");

        // Create Interactor
        visInteractor = new VisMDDGlyphInteractor(this);

    }
    

    public override GameObject CreateVis(GameObject container)
    {
        if (dataSets.Count > 1) use4DData = true;
        
        if(!use4DData) xyzTicks = new int[] { dataSets[0].Keys.Count, 13, 7 };
        else xyzTicks = new int[] { dataSets[0].Keys.Count, 10, dataSets.Count };

        xyzOffset = new float[] { 0.05f, 0.05f, 0.05f };
        
        base.CreateVis(container);

        DataPreparation();

        Debug.Log("Create MDDGlyph");



        double[] datasets = new double[dataSets.Count];
        string[] datasetNames = new string[dataSets.Count];
        for (int i = 0; i < dataSets.Count; i++)
        {
            datasets[i] = i;
            datasetNames[i] = "Dataset_" + i;
        }
        Debug.Log("datasets: " + datasets.Length);

        //## 02: Create Axes and Grids

        // X Axis
        encodedAttribute.Add(-1);
        visContainer.CreateAxis("Attributes", dataSets[0].Keys.ToArray(), (Direction)0);
        visContainer.CreateGrid((Direction)0, (Direction)1);

        // Y Axis
        encodedAttribute.Add(1);
        visContainer.CreateAxis("Attributes Value", new []{minMaxStatisticValues[0].SmallestElement, minMaxStatisticValues[1].LargestElement}, (Direction)1);
        visContainer.CreateGrid((Direction)1, (Direction)2);

        if (!use4DData)
        {
            // Z Axis
            encodedAttribute.Add(2);
            visContainer.CreateAxis("Modality", new[] { minMaxStatisticValues[0].Modality, minMaxStatisticValues[1].Modality }, (Direction)2);
            visContainer.CreateGrid((Direction)2, (Direction)0); 
        }
        else
        {
            // Z Axis
            encodedAttribute.Add(2);
            visContainer.CreateAxis("Timestep", datasetNames, (Direction)2); 
            visContainer.CreateGrid((Direction)2, (Direction)0);
        }


        //## 03: Calculate Channels
        int numberOfFeatures = dataSets[0].Keys.Count;
        int numberOfDatasets = statisticDataSets.Count;
        int currentDataSet = 0;


        double[] xPos = new double[numberOfFeatures * numberOfDatasets];
        double[] zPos = new double[numberOfFeatures * numberOfDatasets];
        double[] barHeight = new double[numberOfFeatures * numberOfDatasets];
        double[] q1 = new double[numberOfFeatures * numberOfDatasets];
        double[] q2 = new double[numberOfFeatures * numberOfDatasets];
        double[] modality = new double[numberOfFeatures * numberOfDatasets];
        Color[] c = new Color[numberOfFeatures * numberOfDatasets];

        // For every Attribute one Data Mark
        //Loop through every dataset statisticValues[] and create a Data Mark for every Attribute in every dataset
        foreach (var dataSet in statisticDataSets)
        {
            // Calculate and combine statistical data
            for (int feature = 0; feature < numberOfFeatures; feature++)
            {
                //Add values of second dataset afterwards 
                int featureIndex = feature + (numberOfFeatures * currentDataSet);

                xPos[featureIndex] = feature;
                zPos[featureIndex] = currentDataSet;
                //Size - Distance between lower and upper quartiles
                q1[featureIndex] = dataSet.ElementAt(feature).Value.LowerQuartile;
                q2[featureIndex] = dataSet.ElementAt(feature).Value.UpperQuartile;
                barHeight[featureIndex] = q2[feature] - q1[feature];
                modality[featureIndex] = dataSet.ElementAt(feature).Value.Modality;
                c[featureIndex] = GetShapeColor(dataSet.ElementAt(feature).Value.Skewness, dataSet.ElementAt(feature).Value.Kurtosis); 
            }

            currentDataSet++;
        }

        //X Axis (Attributes)
        visContainer.SetChannel(VisChannel.XPos, xPos);

        //Y Axis (Mean)
        visContainer.SetChannel(VisChannel.YPos, q1);

        // Y Size (IQR)
        visContainer.SetChannel(VisChannel.YSize, barHeight);

        if (!use4DData)
        {
            //Z Axis (Modality)
            visContainer.SetChannel(VisChannel.ZPos, modality);
        }
        else
        {
            //Z Axis (Timesteps)
            visContainer.SetChannel(VisChannel.ZPos, zPos);
        }

        //Color (Skewness + Kurtosis)
        visContainer.SetChannel(VisChannel.Color, zPos);
        visContainer.SetSpecificColor(c);

        //## 04: Create Data Marks
        visContainer.CreateDataMarks(dataMarkPrefab, new []{0, 1, 0});

        //## 05: Create Color Scalar Bar
        CreateMDDColorBar();

        //## 06: Rescale
        visContainerObject.transform.localScale = new Vector3(width, height, depth);
        //colorBar.transform.localScale = new Vector3(width, height, depth);

        visContainer.GatherDataMarkValueInformation(0);
        visContainer.GatherDataMarkValueInformation(1);

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
            string statisticValuesString = ">> DataSet [" + currentDataSet + "]: ";
            // Output statisticValues for Debug Purposes
            foreach (var statisticValue in statisticValues)
            {
                statisticValuesString += statisticValue.Key + ": \n" + statisticValue.Value.PrintDistributionValues();
                statisticValuesString += "\n";
            }
            Debug.Log(statisticValuesString);
            //#### END PRINT STATISTIC VALUES ####

            currentDataSet++;
        }

        // Calculate min/max values of statisticValues over all Datasets
        minMaxStatisticValues = DistributionValues.GetMinMaxDistValues(aggregatedStatisticValues);
    }



    /// <summary>
    /// Method Calculates the resulting Color based on Skewness and Kurtosis
    /// Kurtosis:
    /// The excess kurtosis is defined as kurtosis minus 3
    /// excess kurtosis = 0: symmetric, Normal distribution
    /// excess kurtosis > 0: more prominent peaks, fatter tails - more outliers
    /// excess kurtosis < 0: more flat peaks, thinner tails - fewer outliers
    /// Skewness:
    /// skewness = 0: symmetric
    /// skewness > 0: right skewed, more Values that are smaller than the mean,
    /// skewness < 0: left skewed, more Values that are bigger than the mean,
    /// </summary>
    /// <param name="skewness"></param>
    /// <param name="kurtosis"></param>
    /// <returns></returns>
    private Color GetShapeColor(double skewness, double kurtosis)
    {
        int baseColors = 3;

        ////Decide base color by checking Skewness Value
        //double ratio = (skewness - minMaxStatisticValues[0].Skewness) / (minMaxStatisticValues[1].Skewness - minMaxStatisticValues[0].Skewness);
        
        //if (double.IsNaN(ratio))
        //{
        //    Debug.LogError("Calculation yielded NaN: Check Results");
        //    ratio = 0;
        //}

        //int colorIndex = Convert.ToInt32(ratio * (baseColors - 1));

        //// clamp the color index to ensure it's within range
        //colorIndex = Math.Min(Math.Max(colorIndex, 0), baseColors - 1);

        // Define final color by checking kurtosis Value (Error Value Green)
        Color finalColor = new Color(0, 1.0f, 0);

        //BreakPoints from https://brownmath.com/stat/shape.htm#Skewness

        // Kurtosis
        double kurtosisVal = 0;

        if (kurtosis >= 1) kurtosisVal = 1;
        else if (kurtosis < 1 && kurtosis > -1) kurtosisVal = 0;
        else if (kurtosis <= -1) kurtosisVal = -1;

        // Skewness
        if (skewness >= 1)
            finalColor = ScaleColor.GetCategoricalColor(kurtosisVal, 0, 1, ColorHelper.viridisPurpleHue);
        else if (skewness < 1 && skewness > -1)
            finalColor = ScaleColor.GetCategoricalColor(kurtosisVal, 0, 1, ColorHelper.viridisGreenHue);
        else if (skewness <= -1)
            finalColor = ScaleColor.GetCategoricalColor(kurtosisVal, 0, 1, ColorHelper.viridisYellowHue);


        return finalColor;

    }

    

    public List<int> GetFiberIDsFromIQRRange(int attribute, int axis)
    {
        int datasetNumber = 0;
        
        var q1 = statisticDataSets[datasetNumber].ElementAt(attribute).Value.LowerQuartile;
        var q2 = statisticDataSets[datasetNumber].ElementAt(attribute).Value.UpperQuartile;

        // Go through every fiber and check if it is in the IQR Range (in the normalizedDataSets)
        List<int> fiberIDs = new List<int>();
        
        for (int i = 0; i < normalizedDataSets[datasetNumber].ElementAt(attribute).Value.Length; i++)
        {
            var value = normalizedDataSets[datasetNumber].ElementAt(attribute).Value[i];
            if (value >= q1 && value <= q2)
            {
                fiberIDs.Add(i);
            }
        }

        return fiberIDs;
    }

    private GameObject CreateMDDColorBar()
    {
        GameObject colorScalarBarContainer = new GameObject("Color Scale");
        colorScalarBarContainer.transform.parent = visContainerObject.transform;

        Vector3 positions = new Vector3(1 + (width * 0.7f), 0.5f, 0.5f);
        List<Color[]> colorSchemeList = new List<Color[]>(){ ColorHelper.viridisPurpleHue , ColorHelper.viridisGreenHue, ColorHelper.viridisYellowHue };

        MDDGlyphColorLegend legend = new MDDGlyphColorLegend();
        GameObject colorLegend = legend.CreateColorLegend(positions, colorSchemeList);
        colorLegend.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        colorLegend.transform.parent = colorScalarBarContainer.transform;


        //ColorScalarBar colorScalarBar = new ColorScalarBar();
        //Vector3[] positions = new Vector3[3];

        //for (int i = 0; i < positions.Length; i++)
        //{
        //    positions[i] = visContainerObject.transform.position;
        //    positions[i].x += 1 + (width * 0.4f + i * 0.25f);
        //    positions[i].y += 0.5f;
        //    positions[i].z += 0.5f;
        //}

        //double[] colorBarValues = new double[2] { minMaxStatisticValues[0].Kurtosis, minMaxStatisticValues[1].Kurtosis};
        //double centerSkewness = 0.5 * (minMaxStatisticValues[1].Skewness - minMaxStatisticValues[0].Skewness) + minMaxStatisticValues[0].Skewness;

        //GameObject colorBar01 = colorScalarBar.CreateColorScalarBar(positions[0], "[" + minMaxStatisticValues[0].Skewness.Round(3) + "]", colorBarValues, 1, ColorHelper.viridisPurpleHue);
        //colorBar01.transform.parent = colorScalarBarContainer.transform;

        //GameObject colorBar02 = colorScalarBar.CreateColorScalarBar(positions[1], "Skewness \n [" + centerSkewness.Round(3) + "]", colorBarValues, 1, ColorHelper.viridisGreenHue);
        //colorBar02.transform.parent = colorScalarBarContainer.transform;

        //GameObject colorBar03 = colorScalarBar.CreateColorScalarBar(positions[2], "[" + minMaxStatisticValues[1].Skewness.Round(3) + "]", colorBarValues, 1, ColorHelper.viridisYellowHue);
        //colorBar03.transform.parent = colorScalarBarContainer.transform;

        return colorScalarBarContainer;
    }

}

