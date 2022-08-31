//using UnityEditor;
using System.IO;
using System;
using UnityEngine;
using SFB;
using System.Data;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using static UnityEngine.EventSystems.EventTrigger;
using System.Collections.Generic;


#if !UNITY_EDITOR && UNITY_WSA_10_0
using Windows.Storage;
using Windows.Storage.Pickers;
#endif

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
public class FileLoadingManager: MonoBehaviour
{
    private string filePath = "";
    private string fileName = "";
    private bool loadingSucceded = false;

    //List<FileLoader> entities = new List<FileLoader>(); // get with var mhdFileLoader = entities.OfType<MhdFileLoader>();
    private FileLoader loaderFactory;
    private VoxelDataset dataset;
    private VolumeRenderedObject renderedVolume;

    #region Getter/Setter
    public FileLoader LoaderFactory { get => loaderFactory; set => loaderFactory = value; }
    public VoxelDataset Dataset { get => dataset; set => dataset = value; }
    public VolumeRenderedObject RenderedVolume { get => renderedVolume; set => renderedVolume = value; }
    #endregion
    
    public async Task<String> loadData()
    {
        #if !UNITY_EDITOR && UNITY_WSA_10_0
                Debug.Log("HOLOLENS 2 PICKER");
                FilePicker_Hololens();
            
        #endif

        #if UNITY_EDITOR
                Debug.Log("UNITY_STANDALONE PICKER");
                FilePicker_Win();
        #endif
        return filePath;
    }

    private async Task loadDataset()
    {
        if (filePath == "")
        {
            filePath = "No Data";
            Debug.LogError("Failed to import datset");
            return;
        }

        fileName = Path.GetFileNameWithoutExtension(filePath);
        DatasetType fileTyp = GetDatasetType(filePath);

        //Choose Loader here
        switch (fileTyp)
        {
            case DatasetType.Raw:
                loadingSucceded = await CreateRawLoader();
                break;
            case DatasetType.Mhd:
                loaderFactory = new MhdFileLoader(filePath);
                loadingSucceded = true;
                break;
            case DatasetType.Csv:
                loaderFactory = new CsvLoader();
                loadingSucceded = true;
                break;
            case DatasetType.DICOM:
                throw new NotImplementedException(fileTyp.ToString() + " extension is currently not supported");
            case DatasetType.Unknown:
                throw new NotImplementedException(fileTyp.ToString() + " extension is currently not supported");
            default:
                return;
        }

        if (!loadingSucceded) return;

        Debug.Log("LoadData");
        await Task.Run(() => loaderFactory.LoadData(filePath));
        Debug.Log("Create Voxel Dataset");
        loaderFactory.CreateDataset();

        dataset = loaderFactory.voxelDataset;

        if (dataset != null)
        {
            //Render GameObject
            GameObject volume = new GameObject("VolumeRenderedObject_" + dataset.datasetName);
            renderedVolume = volume.AddComponent<VolumeRenderedObject>();

            Debug.Log("Create Volume Object");
            await renderedVolume.CreateObject(dataset);
            // Save the texture to your Unity Project
            //AssetDatabase.CreateAsset(dataset.GetDataTexture(), "Assets/Textures/Example3DTexture.asset");

        }
        else
        {
            Debug.LogError("Failed to import dataset");
            return;
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
            case ".zraw":
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

    private async Task<bool> CreateRawLoader()
    {
        GameObject rawFileWindowUI = Instantiate((GameObject)Resources.Load("Prefabs/RawFileWindow"));
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
        Destroy(rawFileWindowUI);

        return startImport;
    }

#if !UNITY_EDITOR && UNITY_WSA_10_0
    private async Task FilePicker_Hololens()
    {

        UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
            {
                var filepicker = new FileOpenPicker();
                filepicker.FileTypeFilter.Add("*");
                filepicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                //filepicker.FileTypeFilter.Add(".txt");

                //if (multiSelection)
                //{
                //    IReadOnlyList<StorageFile> files = await filePicker.PickMultipleFilesAsync();
                //    UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                //    {
                //        UWPFilesSelected(files);
                //    }, true);
                //}

                var file = await filepicker.PickSingleFileAsync();

                UnityEngine.WSA.Application.InvokeOnAppThread(async () =>
                {
                    filePath = (file != null) ? file.Path : "Nothing selected";
                    Debug.Log("Hololens 2 Picker Path = " + filePath);
                    //await loadDataset();
                    await loadDataset();

                }, true);
            }, false);
    }
#endif


#if UNITY_EDITOR
    private async Task FilePicker_Win()
    {

        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
        filePath = paths[0];
        Debug.Log("WIN Picker Path = " + filePath);
        Task asyncTask = loadDataset();
        //StartProgressIndicator(asyncTask);
        await asyncTask;

        //StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "", false, (string[] paths) => { filePath = paths[0]; });

    }
#endif

}
