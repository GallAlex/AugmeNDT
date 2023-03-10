using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Possible Visulization Types to choose from
/// </summary>
public enum VisType
{
    BarChart,
    Scatterplot,
    MDDGlyphs,
    NumberOfVisTypes,
}

/// <summary>
/// Possible Visulization Channels
/// </summary>
public enum VisChannels
{
    XAxis,
    YAxis,
    ZAxis,
    Color,
    Size,
    Orientation,
    NumberOfChannels,
}

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
    public List<Dictionary<string, double[]>> dataSets;         // List of Datasets as Dictionaries with all data attributes with their <name,values>. Dictionaries should all have the same attributes
    public int dimensions = 0;                                  // Number of attributes retrieved from the dataValues.
    public List<int> numberOfValues;                            // Number of values for each attribut from the dataValues.

    // Visualization Properties:
    public string title = "Basic Euclidean Vis";                // Title of vis.
    public int axes = 3;                                        // Amount of Axes for Vis container
    public List<int> encodedAttribute;                          // Cross-reference which encoding (Axes, Color,..) uses which attribute of the data
    //TODO: Enum whith possible encoding needed

    public List<Scale.DataScaleType> dataScaleTypes;            // Applied scaling for dataValues domain of respective encoding
    public float width = 0.25f;                                 // Vis container width in centimeters.
    public float height = 0.25f;                                // Vis container height in centimeters.
    public float depth = 0.25f;                                 // Vis container depth in centimeters.
    public float[] xyzOffset = new[]{0.1f, 0.1f, 0.1f};         // Offset from origin (0,0) and End (1,0) for the Axes (x,y,z).
    public int[] xyzTicks = { 10, 10, 10 };                     // Amount of Ticks between min/max tick for Axes (x,y,z).

    // Interactions
    public VisInteractor visInteractor;                        // Interactor for the Vis    
    private DataVisGroup dataVisGroup;                          // Reference to DataVisGroup

    
    public virtual void InitVisParams(string visTitle, int numberOfAxes, List<Scale.DataScaleType> dataScales, float width, float height, float depth, float[] xyzOffset)
    {
        title = visTitle;
        this.axes = numberOfAxes;
        this.dataScaleTypes = dataScales;
        this.width = width;
        this.height = height;
        this.depth = depth;
        this.xyzOffset = xyzOffset;
    }

    /// <summary>
    /// Gives the Vis acces to its DataVis group
    /// </summary>
    /// <param name="group"></param>
    public void SetDataVisGroup(DataVisGroup group)
    {
        dataVisGroup = group;
    }

    /// <summary>
    /// Returns the DataVisGroup of the Vis
    /// </summary>
    /// <returns></returns>
    public DataVisGroup GetDataVisGroup()
    {
        return dataVisGroup;
    }

    public virtual GameObject CreateVis(GameObject container)
    {
        //TODO: Move into Constructor?

        visContainer = new VisContainer();
        
        visContainerObject = visContainer.CreateVisContainer(title);
        visContainerObject.transform.SetParent(container.transform);

        encodedAttribute = new List<int>();

        visContainer.SetAxisOffsets(xyzOffset);
        visContainer.SetAxisTickNumber(xyzTicks);
        visContainer.SetVisInteractor(visInteractor);

        if (dimensions < axes) axes = dimensions;

        return visContainerObject;
    }

    public virtual void AppendData(Dictionary<string, double[]> values)
    {
        //Todo: Move initialize?
        if (dataSets == null)
        {
            dataSets = new List<Dictionary<string, double[]>>();
            numberOfValues = new List<int>();
        }

        // Preprocess Data
        if (values == null || values.Count < 1)
        {
            Debug.LogError("Appended Data is incorrect (insufficient dimensions, missing values, ...)");
            return;
        }
        dimensions = values.Count;

        //Check other data sets if they have the same amount of attributes
        if (dataSets.Count > 0)
        {
            if (values.Count != dimensions)
            {
                Debug.LogError("Number of data attributes do not match with other loaded datasets (Missing Attributes!)");
                return;
            }
        }

        dataSets.Add(values);
        numberOfValues.Add(values.ElementAt(0).Value.Length);

        // Test if every attribute has the same amount of values
        for (int dim = 0; dim < dimensions; dim++)
        {
            var currentValueCount = values.ElementAt(dim).Value.Length;

            if (currentValueCount <= 0 || currentValueCount - numberOfValues[numberOfValues.Count-1] != 0)
            {
                Debug.LogError("Number of data values do not match (Missing Values!)");
                return;
            }

            //numberOfValues = currentValueCount;
        }


    }

    public virtual List<Dictionary<string, double[]>> GetAppendedData()
    {
        return dataSets;
    }

    public virtual void CreateDataScales()
    {

    }

    public virtual void ChangeAxisAttribute(int axisId, int selectedDimension, int numberOfTicks)
    {
        //Todo: Instead of Axis ID use encoding Id to change that encoding(Axis, Color, Size, Shape, ...)
    }

    public virtual void CreateDataMarks()
    {

    }

    public virtual void ChangeDataMarks()
    {

    }

    public Scale CreateScale(Scale.DataScaleType dataScale, List<double> domain, List<double> range)
    {
        Scale scaleFunction;

        switch (dataScale)
        {
            default:
            case Scale.DataScaleType.Linear:
                scaleFunction = new ScaleLinear(domain, range);
                break;
            case Scale.DataScaleType.Nominal:
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

    /// <summary>
    /// Method returns the selected Visualization child class
    /// </summary>
    /// <param name="vistype"></param>
    /// <returns></returns>
    public static Vis GetSpecificVisType(Enum vistype)
    {
        switch (vistype)
        {
            default:
            case VisType.BarChart:
                return new VisBarChart();
            case VisType.Scatterplot:
                return new VisScatterplot();
            case VisType.MDDGlyphs:
                return new VisMDDGlyphs();
        }
    }

}
