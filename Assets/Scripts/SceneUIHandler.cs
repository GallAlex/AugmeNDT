using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using SFB;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
#endif


public class SceneUIHandler : MonoBehaviour
{

    public string path;
    public string name;
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
#if !UNITY_EDITOR && UNITY_WSA_10_0
        Debug.Log("!UNITY_EDITOR && UNITY_WSA_10_0");
        FilePicker_Hololens();
#endif

#if UNITY_EDITOR
        Debug.Log("UNITY_STANDALONE");
        FilePicker_Win();    
        #endif

        textLabel.text = path;
    }


    private void FilePicker_Hololens()
    {

        #if !UNITY_EDITOR && UNITY_WSA_10_0
            Debug.Log("***********************************");
            Debug.Log("File Picker start.");
            Debug.Log("***********************************");

            UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
            {
                var filepicker = new FileOpenPicker();
                filepicker.FileTypeFilter.Add("*");
                //filepicker.FileTypeFilter.Add(".txt");

                var file = await filepicker.PickSingleFileAsync();
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    Debug.Log("***********************************");
                    string name = (file != null) ? file.Name : "No data";
                    Debug.Log("Name: " + name);
                    Debug.Log("***********************************");
                    string path = (file != null) ? file.Path : "No data";
                    Debug.Log("Path: " + path);
                    Debug.Log("***********************************");



                    //This section of code reads through the file (and is covered in the link)
                    // but if you want to make your own parcing function you can 
                    // ReadTextFile(path);
                    //StartCoroutine(ReadTextFileCoroutine(path));

                }, false);
            }, false);


            Debug.Log("***********************************");
            Debug.Log("File Picker end.");
            Debug.Log("***********************************");
        #endif

    }

    private void FilePicker_Win()
    {
#if UNITY_EDITOR
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
        path = paths[0];


        FileLoader fileLoader = new MhdFileLoader(path);
        fileLoader.loadData(path);
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
            Debug.LogError("Failed to import datset");
        }
        #endif
    }
}
