using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.XR.ARSubsystems.XRFaceMesh;

namespace AugmeNDT
{
    public class VisStackedHistogram : Vis
    {
        // If more than one dataset is loaded, should the z-Axis be for the other Datasets?
        private bool use4DData = false;

        private HistogramValues[] histograms;       // Histograms with same number of bins and same lower & upper bound for each dataset
        private int binCount;                       // Fixed bin count with fixed bin width

        private Attribute frequencies;              // Frequencies for each bin (for each dataset)
        private double[] minMaxYPos;                // Min and Max Y-Position (over all dataset)
        private Attribute yPos;                     // Stacked bars: Position based on the frequency of the previous bin (for each dataset)


        int numberOfBOI = 3; // Number of Bins of Interest (BOI) defines how many bins with the highest change are returned (ranked from most change to less change)
        private GameObject changeIndicatorPrefab;   // Material for the change indicator
        private double[] posMinMaxChange;              // Min and Max change for all positive Indicators
        private double[] negMinMaxChange;              // Min and Max change for all negative Indicators


        public VisStackedHistogram()
        {
            title = "Stacked Histogram";
            axes = 2;

            dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/BarWithOutline");
            tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");

            changeIndicatorPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/ChangeIndicator");
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
                GameObject colorBar = colorScalarBar.CreateColorScalarBar(visContainerObject.transform.position, "Frequency Difference", new[] { negMinMaxChange[0], posMinMaxChange[1] }, 1, ColorHelper.divergingValues);
                //colorBar01.transform.parent = colorScalarBarContainer.transform;
                CreateColorLegend(colorBar);
            }

            DrawBinIndicator();

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

            // Find min and max val over all datasets for histogram bounds
            for (int dataset = 1; dataset < dataEnsemble.GetDataSetCount(); dataset++)
            {
                Debug.Log("Dataset " + dataset + " of " + dataEnsemble.GetDataSetCount());

                double[] currentMinMax = dataEnsemble.GetMinMaxAttrVal(dataset, index, false);

                // Save min val and max val over all datasets 
                if (currentMinMax[0] < minMaxValues[0]) minMaxValues[0] = currentMinMax[0];
                if (currentMinMax[1] > minMaxValues[1]) minMaxValues[1] = currentMinMax[1];

                // Save max number of val over all datasets 
                if (dataEnsemble.GetAttribute(dataset, index).GetNumberOfValues() > maxDataPoints) maxDataPoints = dataEnsemble.GetAttribute(dataset, index).GetNumberOfValues();
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

                Debug.Log("Values of dataset " + dataset + ": \n" + TablePrint.ToStringRow(dataEnsemble.GetAttribute(dataset, index).GetNumericalVal()));
                
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

            Debug.Log("VisStackedHistogram: maxFrequency: " + maxFrequency);
            Debug.Log("VisStackedHistogram: yPos : " + TablePrint.ToStringRow(yPosList.ToArray()));

            // Finish drawable values
            frequencies = new Attribute("Frequencies", frequencyList.ToArray());
            yPos = new Attribute(attribute.GetName(), yPosList.ToArray());
            minMaxYPos = new double[] { 0, maxFrequency };

            Debug.Log("Frequencies per bin: " + frequencies.PrintAttribute());

        }

        /// <summary>
        /// Method connects the same bins with a line between their time steps.
        /// Draws the line/area between the bins which have the highest change.
        /// </summary>
        private void DrawChangeIndicator()
        {
            List<Dictionary<int, double>> marksToDraw = GetHighestChange(numberOfBOI);

            string text = "";
            //Print bins and differences from marksToDraw
            for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount() - 1; dataSet++)
            {
                text += "Marks to draw for dataset " + dataSet + ": \n";
                foreach (KeyValuePair<int, double> entry in marksToDraw[dataSet])
                {
                    text += "Bin " + entry.Key + " with difference " + entry.Value + "\n";
                }
            }
            Debug.Log(text);

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
                    indicator.name = "Line_" + currentID + "_" + nextID;
                    indicator.transform.parent = ChangeIndicator.transform;

                    //Set Interaction details
                    ChangeIndicatorInteractable interactable = indicator.GetComponent<ChangeIndicatorInteractable>();
                    interactable.indicatorID = entry.Key + "_" + dataSet + "_" + (dataSet + 1);
                    interactable.refToClass = this;

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
            int[] arrKeys = Enumerable.Range(0, binCount).ToArray(); // Indices array from 0 to binCount-1

            // Positive and negative changes over all datasets
            List<double> currentPosChangeValues = new List<double>(numberOfBOI * (dataEnsemble.GetDataSetCount() - 1));
            List<double> currentNegChangeValues = new List<double>(numberOfBOI * (dataEnsemble.GetDataSetCount() - 1));

