using UnityEngine;

namespace AugmeNDT{

    /// <summary>
    /// This class represents a Scatterplot which connects Points in different Datasets (Timesteps) (Y Axis) with a line. The Distance between points represents the similarity.
    /// </summary>
    public class VisTimeScatter : Vis
    {
        public VisTimeScatter()
        {
            title = "TimeScatter";
            axes = 2;

            dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
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

            Debug.Log("HeaderVals: \n" + TablePrint.ToStringRow(headerVals));
            Debug.Log("DataSetCol: \n" + TablePrint.ToStringRow(dataSetCol));
            Debug.Log("YPos: \n" + TablePrint.ToStringRow(channelEncoding[VisChannel.YPos].GetNumericalVal()));

            //## 02: Set Remaining Vis Channels (Color,...)
            visContainer.SetChannel(VisChannel.XPos, headerVals);
            SetChannel(VisChannel.YPos, channelEncoding[VisChannel.YPos], false);

            if(channelEncoding.ContainsKey(VisChannel.Color)) visContainer.SetChannel(VisChannel.Color, dataSetCol);
            
            //## 03: Draw all Data Points with the provided Channels 
            visContainer.CreateDataMarks(dataMarkPrefab, new[] { 1, 1, 1 });
            ConnectDataMarks();

            //## 04: Rescale Chart
            visContainerObject.transform.localScale = new Vector3(width, height, depth);


            return visContainerObject;
        }


        private void ConnectDataMarks()
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

                LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
                lineRenderer.widthMultiplier = 0.002f;
                lineRenderer.useWorldSpace = false;


                // Draw a line between the two points if they share the same x value
                lineRenderer.SetPosition(0, currentDataMark.transform.localPosition);
                lineRenderer.SetPosition(1, nextDataMark.transform.localPosition);

            }
            
            
        }


    }
}

