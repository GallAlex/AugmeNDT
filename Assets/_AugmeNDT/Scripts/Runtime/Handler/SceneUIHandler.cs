using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

namespace AugmeNDT{
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

        public Camera mainCamera;


        private SceneObjectHandler sceneObjectHandler;

        private const int AmountShadertypes = 3;
        private int shaderType = 0;

        private void Update()
        {
        
            //if (Physics.Raycast(mainCamera.transform.position, transform.forward, out var hit, Mathf.Infinity))
            //{
            //    var obj = hit.collider.gameObject;

            //    Debug.Log($"looking at {obj.name}", this);
            //}
        }

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
        
            Task<string> asyncLoadingTask = sceneObjectHandler.LoadObject();

            textLabel.text = "Loading ...";

            //Progress Bar
            //indicator = indicatorObject.GetComponent<IProgressIndicator>();
            //StartProgressIndicator(asyncLoadingTask);

            string path = await asyncLoadingTask;
            textLabel.text = path;
        }

        public void CreateVisualization()
        {
            AbstractDataset abstractDataset = sceneObjectHandler.GetAbstractDataset(0);

            for (int i = 1; i < abstractDataset.attributesCount; i++)
            {
                Dictionary<VisChannel, Attribute> setChannels = new Dictionary<VisChannel, Attribute>();
                setChannels.Add(VisChannel.XPos, abstractDataset.GetAttribute(i));
                sceneObjectHandler.AddAbstractVisObject(0, VisType.Histogram, setChannels);
            }

        }

        public void ChangeVisTicks()
        {

            List<int> selectedGroups = new List<int>();
            //Currently add every loaded group into the selection and create a multigroup
            for (int groupID = 0; groupID < sceneObjectHandler.GetAmountOfDataVisGroups(); groupID++)
            {
                //Debug.Log("Adding Group: " + groupID);
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


    }
}

