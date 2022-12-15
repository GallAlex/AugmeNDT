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

    public override GameObject CreateVis()
    {
        base.CreateVis();

        //Initialize dataScales
        dataScales = new List<Scale.DataScale>();
        for (int attrScale = 0; attrScale < dimensions; attrScale++)
        {
            dataScales.Add(Scale.DataScale.Linear);
        }

        if (dimensions < axes) axes = dimensions;

        //## 01: Create Data Scales
        List<Scale> scale = new List<Scale>(axes);

        for (int drawnDim = 0; drawnDim < axes; drawnDim++)
        {
            List<double> domain = new List<double>(2);
            List<double> range = new List<double>(2);

            domain.Add(dataValues.ElementAt(drawnDim).Value.Min());
            domain.Add(dataValues.ElementAt(drawnDim).Value.Max());

            range.Add(0);
            range.Add(1);

            scale.Add(CreateScale(dataScales[drawnDim],domain, range));
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
            var xCoordinate = scale[0].GetScaledValue(dataValues.ElementAt(0).Value[value]);
            channel.position[0] = (float)xCoordinate;

            //Y Axis
            var yCoordinate = scale[1].GetScaledValue(dataValues.ElementAt(1).Value[value]);
            channel.position[1] = (float)yCoordinate;

            //Z Axis
            if (axes == 3)
            {
                var zCoordinate = scale[2].GetScaledValue(dataValues.ElementAt(2).Value[value]);
                channel.position[2] = (float)zCoordinate;
            }

            visContainer.CreateDataMark(dataMarkPrefab, channel);
        }


        //## 04: Rescale
        visContainerObject.transform.localScale = new Vector3(width, height, depth);

        return visContainerObject;
    }

}
