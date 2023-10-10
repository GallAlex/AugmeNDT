using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using static UnityEngine.XR.ARSubsystems.XRFaceMesh;

namespace AugmeNDT{

    /// <summary>
    /// This class represents a Scatterplot which connects Points in different Datasets (Timesteps) (Y Axis) with a line. The Distance between points represents the similarity.
    /// </summary>
    public class VisTimeScatter : Vis
    {
        private ScaleLinear lineScale;      // Scaling for the thickness of the lines
        private double[] minMaxTimeDiff;    // Min/Max Time Difference
        private Dictionary<int[], double> indicatorsToDraw;

        public VisTimeScatter()
        {
            title = "TimeScatter";
            axes = 2;

            dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/BarWithOutline");
            tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
        }

        public override GameObject CreateVis(GameObject container)
        {
            base.CreateVis(container);

            xyzOffset = new float[] { 0.05f, 0.05f, 0.05f };
            xyzTicks = new[] { channelEncoding[VisChannel.XPos].GetNumberOfValues(), 7, 13 };

            SetVisParams();

            for (int i = 0; i < dataEnsemble.GetDataSetCount(); i++)
            {
                dataEnsemble.GetDataSet(i).PrintDatasetValues(false); 
            }

            //## 01:  Create Axes and Grids

            // X Axis
            CreateAxis(channelEncoding[VisChannel.XPos], false, Direction.X);
            visContainer.CreateGrid(Direction.X, Direction.Y);

            //Debug.Log("YPos Min/Max: " + TablePrint.ToStringRow(channelEncoding[VisChannel.YPos].GetMinMaxVal()));

            // Y Axis
            CreateAxis(channelEncoding[VisChannel.YPos], false, Direction.Y);

            // Header Attribute does not extend for every dataset - only n header strings but for multiple datasets i need n + dataset values!!
            double[] headerVals;
            double[] dataSetCol;
            if (channelEncoding[VisChannel.XPos].GetNumberOfValues() !=
                channelEncoding[VisChannel.YPos].GetNumberOfValues())
            {
                headerVals = new double[dataEnsemble.GetDataSetCount() * dataEnsemble.GetDataSet(0).attributesCount];
                dataSetCol = new double[dataEnsemble.GetDataSetCount() * dataEnsemble.GetDataSet(0).attributesCount];

                for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount(); dataSet++)
                {
                    for (int attr = 0; attr < dataEnsemble.GetDataSet(0).attributesCount; attr++)
                    {
                        headerVals[attr + (dataSet * dataEnsemble.GetDataSet(0).attributesCount)] = attr;
                        dataSetCol[attr + (dataSet * dataEnsemble.GetDataSet(0).attributesCount)] = dataSet;
                    }

                }


            }
            else
            {
                headerVals = channelEncoding[VisChannel.XPos].GetNumericalVal();
                dataSetCol = channelEncoding[VisChannel.Color].GetNumericalVal();
            }

            //## 02: Set Remaining Vis Channels (Color,...)
            visContainer.SetChannel(VisChannel.XPos, headerVals);
            SetChannel(VisChannel.YPos, channelEncoding[VisChannel.YPos], false);
            //if(channelEncoding.ContainsKey(VisChannel.Color)) visContainer.SetChannel(VisChannel.Color, dataSetCol);
            
            //## 03: Draw all Data Points with the provided Channels 
            visContainer.CreateDataMarks(dataMarkPrefab, new[] { 1, 1, 1 });

            if (dataEnsemble.GetDataSetCount() > 1)
            {
                DrawDifferenceIndicator();
                AdjustDataMarkPosition();

                //## 04: Create Color Scalar Bar

                LegendColorBar colorScalarBar = new LegendColorBar();
                GameObject colorBar = colorScalarBar.CreateColorScalarBar(visContainerObject.transform.position, "Chi-Squared Difference", minMaxTimeDiff, 1, ColorHelper.whiteToPurpleValues);
                //colorBar01.transform.parent = colorScalarBarContainer.transform;
                CreateColorLegend(colorBar);

            }


            //## 05: Rescale Chart
            visContainerObject.transform.localScale = new Vector3(width, height, depth);


            return visContainerObject;
        }

