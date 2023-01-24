using System;
using System.Threading.Tasks;
using UnityEngine;
using SFB;
using UnityEditor;
using System.IO;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using Windows.Storage;
using Windows.Storage.Pickers;
#endif

/// <summary>
/// Class handles the loaded datasets
/// </summary>
public class SceneFileHandler : MonoBehaviour
{
    [SerializeField]
    private VoxelDataset volumeDataset;
    [SerializeField]
    private PolyFiberData polyFiberDataset;
    
    
    // FileLoadingManager handles the loading of the data
    private FileLoadingManager fileLoadingManager;
    private string filePath;
    
    void Awake()
    {
        // Initialize the FileLoadingManager
        fileLoadingManager = new FileLoadingManager();
    }

    public async Task<string> LoadData()
    {
        string path = await StartPicker();
        polyFiberDataset = fileLoadingManager.PolyDataset;
        
        return path;
    }

    public FileLoadingManager GetFileLoadingManager()
    {
        return fileLoadingManager;
    }


    #region FilePickerMethods

    public async Task<String> StartPicker()
    {
        #if !UNITY_EDITOR && UNITY_WSA_10_0
            Debug.Log("HOLOLENS 2 PICKER");
            await FilePicker_Hololens();

        #endif

        #if UNITY_EDITOR
            Debug.Log("UNITY_STANDALONE PICKER");
            await FilePicker_Win();
        #endif
            return filePath;
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
                    await fileLoadingManager.loadDataset(filePath);

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
        Task asyncTask = fileLoadingManager.loadDataset(filePath);

        //Task asyncTask = loadDataset();
        //StartProgressIndicator(asyncTask);
        await asyncTask;

        //StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "", false, (string[] paths) => { filePath = paths[0]; });
    }
    #endif

    #endregion
}
