using System;
using System.Collections.Generic;
using System.Linq;
using AugmeNDT;
using UnityEngine;

/// <summary>
/// Possible Visulization Types to choose from
/// </summary>
public enum VisType
{
    BarChart,
    Histogram,
    Scatterplot,
    TimeScatter,
    MDDGlyphs,
    NumberOfVisTypes,
}

/// <summary>
/// Possible Visulization Channels
/// </summary>
public enum VisChannel
{
    XPos,
    YPos,
    ZPos,
    XSize,
    YSize,
    ZSize,
    XRotation,
    YRotation,
    ZRotation,
    Color,
    NumberOfVisChannels
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
    public List<AbstractDataset> dataSets;                      // List of Datasets as Dictionaries with all data attributes with their <name,values>. Dictionaries should all have the same attributes
    public int attributeCount = 0;                              // Number of attributes retrieved from the dataValues.
    public List<int> numberOfValues;                            // Number of values for each attribut from the dataValues.

    // Visualization Properties:
    public string title = "Basic Euclidean Vis";                // Title of vis.
    public int axes = 3;                                        // Amount of Axes for Vis container
    public Dictionary<VisChannel, double[]> channelEncoding;         // Cross-reference which encoding (Axes, Color,..) uses which attribute (id) of the data

    public List<Scale.DataScaleType> dataScaleTypes;            // Applied scaling for dataValues domain of respective encoding
    public float width = 0.25f;                                 // Vis container width in centimeters.
    public float height = 0.25f;                                // Vis container height in centimeters.
    public float depth = 0.25f;                                 // Vis container depth in centimeters.
    public float[] xyzOffset = new[]{0.1f, 0.1f, 0.1f};         // Offset from origin (0,0) and End (1,0) for the Axes (x,y,z).
    public int[] xyzTicks = { 10, 10, 10 };                     // Amount of Ticks between min/max tick for Axes (x,y,z).
    public Color[] colorScheme = { Color.cyan, Color.magenta }; // Defines Color Scheme for Color Channel

    // Interactions
    public VisInteractor visInteractor;                         // Interactor for the Vis    
    private DataVisGroup dataVisGroup;                          // Reference to DataVisGroup

    //TODO: visualizedAttributes should be encodedAttribute and uses ChannelValues
    public int[] visualizedAttributes = new[]{0, 1, 2};// Each index represents the attribute which is visualized on the respective axis (if possible)


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

    /// <summary>
    /// Sets a specific attribute from the dataset to a specific channel
    /// </summary>
    /// <param name="visChannel"></param>
    /// <param name="attributeId"></param>
    public void SetChannelEncoding(VisChannel visChannel, int attributeId)
    {
        channelEncoding.Add(visChannel, dataSets[0].GetValues(attributeId));
    }

    public void SetChannelEncoding(VisChannel visChannel, double[] data)
    {
        channelEncoding.Add(visChannel, data);
    }

    public virtual GameObject CreateVis(GameObject container)
    {
        //TODO: Move into Constructor?
        visContainer = new VisContainer();
        visContainerObject = visContainer.CreateVisContainer(title);
        visContainerObject.transform.SetParent(container.transform);

        visContainer.SetAxisOffsets(xyzOffset);
        visContainer.SetAxisTickNumber(xyzTicks);
        visContainer.SetColorScheme(colorScheme);
        visContainer.SetVisInteractor(visInteractor);

        //Set Default Index
        if (channelEncoding== null || channelEncoding.Count == 0)
        {
            channelEncoding = new Dictionary<VisChannel, double[]>();

            SetChannelEncoding(VisChannel.XPos, 0);
            SetChannelEncoding(VisChannel.YPos, 1);
            SetChannelEncoding(VisChannel.ZPos, 2);
        }

        if (attributeCount < axes) axes = attributeCount;

        return visContainerObject;
    }

    public virtual void AppendData(AbstractDataset abstractDataset)
    {
        var values = abstractDataset.GetNumericDic();

        //Todo: Move initialize?
        if (dataSets == null)
        {
            dataSets = new List<AbstractDataset>();
            numberOfValues = new List<int>();
        }

        // Preprocess Data
        if (values == null || values.Count < 1)
        {
            Debug.LogError("Appended Data is incorrect (insufficient dimensions, missing values, ...)");
            return;
        }
        attributeCount = values.Count;

        //Check other data sets if they have the same amount of attributes
        if (dataSets.Count > 0)
        {
            if (values.Count != attributeCount)
            {
                Debug.LogError("Number of data attributes do not match with other loaded datasets (Missing Attributes!)");
                return;
            }
        }

        dataSets.Add(abstractDataset);
        numberOfValues.Add(values.ElementAt(0).Value.Length);

        // Test if every attribute has the same amount of values
        for (int dim = 0; dim < attributeCount; dim++)
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

    public virtual List<AbstractDataset> GetAppendedData()
    {
        return dataSets;
    }

    public virtual void CreateColorLegend(GameObject legend)
    {
        if (legend == null) Debug.LogError("No Legend GameObject created!");
        else visContainer.CreateColorLegend(legend);
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
            case VisType.Histogram:
                return new VisHistogram();
            case VisType.Scatterplot:
                return new VisScatterplot();
            case VisType.TimeScatter:              
                return new VisTimeScatter();
            case VisType.MDDGlyphs:
                return new VisMDDGlyphs();
        }
    }

}
