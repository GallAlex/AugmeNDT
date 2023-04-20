using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Creates the Unit Container, Axes, and DataMarks for 2D/3D Visualizations
/// </summary>
public class VisContainer
{
    public GameObject containerPrefab;
    public GameObject visContainer;
    public GameObject axisContainer;
    public GameObject gridContainer;
    public GameObject dataMarkContainer;

    
    // Main Container Elements
    public List<DataAxis> dataAxisList;     // Axes with Ticks
    public List<DataGrid> dataGridList;     // Grids
    public List<DataMark> dataMarkList;     // Data Marks

    // DataValues
    public Dictionary<VisChannel, double[]> channelValues;  // Data Values for each Channel
    public Dictionary<VisChannel, Scale> channelScale;      // Scaling for each used Channel
    public bool specificColorsSet = false;
    public Color[] specificColorChannel;                    // Color channel filled with specific colors

    // Interactor
    private VisInteractor visInteractor;

    private const float axisMeshLength = 1.0f;
    private Bounds containerBounds;            // Width, Height, Length of the Container
    private bool boundsControl = true;
    private float[] xyzOffset;
    private int[] xyzTicks;
    private Color[] colorScheme;

    #region CREATION OF ELEMENTS

    public GameObject CreateVisContainer(string visName)
    {
        dataAxisList = new List<DataAxis>();
        dataGridList = new List<DataGrid>();
        dataMarkList = new List<DataMark>();

        channelValues = new Dictionary<VisChannel, double[]>();
        channelScale = new Dictionary<VisChannel, Scale>();

        containerPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/DataVisContainer");
        visContainer = GameObject.Instantiate(containerPrefab, containerPrefab.transform.position, Quaternion.identity);
        visContainer.name = visName;

        axisContainer = new GameObject("Axes");
        gridContainer = new GameObject("Grids");
        dataMarkContainer = new GameObject("Data Marks");

        //Add Childs
        axisContainer.transform.parent = visContainer.transform;
        gridContainer.transform.parent = visContainer.transform;
        dataMarkContainer.transform.parent = visContainer.transform;

        //Set the basic container size by using the visContainer
        containerBounds = visContainer.GetComponent<BoxCollider>().bounds;
  
        return visContainer;
    }

    /// <summary>
    /// Method to create a new Axis for a Direction (X/Y/Z), and add it to the list of DataAxis.
    /// The scaling of the axis is calculated by the min and max values of the data.
    /// The values and the Scale are stored in the channelValues and channelScale Dictionary.
    /// </summary>
    /// <param name="axisLabel"></param>
    /// <param name="minMaxValues"></param>
    /// <param name="axisDirection"></param>
    public void CreateAxis(string axisLabel, double[] minMaxValues, Direction axisDirection)
    {
        DataAxis axis = new DataAxis(xyzOffset[(int)axisDirection]);
        //Return Length of current axis
        Scale scale = CreateAxisScale(minMaxValues, xyzOffset[(int)axisDirection]);

        axis.CreateAxis(axisContainer.transform, axisLabel, scale, axisDirection, xyzTicks[(int)axisDirection]);

        dataAxisList.Add(axis);
    }

    /// <summary>
    /// Method to create a new Axis for a Direction (X/Y/Z), and add it to the list of DataAxis.
    /// The scaling of the axis is calculated by the amount of attribute names.
    /// The values and the Scale are stored in the channelValues and channelScale Dictionary.
    /// </summary>
    /// <param name="axisLabel"></param>
    /// <param name="attributeNames"></param>
    /// <param name="axisDirection"></param>
    public void CreateAxis(string axisLabel, string[] attributeNames, Direction axisDirection)
    {
        DataAxis axis = new DataAxis(xyzOffset[(int)axisDirection]);
        //Return Length of current axis
        Scale scale = CreateAxisScale(attributeNames, xyzOffset[(int)axisDirection]);

        axis.CreateAxis(axisContainer.transform, axisLabel, scale, axisDirection, xyzTicks[(int)axisDirection]);

        dataAxisList.Add(axis);
    }

