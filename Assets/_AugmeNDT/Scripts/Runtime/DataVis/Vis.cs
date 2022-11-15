using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Base class to create different data visualizations charts
/// </summary>
public class Vis : MonoBehaviour
{
    // Vis container and used Prefabs
    public VisContainer visContainer;
    public GameObject visContainerObject;
    public GameObject dataMarkPrefab;
    public GameObject tickMarkPrefab;

    // Data
    public Dictionary<string, double[]> dataValues;       // Dictionary all data attributes with their <name,values>.
    public int dimensions = 0;                            // Number of attributes retrieved from the dataValues.
    public int numberOfValues = 0;                        // Number of values for each attribut from the dataValues.

    // Visualization Properties:
    public string title = "Basic Euclidean Vis";                // Title of vis.
    public int axes = 3;                                        // Amount of Axes for Vis container
    public Scale.DataScale dataScale = Scale.DataScale.Linear;  // Applied scaling for dataValues domain
    public float width = 0.5f;                                  // Vis container width in centimeters.
    public float height = 0.5f;                                 // Vis container height in centimeters.
    public float depth = 0.5f;                                  // Vis container depth in centimeters.
    public float[] xyzOffset = { 0.1f, 0.1f, 0.1f };            // Offset from origin (0,0) for Axes (x,y,z).
    public int[] xyzTicks = { 10, 10, 10 };                    // Amount of Ticks between min/max tick for Axes (x,y,z).


    public virtual void InitVisParams(string visTitle, int numberOfAxes, Scale.DataScale dataScale, float width, float height, float depth)
    {
        title = visTitle;
        this.axes = numberOfAxes;
        this.dataScale = dataScale;
        this.width = width;
        this.height = height;
        this.depth = depth;
    }

    public virtual GameObject CreateVis()
    {
        visContainer = new VisContainer();
        if (dataMarkPrefab == null) dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
        if (tickMarkPrefab == null) tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");


        visContainerObject =  visContainer.CreateVisContainer(title);

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

        for (int value = 0; value < numberOfValues; value++)
        {
            //Default:
            DataMark.Channel channel = new DataMark.Channel
            {
                position = new Vector3(0, 0, 0),
                rotation = new Vector3(0, 0, 0),
                color = new Vector4(1, 0, 0, 1),
                size = new Vector3(0.01f, 0.01f, 0.01f)
            };

            for (int currAxis = 0; currAxis < axes; currAxis++)
            {
                var coordinate = scale[currAxis].GetScaledValue((float)dataValues.ElementAt(currAxis).Value[value]);
                channel.position[currAxis] = coordinate;
            }

            visContainer.CreateDataMark(channel);
        }


        //## 04: Rescale
        visContainerObject.transform.localScale = new Vector3(width, height, depth);
        visContainerObject.GetComponent<BoundsControl>().UpdateBounds();

        return visContainerObject;
    }

    public virtual void AppendData(Dictionary<string, double[]> values)
    {
        // Preprocess Data
        if (values == null || values.Count < 1)
        {
            Debug.LogError("Appended Data is incorrect (insufficient dimensions, missing values, ...)");
            return;
        }
        dataValues = values;
        dimensions = values.Count;

        numberOfValues = values.ElementAt(0).Value.Length;

        // Test if every attribute has the same amount of values
        for (int dim = 0; dim < dimensions; dim++)
        {
            var currentValueCount = values.ElementAt(dim).Value.Length;

            if (currentValueCount <= 0 || currentValueCount - numberOfValues != 0)
            {
                Debug.LogError("Number of data values do not match (Missing Values!)");
                return;
            }

            numberOfValues = currentValueCount;
        }


    }

    public void ChangeAxisAttribute(int axisId, int selectedDimension, int numberOfTicks)
    {
        // Calculate new Scale based on selected Attribute

        List<float> domain = new List<float>(2);
        List<float> range = new List<float> { 0, 1 };

        domain.Add((float)dataValues.ElementAt(selectedDimension).Value.Min());
        domain.Add((float)dataValues.ElementAt(selectedDimension).Value.Max());

        Scale scale = CreateScale(domain, range);


        visContainer.ChangeAxis(axisId, dataValues.ElementAt(selectedDimension).Key, numberOfTicks, scale);
    }


    public Scale CreateScale(List<float> domain, List<float> range)
    {
        Scale scaleFunction;

        switch (dataScale)
        {
            default:
            case Scale.DataScale.Linear:
                scaleFunction = new ScaleLinear(domain, range);
                break;
            case Scale.DataScale.Ordinal:
                throw new NotImplementedException();
                break;

        }

        return scaleFunction;
    }

    public virtual void UpdateVis()
    {
        // Update different Channels/Marks of Vis (Data, Scale, Color,...)
    }
}
