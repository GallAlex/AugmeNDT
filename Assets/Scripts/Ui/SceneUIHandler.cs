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
        Task<string> asyncTask = fileLoadingManager.loadData();

        //Progress Bar
        indicator = indicatorObject.GetComponent<IProgressIndicator>();
        StartProgressIndicator(asyncTask);

        string path = await asyncTask;
        textLabel.text = path;
    }

    public void changeShader()
    {
        shaderType = (shaderType + 1) % AmountShadertypes;
        fileLoadingManager.RenderedVolume.ChangeShader(listOfShaders[shaderType]);
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

