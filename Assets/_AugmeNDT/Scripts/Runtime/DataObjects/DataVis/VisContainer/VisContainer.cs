using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
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

    
    // Main Container Elements
    public List<DataAxis> dataAxisList;     // Axes with Ticks
    public List<DataGrid> dataGridList;     // Grids
    public List<DataMark> dataMarkList;     // Data Marks

    // Interactor
    private VisInteractor visInteractor;

    private const float axisMeshLength = 1.0f;
    private Bounds containerBounds;            // Width, Height, Length of the Container
    private bool boundsControl = true;
    private float[] xyzOffset;
    private int[] xyzTicks;                     


    #region CREATION OF ELEMENTS

    public GameObject CreateVisContainer(string visName)
    {
        dataAxisList = new List<DataAxis>();
        dataGridList = new List<DataGrid>();
        dataMarkList = new List<DataMark>();

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

    public void CreateAxis(string axisLabel, Direction axisDirection, Scale dataScale)
    {
        DataAxis axis = new DataAxis(xyzOffset[(int)axisDirection]);
        //Return Length of current axis

        axis.CreateAxis(axisContainer.transform, axisLabel, axisDirection, dataScale, xyzTicks[(int)axisDirection]);

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

    public void CreateDataMark(GameObject markPrefab, DataMark.Channel channel)
    {
		//TODO: Performance of adding prefab every time?
        DataMark dataMark = new DataMark(dataMarkList.Count, markPrefab);
        
        dataMark.SetVisInteractor(visInteractor);
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

    private Scale CreateScale(Scale.DataScaleType dataScale, List<double> domain, List<double> range, List<string> names)
    {
        Scale scaleFunction;

        switch (dataScale)
        {
            default:
            case Scale.DataScaleType.Linear:
                scaleFunction = new ScaleLinear(domain, range);
                break;
            case Scale.DataScaleType.Nominal:
                scaleFunction = new ScaleNominal(domain, range, names);
                break;

        }

        return scaleFunction;
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


}
