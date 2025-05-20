// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using UnityEngine;

namespace AugmeNDT{
    public class VisScatterplot : Vis
    {
        public VisScatterplot()
        {
            title = "Basic Scatterplot";
            axes = 3;

            dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Sphere");
            tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
        }

        public override GameObject CreateVis(GameObject container)
        {
            base.CreateVis(container);

            xyzTicks = new[] { 11, 11, 11 };

            SetVisParams();

            //## 01:  Create Axes and Grids
            Debug.Log("Number of Header Variables: " + channelEncoding[VisChannel.XPos].GetNumberOfValues());

            // X Axis
            CreateAxis(channelEncoding[VisChannel.XPos], false, Direction.X);
            visContainer.CreateGrid(Direction.X, Direction.Y);

            // Y Axis
            CreateAxis(channelEncoding[VisChannel.YPos], false, Direction.Y);
            //visContainer.CreateAxis(dataSets[0].GetAttrName(2), dataSets[0].GetValues(2), Direction.Y);

            // Z Axis
            if (axes == 3)
            {
                //visContainer.CreateAxis(dataSets[0].GetAttrName(3), dataSets[0].GetValues(3), Direction.Z);
                CreateAxis(channelEncoding[VisChannel.ZPos], false, Direction.Z);
                visContainer.CreateGrid(Direction.Y, Direction.Z);
                visContainer.CreateGrid(Direction.Z, Direction.X);
            }

            //## 02: Set Remaining Vis Channels (Color,...)
            SetChannel(VisChannel.XPos, channelEncoding[VisChannel.XPos], false);
            SetChannel(VisChannel.YPos, channelEncoding[VisChannel.YPos], false);
            if (axes == 3) SetChannel(VisChannel.ZPos, channelEncoding[VisChannel.ZPos], false);
            SetChannel(VisChannel.Color, channelEncoding[VisChannel.Color], false);

            //## 03: Draw all Data Points with the provided Channels 
            visContainer.CreateDataMarks(dataMarkPrefab, new []{1,1,1});

            //## 04: Rescale Chart
            visContainerObject.transform.localScale = new Vector3(width, height, depth);

            //visContainer.GatherDataMarkValueInformation(0);
            //visContainer.GatherDataMarkValueInformation(1);

            return visContainerObject;
        }

    }
}
