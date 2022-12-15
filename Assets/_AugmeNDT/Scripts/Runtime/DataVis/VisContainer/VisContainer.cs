using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Unity.VisualScripting;
using UnityEngine;
using static DataMark;

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

    private const float axisMeshLength = 1.0f;
    private float worldSpaceWidth = 1.0f;   // X
    private float worldSpaceHeight = 1.0f;  // Y
    private float worldSpaceLength = 1.0f;  // Z

    //TODO: Make Properties of VisContainer as scriptableObject which can be set in Editor?
    // Main Container Elements
    public List<DataAxis> dataAxisList;     // Axes with Ticks
    public List<DataGrid> dataGridList;     // Grids
    public List<DataMark> dataMarkList;     // Data Marks

    public bool boundsControl = true;
    public Vector3 xyzOffset;
    public int[] xyzTicks;                     


    #region CREATION OF ELEMENTS

    public GameObject CreateVisContainer(string visName)
    {
        dataAxisList = new List<DataAxis>();
        dataGridList = new List<DataGrid>();
        dataMarkList = new List<DataMark>();

        //visContainer = new GameObject(visName);
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

        return visContainer;
    }

    public void CreateAxis(string axisLabel, Direction axisDirection, Scale dataScale)
    {
        DataAxis axis = new DataAxis();
        //Return Length of current axis

        axis.CreateAxis(axisContainer.transform, axisLabel, axisDirection, dataScale, xyzTicks[(int)axisDirection]);

        dataAxisList.Add(axis);
    }

    //TODO: Create multiple grids on differetn positions (at tick position?)
    public void CreateGrid(Direction axis1, Direction axis2)
    {
        DataGrid grid = new DataGrid();
        Direction[] axisDirections = { axis1, axis2 };

        grid.CreateGrid(gridContainer.transform, worldSpaceWidth, worldSpaceHeight, axisDirections, xyzTicks[(int)axis1] + 1, xyzTicks[(int)axis2] + 1);

        dataGridList.Add(grid);
    }

    public void CreateDataMark(GameObject markPrefab, DataMark.Channel channel)
    {
        DataMark dataMark = new DataMark(markPrefab);
        dataMark.CreateDataMark(dataMarkContainer.transform, channel);

        dataMarkList.Add(dataMark);
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
    /// Defines the tick offest for every Axis
    /// Moves the first tick from the axis origin and the last tick from the end of the axis by by the offset
    /// </summary>
    /// <param name="xyzOffset"></param>
    public void SetAxisOffsets(Vector3 xyzOffset)
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

    public void EnableBoundingBox(bool enable)
    {
        boundsControl = enable;
        BoundingBoxVisibility();
    }


    private Vector3 UpdateAxisLength()
    {
        worldSpaceWidth = axisMeshLength * visContainer.transform.localScale.x;
        worldSpaceHeight = axisMeshLength * visContainer.transform.localScale.y;
        worldSpaceLength = axisMeshLength * visContainer.transform.localScale.z;

        return new Vector3(worldSpaceWidth, worldSpaceHeight, worldSpaceLength);
    }

    private void BoundingBoxVisibility()
    {
        //TODO
    }

    private void CalculateSpacing(Direction axisDirection)
    {
        // Based on Data which is assigned to Axis define gridSize/numberOfTicks
        // dataAxisList.at(0) has DataAttribut "A1" with minValue & maxValue and numberOfTicks
        // VisContainer can supply ranges from respective Axis to different VisCalculation (Bar Chart, Line Chart,...)
        // TODO: Should the range Calculation be in own class which is added to VisContainer/VisCalculation? Should a scriptable class store it?

    }

}
