using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
        xyzOffset = new float[] { 0.05f, 0.05f, 0.05f };
        base.CreateVis(container);

        xyzTicks = new[] { dataSets[0].GetAttributeNames().Count, 7, 13 };
        visContainer.SetAxisTickNumber(xyzTicks);

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

        //## 03: Draw all Data Points with the provided Channels 
        visContainer.CreateDataMarks(dataMarkPrefab, new[] { 1, 1, 1 });

        //## 04: Rescale Chart
        visContainerObject.transform.localScale = new Vector3(width, height, depth);


        return visContainerObject;
    }


}

