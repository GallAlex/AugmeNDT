using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Creates the Unit Container for Visualizations
/// </summary>
public class VisContainer
{
    public GameObject visContainer;
    public GameObject axisContainer;
    public GameObject gridContainer;
    public GameObject dataMarkContainer;

    private float worldSpaceWidth = 1.0f;   // X
    private float worldSpaceHeight = 1.0f;  // Y
    private float worldSpaceLength = 1.0f;  // Z

    //TODO: Make Properties of VisContainer as scriptableObject which can be set in Editor?
    // Main Container Elements
    public List<DataAxis> dataAxisList;     // Axes with Ticks
    public List<DataGrid> dataGridList;     // Grids
    public List<DataMark> dataMarkList;     // Data Marks
    public const float axisOffset = 0.1f;
    public const int xyzTicks = 10;


    public GameObject CreateVisContainer(string visName)
    {
        dataAxisList = new List<DataAxis>();
        dataGridList = new List<DataGrid>();

        visContainer = new GameObject(visName);
        axisContainer = new GameObject("Axes");
        gridContainer = new GameObject("Grids");
        dataMarkContainer = new GameObject("Data Marks");

        //Add Childs
        axisContainer.transform.parent = visContainer.transform;
        gridContainer.transform.parent = visContainer.transform;
        dataMarkContainer.transform.parent = visContainer.transform;

        return visContainer;
    }

    //TODO: Convert values to 0 to 1 here?
    //TODO: Select how many Axes
    public void CreateAxis(string axisLabel, Direction axisDirection)
    {
        DataAxis axis = new DataAxis();
        axis.CreateAxis(axisContainer.transform, axisLabel, axisDirection, xyzTicks);

        dataAxisList.Add(axis);
    }

    public void CreateGrid(Direction axis1, Direction axis2)
    {
        DataGrid grid = new DataGrid();
        Direction[] axisDirections = { axis1, axis2 };

        grid.CreateGrid(gridContainer.transform, worldSpaceWidth, worldSpaceHeight, axisDirections, xyzTicks, xyzTicks);

        dataGridList.Add(grid);
    }

    public void CreateDataMark(DataMark.Channel channel)
    {
        DataMark dataMark = new DataMark();
        dataMark.CreateDataMark(dataMarkContainer.transform, channel);

        //dataMarkList.Add(dataMark);
    }

    private void CalculateSpacing(Direction axisDirection)
    {
        // Based on Data which is assigned to Axis define gridSize/numberOfTicks
        // dataAxisList.at(0) has DataAttribut "A1" with minValue & maxValue and numberOfTicks
        // VisContainer can supplay ranges from respective Axis to different VisCalculation (Bar Chart, Line Chart,...)
        // TODO: Should the range Calculation be in own class which is added to VisContainer/VisCalculation? Should a scriptable class store it?

    }

}
