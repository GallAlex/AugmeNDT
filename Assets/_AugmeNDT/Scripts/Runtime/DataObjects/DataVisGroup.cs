using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// Class groups & stores all loaded data & its derived representations for one (related/matching) dataset.
/// A group can consist of different data files like raw files and spatial & abstract data (csv)
/// Possible representations include volume renderings, polygonal renderings, as well as data visualizations derived from them 
/// </summary>
[Serializable]
public class DataVisGroup
{
    private static int IDCounter = 0;

    // Unique number which explicitly identifies the data and its representations. 
    private readonly int ID;


    #region DATASETS

    // Dataset for Volumes consiting of Voxels
    private VoxelDataset voxelData;

    //Todo: Use IPolygonDataset instead of PolyFiberData (for this use only infor which is relevant for drawing and sav the rest into csvAbstractData)
    // Dataset for spatial Polygons consiting of Fibers (derived from csv files)
    private PolyFiberData polyData;

    private Dictionary<string, double[]> csvAbstractData;

    #endregion


    #region REPRESENTATIONS

    // Parent Container which stores all representations of the group
    [SerializeField]
    private GameObject dataVisGroupContainer;

    // Representation for Dataset of Volumes consiting of Voxels
    [SerializeField]
    private VolumeRenderedObject volumeRenObj;

    // Representation for Dataset of spatial Polygons consiting of Fibers
    [SerializeField]
    private PolyFiberRenderedObject polyFiberRenObj;

    // Representation for Abstract Dataset from csv files and derived from csv files of polygonal data 
    [SerializeField]
    private List<Vis> visualizations;

    #endregion

    [SerializeField]
    private bool hasVolumeDataset = false;
    [SerializeField]
    private bool hasPolygonalDataset = false;
    [SerializeField]
    private bool hasAbstractDataset = false;

    /// <summary>
    /// Generates a new DataVis group with continuous increasing ID
    /// </summary>
    public DataVisGroup()
    {
        ID = IDCounter++;

        dataVisGroupContainer = new GameObject("DataVisGroup_" + ID);
        visualizations = new List<Vis>();
    }


    #region Getter/Setter

    /// <summary>
    /// Returns unique identifier of the group
    /// </summary>
    /// <returns></returns>
    public int GetGroupID()
    {
        return ID;
    }

    /// <summary>
    /// Returns the parent container of all group representations
    /// </summary>
    /// <returns></returns>
    public GameObject GetGroupContainer()
    {
        return dataVisGroupContainer;
    }
    
    /// <summary>
    /// Store volume dataset
    /// </summary>
    /// <param name="data"></param>
    public void SetVoxelData(VoxelDataset data)
    {
        voxelData = data;
        hasVolumeDataset = true;
    }

    /// <summary>
    /// Gets the stored volume dataset
    /// </summary>
    public VoxelDataset GetVoxelData()
    {
        if (hasVolumeDataset && voxelData != null)
        {
            return voxelData;
        }
        Debug.LogError("No Voxel Dataset stored in this group");
        return null;
    }

    /// <summary>
    /// Stores the drawable spatial csv polygon data
    /// </summary>
    /// <param name="data"></param>
    public void SetPolyData(PolyFiberData data)
    {
        polyData = data;
        hasAbstractDataset = true;
        hasPolygonalDataset = true;
    }

    /// <summary>
    /// Gets the stored drawable spatial csv polygon data
    /// </summary>
    public PolyFiberData GetPolyData()
    {
        if (hasPolygonalDataset && polyData != null)
        {
            return polyData;
        }
        Debug.LogError("No drawable polygon data stored in this group");
        return null;
    }

    /// <summary>
    /// Stores the csv data used in abstract visualizations
    /// </summary>
    /// <param name="data"></param>
    public void SetAbstractCsvData(Dictionary<string, double[]> data)
    {
        csvAbstractData = data;
        hasAbstractDataset = true;
    }

    /// <summary>
    /// Gets the stored csv data used in abstract visualizations
    /// </summary>
    public Dictionary<string, double[]> GetAbstractCsvData()
    {
        if (hasAbstractDataset && csvAbstractData != null)
        {
            return csvAbstractData;
        }
        Debug.LogError("No csv data stored in this group");
        return null;
    }

    #endregion

