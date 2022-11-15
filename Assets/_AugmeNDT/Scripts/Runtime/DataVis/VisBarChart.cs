using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

public class VisBarChart : Vis
{

    public VisBarChart()
    {
        title = "Basic Bar Chart";                                  
        axes = 3;                                                   
        dataScale = Scale.DataScale.Linear;


        dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
        tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
    }

    public override GameObject CreateVis()
    {
        visContainer = new VisContainer();
        visContainerObject = visContainer.CreateVisContainer(title);

        if (dimensions < axes) axes = dimensions;

        //## 01: Create Data Scales
        List<Scale> scale = new List<Scale>(axes);

        for (int drawnDim = 0; drawnDim < axes; drawnDim++)
        {
            List<float> domain = new List<float>(2);
            List<float> range = new List<float>(2);

            domain.Add((float)dataValues.ElementAt(drawnDim).Value.Min());
            domain.Add((float)dataValues.ElementAt(drawnDim).Value.Max());

            range.Add(0);
            range.Add(1);

            scale.Add(CreateScale(domain, range));
        }

        //## 02: Create Axes and Grids
        for (int currAxis = 0; currAxis < axes; currAxis++)
        {
            int nextDim = (currAxis + 1) % axes;
            //visContainer.CreateAxis(((Direction)currAxis).ToString() + " Label", (Direction)currAxis);
            visContainer.CreateAxis(dataValues.ElementAt(currAxis).Key, (Direction)currAxis, scale[currAxis]);
            visContainer.CreateGrid((Direction)currAxis, (Direction)nextDim);
        }


        //## 03: Create Data Points
        // Scale Bars
        for (int value = 0; value < numberOfValues; value++)
        {
            //Default:
            DataMark.Channel channel = DataMark.DefaultDataChannel();

            //X Axis
            var xCoordinate = scale[0].GetScaledValue((float)dataValues.ElementAt(0).Value[value]);
            channel.position[0] = xCoordinate;

            //Y Axis
            var barHeight = scale[1].GetScaledValue((float)dataValues.ElementAt(1).Value[value]);
            channel.size[1] = barHeight;

            //Z Axis
            if (axes == 3)
            {
                var zCoordinate = scale[2].GetScaledValue((float)dataValues.ElementAt(2).Value[value]);
                channel.position[2] = zCoordinate;
            }

            visContainer.CreateDataMark(channel);
        }


        //## 04: Rescale
        visContainerObject.transform.localScale = new Vector3(width, height, depth);

        return visContainerObject;
    }
}
