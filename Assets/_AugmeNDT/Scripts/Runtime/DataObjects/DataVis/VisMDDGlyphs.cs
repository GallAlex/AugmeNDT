using AugmeNDT;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class is used to create a multidimensional distribution Glyph chart visualization.
/// </summary>
public class VisMDDGlyphs : Vis
{
    // Stores the minimum and maximum for each statistic value based on all dataset
    private List<DistributionValues> minMaxStatisticValues;

    // Stores for each attribute the difference value between first dataset and the next one
    private List<double[]> timeDifference;

    // If more than one dataset is loaded, should the z-Axis be for the other Datasets?
    private bool use4DData = false;

    private GameObject colorLegend;
    private VisTimeScatter timeScatter;


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
        
        if(!use4DData) xyzTicks = new int[] { dataSets[0].numericalValues.Count, 13, 7 };
        else xyzTicks = new int[] { dataSets[0].numericalValues.Count, 10, dataSets.Count };

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
        //Debug.Log("datasets: " + datasets.Length);

        //## 02: Create Axes and Grids

        // X Axis
        //encodedAttribute.Add(-1);
        visContainer.CreateAxis("Attributes", dataSets[0].GetAttributeNames().ToArray(), (Direction)0);
        visContainer.CreateGrid((Direction)0, (Direction)1);

        // Y Axis
        //encodedAttribute.Add(1);
        visContainer.CreateAxis("Attributes Value", new []{minMaxStatisticValues[0].SmallestElement, minMaxStatisticValues[1].LargestElement}, (Direction)1);
        visContainer.CreateGrid((Direction)1, (Direction)2);

        if (!use4DData)
        {
            // Z Axis
            //encodedAttribute.Add(2);
            visContainer.CreateAxis("Modality", new[] { minMaxStatisticValues[0].Modality, minMaxStatisticValues[1].Modality }, (Direction)2);
            visContainer.CreateGrid((Direction)2, (Direction)0); 
        }
        else
        {
            // Z Axis
            //encodedAttribute.Add(2);
            visContainer.CreateAxis("Timestep", datasetNames, (Direction)2); 
            visContainer.CreateGrid((Direction)2, (Direction)0);
        }


        //## 03: Calculate Channels
        MDDGlyphColorLegend legend = new MDDGlyphColorLegend(this);
        legend.SetMinMaxSkewKurtValues(new double[]{minMaxStatisticValues[0].Skewness, minMaxStatisticValues[1].Skewness}, new double[] { minMaxStatisticValues[0].Kurtosis, minMaxStatisticValues[1].Kurtosis });

        int numberOfFeatures = dataSets[0].GetAttributeNames().Count;
        int numberOfDatasets = dataSets[0].statisticValues.Count;
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
        foreach (var dataSet in dataSets)
        {
            // Calculate and combine statistical data
            for (int feature = 0; feature < numberOfFeatures; feature++)
            {
                //Add values of second dataset afterwards 
                int featureIndex = feature + (numberOfFeatures * currentDataSet);

                xPos[featureIndex] = feature;
                zPos[featureIndex] = currentDataSet;
                //Size - Distance between lower and upper quartiles
                q1[featureIndex] = dataSet.statisticValues.ElementAt(feature).Value.LowerQuartile;
                q2[featureIndex] = dataSet.statisticValues.ElementAt(feature).Value.UpperQuartile;
                barHeight[featureIndex] = q2[feature] - q1[feature];
                modality[featureIndex] = dataSet.statisticValues.ElementAt(feature).Value.Modality;

                //c[featureIndex] = legend.GetColoring(dataSet.statisticValues.ElementAt(feature).Value.Skewness, dataSet.statisticValues.ElementAt(feature).Value.Kurtosis); 
                //TODO: Add non special coloring
            }

            currentDataSet++;
        }

