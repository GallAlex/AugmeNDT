// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System;
using System.Collections.Generic;
using System.Linq;
using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;

namespace AugmeNDT
{
    public class VisStackedHistogram : Vis
    {
        private bool use4DData = false;                     // If more than one dataset is loaded, should the z-Axis be for the other Datasets?

        private HistogramValues[] histograms;       // Histograms with same number of bins and same lower & upper bound for each dataset
        private int binCount;                       // Fixed bin count with fixed bin width

        private Attribute frequencies;              // Frequencies for each bin (for each dataset)
        private double[] minMaxYPos;                // Min and Max Y-Position (over all dataset)
        private Attribute yPos;                     // Stacked bars: Position based on the frequency of the previous bin (for each dataset)


        private int numberOfBOI = 3;                        // Number of Bins of Interest (BOI) defines how many bins with the highest change are returned (ranked from most change to less change)
        private GameObject changeIndicatorPrefab;   // Material for the change indicator
        
        private double[] minMaxChange;              // Min and Max change
        private double[] posMinMaxChange;           // Min and Max change for all positive Indicators
        private double[] negMinMaxChange;           // Min and Max change for all negative Indicators
        public GameObject text3DPrefab;

        public VisStackedHistogram()
        {
            title = "Stacked Histogram";
            axes = 2;

            dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/BarWithOutline");
            tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");

            changeIndicatorPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/ChangeIndicator");
            text3DPrefab = (GameObject)Resources.Load("Prefabs/Text3D");
        }


        public override GameObject CreateVis(GameObject container)
        {
            base.CreateVis(container);

            xyzOffset = new float[] { 0.05f, 0.05f, 0.05f };

            SetVisParams();

            if (dataSets.Count > 1) use4DData = true;

            //## 00: Preprocess Data
            PrepareHistogram(channelEncoding[VisChannel.YPos]);

            // TODO: Test Setting
            numberOfBOI = binCount;

            xyzTicks = new[] { channelEncoding[VisChannel.XPos].GetNumberOfValues(), binCount, 13 };
            visContainer.SetAxisTickNumber(xyzTicks);

            //Debug.Log("VisStackedHistogram X.Pos: \n" + channelEncoding[VisChannel.XPos].PrintAttribute());

            // Number of Datasets does not extend for every Bin - only n Datasets (time steps) strings but for multiple bins i
            double[] timesteps;
            if (channelEncoding[VisChannel.XPos].GetNumberOfValues() !=
                binCount)
            {
                timesteps = new double[dataEnsemble.GetDataSetCount() * binCount];

                for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount(); dataSet++)
                {
                    for (int binIndex = 0; binIndex < binCount; binIndex++)
                    {
                        timesteps[binIndex + (dataSet * binCount)] = dataSet;
                    }

                }

            }
            else
            {
                Debug.Log("VisStackedHistogram: else-branch reached");
                timesteps = channelEncoding[VisChannel.XPos].GetNumericalVal();
            }

            /*

            Debug.Log("VisStackedHistogram VisChannel.XPos: \n" + TablePrint.ToStringRow(channelEncoding[VisChannel.XPos].GetNumericalVal()));
            Debug.Log("DRAW Timesteps: \n" + TablePrint.ToStringRow(timesteps));
            Debug.Log("VisStackedHistogram minMaxYPos: \n" + TablePrint.ToStringRow(minMaxYPos));
            Debug.Log("DRAW yPos: \n" + TablePrint.ToStringRow(yPos.GetNumericalVal()));
            Debug.Log("DRAW frequencies: \n" + TablePrint.ToStringRow(frequencies.GetNumericalVal()));
            
            */
            
            //## 01:  Create Axes and Grids

            // X Axis
            // Nominal Ranges for the Time Steps
            CreateAxis(channelEncoding[VisChannel.XPos], false, Direction.X);
            visContainer.CreateGrid(Direction.X, Direction.Y);