            // Compare all frequencies of the first dataset and with the next one (stop at before the last dataset)  
            for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount() - 1; dataSet++)
            {
                Dictionary<int, double> currentHighestChanges = new Dictionary<int, double>(binCount); // Dictionary with the highest changes for the bins of the current dataset to the next one
                double[] currentDifferences = histograms[dataSet].GetBinFrequencyDifference(histograms[dataSet + 1]);

                // Check for highest absolut changes by sorting the array (and its indices array) (descending order)
                Array.Sort(currentDifferences, arrKeys, Comparer<double>.Create((x, y) => Math.Abs(y).CompareTo(Math.Abs(x))));

                //Print sorted array
                Debug.Log("Sorted differences of Dataset " + dataSet + ": \n" + TablePrint.ToStringRow(currentDifferences));

                for (int boi = 0; boi < numberOfBOI; boi++)
                {
                    currentHighestChanges.Add(arrKeys[boi], currentDifferences[boi]);

                    // Get the (pos/neg) min max values 
                    if (currentDifferences[boi] > 0) currentPosChangeValues.Add(currentDifferences[boi]);
                    if (currentDifferences[boi] < 0) currentNegChangeValues.Add(currentDifferences[boi]);
                }

                highestChanges.Add(currentHighestChanges);
            }

            Debug.Log("Pos Vals: " + TablePrint.ToStringRow(currentPosChangeValues.ToArray()));
            Debug.Log("Neg Vals: " + TablePrint.ToStringRow(currentNegChangeValues.ToArray()));

            // Store Min/Max Value for all positive and negative changes over all Dataset
            if (currentPosChangeValues.Count > 0)
            {
                posMinMaxChange = new[] { currentPosChangeValues.Min(), currentPosChangeValues.Max() };
                Debug.Log("posMinMaxChange Vals: " + TablePrint.ToStringRow(posMinMaxChange));
            }
            if (currentNegChangeValues.Count > 0)
            {
                negMinMaxChange = new[] { currentNegChangeValues.Min(), currentNegChangeValues.Max() };
                Debug.Log("negMinMaxChange Vals: " + TablePrint.ToStringRow(negMinMaxChange));
            }
            else
            {
                posMinMaxChange = new[] { 0.0, 0.0 };
                negMinMaxChange = new[] { 0.0, 0.0 };

            }

            return highestChanges;
        }

        /// <summary>
        /// Method creates a different colour scheme for the change indicator depending on whether the value is positive or negative.
        /// Uses ase starting value zero till the smallest negative value or the largest positive value over all change indicators.
        /// </summary>
        /// <param name="change"></param>
        /// <returns></returns>
        private Color CreateChangeIndicatorColor(double change)
        {
            return ScaleColor.GetCategoricalColor(change, negMinMaxChange[0], 0, posMinMaxChange[1], ColorHelper.divergingValues);

            if (change < 0) return ScaleColor.GetCategoricalColor(change, negMinMaxChange[0], 0, ColorHelper.blueHueValues);
            else if (change > 0) return ScaleColor.GetCategoricalColor(change, 0, posMinMaxChange[1], ColorHelper.redHueValues);
            else return Color.green; // Erorr

        }

        /// <summary>
        /// Method draws a line between the same bins of different datasets (timesteps).
        /// The Line is always drawn from the top of the bin to the top of the bin in the next timestep.
        /// </summary>
        private void DrawBinIndicator()
        {
            GameObject lineMark = new GameObject("BinConnectors");
            lineMark.transform.parent = visContainer.dataMarkContainer.transform;

            // Connect the same bins in all datasets with a line
            for (int binIndex = 0; binIndex < binCount; binIndex++)
            {
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

                }

            }
            
        }

        public void OnTouchIndicator(string indicatorId)
        {
            // Check which Indicator is selected
            List<int> Ids = VisInteractor.GetIDNumbers(indicatorId);
            int binIndex = Ids[0];
            int dataSet = Ids[1];
            int nextDataSet = Ids[2];

            Debug.Log("OnTouchIndicator:\n Bin: " + binIndex + " dataSet: " + dataSet + " nextDataSet: " + nextDataSet);

            // Get the DataVisGroup of the selected Indicator
            //GetDataVisGroup();

            // Select all values == fibers which are covered by the encoded range in the bins 
            //List<int> selectedFiberIds = visMddGlyphs.GetFiberIDsFromIQRRange(selectedGlyph);

            //Debug.Log("[" + selectedFiberIds.Count + "] Selected Fibers");

            // Color Polyfibers of selected FiberIds in the respective DataVisGroup
            //visMddGlyphs.GetDataVisGroup().HighlightPolyFibers(selectedFiberIds, Color.white);

        }

    }
}