    //TODO: Create multiple grids on differetn positions (at tick position?)
    public void CreateGrid(Direction axis1, Direction axis2)
    {
        DataGrid grid = new DataGrid();
        Direction[] axisDirections = { axis1, axis2 };

        grid.CreateGrid(gridContainer.transform, containerBounds, xyzOffset[(int)axis1], xyzOffset[(int)axis2], axisDirections, xyzTicks[(int)axis1], xyzTicks[(int)axis2]);

        dataGridList.Add(grid);
    }

    public void CreateDataMarks(GameObject markPrefab)
    {
        int numberOfMarks = channelValues.ElementAt(1).Value.Length;

        Debug.Log("Number of Marks: " + numberOfMarks);

        for (int mark = 0; mark < numberOfMarks; mark++)
        {
            //TODO: Performance of adding prefab every time?
            DataMark dataMark = new DataMark(dataMarkList.Count, markPrefab);
            dataMark.SetVisInteractor(visInteractor);

            //Create Values
            DataMark.Channel channel = DataMark.DefaultDataChannel();
            channel = GetDataMarkChannelValues(channel, mark);

            dataMark.CreateDataMark(dataMarkContainer.transform, channel);
            dataMarkList.Add(dataMark);
        }
    }

    public void SetChannel(VisChannel visChannel, double[] dataValues)
    {
        switch (visChannel)
        {
            case VisChannel.XPos:
                channelScale.Add(visChannel, GetAxisScale(Direction.X));  //Uses the X-Axis Scale
                channelValues.Add(visChannel, dataValues);
                break;
            case VisChannel.YPos:
                channelScale.Add(visChannel, GetAxisScale(Direction.Y));  //Uses the Y-Axis Scale
                channelValues.Add(visChannel, dataValues);
                break;
            case VisChannel.ZPos:
                channelScale.Add(visChannel, GetAxisScale(Direction.Z));  //Uses the Z-Axis Scale
                channelValues.Add(visChannel, dataValues);
                break;
            case VisChannel.XSize:
                channelScale.Add(visChannel, CreateSizeScale(dataValues, xyzOffset[(int)Direction.X]));
                channelValues.Add(visChannel, dataValues);
                break;
            case VisChannel.YSize:
                channelScale.Add(visChannel, CreateSizeScale(dataValues, xyzOffset[(int)Direction.Y]));
                channelValues.Add(visChannel, dataValues);
                break;
            case VisChannel.ZSize:
                channelScale.Add(visChannel, CreateSizeScale(dataValues, xyzOffset[(int)Direction.Z]));
                channelValues.Add(visChannel, dataValues);
                break;
            case VisChannel.XRotation:
            case VisChannel.YRotation:
            case VisChannel.ZRotation:
                //Todo: Rotation [0, 359] so that min != max
                channelScale.Add(visChannel, GetChannelScale(dataValues, new[] { 0.0, 360.0 }));
                channelValues.Add(visChannel, dataValues);
                break;
            case VisChannel.Color:
                channelValues.Add(visChannel, dataValues);
                break;
            default:
                break;
        }

    }

    public void SetSpecificColor(Color[] colorValues)
    {
        specificColorsSet = true;
        specificColorChannel = colorValues;
    }

    public DataMark.Channel GetDataMarkChannelValues(DataMark.Channel channel, int valueIndex)
    {
        //Fill DataMark for every Channel which has Data set
        foreach (var setChannel in channelValues)
        {
            switch (setChannel.Key)
            {
                case VisChannel.XPos:
                    channel.position.x = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                    break;
                case VisChannel.YPos:
                    channel.position.y = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                    break;
                case VisChannel.ZPos:
                    channel.position.z = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                    break;
                case VisChannel.XSize:
                    channel.size.x = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                    break;
                case VisChannel.YSize:
                    channel.size.y = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                    break;
                case VisChannel.ZSize:
                    channel.size.z = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                    break;
                case VisChannel.XRotation:
                    channel.rotation.x = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                    break;
                case VisChannel.YRotation:
                    channel.rotation.y = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                    break;
                case VisChannel.ZRotation:
                    channel.rotation.z = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                    break;
                case VisChannel.Color:
                    if(specificColorsSet) channel.color = specificColorChannel[valueIndex];
                    else channel.color = ScaleColor.GetInterpolatedColor(setChannel.Value[valueIndex], setChannel.Value.Min(), setChannel.Value.Max(), colorScheme);
                    break;
                default:
                    break;
            }
        }

        return channel;
    }

