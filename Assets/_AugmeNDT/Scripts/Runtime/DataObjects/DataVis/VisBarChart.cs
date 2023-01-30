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

        dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
        tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
    }

    public override GameObject CreateVis(GameObject container)
    {
        base.CreateVis(container);

        //Initialize dataScales
        dataScaleTypes = new List<Scale.DataScale>();
        for (int attrScale = 0; attrScale < dimensions; attrScale++)
        {
            dataScaleTypes.Add(Scale.DataScale.Linear);
        }

        //## 01: Create Data Scales
        List<Scale> scale = new List<Scale>(axes);

        for (int drawnDim = 0; drawnDim < axes; drawnDim++)
        {
            List<double> domain = new List<double>(2);
            List<double> range = new List<double>(2);

            domain.Add((double)dataSets[0].ElementAt(drawnDim).Value.Min());
            domain.Add((double)dataSets[0].ElementAt(drawnDim).Value.Max());

            range.Add(0);
            range.Add(1);

            scale.Add(CreateScale(dataScaleTypes[drawnDim], domain, range));
        }

        //## 02: Create Axes and Grids
        for (int currAxis = 0; currAxis < axes; currAxis++)
        {
            encodedAttribute.Add(currAxis);
            int nextDim = (currAxis + 1) % axes;
            //visContainer.CreateAxis(((Direction)currAxis).ToString() + " Label", (Direction)currAxis);
            visContainer.CreateAxis(dataSets[0].ElementAt(currAxis).Key, (Direction)currAxis, scale[currAxis]);
            visContainer.CreateGrid((Direction)currAxis, (Direction)nextDim);
        }


        //## 03: Create Data Points
        // Scale Bars
        for (int value = 0; value < numberOfValues[0]; value++)
        {
            //Default:
            DataMark.Channel channel = DataMark.DefaultDataChannel();

            //X Axis
            var xCoordinate = scale[0].GetScaledValue(dataSets[0].ElementAt(0).Value[value]);
            channel.position[0] = (float)xCoordinate;

            //Y Axis
            var barHeight = scale[1].GetScaledValue(dataSets[0].ElementAt(1).Value[value]);
            channel.size[1] = (float)barHeight;

            //Z Axis
            if (axes == 3)
            {
                var zCoordinate = scale[2].GetScaledValue(dataSets[0].ElementAt(2).Value[value]);
                channel.position[2] = (float)zCoordinate;
            }

            visContainer.CreateDataMark(dataMarkPrefab, channel);
        }


        //## 04: Rescale
        visContainerObject.transform.localScale = new Vector3(width, height, depth);

        return visContainerObject;
    }

    public override void ChangeAxisAttribute(int axisId, int selectedDimension, int numberOfTicks)
    {
        // Record new selected attribute
        encodedAttribute[axisId] = selectedDimension;

        // Calculate new Scale based on selected Attribute
        List<double> domain = new List<double>(2);
        List<double> range = new List<double> { 0, 1 };

        domain.Add(dataSets[0].ElementAt(selectedDimension).Value.Min());
        domain.Add(dataSets[0].ElementAt(selectedDimension).Value.Max());

        Scale scale = CreateScale(dataScaleTypes[axisId], domain, range);


        visContainer.ChangeAxis(axisId, dataSets[0].ElementAt(selectedDimension).Key, scale, numberOfTicks);

        //Change Data Marks
        ChangeDataMarks();
    }

    public override void ChangeDataMarks()
    {
        for (int value = 0; value < numberOfValues[0]; value++)
        {
            //Default:
            DataMark.Channel channel = DataMark.DefaultDataChannel();

            //X Axis
            var xCoordinate = visContainer.dataAxisList[0].dataScale.GetScaledValue(dataSets[0].ElementAt(encodedAttribute[0]).Value[value]);
            channel.position[0] = (float)xCoordinate;

            //Y Height
            var barHeight = visContainer.dataAxisList[1].dataScale.GetScaledValue(dataSets[0].ElementAt(encodedAttribute[1]).Value[value]);
            channel.size[1] = (float)barHeight;

            //Z Axis
            if (axes == 3)
            {
                var zCoordinate = visContainer.dataAxisList[2].dataScale.GetScaledValue(dataSets[0].ElementAt(encodedAttribute[2]).Value[value]);
                channel.position[2] = (float)zCoordinate;
            }

            visContainer.ChangeDataMark(value, channel);
        }
    }
}
