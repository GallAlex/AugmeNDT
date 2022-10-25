using System;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;


public class SceneUIHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject indicatorObject;
    private IProgressIndicator indicator;

    [SerializeField]
    TMP_Text textLabel;

    FileLoadingManager fileLoadingManager;
    public List<Shader> listOfShaders;

    private const int AmountShadertypes = 3;
    private int shaderType = 0;



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
    /// Starts the Loading of Files on the specific device
    /// </summary>
    public async void openFile()
    {
        Debug.Log("Started loading file with ...");
        textLabel.text = "Loading ...";

        fileLoadingManager = new FileLoadingManager();
        Task<string> asyncLoadingTask = fileLoadingManager.loadData();

        //Progress Bar
        indicator = indicatorObject.GetComponent<IProgressIndicator>();
        StartProgressIndicator(asyncLoadingTask);

        string path = await asyncLoadingTask;
        textLabel.text = path;
    }

    public void createVisualization()
    {
        //Todo: Resize Vis?

        //VisContainer vis = new VisContainer();
        //vis.CreateVisContainer("Basic Euclidean Space");
        ////vis.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        //vis.CreateAxis("X Label", Direction.X);
        //vis.CreateAxis("Y label", Direction.Y);
        //vis.CreateAxis("Z label", Direction.Z);

        //vis.CreateGrid(Direction.X, Direction.Y);
        //vis.CreateGrid(Direction.X, Direction.Z);
        //vis.CreateGrid(Direction.Y, Direction.Z);

        //for (float x = 0; x <= 1; x+= 0.01f)
        //{
        //    const float pi = 3.14f;
        //    double y = 0.5 * (1 + Math.Sin(2 * pi * x));
        //    DataMark.Channel channel = new DataMark.Channel
        //    {
        //        position = new Vector3(x, (float)y, 0.5f),
        //        rotation = new Vector3(0, 0, 0),
        //        color = new Vector4((float)y, 0, 0, 1)
        //    };

        //    vis.CreateDataMark(channel);
        //}

        //Data
        var testDat = new Dictionary<string, double[]>()
        {
            { "Val 1", new double[]{1, 2, 3, 4, 5, 6} },
            { "Val 2", new double[]{100, 120, 50, 200, 155, 100} },
            { "Val 3", new double[]{0.01, 0.02, 0.04, 0.06, 0.08, 0.1}}
        };

        Vis vis = new Vis();
        vis.AppendData(testDat);
        vis.CreateVis();

        //Vis vis2 = new Vis();
        //vis2.InitVisParams("Test 2", 2, 0.1f, 0.1f, 0.1f);
        //vis2.CreateVis();
    }

    public void changeShader()
    {
        shaderType = (shaderType + 1) % AmountShadertypes;
        fileLoadingManager.VolumeRenderedObject.ChangeShader(listOfShaders[shaderType]);
    }

    private void ShowDrivesAndFolders()
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

