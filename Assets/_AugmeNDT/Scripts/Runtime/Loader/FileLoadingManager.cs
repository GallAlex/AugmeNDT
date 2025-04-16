using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;


using SimpleFileBrowser;


#if !UNITY_EDITOR && UNITY_WSA_10_0
using Windows.Storage;
using Windows.Storage.Pickers;
#endif


namespace AugmeNDT
{

    /// <summary>
    /// Concrete class for loading a file based on its extension and selects the appropriate loader (factory) for it.
    /// Loader depends on System (Hololens2, Windows,...)
    /// </summary>
    public class FileLoadingManager
    {
        public enum DatasetType
        {
            Primary,
            Secondary,
            Unknown
        }

        private bool loadingSucceded = false;

        //List<FileLoader> entities = new List<FileLoader>(); // get with var mhdFileLoader = entities.OfType<MhdFileLoader>();
        private FileLoader loaderFactory;

        private string filePath = "";
        //private string fileName = "";

        private DataVisGroup dataVisGroup;

        #region Getter/Setter
        public FileLoader LoaderFactory { get => loaderFactory; set => loaderFactory = value; }
        #endregion


        public async Task<bool> LoadDataset()
        {
            try
            {
                if (filePath == "")
                {
                    filePath = "No Data";
                    Debug.LogError("Failed to import dataset");
                    return false;
                }

                //fileName = Path.GetFileNameWithoutExtension(filePath);
                FileExtension fileTyp = GetDatasetType(filePath);

                //Choose Loader here
                switch (fileTyp)
                {
                    case FileExtension.Raw:
                        loadingSucceded = await CreateRawLoader(filePath);
                        break;
                    case FileExtension.Mhd:
                        loaderFactory = new MhdFileLoader(filePath);
                        loadingSucceded = true;
                        break;
                    case FileExtension.Csv:
                        loaderFactory = new CsvLoader();
                        loadingSucceded = true;
                        break;
                    case FileExtension.DICOM:
                        loadingSucceded = false;
                        throw new NotImplementedException(fileTyp.ToString() + " extension is currently not supported");
                    case FileExtension.Unknown:
                        loadingSucceded = false;
                        throw new NotImplementedException(fileTyp.ToString() + " extension is currently not supported");
                    default:
                        return false;
                }

                if (!loadingSucceded) return false;

                Debug.Log("LoadData...");
                await Task.Run(() => loaderFactory.LoadData(filePath));

                // Create new group for the loading action
                dataVisGroup = new DataVisGroup();

                StoreDataVisGroup();

                //TODO: Create and return one single Dataset class for primary and secondary data?

            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            return loadingSucceded;
        }

        /// <summary>
        /// Returns the detected extension type of the file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static FileExtension GetDatasetType(string filePath)
        {
            FileExtension datasetType;

            // Get .* extension
            string extension = Path.GetExtension(filePath);

            switch (extension)
            {
                case ".raw":
                    datasetType = FileExtension.Raw;
                    break;
                case ".mhd":
                    datasetType = FileExtension.Mhd;
                    break;
                case ".csv":
                    datasetType = FileExtension.Csv;
                    break;
                case ".dicom":
                case ".dcm":
                    datasetType = FileExtension.DICOM;
                    break;
                default:
                    datasetType = FileExtension.Unknown;
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
                loaderFactory = new RawFileLoader(filePath, rawFileWindow.XDim, rawFileWindow.YDim, rawFileWindow.ZDim, 1, 1,1, rawFileWindow.DataFormat, rawFileWindow.Endianness, rawFileWindow.BytesToSkip);
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
        private void StoreDataVisGroup()
        {
            // Primary Datasets
            if ((loaderFactory.datasetType == DatasetType.Primary))
            {
                if (loaderFactory.voxelDataset != null) dataVisGroup.SetVoxelData(loaderFactory.voxelDataset);
            }
            // Secondary Datasets
            else if ((loaderFactory.datasetType == DatasetType.Secondary))
            {

                if (loaderFactory.secondaryDataType == ISecondaryData.SecondaryDataType.Abstract)
                {
                    if (loaderFactory.abstractDataset != null) dataVisGroup.SetAbstractCsvData(loaderFactory.abstractDataset);
                }
                if (loaderFactory.secondaryDataType == ISecondaryData.SecondaryDataType.Spatial)
                {
                    if (loaderFactory.polyFiberDataset != null)
                    {
                        dataVisGroup.SetPolyData(loaderFactory.polyFiberDataset);
                        dataVisGroup.SetAbstractCsvData(loaderFactory.polyFiberDataset.ExportForDataVis());
                    }
                }
            }
            // Unknown
            else
            {
                Debug.LogError("No Primary or Secondary Data detected");
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

        public void SetFilePath(string filePath)
        {
            this.filePath = filePath;
        }


        #region FilePickerMethods

        public async Task<String> StartPicker()
        {
            filePath = ""; //Clear filePath

#if !UNITY_EDITOR && UNITY_WSA_10_0
                Debug.Log("HOLOLENS 2 PICKER");
                return await FilePicker_Hololens();

#endif

#if !UNITY_EDITOR && UNITY_ANDROID
                Debug.Log("Android PICKER");
                return await FilePicker_Android();
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
        
            return "";
        }
#endif


#if !UNITY_EDITOR && UNITY_ANDROID

        /// <summary>
        /// Request the storage permission during Runtime for Android
        /// </summary>
        private void RequestStoragePermission()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) ||
                !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
        }

        private async Task<String> FilePicker_Android()
        {
            RequestStoragePermission();

            // Set Storage Permission in Magic Leap Settings (on device) then use the following path works
            //filePath = "/storage/emulated/0/Datasets/10min_01.csv";
            //return filePath;
            
            filePath = await ShowFileBrowser();
            Debug.Log("ANDROID Picker Path = " + filePath);

            return filePath;
        }

        public async Task<String> ShowFileBrowser()
        {
            var tcs = new TaskCompletionSource<string>();

            // Get SimpleFileBrowserVRCanvas GamObject
            FileBrowser.CustomPrefabName = "Prefabs/UIPrefabs/SimpleFileBrowserVRCanvas";

            FileBrowser.SetFilters(true, new FileBrowser.Filter("Volumes", ".mhd", ".raw"), new FileBrowser.Filter("Datasets", ".csv"));
            //FileBrowser.SetDefaultFilter(".jpg");
            FileBrowser.AddQuickLink("Datasets", "/storage/emulated/0/Datasets/", null);
            // Show the file
            string initialPath = "/storage/emulated/0/Datasets/";
            FileBrowser.ShowLoadDialog((paths) =>
                {
                    if (paths.Length > 0)
                    {
                        string selectedFile = paths[0]; // Takes the first selected file

                        UnityEngine.Debug.Log("Selected file: " + selectedFile);
                        filePath = selectedFile;
                        tcs.SetResult(filePath);
                    }
                },
                () => {
                    UnityEngine.Debug.Log("File selection cancelled");
                    tcs.SetResult("");
                },  // Canceled
                FileBrowser.PickMode.Files, false, initialPath, null, "Select a file", "Load");

            return await tcs.Task;
        }

#endif


#if UNITY_EDITOR

        private async Task<String> FilePicker_Win()
        {
            /*
            // FileBrowser Solution
            var tcs = new TaskCompletionSource<string>();
            MonoBehaviour mono = GameObject.FindAnyObjectByType<MonoBehaviour>();
            mono.StartCoroutine(ShowLoadDialogCoroutine(tcs));

            filePath = await tcs.Task;
            */

            string path = EditorUtility.OpenFilePanel("Open File...", "", "");
            if (path.Length != 0)
            {
                filePath = path;
            }

            Debug.Log("WIN Picker Path = " + filePath);

            return filePath;
        }

        IEnumerator ShowLoadDialogCoroutine(TaskCompletionSource<string> tcs)
        {
            FileBrowser.CustomPrefabName = "Prefabs/UIPrefabs/SimpleFileBrowserVRCanvas";

            // Show a load file dialog and wait for a response from user
            // Load file/folder: file, Allow multiple selection: true
            // Initial path: default (Documents), Initial filename: empty
            // Title: "Load File", Submit button text: "Load"
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, null, null, "Select Files", "Load");

            // Dialog is closed
            // Print whether the user has selected some files or cancelled the operation (FileBrowser.Success)
            Debug.Log(FileBrowser.Success);

            if (FileBrowser.Success)
            {
                OnFilesSelected(FileBrowser.Result, tcs); // FileBrowser.Result is null, if FileBrowser.Success is false
            }
            else
            {
                tcs.SetResult("");
            }

        }

        void OnFilesSelected(string[] filePaths, TaskCompletionSource<string> tcs)
        {
            // Get the file path of the first selected file
            filePath = filePaths[0];
            tcs.SetResult(filePath);
        }
        

#endif

        #endregion

    }
}