    protected bool Equals(DataVisGroup other)
    {
        return ID == other.ID;
    }

    #region Rendering

    /// <summary>
    /// Renders Volume Objects from Voxel datasets
    /// </summary>
    public void RenderVolumeObject()
    {
        if (!hasVolumeDataset || voxelData == null)
        {
            Debug.LogError("Error with volume vis data");
            return;
        }

        Debug.Log("Render Volume Object");

        volumeRenObj = new VolumeRenderedObject();
        volumeRenObj.SetDataVisGroup(this);
        volumeRenObj.CreateObject(dataVisGroupContainer, voxelData);

        // Save the texture of volume in this Group (in Unity Project?)
        //AssetDatabase.CreateAsset(dataset.GetDataTexture(), "Assets/Textures/Example3DTexture.asset");
    }

    /// <summary>
    /// Renders Polygonal Objects from derived csv files (with spatial informations)
    /// </summary>
    public void RenderPolyObject()
    {
        if (!hasPolygonalDataset || polyData == null)
        {
            Debug.LogError("Error with spatial vis data");
            return;
        }

        Debug.Log("Render Poly Object");

        polyFiberRenObj = new PolyFiberRenderedObject();
        polyFiberRenObj.SetDataVisGroup(this);
        polyFiberRenObj.CreateObject(dataVisGroupContainer, polyData);
    }

    /// <summary>
    /// Renders Abstract Visualization Objects from csv files
    /// </summary>
    public void RenderAbstractVisObject(VisType visType)
    {
        if (!hasAbstractDataset || csvAbstractData == null)
        {
            Debug.LogError("Error with abstract vis data");
            return;
        }

        Debug.Log("Render Abstract Vis Object");

        Vis vis = Vis.GetSpecificVisType(visType);
        visualizations.Add(vis);

        vis.AppendData(csvAbstractData);
        vis.SetDataVisGroup(this);
        vis.CreateVis(dataVisGroupContainer);
    }

    /// <summary>
    /// Render all representations of the group, for which data is available.
    /// </summary>
    /// <param name="visType"></param>
    public void RenderAll(VisType visType)
    {
        if (hasVolumeDataset) RenderVolumeObject();
        if (hasPolygonalDataset) RenderPolyObject();
        if (hasAbstractDataset) RenderAbstractVisObject(visType);
    }

    #endregion


    /// <summary>
    /// Arranges all representations of the group in a [2 by n] grid and moves it to the best initial start position
    /// </summary>
    public void ArrangeObjectsSpatially()
    {
        GridObjectCollection gridColl = dataVisGroupContainer.AddComponent<GridObjectCollection>();
        gridColl.CellWidth = 0.28f;     //Todo: Use biggest dimension of all representations
        gridColl.CellHeight = 0.28f;    //Todo: Use biggest dimension of all representations
        gridColl.SortType = CollationOrder.ChildOrder;
        gridColl.Layout = LayoutOrder.ColumnThenRow;
        gridColl.Columns = 2;
        gridColl.Anchor = LayoutAnchor.BottomLeft;
        gridColl.AnchorAlongAxis = true;
        
        gridColl.UpdateCollection();

        GlobalScaleAndPos.SetToBestInitialStartPos(dataVisGroupContainer.transform);
    }


    //#####################     VOLUME METHODS      #####################

    public void ChangeVolumeShader(Shader shader)
    {
        volumeRenObj.ChangeShader(shader);
    }

    //#####################     POLY MODEL METHODS  #####################

    public void HighlightPolyFibers(List<int> fiberIDs, Color selectedColor)
    {
        polyFiberRenObj.HighlightFibers(fiberIDs, selectedColor);
    }

    //#####################     VIS CHART METHODS   #####################

    public void ChangeAxis(int selectedVis, int axisID, int selectedDimension, int numberOfTicks)
    {
        //TODO: Change int selectedVis to reference of Object during interaction
        visualizations[selectedVis].ChangeAxisAttribute(axisID, selectedDimension, numberOfTicks);
    }

    /// <summary>
    /// Aligns the grid positions of all visualizations to be furthest away from the viewer
    /// </summary>
    /// <param name="selectedVis"></param>
    public void AlignGridPositions()
    {
        foreach (Vis vis in visualizations)
        {
            vis.UpdateVis();
        }
    }

}
