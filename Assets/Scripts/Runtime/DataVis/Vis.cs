using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int dimensions = 0;                                  // Number of attributes retrieved from the dataValues.

    // Visualization Properties:
    public string title = "Basic Euclidean Vis";        // Title of vis.
    public int axes = 3;                                // Amount of Axes for Vis container
    public Scale.DataScale dataScale = Scale.DataScale.Linear;      // Applied scaling for dataValues domain
    public float width = 0.5f;                          // Vis container width in centimeters.
    public float height = 0.5f;                         // Vis container height in centimeters.
    public float depth = 0.5f;                          // Vis container depth in centimeters.
    public float[] xyzOffset = { 0.1f, 0.1f, 0.1f };    // Offset from origin (0,0) for Axes (x,y,z).
    public int[] xyzTicks = { 10, 10 ,10 };             // Amount of Ticks for Axes (x,y,z).

    private void Awake()
    {
        visContainer = new VisContainer();

        if (dataMarkPrefab == null) dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
        if (tickMarkPrefab == null) tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");

    }

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

        //## 01: Create Data Scales
        int[] selectedDimensions = { 0, 1, 2 };
        List<Scale> scale = new List<Scale>(selectedDimensions.Length);

        for (int drawnDim = 0; drawnDim < selectedDimensions.Length; drawnDim++)
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
            visContainer.CreateAxis(dataValues.ElementAt(selectedDimensions[currAxis]).Key, (Direction)currAxis, scale[currAxis]);
            visContainer.CreateGrid((Direction)currAxis, (Direction)nextDim);
        }


        //## 03: Create Data Points
        for (int value = 0; value < dimensions; value++)
        {
            float xVal = scale[selectedDimensions[0]].GetScaledValue((float)dataValues.ElementAt(selectedDimensions[0]).Value[value]);
            float yVal = scale[selectedDimensions[1]].GetScaledValue((float)dataValues.ElementAt(selectedDimensions[1]).Value[value]);
            float zVal = scale[selectedDimensions[2]].GetScaledValue((float)dataValues.ElementAt(selectedDimensions[2]).Value[value]);
            DataMark.Channel channel = new DataMark.Channel
            {
                position = new Vector3(xVal, yVal, zVal),
                rotation = new Vector3(0, 0, 0),
                color = new Vector4(1, 0, 0, 1)
            };

            visContainer.CreateDataMark(channel);
        }


        //for (float x = 0; x <= 1; x += 0.01f)
        //{
        //    const float pi = 3.14f;
        //    double y = 0.5 * (1 + Math.Sin(2 * pi * x));
        //    float z = 0;
        //    if (axes > 2) z = 0.5f;

        //    DataMark.Channel channel = new DataMark.Channel
        //    {
        //        position = new Vector3(x, (float)y, z),
        //        rotation = new Vector3(0, 0, 0),
        //        color = new Vector4((float)y, 0, 0, 1)
        //    };

        //    visContainer.CreateDataMark(channel);
        //}


        //## 04: Rescale
        visContainerObject.transform.localScale = new Vector3(width, height, depth);


        return visContainerObject;
    }

    public virtual void AppendData(Dictionary<string, double[]> values)
    {
        // Preprocess Data
        dataValues = values;
        dimensions = values.ElementAt(0).Value.Length;
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

    public void UpdateVis()
    {
        // Update different Channels/Marks of Vis (Data, Scale, Color,...)
    }
}
