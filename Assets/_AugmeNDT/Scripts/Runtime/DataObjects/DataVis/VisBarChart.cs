using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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

        //## 01: Create Axes and Grids

        for (int currAxis = 0; currAxis < axes; currAxis++)
        {
            encodedAttribute.Add(currAxis);
            int nextDim = (currAxis + 1) % axes;
            visContainer.CreateAxis(dataSets[0].ElementAt(currAxis).Key, dataSets[0].ElementAt(currAxis).Value, (Direction)currAxis);
            visContainer.CreateGrid((Direction)currAxis, (Direction)nextDim);
        }

        //## 02: Set Remaining Vis Channels (Color,...)
        visContainer.SetChannel(VisChannel.XPos, dataSets[0].ElementAt(0).Value);
        visContainer.SetChannel(VisChannel.YSize, dataSets[0].ElementAt(1).Value);
        if (axes == 3) visContainer.SetChannel(VisChannel.ZPos, dataSets[0].ElementAt(2).Value);

        visContainer.SetChannel(VisChannel.Color, dataSets[0].ElementAt(3).Value);

        //## 03: Draw all Data Points with the provided Channels 
        visContainer.CreateDataMarks(dataMarkPrefab, new[] { 0, 1, 0 });

        //## 04: Create Color Scalar Bar
        GameObject colorScalarBarContainer = new GameObject("Color Scale");
        colorScalarBarContainer.transform.parent = visContainerObject.transform;

        ColorScalarBar colorScalarBar = new ColorScalarBar();

        double[] minMaxColorVal = new[]
            { dataSets[0].ElementAt(3).Value.Min().Round(3), dataSets[0].ElementAt(3).Value.Max().Round(3) };
        GameObject colorBar01 = colorScalarBar.CreateColorScalarBar(visContainerObject.transform.position, dataSets[0].ElementAt(3).Key, minMaxColorVal, 1, colorScheme);
        colorBar01.transform.parent = colorScalarBarContainer.transform;

        //## 05: Rescale Chart
        visContainerObject.transform.localScale = new Vector3(width, height, depth);

        return visContainerObject;
    }

    public override void ChangeAxisAttribute(int axisId, int selectedDimension, int numberOfTicks)
    {
        /*
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
        */
    }

    public override void ChangeDataMarks()
    {
        /*
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
        */
    }

}
