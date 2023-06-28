using UnityEngine;

namespace AugmeNDT{
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

            //## 01:  Create Axes and Grids

            // X Axis
            CreateAxis(channelEncoding[VisChannel.XPos], false, Direction.X);
            visContainer.CreateGrid(Direction.X, Direction.Y);

            // Y Axis
            CreateAxis(channelEncoding[VisChannel.YPos], false, Direction.Y);

            //## 02: Set Remaining Vis Channels (Color,...)
            SetChannel(VisChannel.XPos, channelEncoding[VisChannel.XPos], false);
            SetChannel(VisChannel.YPos, channelEncoding[VisChannel.YPos], false);
            //visContainer.SetChannel(VisChannel.Color, dataSets[0].ElementAt(3).Value);


        /*
            //## 00:  Preprocessing
            double[] yValues = new double[dataSets[0].GetAttributeNames().Count];
            double[] xValues = new double[dataSets[0].GetAttributeNames().Count];
            for (int i = 0; i < dataSets[0].GetAttributeNames().Count; i++)
            {
                yValues[i] = dataSets[0].GetStatisticDic()[dataSets[0].GetAttributeName(i)].Modality;
                xValues[i] = i;
            }

            //## 01:  Create Axes and Grids

            // X Axis
            visContainer.CreateAxis("Attributes", dataSets[0].GetAttributeNames().ToArray(), Direction.X);
            visContainer.CreateGrid(Direction.X, Direction.Y);

            // Y Axis
            visContainer.CreateAxis("Modality", yValues, Direction.Y);


            //## 02: Set Remaining Vis Channels (Color,...)
            visContainer.SetChannel(VisChannel.XPos, xValues);
            visContainer.SetChannel(VisChannel.YPos, yValues);
            //visContainer.SetChannel(VisChannel.Color, dataSets[0].ElementAt(3).Value);
        */

            //## 03: Draw all Data Points with the provided Channels 
            visContainer.CreateDataMarks(dataMarkPrefab, new[] { 1, 1, 1 });

            //## 04: Rescale Chart
            visContainerObject.transform.localScale = new Vector3(width, height, depth);


            return visContainerObject;
        }


    }
}