        if (use4DData)
        {
            timeDifference = new List<double[]>(dataSets[0].numericalAttributeCount);

            // For all attributes (attributeCount)
            for (int i = 0; i < dataSets[0].numericalAttributeCount; i++)
            {
                List<double> differences = new List<double>();
                string temp = "CalculateChiSquaredMetric of " + dataSets[0].GetAttributeName(i) + ": \n";

                // Calculate time difference between all datasets
                for (int dataSetFirstId = 0; dataSetFirstId < dataSets.Count; dataSetFirstId++)
                {
                    for (int dataSetSecondId = dataSetFirstId + 1; dataSetSecondId < dataSets.Count; dataSetSecondId++)
                    {
                        var diff = ChiSquaredMetric.CalculateChiSquaredMetric(dataSets[dataSetFirstId].GetNormalizedValues(i), dataSets[dataSetSecondId].GetNormalizedValues(i));
                        differences.Add(diff);
                        temp += dataSetFirstId + " -> " + dataSetSecondId + " = " + diff + "\n";

                    }
                }
                Debug.Log(temp);

                timeDifference.Add(differences.ToArray());
            }

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
        CreateMDDGlyphColors(legend);

        //## 04: Create Data Marks
        visContainer.CreateDataMarks(dataMarkPrefab, new []{1, 0, 1});

        //## 05: Create Color Scalar Bar
        colorLegend = legend.GetColorLegend();
        CreateColorLegend(colorLegend);


        //visContainer.GatherDataMarkValueInformation(0);
        //visContainer.GatherDataMarkValueInformation(1);

        //visContainer.CreateDataMarkAxisLine(0);
        //visContainer.CreateDataMarkAxisLine(1);


        //## 06: Rescale
        visContainerObject.transform.localScale = new Vector3(width, height, depth);
        //colorBar.transform.localScale = new Vector3(width, height, depth);


        //## 07: Set up individual Interctions
        SetUpVisTransitionInteractor(visContainerObject);

        return visContainerObject;
    }

    public override void ChangeDataMarks()
    {
        Debug.Log("Change MDDGlyph");
        visContainer.ChangeDataMarks();
    }

    /// <summary>
    /// Calculates the minMax statistical Values over all datasets
    /// </summary>
    private void DataPreparation()
    {
        int currentDataSet = 0;

        // Gather all statisticValues from all all Features in all datasets in one List (dataSets.Count * numberOfStatisticValues)
        List<DistributionValues> aggregatedStatisticValues = new List<DistributionValues>();

        //TODO: Calculate statistic data from every dataset by looping through dataValues[]
        //TODO: Calculate global Min/Max over every dataset by looking at local min/max for every Attribute in a Dataset
        foreach (var data in dataSets)
        {
            // Add the statistic Values of all Features from the current datasets in one List
            aggregatedStatisticValues.AddRange(data.statisticValues.Values.ToList());

            Debug.Log(">> MDDGlyh - DataSet [" + currentDataSet + "]: \n");
            data.PrintStatisticValues();

            currentDataSet++;
        }

        // Calculate min/max values of statisticValues over all Datasets
        minMaxStatisticValues = DistributionValues.GetMinMaxDistValues(aggregatedStatisticValues);
    }


    public void CreateMDDGlyphColors(MDDGlyphColorLegend legend)
    {

        Debug.Log(">> Create NEW MDDGlyph Colors");

        int numberOfFeatures = dataSets[0].numericalAttributeCount;
        int numberOfDatasets = dataSets[0].statisticValues.Count;
        int currentDataSet = 0;

        Color[] c = new Color[numberOfFeatures * numberOfDatasets];

        // For every Attribute one Data Mark
        //Loop through every dataset statisticValues[] and create a Data Mark for every Attribute in every dataset
        foreach (var dataSet in dataSets)
        {
            // Calculate and combine statistical data
            for (int feature = 0; feature < numberOfFeatures; feature++)
            {
                //Add values of second dataset afterwards 
                int featureIndex = feature + (numberOfFeatures * currentDataSet);

                c[featureIndex] = legend.GetColoring(dataSet.statisticValues.ElementAt(feature).Value.Skewness, dataSet.statisticValues.ElementAt(feature).Value.Kurtosis);
            }

            currentDataSet++;
        }

        visContainer.SetSpecificColor(c);
    }