            // Y Axis
            // Frequency of the Histogram Bins
            visContainer.CreateAxis(frequencies.GetName(), minMaxYPos, Direction.Y);

            //## 02: Set Remaining Vis Channels (Color,...)
            visContainer.SetChannel(VisChannel.XPos, timesteps);
            SetChannel(VisChannel.YPos, yPos, false);
            SetChannel(VisChannel.YSize, frequencies, false);
            //SetChannel(VisChannel.Color, frequencies, false);

            //## 03: Draw all Data Points with the provided Channels 
            visContainer.CreateDataMarks(dataMarkPrefab, new[] { 1, 0, 1 });

            // Only if we have multiple Bars
            if (use4DData)
            {
                DrawChangeIndicator();

                //## 04: Create Color Scalar Bar

                LegendColorBar colorScalarBar = new LegendColorBar();
                GameObject colorBar = colorScalarBar.CreateColorScalarBar(visContainerObject.transform.position, "Frequency Difference", new[] { minMaxChange[0], minMaxChange[1] }, 0.0, 1, ColorHelper.divergingValues);
                //colorBar01.transform.parent = colorScalarBarContainer.transform;
                CreateColorLegend(colorBar);
            }

            DrawBinIndicator(true);

            //## 05: Rescale Chart
            visContainerObject.transform.localScale = new Vector3(width, height, depth);


            return visContainerObject;

        }

        private void PrepareHistogram(Attribute attribute)
        {
            histograms = new HistogramValues[dataEnsemble.GetDataSetCount()];                   // Histogram for each dataset
            int index = dataEnsemble.GetDataSet(0).attributeNames.IndexOf(attribute.GetName()); // Index of Attribute
            double[] minMaxValues = dataEnsemble.GetMinMaxAttrVal(0, index, false);             // Start with vals from first dataset
            int maxDataPoints = dataEnsemble.GetAttribute(0, index).GetNumberOfValues();        // Max number of occurencies/data points in a dataset
            int minDataPoints = dataEnsemble.GetAttribute(0, index).GetNumberOfValues();        // Max number of occurencies/data points in a dataset

            // Find min and max val over all datasets for histogram bounds
            for (int dataset = 1; dataset < dataEnsemble.GetDataSetCount(); dataset++)
            {
                //Debug.Log("Dataset " + dataset + " of " + dataEnsemble.GetDataSetCount());

                double[] currentMinMax = dataEnsemble.GetMinMaxAttrVal(dataset, index, false);

                // Save min val and max val over all datasets 
                if (currentMinMax[0] < minMaxValues[0]) minMaxValues[0] = currentMinMax[0];
                if (currentMinMax[1] > minMaxValues[1]) minMaxValues[1] = currentMinMax[1];

                // Save max number of val over all datasets 
                int numberOfDataPoints = dataEnsemble.GetAttribute(dataset, index).GetNumberOfValues();
                if (numberOfDataPoints > maxDataPoints) maxDataPoints = numberOfDataPoints;
                if (numberOfDataPoints < minDataPoints) minDataPoints = numberOfDataPoints;

            }

            binCount = DistributionCalc.GetNumberOfBins(maxDataPoints); // Number of bins
            List<double> frequencyList = new List<double>();            // List of bin frequencies for all datasets
            double maxFrequency = 0;                                    // Get highest sum of frequency over all datasets for y-Axis
            List<double> yPosList = new List<double>();                 // List of bin positions for all datasets

            /*

            Debug.Log("VisStackedHistogram: Index of Attribute " + attribute.GetName() + " is " + index);
            Debug.Log("VisStackedHistogram: MinMaxValues: " + minMaxValues[0] + " " + minMaxValues[1]);
            Debug.Log("VisStackedHistogram: MaxDataPoints: " + maxDataPoints);
            Debug.Log("VisStackedHistogram: binCount: " + binCount); 
            
             */

            // Create Histogram for all datasets
            for (int dataset = 0; dataset < dataEnsemble.GetDataSetCount(); dataset++)
            {
                histograms[dataset] = new HistogramValues(dataEnsemble.GetAttribute(dataset, index).GetNumericalVal(), binCount, minMaxValues[0], minMaxValues[1]);

                double sumedFrequency = 0;
                double[] currentFrequency = histograms[dataset].GetBinFrequencies();
                frequencyList.AddRange(currentFrequency);

                //Sum up frequencies 
                for (int bin = 0; bin < binCount; bin++)
                {

                    if (bin == 0) yPosList.Add(0);
                    else yPosList.Add(sumedFrequency);  // Sum up previous frequencies (ySizes) 

                    sumedFrequency += currentFrequency[bin];
                }

                if (sumedFrequency > maxFrequency) maxFrequency = sumedFrequency;
            }

            //Debug.Log("VisStackedHistogram: maxFrequency: " + maxFrequency);
            //Debug.Log("VisStackedHistogram: yPos : " + TablePrint.ToStringRow(yPosList.ToArray()));

            // Finish drawable values
            frequencies = new Attribute("Frequencies", frequencyList.ToArray());
            yPos = new Attribute(attribute.GetName(), yPosList.ToArray());
            minMaxYPos = new double[] { 0, maxFrequency };

        }

