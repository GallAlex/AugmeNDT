using System;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using SFB;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using System.Collections.Generic;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using Windows.Storage;
using Windows.Storage.Pickers;
#endif

public class SceneUIHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject indicatorObject;
    private IProgressIndicator indicator;

    [SerializeField]
    TMP_Text textLabel;

    public List<Shader> listOfShaders;

    public string path = "";
    public string fileName = "";

    public const int AMOUNT_SHADERTYPES = 3;
    public int shaderType = 0;

    private VoxelDataset dataset;
    private VolumeRenderedObject renderedVolume;

    void Start()
    {
        Debug.Log("DeviceName: " + SystemInfo.deviceName);
        Debug.Log("GraphicsMemorySize: " + (double)SystemInfo.graphicsMemorySize + " MB");
        Debug.Log("MaxTextureSize: " + (double)SystemInfo.maxTextureSize);
        Debug.Log("MaxGraphicsBufferSize: " + (double)SystemInfo.maxGraphicsBufferSize / 1024.0f / 1024.0f + " MB");
    }

    /// <summary>
    /// Toggles the visibility of the given object
    /// </summary>
    /// <param name="volumeObject"></param>
    public void toggleVolumeVisibility(GameObject volumeObject)
    {
        volumeObject.SetActive(!volumeObject.activeSelf);
    }

    /// <summary>
    /// Loads files from the internal storage
    /// Loader depends on System (Hololens2, Windows,...)
    /// </summary>
    /// <param name="textLabel"></param>
    public void openFile()
    {
        Debug.Log("Started loading file with ...");
        textLabel.text = "Loading ...";

        //Progress Bar
        indicator = indicatorObject.GetComponent<IProgressIndicator>();

        #if !UNITY_EDITOR && UNITY_WSA_10_0
            Debug.Log("HOLOLENS 2 PICKER");
            FilePicker_Hololens();
            
        #endif

        #if UNITY_EDITOR
            Debug.Log("UNITY_STANDALONE PICKER");
            FilePicker_Win();
        #endif


        //path = @"C:\Data\Users\alexander.gall@chello.at\Documents\Datasets\Msg1-v2-1mm-nr2-25_5-P4-10_5umVS-1931x156x1702-AD-cutout.mhd";
        //textLabel.text = path;


        // Wait for loading to finish
        //Debug.Log("Path for loading = " + path);
        //textLabel.text = path;
        //loadDataset();

    }

#if !UNITY_EDITOR && UNITY_WSA_10_0
    private void FilePicker_Hololens()
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
                    path = (file != null) ? file.Path : "Nothing selected";
                    fileName = (file != null) ? file.Name : "Nothing selected";
                    Debug.Log("Hololens 2 Picker Path = " + path);
                    //await loadDataset();
                    await loadDataset();

                }, true);
            }, false);



    }
#endif


#if UNITY_EDITOR
    private async void FilePicker_Win()
    {

        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
        path = paths[0];
        Debug.Log("WIN Picker Path = " + path);
        Task asyncTask = loadDataset();
        StartProgressIndicator(asyncTask);
        await asyncTask;
        
        //StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "", false, (string[] paths) => { path = paths[0]; });

    }
#endif

    private async Task loadDataset()
    {
        if (path == "")
        {
            path = "No Data";
            Debug.LogError("Failed to import datset");
            return;
        }

        textLabel.text = path;

        FileLoader fileLoader = new MhdFileLoader(path);
        fileLoader.CreateDataset();
        await Task.Run(()=> fileLoader.LoadData(path));

        dataset = fileLoader.voxelDataset;

        if (dataset != null)
        {
            //Render GameObject
            GameObject volume = new GameObject("VolumeRenderedObject_" + dataset.datasetName);
            renderedVolume = volume.AddComponent<VolumeRenderedObject>();

            renderedVolume.CreateObject(dataset);
            // Save the texture to your Unity Project
            //AssetDatabase.CreateAsset(dataset.GetDataTexture(), "Assets/Textures/Example3DTexture.asset");

        }
        else
        {
            textLabel.text = "Choose File";
            Debug.LogError("Failed to import dataset");
            return;
        }

        
    }

    public void changeShader()
    {
        shaderType = (shaderType + 1) % AMOUNT_SHADERTYPES;
        renderedVolume.ChangeShader(listOfShaders[shaderType]);
    }

    private void showDrivesAndFolders()
    {
        Debug.Log("\nDrives:");

        //### List Drives/Folders ###//
        string[] drives = Directory.GetLogicalDrives();

        foreach (string drive in drives)
        {
            Debug.Log(drive);

            Debug.Log("\nFolder:");
            string[] myDirs = Directory.GetDirectories(drive);

            foreach (var myDir in myDirs)
            {
                Debug.Log(myDir);
            }
        }
        //### List Drives/Folders ###//
    }

    private async void StartProgressIndicator(Task trackedTask)
    {
        indicator.Message = "Opening File...";
        await indicator.OpenAsync();

        indicator.Message = "Waiting for loading to complete...";
        while (!trackedTask.IsCompleted)
        {
            await Task.Yield();
        }

        indicator.Message = "Loading File completed!";
        await indicator.CloseAsync();
    }

}