        /// <summary>
        /// Method connects all DataMarks of the same Attribute with a line between their time steps.
        /// </summary>
        private void DrawDifferenceIndicator()
        {
            GameObject lineMark = new GameObject("TimeLines");
            lineMark.transform.parent = visContainer.dataMarkContainer.transform;

            if (indicatorsToDraw == null) indicatorsToDraw = GetIndividualTimeDifferences();

            // From Difference [min, max] to [0.0d, 0.009d]
            lineScale = new ScaleLinear(minMaxTimeDiff.ToList(), new List<double>() { 0.0006d, 0.006d });

            foreach (KeyValuePair< int[], double> entry in indicatorsToDraw)
            {
                GameObject currentDataMark = visContainer.dataMarkList[entry.Key[0]].GetDataMarkInstance();
                GameObject nextDataMark = visContainer.dataMarkList[entry.Key[1]].GetDataMarkInstance();

                GameObject line = new GameObject("Line_" + entry.Key[0] + "_" + entry.Key[1]);
                line.transform.parent = lineMark.transform;

                float lineThickness = CreateDifferenceIndicatorWidth(entry.Value);
                Color lineColor = CreateDifferenceIndicatorColor(entry.Value);

                LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = lineColor;
                lineRenderer.endColor = lineColor;
                lineRenderer.startWidth = lineThickness;
                lineRenderer.endWidth = lineThickness;
                lineRenderer.useWorldSpace = false;

                // Draw a line between the two points if they share the same x value
                lineRenderer.SetPosition(0, currentDataMark.transform.localPosition);
                lineRenderer.SetPosition(1, nextDataMark.transform.localPosition);


            }

        }

        /// <summary>
        /// Method iterates between all DataMark y positions and if they are overlapping (y values too close) shift each of them at half their size to the left/right
        /// </summary>
        private void AdjustDataMarkPosition()
        {
            if(indicatorsToDraw == null) indicatorsToDraw = GetIndividualTimeDifferences();

            foreach (KeyValuePair<int[], double> entry in indicatorsToDraw)
            {
                GameObject currentDataMark = visContainer.dataMarkList[entry.Key[0]].GetDataMarkInstance();
                GameObject nextDataMark = visContainer.dataMarkList[entry.Key[1]].GetDataMarkInstance();

                // Get Y Position of both DataMarks
                float currentYPos = currentDataMark.transform.localPosition.y;
                float nextYPos = nextDataMark.transform.localPosition.y;
                float ySize = currentDataMark.transform.localScale.y;
                float halfXSize = currentDataMark.transform.localScale.x / 2.0f;

                // Check if y size is overlapping (should be bigger then half of the bar size)
                if (Math.Abs(currentYPos - nextYPos) < ySize)
                {
                    // Shift both DataMarks to the left/right
                    currentDataMark.transform.localPosition = new Vector3(currentDataMark.transform.localPosition.x - halfXSize, currentDataMark.transform.localPosition.y, currentDataMark.transform.localPosition.z);
                    nextDataMark.transform.localPosition = new Vector3(nextDataMark.transform.localPosition.x + halfXSize, nextDataMark.transform.localPosition.y, nextDataMark.transform.localPosition.z);
                }

            }

        }

        /// <summary>
        /// Method returns a Dictionary with the Time Differences between two DataMark IDs
        /// </summary>
        /// <returns></returns>
        private Dictionary<int[], double> GetIndividualTimeDifferences()
        {
            int attrCount = dataEnsemble.GetDataSet(0).attributesCount; // Number of attributes (taken from first dataset 1)
            Dictionary<int[], double> timeDifference = new Dictionary<int[], double>(attrCount * (dataEnsemble.GetDataSetCount() - 1));
            minMaxTimeDiff = new double[]{Double.MaxValue, Double.MinValue, };
            
            double[] summedTimePoints = channelEncoding[VisChannel.YPos].GetNumericalVal();

            for (int attr = 0; attr < attrCount; attr++)
            {
                for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount() - 1; dataSet++)
                {
                    int currentDataMarkId = attr + (dataSet * attrCount);
                    int nextDataMarkId = attr + ((dataSet + 1) * attrCount);

                    var currDiff = summedTimePoints[nextDataMarkId] - summedTimePoints[currentDataMarkId];

                    //Store Min/Max Diff
                    if (currDiff < minMaxTimeDiff[0]) minMaxTimeDiff[0] = currDiff;
                    if (currDiff > minMaxTimeDiff[1]) minMaxTimeDiff[1] = currDiff;

                    timeDifference.Add(new []{currentDataMarkId, nextDataMarkId}, currDiff);
                }
            }

            return timeDifference;
        }

        private float CreateDifferenceIndicatorWidth(double difference)
        {
            return (float)lineScale.GetScaledValue(difference);

        }

        private Color CreateDifferenceIndicatorColor(double difference)
        {

            return ScaleColor.GetCategoricalColor(lineScale.GetScaledValue(difference), lineScale.range[0], lineScale.range[1], ColorHelper.whiteToPurpleValues);
        }


    }
}

