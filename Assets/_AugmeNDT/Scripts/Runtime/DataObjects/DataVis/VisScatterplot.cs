using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        //## 01:  Create Axes and Grids

        // X Axis
        visContainer.CreateAxis(dataSets[0].GetAttributeName(0), dataSets[0].GetValues(0), Direction.X);
        visContainer.CreateGrid(Direction.X, Direction.Y);

        // Y Axis
        visContainer.CreateAxis(dataSets[0].GetAttributeName(1), dataSets[0].GetValues(1), Direction.Y);

        // Z Axis
        if (axes == 3)
        {
            visContainer.CreateAxis(dataSets[0].GetAttributeName(2), dataSets[0].GetValues(2), Direction.Z);
            visContainer.CreateGrid(Direction.Y, Direction.Z);
            visContainer.CreateGrid(Direction.Z, Direction.X);
        }

        //## 02: Set Remaining Vis Channels (Color,...)
        visContainer.SetChannel(VisChannel.XPos, dataSets[0].GetValues(0));
        visContainer.SetChannel(VisChannel.YPos, dataSets[0].GetValues(1));
        if (axes == 3) visContainer.SetChannel(VisChannel.ZPos, dataSets[0].GetValues(2));
        visContainer.SetChannel(VisChannel.Color, dataSets[0].GetValues(3));

        //## 03: Draw all Data Points with the provided Channels 
        visContainer.CreateDataMarks(dataMarkPrefab, new []{1,1,1});

        //## 04: Rescale Chart
        visContainerObject.transform.localScale = new Vector3(width, height, depth);

        //visContainer.GatherDataMarkValueInformation(0);
        //visContainer.GatherDataMarkValueInformation(1);

        return visContainerObject;
    }

}
