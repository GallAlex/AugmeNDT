using System;
using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
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
        StackedHistogram,
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
        public List<AbstractDataset> dataSets;                          // List of Datasets as Dictionaries with all data attributes with their <name,values>. Dictionaries should all have the same attributes
        public DataEnsemble dataEnsemble; 
        public int attributeCount = 0;                                  // Number of attributes retrieved from the dataValues.
        public List<int> numberOfValues;                                // Number of values for each attribut from the dataValues.

        // Visualization Properties:
        public string title = "Basic Euclidean Vis";                    // Title of vis.
        public int axes = 3;                                            // Amount of Axes for Vis container
        public Dictionary<VisChannel, Attribute> channelEncoding;       // Cross-reference which encoding (Axes, Color,..) uses which attribute of the data
        private Dictionary<VisChannel, int[]> definedChannelEncoding;   // Internal reference between channel and attribute id (gets linked after appending the data)

        public float width = 0.25f;                                     // Vis container width in centimeters.
        public float height = 0.25f;                                    // Vis container height in centimeters.
        public float depth = 0.25f;                                     // Vis container depth in centimeters.
        public float[] xyzOffset = new[]{0.1f, 0.1f, 0.1f};             // Offset from origin (0,0) and End (1,0) for the Axes (x,y,z).
        public int[] xyzTicks = { 11, 11, 11 };                         // Amount of Ticks between min/max tick for Axes (x,y,z).
        public Color[] colorScheme = { Color.cyan, Color.white, Color.magenta };     // Defines Color Scheme for Color Channel

        // Interactions
        public VisInteractor visInteractor;                             // Interactor for the Vis    
        private DataVisGroup dataVisGroup;                              // Reference to DataVisGroup

        

        public Vis()
        {
            visContainer = new VisContainer();
            visContainerObject = visContainer.CreateVisContainer(title);

            definedChannelEncoding = new Dictionary<VisChannel, int[]>();
            channelEncoding = new Dictionary<VisChannel, Attribute>();

            dataEnsemble = new DataEnsemble();

            //Set default channel encodings
            DefineChannelToData(VisChannel.XPos, 1);
            DefineChannelToData(VisChannel.YPos, 2);
            DefineChannelToData(VisChannel.ZPos, 3);
            DefineChannelToData(VisChannel.Color, 3);
        }

        public virtual void DeleteVis()
        {
            visContainer.DeleteVisContainer();
            visContainer = null;
        }

        public virtual void SetVisParams()
        {
            visContainer.SetTitle(title);
            visContainer.SetAxisOffsets(xyzOffset);
            visContainer.SetAxisTickNumber(xyzTicks);
            visContainer.SetColorScheme(colorScheme);
            visContainer.SetVisInteractor(visInteractor);
        }

        /// <summary>
        /// Method sets the position of the anchored vis container
        /// </summary>
        /// <param name="pos"></param>
        public void SetVisContainerPosition(Vector3 pos)
        {
            visContainerObject.transform.localPosition = pos;
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
        /// Defines a specific attribute id to a specific vis channel
        /// Data gets linked to the vis channel after appending data
        /// </summary>
        /// <param name="visChannel"></param>
        /// <param name="attributeId"></param>
        public void DefineChannelToData(VisChannel visChannel, int attributeId)
        {
            definedChannelEncoding[visChannel] = new int[] { attributeId, -1 };
        }

        /// <summary>
        /// Defines a specific derived attribute id to a specific vis channel
        /// Data gets linked to the vis channel after appending data
        /// </summary>
        /// <param name="visChannel"></param>
        /// <param name="derivedAttrId"></param>
        public void DefineChannelToData(VisChannel visChannel, DerivedAttributes.DerivedAttributeCalc derivedAttrId)
        {
            definedChannelEncoding[visChannel] = new int[] {-1, (int)derivedAttrId };
        }

        /// <summary>
        /// Sets a specific attribute to a specific channel
        /// Replaces the previous attribute if already set
        /// </summary>
        /// <param name="visChannel"></param>
        /// <param name="attributeId"></param>
        public void SetChannelEncoding(VisChannel visChannel, Attribute attribute)
        {
            channelEncoding[visChannel] = attribute;
        }

        /// <summary>
        /// Sets a specific attribute from the dataset to a specific channel
        /// Replaces the previous attribute if already set
        /// </summary>
        /// <param name="visChannel"></param>
        /// <param name="attributeId"></param>
        private void SetChannelEncoding(VisChannel visChannel, int attributeId)
        {
            channelEncoding[visChannel] = dataSets[0].GetAttribute(attributeId);
        }

        /// <summary>
        /// Sets a specific derived attribute from one attribute from the dataset to a specific channel
        /// Replaces the previous attribute if already set
        /// </summary>
        /// <param name="visChannel"></param>
        /// <param name="derivedAttrId"></param>
        //private void SetChannelEncoding(VisChannel visChannel, DerivedAttributes.DerivedAttributeCalc derivedAttrId)
        //{
        //    channelEncoding[visChannel] = dataSets[0].GetDerivedAttribute(derivedAttrId);
        //}

        /// <summary>
        /// Links the defined channels to the data. Has to be called after appending the data!
        /// </summary>
        public void LinkChannelToData()
        {
            if (dataSets == null || dataSets.Count <= 0)
            {
                Debug.LogError("No Dataset defined for Visualization!");
                return;
            }

            foreach (var definedChannel in definedChannelEncoding)
            {
                // Attribute id [0] is not defined - derived attribute [1] used
                if (definedChannel.Value[0] == -1)
                {
                    //SetChannelEncoding(definedChannel.Key, (AbstractDataset.DerivedAttributes)definedChannel.Value[1]);
                }
                // Attribute id [0] is defined - attribute used
                else
                {
                    SetChannelEncoding(definedChannel.Key, definedChannel.Value[0]);
                }
            } 
            
        }

        public virtual GameObject CreateVis(GameObject container)
        {
            if (dataSets == null)
            {
                DeleteVis();
                return null;
            }
            
            visContainerObject.transform.SetParent(container.transform);
            visContainer.SetHandleText(dataSets[0].datasetName);

            //Set default channel encodings
            //SetChannelEncoding(VisChannel.XPos, dataSets[0].GetHeader());
            //SetChannelEncoding(VisChannel.YPos, new Attribute("Attribute Values", new[] { 0.0, 1.0 }));
            //SetChannelEncoding(VisChannel.ZPos, dataSets[0].GetAttribute(3));
            //SetChannelEncoding(VisChannel.Color, dataSets[0].GetAttribute(1));

            if (attributeCount < axes) axes = attributeCount;

            return visContainerObject;
        }

        public virtual void AppendData(AbstractDataset abstractDataset)
        {
            if (dataSets == null)
            {
                dataSets = new List<AbstractDataset>();
                numberOfValues = new List<int>();
            }

            attributeCount = abstractDataset.attributesCount;

            dataSets.Add(abstractDataset);
            numberOfValues.Add(abstractDataset.numberOfValues[0]);

            dataEnsemble.AddAbstractDataSet(abstractDataset);

            //Link Default Index to Data
            LinkChannelToData();
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

        public void SetChannel(VisChannel visChannel, Attribute attribute, bool normalized)
        {
            double[] values;

            if (normalized) values = attribute.GetNumericalValNorm();
            else values = attribute.GetNumericalVal();

            if (attribute.IsNumerical())
            {
                visContainer.SetChannel(visChannel, values);
            }
            else
            {
                visContainer.SetChannel(visChannel, attribute.GetNumericalVal());
            }

        }

        public void CreateAxis(Attribute attribute, bool normalized, Direction axisDirection)
        {
            double[] minMaxValues;

            if (normalized) minMaxValues = attribute.GetMinMaxValNorm(); 
            else minMaxValues = attribute.GetMinMaxVal();

            if (attribute.IsNumerical())
            {
                visContainer.CreateAxis(attribute.GetName(), minMaxValues, axisDirection);
            }
            else
            {
                visContainer.CreateAxis(attribute.GetName(), attribute.GetTextualVal(), axisDirection);
            }
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
            if(visContainerObject != null) visContainer.MoveGridBasedOnViewingDirection();

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
                case VisType.StackedHistogram:
                    return new VisStackedHistogram();
            }
        }

    }
}