    #endregion

    #region CHANGE OF ELEMENTS

    public void ChangeAxis(int axisID, string axisLabel, Scale dataScale, int numberOfTicks)
    {
        if (axisID < 0 || axisID > dataAxisList.Count)
        {
            Debug.LogError("Selected axis does not exist");
            return;
        }

        dataAxisList[axisID].ChangeAxis(axisLabel, dataScale, numberOfTicks);

    }

    public void ChangeDataMark(int dataMarkID, DataMark.Channel channel)
    {
        if (dataMarkID < 0 || dataMarkID > dataMarkList.Count)
        {
            Debug.LogError("Data Mark does not exist");
            return;
        }

        dataMarkList[dataMarkID].ChangeDataMark(channel);

    }

    #endregion

    /// <summary>
    /// Returns the current applied Scale for the selected Axis
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    public Scale GetAxisScale(Direction axis)
    {
        if (dataAxisList.Count < (int)axis + 1)
        {
            Debug.LogError("Selected Axis is not created");
            return null;
        }

        return dataAxisList[(int)axis].dataScale;
    }

    /// <summary>
    /// Defines the tick offest for every Axis
    /// Moves the first tick from the axis origin and the last tick from the end of the axis by by the offset
    /// </summary>
    /// <param name="xyzOffset"></param>
    public void SetAxisOffsets(float[] xyzOffset)
    {
        this.xyzOffset = xyzOffset;
    }

    /// <summary>
    /// Sets the amount of Ticks for each Axis
    /// </summary>
    /// <param name="xyzTicks"></param>
    public void SetAxisTickNumber(int[] xyzTicks)
    {
        this.xyzTicks = xyzTicks;
    }

    // Sets the Interactor of the respective Vis
    public void SetVisInteractor(VisInteractor interactor)
    {
        visInteractor = interactor;
    }

    public void SetContainerBounds(Bounds cBounds)
    {
        containerBounds = cBounds;
    }

    public void EnableBoundingBox(bool enable)
    {
        boundsControl = enable;
        BoundingBoxVisibility();
    }

    private Vector3 GetCenterOfVisContainer()
    {
        Vector3 center = visContainer.transform.position + visContainer.transform.localScale / 2f; ;
        return center;
    }

    public void BoundingBoxVisibility()
    {
        //TODO
    }