        /// <summary>
        /// Method connects the selected bins (BOI) with a Polygon between their time steps.
        /// The Polygon spans from the Top of the Bins from one Timstep to the other to the Bottoms of it.
        /// The color of the Polygon is based on the change in frequency of the bin from one timestep to the other.
        /// </summary>
        private void DrawChangeIndicator()
        {
            List<Dictionary<int, double>> marksToDraw = null;
            // Either get the specific amount of ins with highest change or all changes
            if(numberOfBOI != binCount) marksToDraw = GetHighestChange(numberOfBOI);
            else marksToDraw = GetAllChanges();

            GameObject ChangeIndicator = new GameObject("BinDifference Indicator");
            ChangeIndicator.transform.parent = visContainer.dataMarkContainer.transform;

            for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount() -1; dataSet++)
            {

                foreach (KeyValuePair<int, double> entry in marksToDraw[dataSet])
                {

                    // If value is zero - no changes found
                    //if (entry.Value == 0) continue;

                    // TODO: Reference DataMarks by something better then ID? DataMark has Row/Index ID and dataset ID (select first row of first Dataset with first row of second dataset)
                    int currentID = entry.Key + (dataSet * binCount);
                    int nextID = entry.Key + ((dataSet+ 1) * binCount);

                    GameObject currentDataMark = visContainer.dataMarkList[currentID].GetDataMarkInstance();
                    GameObject nextDataMark = visContainer.dataMarkList[nextID].GetDataMarkInstance();

                    GameObject indicator = GameObject.Instantiate(changeIndicatorPrefab);
                    indicator.name = "DiffIndicator_B" + entry.Key + "_" + dataSet + "_" + (dataSet + 1);
                    indicator.transform.parent = ChangeIndicator.transform;

                    //Set Interaction details
                    ChangeIndicatorInteractable interactable = indicator.GetComponent<ChangeIndicatorInteractable>();
                    interactable.indicatorID = entry.Key + "_" + dataSet + "_" + (dataSet + 1);
                    interactable.RefToClass = this;

                    var meshFilter = indicator.GetComponent<MeshFilter>();
                    var meshCollider = indicator.GetComponent<MeshCollider>();
                    var meshRenderer = indicator.GetComponent<MeshRenderer>();

                    meshRenderer.material.color = CreateChangeIndicatorColor(entry.Value);

                    //## Create Quad Mesh

                    // Create vertices
                    Vector3[] vertices = new Vector3[8];

                    //Front Side
                    vertices[0] = AncerPointCalc.GetAncorPoint(currentDataMark.transform, AncerPointCalc.AncorPointX.Right, AncerPointCalc.AncorPointY.Bottom, AncerPointCalc.AncorPointZ.Front);
                    vertices[1] = AncerPointCalc.GetAncorPoint(nextDataMark.transform, AncerPointCalc.AncorPointX.Left, AncerPointCalc.AncorPointY.Bottom, AncerPointCalc.AncorPointZ.Front);
                    vertices[2] = AncerPointCalc.GetAncorPoint(nextDataMark.transform, AncerPointCalc.AncorPointX.Left, AncerPointCalc.AncorPointY.Top, AncerPointCalc.AncorPointZ.Front);
                    vertices[3] = AncerPointCalc.GetAncorPoint(currentDataMark.transform, AncerPointCalc.AncorPointX.Right, AncerPointCalc.AncorPointY.Top, AncerPointCalc.AncorPointZ.Front);
                    //Back Side
                    vertices[4] = AncerPointCalc.GetAncorPoint(currentDataMark.transform, AncerPointCalc.AncorPointX.Right, AncerPointCalc.AncorPointY.Top, AncerPointCalc.AncorPointZ.Back);
                    vertices[5] = AncerPointCalc.GetAncorPoint(nextDataMark.transform, AncerPointCalc.AncorPointX.Left, AncerPointCalc.AncorPointY.Top, AncerPointCalc.AncorPointZ.Back);
                    vertices[6] = AncerPointCalc.GetAncorPoint(nextDataMark.transform, AncerPointCalc.AncorPointX.Left, AncerPointCalc.AncorPointY.Bottom, AncerPointCalc.AncorPointZ.Back);
                    vertices[7] = AncerPointCalc.GetAncorPoint(currentDataMark.transform, AncerPointCalc.AncorPointX.Right, AncerPointCalc.AncorPointY.Bottom, AncerPointCalc.AncorPointZ.Back);

                    int[] triangles = {
                        0, 2, 1, //face front
                        0, 3, 2,
                        2, 3, 4, //face top
                        2, 4, 5,
                        1, 2, 5, //face right
                        1, 5, 6,
                        0, 7, 4, //face left
                        0, 4, 3,
                        5, 4, 7, //face back
                        5, 7, 6,
                        0, 6, 7, //face bottom
                        0, 1, 6
                    };

                    Mesh mesh = meshFilter.mesh;
                    mesh.Clear();
                    mesh.vertices = vertices;
                    mesh.triangles = triangles;
                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();
                    mesh.Optimize();

                    // Check if traingles are coplanar befor adding mesh to collider
                    // Therefore check if y Scale of the DataMark (bins) is close to zero
                    if (currentDataMark.transform.localScale.y <= 0.0000001f  && nextDataMark.transform.localScale.y <= 0.0000001f)
                    {
                        meshCollider.enabled = false;
                        continue;
                    }


                    // Add to collider
                    meshCollider.sharedMesh = mesh;

                }

            }

        }


        // TODO: Move to DistributionCalc?
        /// <summary>
        /// Returns the index of the bin/bins with the highest change in frequency to the bin/bins in the next timestep.
        /// The number of Bins of Interest (BOI) defines how many bins with the highest change are returned (ranked from most change to less change).
        /// </summary>
        /// <param name="numberOfBOI"></param>
        /// <returns>List of comparision from first dataset (timestep) to the next with the stroed BOI</returns>
        private List<Dictionary<int, double>> GetHighestChange(int numberOfBOI)
        {
            List<Dictionary<int, double>> highestChanges = new List<Dictionary<int, double>>(dataEnsemble.GetDataSetCount() - 1);
            
            // Positive and negative changes over all datasets
            List<double> currentPosChangeValues = new List<double>(numberOfBOI * (dataEnsemble.GetDataSetCount() - 1));
            List<double> currentNegChangeValues = new List<double>(numberOfBOI * (dataEnsemble.GetDataSetCount() - 1));

            // Compare all frequencies of the first dataset and with the next one (stop at before the last dataset)  
            for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount() - 1; dataSet++)
            {
                Dictionary<int, double> currentHighestChanges = new Dictionary<int, double>(binCount); // Dictionary with the highest changes for the bins of the current dataset to the next one
                int[] arrKeys = Enumerable.Range(0, binCount).ToArray(); // Indices array from 0 to binCount-1
                double[] currentDifferences = histograms[dataSet].GetBinFrequencyDifference(histograms[dataSet + 1]);

                // Check for highest absolut changes by sorting the array (and its indices array) (descending order)
                Array.Sort(currentDifferences, arrKeys, Comparer<double>.Create((x, y) => Math.Abs(y).CompareTo(Math.Abs(x))));

                //Print sorted array
                //Debug.Log("Sorted differences of Dataset " + dataSet + ": \n" + TablePrint.ToStringRow(currentDifferences));

                for (int boi = 0; boi < numberOfBOI; boi++)
                {
                    Debug.Log("Bin " + arrKeys[boi] + " has difference " + currentDifferences[boi] + "\n Boi: " + boi);
                    currentHighestChanges.Add(arrKeys[boi], currentDifferences[boi]);

                    // Get the (pos/neg) min max values 
                    if (currentDifferences[boi] > 0) currentPosChangeValues.Add(currentDifferences[boi]);
                    if (currentDifferences[boi] < 0) currentNegChangeValues.Add(currentDifferences[boi]);
                }

                highestChanges.Add(currentHighestChanges);
            }

            // Store Min/Max Value for all positive and negative changes over all Dataset
            if (currentPosChangeValues.Count > 0)
            {
                posMinMaxChange = new[] { currentPosChangeValues.Min(), currentPosChangeValues.Max() };
            }
            if (currentNegChangeValues.Count > 0)
            {
                negMinMaxChange = new[] { currentNegChangeValues.Min(), currentNegChangeValues.Max() };
            }
            else
            {
                posMinMaxChange = new[] { 0.0, 0.0 };
                negMinMaxChange = new[] { 0.0, 0.0 };
            }

            minMaxChange = new double[] { negMinMaxChange[0], posMinMaxChange[1] };

            return highestChanges;
        }

        /// <summary>
        /// Returns the index of the bins with their change in frequency between timesteps.
        /// </summary>
        /// <returns></returns>
        private List<Dictionary<int, double>> GetAllChanges()
        {
            List<Dictionary<int, double>> changes = new List<Dictionary<int, double>>(dataEnsemble.GetDataSetCount() - 1);
            minMaxChange = new double[] { double.MaxValue, double.MinValue };
            
            for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount() - 1; dataSet++)
            {
                Dictionary<int, double> currentChanges = new Dictionary<int, double>(binCount);
                double[] currentDifferences = histograms[dataSet].GetBinFrequencyDifference(histograms[dataSet + 1]);

                for (int bin = 0; bin < binCount; bin++)
                {
                    currentChanges.Add(bin, currentDifferences[bin]);

                    // Fill min max Array
                    if (currentDifferences[bin] < minMaxChange[0]) minMaxChange[0] = currentDifferences[bin];
                    if (currentDifferences[bin] > minMaxChange[1]) minMaxChange[1] = currentDifferences[bin];
                }

                changes.Add(currentChanges);
            }

            return changes;
        }

        /// <summary>
        /// Method creates a different colour scheme for the change indicator depending on whether the value is positive or negative.
        /// Uses ase starting value zero till the smallest negative value or the largest positive value over all change indicators.
        /// </summary>
        /// <param name="change"></param>
        /// <returns></returns>
        private Color CreateChangeIndicatorColor(double change)
        {
            return ScaleColor.GetCategoricalColor(change, minMaxChange[0], 0.0f, minMaxChange[1], ColorHelper.divergingValues);
        }

        /// <summary>
        /// Method draws a line between the same bins of different datasets (timesteps).
        /// The Line is always drawn from the top of the bin to the top of the bin in the next timestep.
        /// If addIntervalLabel is true, the Indicator is extended and the interval label is added to the end of the line.
        /// </summary>
        /// <param name="addIntervalLabel"></param>
        private void DrawBinIndicator(bool addIntervalLabel)
        {
            GameObject lineMark = new GameObject("BinConnectors");
            lineMark.transform.parent = visContainer.dataMarkContainer.transform;

            float ySize = 1.0f;
            float spacing = ySize / binCount;

            // Connect the same bins in all datasets with a line
            for (int binIndex = 0; binIndex < binCount; binIndex++)
            {
                // Draw the label for StackedHistogram without timesteps
                //TODO: First line and Label should also be available for StackedHistogram without timesteps

                for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount() - 1; dataSet++)
                {
                    // Get the DataMarks for the same bin in different datasets
                    int currentDataMark = binIndex + (dataSet * binCount);
                    int nextDataMark = binIndex + ((dataSet + 1) * binCount);

                    GameObject line = new GameObject("BinLine_" + binIndex);
                    line.transform.parent = lineMark.transform;

                    float lineThickness = 0.0008f;
                    Color lineColor = Color.white;

                    LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    lineRenderer.startColor = lineColor;
                    lineRenderer.endColor = lineColor;
                    lineRenderer.startWidth = lineThickness;
                    lineRenderer.endWidth = lineThickness;
                    lineRenderer.widthMultiplier = 1f;
                    lineRenderer.useWorldSpace = false;
                    
                    // Draw a line between the two points if they share the same x value
                    lineRenderer.SetPosition(0, AncerPointCalc.GetAncorPoint(visContainer.dataMarkList[currentDataMark].GetDataMarkInstance().transform, AncerPointCalc.AncorPointX.Right, AncerPointCalc.AncorPointY.Top, AncerPointCalc.AncorPointZ.Front));
                    lineRenderer.SetPosition(1, AncerPointCalc.GetAncorPoint(visContainer.dataMarkList[nextDataMark].GetDataMarkInstance().transform, AncerPointCalc.AncorPointX.Left, AncerPointCalc.AncorPointY.Top, AncerPointCalc.AncorPointZ.Front));


                    if (addIntervalLabel && (dataSet + 1 ) == (dataEnsemble.GetDataSetCount() - 1))
                    {
                        lineRenderer.positionCount = 4;

                        Vector3 lastPoint = AncerPointCalc.GetAncorPoint(visContainer.dataMarkList[nextDataMark].GetDataMarkInstance().transform, AncerPointCalc.AncorPointX.Left, AncerPointCalc.AncorPointY.Top, AncerPointCalc.AncorPointZ.Front);
                        // Extend the line with 2 times the width (x) of a DataMark 
                        Vector3 straightLinePos = lastPoint + new Vector3(visContainer.dataMarkList[nextDataMark].GetDataMarkInstance().transform.localScale.x, 0, 0);
                        lineRenderer.SetPosition(2, straightLinePos);
                        // Move it down half the height(y) of a DataMark
                        //Vector3 diagonalLinePos = straightLinePos + new Vector3(visContainer.dataMarkList[nextDataMark].GetDataMarkInstance().transform.localScale.x * 1.5f, -(visContainer.dataMarkList[nextDataMark].GetDataMarkInstance().transform.localScale.y / 2), 0);

                        // USe Spacing
                        Vector3 diagonalLinePos = new Vector3(straightLinePos.x + visContainer.dataMarkList[nextDataMark].GetDataMarkInstance().transform.localScale.x * 1.5f, spacing * (binIndex+1), 0);
                        lineRenderer.SetPosition(3, diagonalLinePos);

                        string binInterval = histograms[0].GetBinIntervals()[binIndex];

                        CreateValueText((diagonalLinePos), lineRenderer.transform, binInterval);
                    }

                }

            }
            
        }

        public void OnTouchIndicator(string indicatorId)
        {
            // Resets previous Higlighting of the PolyFibers
            foreach (var group in multiGroups.Values)
            {
                group.ResetPolyFibersHighlight();
            }

            // Check which Indicator is selected
            List<int> Ids = VisInteractor.GetIDNumbers(indicatorId);
            int binIndex = Ids[0];
            int dataSet = Ids[1];
            int nextDataSet = Ids[2];


            // Bin Value Range AND Attribute Id should be the same for both Datasets !!
            int attr = dataEnsemble.GetDataSet(dataSet).attributeNames.IndexOf(channelEncoding[VisChannel.YPos].GetName());
            //int attrOfNextDataset = dataEnsemble.GetDataSet(nextDataSet).attributeNames.IndexOf(channelEncoding[VisChannel.YPos].GetName());
            double[] valueRangeOfBin = new[] { histograms[dataSet].GetLowerBinBound(binIndex), histograms[dataSet].GetUpperBinBound(binIndex) };
            //double[] valueRangeOfBinInDataset = new[] { histograms[dataSet].GetLowerBinBound(binIndex), histograms[dataSet].GetUpperBinBound(binIndex) };
            //double[] valueRangeOfBinInNextDataset = new[] { histograms[nextDataSet].GetLowerBinBound(binIndex), histograms[nextDataSet].GetUpperBinBound(binIndex) };

            //string temp = "Bin : " + binIndex + " touched from timeStep: " + dataSet + " to " + nextDataSet + "\n";
            //temp += "Bin " + binIndex + " of Dataset " + dataSet + " has frequency: " + frequencies.GetNumericalVal()[binIndex + (dataSet * binCount)] + "\n";
            //temp += "Bin " + binIndex + " of Dataset " + nextDataSet + " has frequency: " + frequencies.GetNumericalVal()[binIndex + (nextDataSet * binCount)] + "\n";
            //temp += "Bin Range: " + valueRangeOfBin[0] + " - " + valueRangeOfBin[1];
            //Debug.Log(temp);

            // Select all values == fibers which are covered by the encoded range in the bins 
            //List<int> selectedFiberIds = visMddGlyphs.GetFiberIDsFromIQRRange(selectedGlyph);
            List<int> fiberIDsDataset = dataEnsemble.GetIndexOfAttrValRange(dataSet, attr, valueRangeOfBin, false);
            List<int> fiberIDsNextDataset = dataEnsemble.GetIndexOfAttrValRange(nextDataSet, attr, valueRangeOfBin, false);

            //Debug.Log("Dataset [" + dataSet + "] " + "Selected <" + fiberIDsDataset.Count + "> Fibers \n" + "Dataset [" + nextDataSet + "] " + "Selected <" + fiberIDsNextDataset.Count + "> Fibers");

            // Color Polyfibers of selected FiberIds in the respective DataVisGroup
            var groupDataset = multiGroups.Values.ElementAt(dataSet);
            var groupNextDataset = multiGroups.Values.ElementAt(nextDataSet);

            groupDataset.HighlightPolyFibers(fiberIDsDataset, Color.red);
            groupNextDataset.HighlightPolyFibers(fiberIDsNextDataset, Color.yellow);

        }

        private void CreateValueText(Vector3 position, Transform transform, string text)
        {
            GameObject colorBarText = GameObject.Instantiate(text3DPrefab, position, Quaternion.identity, transform);
            TextMeshPro barText = colorBarText.GetComponent<TextMeshPro>();
            barText.rectTransform.pivot = new Vector2(0.0f, 0.5f);
            barText.margin = new Vector4(2, 0, 0, 0);
            barText.text = text;
            barText.alignment = TextAlignmentOptions.MidlineLeft;
            barText.fontSize = 50;
        }


    }
}
