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

            //Debug.Log("HeaderVals: \n" + TablePrint.ToStringRow(headerVals));
            //Debug.Log("DataSetCol ["+ dataSetCol.Length + "]: \n" + TablePrint.ToStringRow(dataSetCol));
            //Debug.Log("YPos: \n" + TablePrint.ToStringRow(channelEncoding[VisChannel.YPos].GetNumericalVal()));

            //## 02: Set Remaining Vis Channels (Color,...)
            visContainer.SetChannel(VisChannel.XPos, headerVals);
            SetChannel(VisChannel.YPos, channelEncoding[VisChannel.YPos], false);
            //if(channelEncoding.ContainsKey(VisChannel.Color)) visContainer.SetChannel(VisChannel.Color, dataSetCol);
            
            //## 03: Draw all Data Points with the provided Channels 
            visContainer.CreateDataMarks(dataMarkPrefab, new[] { 1, 1, 1 });
            
            //DrawDifferenceIndicator();
            DrawDifferenceIndicatorByArray();

            //## 04: Rescale Chart
            visContainerObject.transform.localScale = new Vector3(width, height, depth);


            return visContainerObject;
        }

        /// <summary>
        /// Method connects all DataMarks of the same Attribute with a line between their time steps.
        /// Alsways draws from first timestep  to the next one 
        /// </summary>
        private void DrawDifferenceIndicator()
        {
            GameObject lineMark = new GameObject("TimeLines");
            lineMark.transform.parent = visContainer.dataMarkContainer.transform;

            int attributes = channelEncoding[VisChannel.XPos].GetNumberOfValues();

            for (int dataMark = 0; dataMark < attributes * dataEnsemble.GetDataSetCount(); dataMark++)
            {
                // Check only Marks for the same Attribute
                if (attributes + dataMark > attributes * dataEnsemble.GetDataSetCount()-1) break;

                var currentDataMark = visContainer.dataMarkList[dataMark].GetDataMarkInstance();
                var nextDataMark = visContainer.dataMarkList[dataMark + attributes].GetDataMarkInstance();

                GameObject line = new GameObject("Line_" + dataMark + "_" + (dataMark + attributes));
                line.transform.parent = lineMark.transform;

                //TODO: Chamge to MeshTopology.Lines?

                LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
                lineRenderer.widthMultiplier = CreateDifferenceIndicatorWidth(0.002);
                lineRenderer.useWorldSpace = false;


                // Draw a line between the two points if they share the same x value
                lineRenderer.SetPosition(0, currentDataMark.transform.localPosition);
                lineRenderer.SetPosition(1, nextDataMark.transform.localPosition);

            }
        }

        // Difference to DrawDifferenceIndicator: Run trhough the Array and get back from this to the DataMark
        private void DrawDifferenceIndicatorByArray()
        {
            GameObject lineMark = new GameObject("TimeLines");
            lineMark.transform.parent = visContainer.dataMarkContainer.transform;

            Dictionary<int[], double> indicatorsToDraw = GetIndividualTimeDifferences();

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


            /*

            int attrCount = dataEnsemble.GetDataSet(0).attributesCount; // Number of attributes (taken from first dataset 1)
            double[] summedTimePoints = channelEncoding[VisChannel.YPos].GetNumericalVal();

            Debug.Log("SummedTimePoints values [ " + summedTimePoints.Length  +" ]: " + TablePrint.ToStringRow(summedTimePoints));

            // From Difference [min, max] to [0.0d, 0.009d]
            lineScale = new ScaleLinear(channelEncoding[VisChannel.YPos].GetMinMaxVal().ToList(),new List<double>(){ 0.0009d, 0.006d });
            Debug.Log("Min/Max SummedTimePoints: " + TablePrint.ToStringRow(channelEncoding[VisChannel.YPos].GetMinMaxVal()));

            Debug.Log("Time Diff Array: " + TablePrint.ToStringRow(summedTimePoints));

            for (int attr = 0; attr < attrCount; attr++)
            {
                for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount() - 1; dataSet++)
                {
                    int currentDataMarkId = attr + (dataSet * attrCount);
                    int nextDataMarkId = attr + ((dataSet + 1) * attrCount);

                    // Subtract
                    double timeDifference = summedTimePoints[nextDataMarkId];
                    Debug.Log("Current DataMark: " + currentDataMarkId + " | Next DataMark: " + nextDataMarkId + "\n Difference: " + summedTimePoints[nextDataMarkId]);



                    GameObject currentDataMark = visContainer.dataMarkList[currentDataMarkId].GetDataMarkInstance();
                    GameObject nextDataMark = visContainer.dataMarkList[nextDataMarkId].GetDataMarkInstance();

                    GameObject line = new GameObject("Line_" + currentDataMarkId + "_" + nextDataMarkId);
                    line.transform.parent = lineMark.transform;

                    //TODO: Chamge to MeshTopology.Lines?
                    float lineThickness = CreateDifferenceIndicatorWidth(summedTimePoints[nextDataMarkId]);
                    Color lineColor = CreateDifferenceIndicatorColor(summedTimePoints[nextDataMarkId]);

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
            */

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

            return ScaleColor.GetCategoricalColor(lineScale.GetScaledValue(difference), lineScale.range[0], lineScale.range[1], ColorHelper.redHueValues);
        }


    }
}

