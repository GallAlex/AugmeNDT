using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates the Unit Container for Visualizations
/// </summary>
public class VisContainer : MonoBehaviour
{
    public GameObject visContainer;

    private float worldSpaceWidth = 1.0f;   // X
    private float worldSpaceHeight = 1.0f;  // Y
    private float worldSpaceLength = 1.0f;  // Z

    // Main Container Elements
    public List<DataAxis> dataAxisList;  // Axes with Ticks
    public List<DataGrid> dataGridList; // Grids

    public GameObject CreateVisContainer(string visName)
    {
        dataAxisList = new List<DataAxis>();
        visContainer = new GameObject(visName);

        return visContainer;
    }

    //TODO: Convert values to 0 to 1 here?
    //TODO: Select how many Axes
    public void CreateAxis(string axisLabel, Direction axisDirection)
    {
        DataAxis axis = new DataAxis();
        axis.CreateAxis(visContainer.transform, axisLabel, axisDirection);

        dataAxisList.Add(axis);
    }
}
