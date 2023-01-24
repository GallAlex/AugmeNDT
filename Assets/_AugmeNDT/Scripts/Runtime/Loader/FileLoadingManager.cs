//using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;



public enum DatasetType
{
    Raw,
    Mhd,
    Csv,
    DICOM,
    Unknown
}

/// <summary>
/// Concrete class for loading a file based on its extension and selects the appropriate loader (factory) for it.
/// Loader depends on System (Hololens2, Windows,...)
/// </summary>
public class FileLoadingManager
{
    private bool loadingSucceded = false;
    bool isPolyObject = false;

    //List<FileLoader> entities = new List<FileLoader>(); // get with var mhdFileLoader = entities.OfType<MhdFileLoader>();
    private FileLoader loaderFactory;
    private VoxelDataset volumeDataset;
    private PolyFiberData polyFiberDataset;

    public List<VolumeRenderedObject> volumeRenderedObjectList;        // Stores all loaded & rendered Volumes
    public List<PolyFiberRenderedObject> polyFiberRenderedObjectList;  // Stores all loaded & rendered Poly Models
    public List<Vis> visList;                                          // Stores all loaded & rendered Visualizations

    #region Getter/Setter
    public FileLoader LoaderFactory { get => loaderFactory; set => loaderFactory = value; }
    public VoxelDataset VolumeDataset { get => volumeDataset; set => volumeDataset = value; }
    public PolyFiberData PolyDataset { get => polyFiberDataset; set => polyFiberDataset = value; }
    #endregion


    public FileLoadingManager()
    {
        volumeRenderedObjectList = new List<VolumeRenderedObject>();
        polyFiberRenderedObjectList = new List<PolyFiberRenderedObject>();
        visList = new List<Vis>();
    }
    

    public async Task loadDataset(string filePath)
    {
        try
        {
            if (filePath == "")
            {
                filePath = "No Data";
                Debug.LogError("Failed to import datset");
                return;
            }

            //fileName = Path.GetFileNameWithoutExtension(filePath);
            DatasetType fileTyp = GetDatasetType(filePath);

            //Choose Loader here
            switch (fileTyp)
            {
                case DatasetType.Raw:
                    loadingSucceded = await CreateRawLoader(filePath);
                    isPolyObject = false;
                    break;
                case DatasetType.Mhd:
                    loaderFactory = new MhdFileLoader(filePath);
                    loadingSucceded = true;
                    isPolyObject = false;
                    break;
                case DatasetType.Csv:
                    loaderFactory = new CsvLoader();
                    loadingSucceded = true;
                    isPolyObject = true;
                    break;
                case DatasetType.DICOM:
                    throw new NotImplementedException(fileTyp.ToString() + " extension is currently not supported");
                case DatasetType.Unknown:
                    throw new NotImplementedException(fileTyp.ToString() + " extension is currently not supported");
                default:
                    return;
            }

            if (!loadingSucceded) return;

            Debug.Log("LoadData...");
            await Task.Run(() => loaderFactory.LoadData(filePath));

            if (!isPolyObject)
            {
                //if (dataset == null)
                //{
                //    Debug.LogError("Failed to import dataset");
                //    return;
                //}
                await RenderVolumeObject();
            }
            else
            {
                await RenderPolyObject();
            }

            //renderContainer.transform.position = new Vector3(-0.2f, 0.1f, 0.5f); // Best pos Hololens

        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

    }

    /// <summary>
    /// Returns the detected extension type of the file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static DatasetType GetDatasetType(string filePath)
    {
        DatasetType datasetType;

        // Get .* extension
        string extension = Path.GetExtension(filePath);

        switch (extension)
        {
            case ".raw":
                datasetType = DatasetType.Raw;
                break;
            case ".mhd":
                datasetType = DatasetType.Mhd;
                break;
            case ".csv":
                datasetType = DatasetType.Csv;
                break;
            case ".dicom":
            case ".dcm":
                datasetType = DatasetType.DICOM;
                break;
            default:
                datasetType = DatasetType.Unknown;
                throw new NotImplementedException("Data extension format [" + extension + "] not supported");
        }

        return datasetType;
    }

    private async Task<bool> CreateRawLoader(string filePath)
    {
        GameObject rawFileWindowUI = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/UIPrefabs/RawFileWindow"));
        RawFileWindow rawFileWindow = rawFileWindowUI.GetComponent<RawFileWindow>();

        bool startImport = await rawFileWindow.WaitForInput();

        if (startImport)
        {
            loaderFactory = new RawFileLoader(filePath, rawFileWindow.XDim, rawFileWindow.YDim, rawFileWindow.ZDim, rawFileWindow.DataFormat, rawFileWindow.Endianness, rawFileWindow.BytesToSkip);
        }
        else
        {
            Debug.LogError("Raw loading canceled");
        }
        GameObject.Destroy(rawFileWindowUI);

        return startImport;
    }

    private async Task RenderVolumeObject()
    {
        volumeDataset = loaderFactory.voxelDataset;
        
        Debug.Log("Create Volume Object");

        //Render Volume Object
        VolumeRenderedObject volumeRenderedObject = new VolumeRenderedObject();
        volumeRenderedObjectList.Add(volumeRenderedObject);

        await volumeRenderedObject.CreateObject(volumeDataset);

        // Save the texture to your Unity Project
        //AssetDatabase.CreateAsset(dataset.GetDataTexture(), "Assets/Textures/Example3DTexture.asset");
    }

    private async Task RenderPolyObject()
    {
        polyFiberDataset = loaderFactory.polyFiberDataset;
        Debug.Log("Create Poly Object");

        //Render Poly Object
        PolyFiberRenderedObject polyFiberRenderedObject = new PolyFiberRenderedObject();
        polyFiberRenderedObjectList.Add(polyFiberRenderedObject);

        // Render Visualization
        Vis vis = new VisMDDGlyphs();
        visList.Add(vis);

        vis.AppendData(polyFiberDataset.ExportForDataVis());
        vis.CreateVis();

        await polyFiberRenderedObject.CreateObject(polyFiberDataset);
    }

}
