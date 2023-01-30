using System;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;
using UnityEngine;

/// <summary>
/// Class handels the interactions with global GUI elements inside the scene (Windows, Buttons,...)
/// </summary>
public class SceneUIHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject indicatorObject;
    private IProgressIndicator indicator;

    [SerializeField]
    public TMP_Text textLabel;

    public ScrollGridUtility scrollGridUtility;

    public List<Shader> listOfShaders;

    private SceneObjectHandler sceneObjectHandler;

    private const int AmountShadertypes = 3;
    private int shaderType = 0;
    private int ticks = -1;
    private int attribute = 0;

    
    /// <summary>
    /// Sets a reference to the sceneVisHandler
    /// </summary>
    /// <param name="sceneVisHandler"></param>
    public void SetSceneObjectHandler(SceneObjectHandler handler)
    {
        sceneObjectHandler = handler;
    }

    /// <summary>
    /// Toggles the visibility of the given object
    /// </summary>
    /// <param name="volumeObject"></param>
    public void ToggleObjectVisibility(GameObject volumeObject)
    {
        volumeObject.SetActive(!volumeObject.activeSelf);
    }

    /// <summary>
    /// Starts the Loading of Files on the specific device
    /// </summary>
    public async void OpenFile()
    {
        Debug.Log("Started loading file with ...");
        textLabel.text = "Loading ...";

        Task<string> asyncLoadingTask = sceneObjectHandler.loadObject();
        
        //Progress Bar
        //indicator = indicatorObject.GetComponent<IProgressIndicator>();
        //StartProgressIndicator(asyncLoadingTask);

        string path = await asyncLoadingTask;
        textLabel.text = path;
    }

    public void CreateVisualization()
    {
        attribute++;
        attribute = attribute % 8;

        sceneObjectHandler.ChangeAxis(0, 0, 1, attribute, 5);
    }

    public void ChangeVisTicks()
    {

        List<int> selectedGroups = new List<int>();
        //Currently add every loaded group into the selection and create a multigroup
        for (int groupID = 0; groupID < sceneObjectHandler.GetAmountOfDataVisGroups(); groupID++)
        {
            selectedGroups.Add(groupID);
        }

        //Creates a MultiGroup
        sceneObjectHandler.CreateMultiGroup(selectedGroups);
        sceneObjectHandler.RenderAbstractVisObjectForMultiGroup();
        
    }

    public void FillGridObject()
    {
        /*
    
        Dictionary<string, double[]> dataVal = sceneObjectHandler.visObjectList[0].dataValues;
        scrollGridUtility.FillScrollGrid(dataVal.Keys.ToList());

        //// Load Button Prefab
        //GameObject buttonPrefab = (GameObject)Resources.Load("Prefabs/Button_32x96");

        //Debug.Log("visList count: " + fileLoadingManager.visList.Count);
        ////TODO: Change to dynamic loading based on looked at Vis
        //Dictionary<string, double[]> dataVal = fileLoadingManager.visList[0].dataValues;

        //// Create Buttons for each Letter and add them to the Grid
        //for (int i = 0; i < dataVal.Count; i++)
        //{
        //    GameObject buttonInstance = Instantiate(buttonPrefab, gridObjectContainer.transform);
        //    buttonInstance.name = dataVal.ElementAt(i).Key;
        //    buttonInstance.transform.SetParent(gridObjectContainer.transform, false);
        //    buttonInstance.GetComponentInChildren<TextMeshPro>().text = dataVal.ElementAt(i).Key;
        //}

        //// Add all the element's renderers to the clipping box
        //Renderer[] renderersToClip = gridObjectContainer.GetComponentsInChildren<Renderer>();
        //for (int i = 0; i < renderersToClip.Length; i++)
        //{
        //    clippingBox.AddRenderer(renderersToClip[i]);
        //}

        //gridObjectContainer.GetComponentInChildren<GridObjectCollection>().UpdateCollection();
        //scrollingObjectColl.UpdateContent();
        
        */
    }

    public void ChangeShader()
    {
        shaderType = (shaderType + 1) % AmountShadertypes;
        sceneObjectHandler.ChangeVolumeShader(0, listOfShaders[shaderType]);
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

    private Dictionary<string, double[]> Generate3DSinData()
    {
        Dictionary<string, double[]> data = new Dictionary<string, double[]>();
 
        List<double> xValues = new List<double>();
        List<double> yValues = new List<double>();
        List<double> zValues = new List<double>();

        for (double x = 0; x <= 1; x += 0.01f)
        {
            for (double z = 0; z <= 1; z += 0.01f)
            {
                const double pi = 3.14f;
                //2D: y = 0.5 * (1 + Math.Sin(2 * pi * x))
                xValues.Add(x);
                yValues.Add(1 + Math.Sin(4 * pi * x));
                //yValues.Add(Math.Sin(Math.Sqrt(x + z)));
                zValues.Add(z);

            }

        }

        data.Add("X", xValues.ToArray());
        data.Add("Y", yValues.ToArray());
        data.Add("Z", zValues.ToArray());

        Debug.Log("Number of DataMarks: " + xValues.Count);

        return data;
    }

    private Dictionary<string, double[]> Generate2DSinData()
    {
        Dictionary<string, double[]> data = new Dictionary<string, double[]>();

        List<double> xValues = new List<double>();
        List<double> yValues = new List<double>();

        for (double x = 0; x <= 1; x += 0.001f)
        {
            const double pi = 3.14f;
            //2D: y = 0.5 * (1 + Math.Sin(2 * pi * x))
            xValues.Add(x);
            yValues.Add(0.5 * (1 + Math.Sin(4 * pi * x)));
        }

        data.Add("X", xValues.ToArray());
        data.Add("Y", yValues.ToArray());

        Debug.Log("Number of DataMarks: " + xValues.Count);

        return data;
    }

}

