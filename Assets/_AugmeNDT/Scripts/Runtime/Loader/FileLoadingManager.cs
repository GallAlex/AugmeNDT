using SimpleFileBrowser;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using Windows.Storage;
using Windows.Storage.Pickers;
#endif

/// <summary>
/// Concrete class for loading a file based on its extension and selects the appropriate loader (factory) for it.
/// Loader depends on System (Hololens2, Windows,...)
/// </summary>
public class FileLoadingManager
{
    private bool loadingSucceded = false;

    //List<FileLoader> entities = new List<FileLoader>(); // get with var mhdFileLoader = entities.OfType<MhdFileLoader>();
    private FileLoader loaderFactory;

    private string filePath = "";
    private DataVisGroup dataVisGroup;

    #region Getter/Setter
    public FileLoader LoaderFactory { get => loaderFactory; set => loaderFactory = value; }
    #endregion


    public async Task LoadDataset()
    {
        try
        {
            if (filePath == "")
            {
                filePath = "No Data";
                Debug.LogError("Failed to import dataset");
                return;
            }

            //fileName = Path.GetFileNameWithoutExtension(filePath);
            DatasetType fileTyp = GetDatasetType(filePath);

            //Choose Loader here
            switch (fileTyp)
            {
                case DatasetType.Raw:
                    loadingSucceded = await CreateRawLoader(filePath);
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

            Debug.Log("LoadData...");
            await Task.Run(() => loaderFactory.LoadData(filePath));

            // Create new group for the loading action
            dataVisGroup = new DataVisGroup();

            StoreDataVisGroup(fileTyp);

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


    /// <summary>
    /// Methods stores the currently loaded datasets in a DataVisGroup
    /// </summary>
    private void StoreDataVisGroup(DatasetType type)
    {

        if ((type == DatasetType.Raw || type == DatasetType.Mhd) && loaderFactory.voxelDataset != null)
        {
            dataVisGroup.SetVoxelData(loaderFactory.voxelDataset);
        }
        if (type == DatasetType.Csv && loaderFactory.polyFiberDataset != null)
        {
            // derived from spatial csv
            dataVisGroup.SetPolyData(loaderFactory.polyFiberDataset);
        }
        if (type == DatasetType.Csv && loaderFactory.polyFiberDataset != null)
        {
            // Todo: Add Enum for spatial csv and abstract csv
            // pure abstract csv
            //dataVisGroup.SetAbstractCsvData(loaderFactory.polyFiberDataset);
        }

    }

    /// <summary>
    /// Returns the most recently DataVisGroup containing the loaded file
    /// </summary>
    /// <returns></returns>
    public DataVisGroup GetDataVisGroup()
    {
        return dataVisGroup;
    }


    #region FilePickerMethods

    public async Task<String> StartPicker()
    {
#if !UNITY_EDITOR && UNITY_WSA_10_0
        Debug.Log("HOLOLENS 2 PICKER");
            return await FilePicker_Hololens();

#endif

#if UNITY_EDITOR
        Debug.Log("UNITY_STANDALONE PICKER");
        return await FilePicker_Win();
#endif

    }


    #if !UNITY_EDITOR && UNITY_WSA_10_0
    private async Task<String> FilePicker_Hololens()
    {
        var completionSource = new TaskCompletionSource<String>();
        
        // Calls to UWP must be made on the UI thread.
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

                //TODO: Currently called after the file picker is closed.
                // Pass back Infos - Calls to Unity must be made on the main thread.
                UnityEngine.WSA.Application.InvokeOnAppThread(async () =>
                {
                    filePath = (file != null) ? file.Path : "";
                    Debug.Log("Hololens 2 Picker Path = " + filePath);

                    // sets the completion source task to finished
                    completionSource.SetResult(filePath);

                }, true);

            }, true);
        
        return await completionSource.Task;
    }
    #endif


    #if UNITY_EDITOR
    private async Task<String> FilePicker_Win()
    {

        string path = EditorUtility.OpenFilePanel("Open File...", "", "");
        if (path.Length != 0)
        {
            filePath = path;
        }

        Debug.Log("WIN Picker Path = " + filePath);

        //var fileBrowser = FileBrowser.ShowLoadDialog((path) => { filePath = path[0]; }, () => { filePath = ""; }, FileBrowser.PickMode.Files, false, null, null, "Open File", "Load");
        //StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "", false, (string[] paths) => { filePath = paths[0]; });

        return filePath;
    }

    #endif

    #endregion

}
