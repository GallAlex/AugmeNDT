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
[Serializable]
public class Vis
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
    public List<int> encodedAttribute;                          // Cross-reference which encoding (Axes, Color,..) uses which attribute of the data
    //TODO: Enum whith possible encoding needed

    public List<Scale.DataScale> dataScaleTypes;                    // Applied scaling for dataValues domain of respective encoding
    public float width = 0.2f;                                  // Vis container width in centimeters.
    public float height = 0.2f;                                 // Vis container height in centimeters.
    public float depth = 0.2f;                                  // Vis container depth in centimeters.
    public Vector3 xyzOffset = new(0.1f, 0.1f, 0.1f);           // Offset from origin (0,0) for Axes (x,y,z).
    public int[] xyzTicks = { 10, 10, 10 };                     // Amount of Ticks between min/max tick for Axes (x,y,z).


    public virtual void InitVisParams(string visTitle, int numberOfAxes, List<Scale.DataScale> dataScales, float width, float height, float depth, Vector3 xyzOffset)
    {
        title = visTitle;
        this.axes = numberOfAxes;
        this.dataScaleTypes = dataScales;
        this.width = width;
        this.height = height;
        this.depth = depth;
        this.xyzOffset = xyzOffset;
    }

    public virtual GameObject CreateVis()
    {
        //TODO: Move into Constructor?

        visContainer = new VisContainer();
        
        visContainerObject =  visContainer.CreateVisContainer(title);
        encodedAttribute = new List<int>();

        visContainer.SetAxisOffsets(xyzOffset);
        visContainer.SetAxisTickNumber(xyzTicks);

        if (dimensions < axes) axes = dimensions;

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

    public virtual Dictionary<string, double[]> GetAppendedData()
    {
        return dataValues;
    }

    public virtual void ChangeAxisAttribute(int axisId, int selectedDimension, int numberOfTicks)
    {
        //Todo: Instead of Axis ID use encoding Id to change that encoding(Axis, Color, Size, Shape, ...)
    }

    public virtual void ChangeDataMarks()
    {

    }
    
    public Scale CreateScale(Scale.DataScale dataScale, List<double> domain, List<double> range)
    {
        Scale scaleFunction;

        switch (dataScale)
        {
            default:
            case Scale.DataScale.Linear:
                scaleFunction = new ScaleLinear(domain, range);
                break;
            case Scale.DataScale.Nominal:
                //scaleFunction = new ScaleNominal(domain, range, new List<string>(dataValues.Keys));
                throw new NotImplementedException("Nominal Scale is currently not implemented");
                break;

        }

        return scaleFunction;
    }


    public virtual void UpdateVis()
    {
        // Update Grid
        visContainer.MoveGridBasedOnViewingDirection();

        // Update different Channels/Marks of Vis (Data, Scale, Color,...)
    }
}