    public List<int> GetFiberIDsFromIQRRange(int attribute, int axis)
    {
        int datasetNumber = 0;
        
        var q1 = dataSets[datasetNumber].statisticValues.ElementAt(attribute).Value.LowerQuartile;
        var q2 = dataSets[datasetNumber].statisticValues.ElementAt(attribute).Value.UpperQuartile;

        // Go through every fiber and check if it is in the IQR Range (in the normalizedDataSets)
        List<int> fiberIDs = new List<int>();
        
        for (int i = 0; i < dataSets[datasetNumber].normalizedValues.ElementAt(attribute).Value.Length; i++)
        {
            var value = dataSets[datasetNumber].normalizedValues.ElementAt(attribute).Value[i];
            if (value >= q1 && value <= q2)
            {
                fiberIDs.Add(i);
            }
        }

        return fiberIDs;
    }

    private void SetUpVisTransitionInteractor(GameObject container)
    {
        // Get VisContainer and add Component with update routine
        MDDTransition transitionScript = container.AddComponent<MDDTransition>();
         
        // Set class to call when Object is moved
        transitionScript.SetMDDVis(this);

        timeScatter = new VisTimeScatter();
        if (!use4DData)
        {
            timeScatter.axes = 2;
            timeScatter.width = 1;
            timeScatter.height = 1;
            timeScatter.depth = 1;
            //TODO: Copy the properties of the MDDGlyph Vis like offset, ticks,...
            //AbstractDataset statisticalData = new AbstractDataset();
            timeScatter.AppendData(dataSets[0]);
            timeScatter.CreateVis(visContainerObject);
            timeScatter.visContainerObject.transform.Rotate(90,0,0);
            timeScatter.visContainerObject.SetActive(false);
            // Use Node-Link diagram to show the change between timesteps (y axis is chi-squared metric)
        }
        else
        {
            // Use as Y Axis the Change between Timesteps (ordered)
            // USe as X Axis the Attributes
            timeScatter.axes = 2;
            timeScatter.width = 1;
            timeScatter.height = 1;
            timeScatter.depth = 1;
            //TODO: Copy the properties of the MDDGlyph Vis like offset, ticks,...

            Dictionary<string, double[]> timeData = new Dictionary<string, double[]>();
            for (var index = 0; index < dataSets[0].GetAttributeNames().Count; index++)
            {
                timeData.Add(dataSets[0].GetAttributeName(index), timeDifference[index]);
            }

            AbstractDataset timeDataset = new AbstractDataset(dataSets[0].GetAttributeNames(), timeData);
            timeScatter.AppendData(timeDataset);
            timeDataset.PrintStatisticValues();

            timeScatter.CreateVis(visContainerObject);
            timeScatter.visContainerObject.transform.Rotate(90, 0, 0);
            timeScatter.visContainerObject.SetActive(false);
        }

        timeScatter.visContainer.RemoveContainerHandle();
    }

    public void ApplyMDDTransition(bool apply2DTransiton)
    {
        if (apply2DTransiton)
        {
            // Hide the whole VisContainer and show a new one
            visContainer.visContainer.SetActive(false);
            colorLegend.SetActive(false);
            // Create new 2D Vis 
            timeScatter.visContainerObject.SetActive(true);

        }
        else
        {
            // Make the whole VisContainer visible
            visContainer.visContainer.SetActive(true);
            colorLegend.SetActive(true);
            //Hide 2D Vis
            timeScatter.visContainerObject.SetActive(false);

        }

    }

}

