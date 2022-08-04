using System;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using SFB;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using Windows.Storage;
using Windows.Storage.Pickers;
#endif


public class SceneUIHandler : MonoBehaviour
{

    public string path = "";
    public string name = "";

    private VoxelDataset dataset;
    private VolumeRenderedObject renderedVolume;

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
    public void openFile(TMP_Text textLabel)
    {
        Debug.Log("Started loading file with ...");
        textLabel.text = "Loading ...";


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
                //filepicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
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
                    name = (file != null) ? file.Name : "Nothing selected";
                    Debug.Log("Hololens 2 Picker Path = " + path);
                    //await loadDataset();
                    await loadDataset();

                }, true);
            }, false);



    }
#endif


#if UNITY_EDITOR
    private void FilePicker_Win()
    {

        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
        path = paths[0];
        loadDataset();
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

        Debug.Log("Loading File at: " + path);

        FileLoader fileLoader = new MhdFileLoader(path);
        fileLoader.createDataset();
        await Task.Run(()=> fileLoader.loadData(path));

        Debug.Log("Await Loading File finished");
        dataset = fileLoader.voxelDataset;

        if (dataset != null)
        {
            Debug.Log("Rendering File Data");
            //Render GameObject
            GameObject volume = new GameObject("VolumeRenderedObject_" + dataset.datasetName);
            renderedVolume = volume.AddComponent<VolumeRenderedObject>();
            renderedVolume.CreateObject(dataset);
            // Save the texture to your Unity Project
            //AssetDatabase.CreateAsset(dataset.GetDataTexture(), "Assets/Textures/Example3DTexture.asset");

        }
        else
        {
            path = "Failed to import dataset";
            Debug.LogError("Failed to import dataset");
            return;
        }

        Debug.Log(">End of loadDataset()<");
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

}