    /// <summary>
    /// Method moves the Grid in the VisContainer side which is further away from the camera
    /// For this the Container is viewed as consiting out of 8 octants. 
    /// </summary>
    public void MoveGridBasedOnViewingDirection()
    {
        if (dataGridList.Count < 3) return;

        Vector3 center = GetCenterOfVisContainer();
        Vector3 cDir = Camera.main.transform.position;

        // Calculate in which octant  (8 possibilities) of the cube the camera is located
        // >-Bottom->  --+  |  +-+   >-Top->  -++  |  +++
        // >-Bottom->  ---  |  +--   >-Top->  -+-  |  ++-

        //## Bottom Part ##
        // --+
        if (cDir.x < center.x && cDir.y < center.y && cDir.z > center.z)
        {
            //XY
            dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            //YZ
            dataGridList[1].GetGridObject().transform.localPosition = new Vector3(1, 0, 0);
            //XZ
            dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 1, 0);
        }
        //+-+
        else if (cDir.x > center.x && cDir.y < center.y && cDir.z > center.z)
        {
            //XY
            dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            //YZ
            dataGridList[1].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            //XZ
            dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 1, 0);
        }
        //---
        else if (cDir.x < center.x && cDir.y < center.y && cDir.z < center.z)
        {
            //XY
            dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 1);
            //YZ
            dataGridList[1].GetGridObject().transform.localPosition = new Vector3(1, 0, 0);
            //XZ
            dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 1, 0);
        }
        //+--
        else if (cDir.x > center.x && cDir.y < center.y && cDir.z < center.z)
        {
            //XY
            dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 1);
            //YZ
            dataGridList[1].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            //XZ
            dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 1, 0);
        }
        //## Top Part ##
        // -++
        if (cDir.x < center.x && cDir.y > center.y && cDir.z > center.z)
        {
            //XY
            dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            //YZ
            dataGridList[1].GetGridObject().transform.localPosition = new Vector3(1, 0, 0);
            //XZ
            dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
        }
        //+++
        else if (cDir.x > center.x && cDir.y > center.y && cDir.z > center.z)
        {
            //XY
            dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            //YZ
            dataGridList[1].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            //XZ
            dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
        }
        //-+-
        else if (cDir.x < center.x && cDir.y > center.y && cDir.z < center.z)
        {
            //XY
            dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 1);
            //YZ
            dataGridList[1].GetGridObject().transform.localPosition = new Vector3(1, 0, 0);
            //XZ
            dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
        }
        //++-
        else if (cDir.x > center.x && cDir.y > center.y && cDir.z < center.z)
        {
            //XY
            dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 1);
            //YZ
            dataGridList[1].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            //XZ
            dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
        }

    }

    /// <summary>
    /// Returns the min position and max position of the specific Axis in the Container including the offset
    /// </summary>
    /// <returns></returns>
    private float[] GetAxisOffsetCoord(Direction axis)
    {
        Vector3 min = containerBounds.min;
        Vector3 max = containerBounds.max;
        return new[] { min[(int)axis] + xyzOffset[(int)axis], max[(int)axis] - xyzOffset[(int)axis] };
    }

    /// <summary>
    /// Create Range for an Axis based on the Tick Offset and the Axis Length
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    private List<double> GetAxisRange(Direction axis)
    {
        float[] axisOffsetCoord = GetAxisOffsetCoord(axis);
        
        return new List<double> { axisOffsetCoord[0], axisOffsetCoord[1]};
    }

    /// <summary>
    /// Method returns the Scale Function for double Values added to the Axis
    /// Uses ScaleLinear
    /// </summary>
    /// <param name="dataValues"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    private Scale CreateAxisScale(double[] dataValues, float offset)
    {
        List<double> range = new List<double>(2)
        {
            0.0d + offset,
            1.0d - offset
        };

        List<double> domain = new List<double>(2)
        {
            dataValues.Min(),
            dataValues.Max()
        };

        Scale scaleFunction = new ScaleLinear(domain, range);

        return scaleFunction;
    }

    /// <summary>
    /// Method returns the Scale Function for string Values added to the Axis
    /// Uses ScaleNominal
    /// </summary>
    /// <param name="dataValues"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    private Scale CreateAxisScale(string[] dataValues, float offset)
    {
        List<double> range = new List<double>(2)
        {
            0.0d + offset,
            1.0d - offset
        };

        List<double> domain = new List<double>(2)
        {
            0,
            dataValues.Length - 1
        };

        Scale scaleFunction = new ScaleNominal(domain, range, dataValues.ToList());

        return scaleFunction;
    }

    /// <summary>
    /// Method returns the Scale Function for double Values representing the Channel Size
    /// Size scaling is ranged between [0, 1 - 2*offset], where offset depends on the respective axis (and its offsets).
    /// The axis is assumed to between [0, 1]
    /// Uses ScaleLinear
    /// </summary>
    /// <param name="dataValues"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    private Scale CreateSizeScale(double[] dataValues, float offset)
    {
        List<double> range = new List<double>(2)
        {
            0.0d,                  // smallest size is 0
            1.0d - (offset * 2.0f) // biggest size depends on axis
        };

        List<double> domain = new List<double>(2)
        {
            dataValues.Min(),
            dataValues.Max()
        };

        Scale scaleFunction = new ScaleLinear(domain, range);

        return scaleFunction;
    }

    /// <summary>
    /// Method returns the Scale Function for double Values
    /// Uses ScaleLinear
    /// </summary>
    /// <param name="dataValues"></param>
    /// <param name="rangeVal"></param>
    /// <returns></returns>
    private Scale GetChannelScale(double[] dataValues, double[] rangeVal)
    {
        List<double> range = new List<double>(2)
        {
            rangeVal[0],
            rangeVal[1]
        };

        List<double> domain = new List<double>(2)
        {
            dataValues.Min(),
            dataValues.Max()
        };

        Scale scaleFunction = new ScaleLinear(domain, range);

        return scaleFunction;
    }
    public void SetColorScheme(Color[] colorScheme)
    {
        this.colorScheme = colorScheme;
    }

}
