using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace AugmeNDT
{
    public class TDAHandler : MonoBehaviour
    {
        public static TDAHandler instance;
        private TopologyConfigData topologyConfigData;

        private void Awake()
        {
            instance = this;
        }

        public void ActivateTDAScenes(TopologyConfigData config)
        {
            // SetActive=false to hide LoadPanel
            GameObject dataLoadingMenu = GameObject.Find("DataLoadingMenu");
            dataLoadingMenu.SetActive(false);

            topologyConfigData = config;
            InitialTopologicalGameObjects();
        }

        private void InitialTopologicalGameObjects()
        {
            Transform sceneObjects = GameObject.Find("Scene Objects").transform;
            Transform dataVisGroup = GameObject.Find("DataVisGroup_0").transform;

            // Loads necessary classes for all scenes
            GameObject commonObjectsForTopologicalVis = new GameObject("CommonObjectsForTopologicalVis");
            commonObjectsForTopologicalVis.transform.parent = sceneObjects;
            commonObjectsForTopologicalVis.AddComponent<VectorObjectVis>();
            commonObjectsForTopologicalVis.AddComponent<CreateCriticalPoints>();

            ManageScenes(sceneObjects, dataVisGroup);
            StartCoroutine(InitializeAfterStart(dataVisGroup));
        }

        private void ManageScenes(Transform sceneObjects, Transform dataVisGroup)
        {
            // 2D visualizations
            if (topologyConfigData.SceneNumber == 1 || topologyConfigData.SceneNumber == 2)
            {
                // Create pool for fast rendering
                GameObject poolObj = new GameObject("StreamLineObjectPool");
                poolObj.transform.SetParent(sceneObjects.transform);
                poolObj.AddComponent<StreamLineObjectPool>();

                // Create Interactive object pool for fast rendering
                GameObject interactivePoolObj = new GameObject("InteractiveStreamLineObjectPool");
                interactivePoolObj.transform.SetParent(sceneObjects.transform);
                interactivePoolObj.AddComponent<InteractiveStreamLineObjectPool>();

                // Create main object
                GameObject rectangleManagers2D = new GameObject("RectangleManagers2D");
                rectangleManagers2D.transform.parent = sceneObjects.transform;
                rectangleManagers2D.AddComponent<RectangleManager>();

                // Add essential components
                rectangleManagers2D.AddComponent<StreamLine2D>();
                rectangleManagers2D.AddComponent<FlowObject2DManager>();
                rectangleManagers2D.AddComponent<Glyph2DVectorField>();
                rectangleManagers2D.AddComponent<DuplicateStreamLine2D>();
            }

        }

        private IEnumerator InitializeAfterStart(Transform dataVisGroup)
        {
            yield return null;

            // 2D visualization
            if (topologyConfigData.SceneNumber == 1 || topologyConfigData.SceneNumber == 2)
            {
                if (topologyConfigData.SceneNumber == 2)
                    MakeTransparent();

                RectangleManager.rectangleManager.ShowRectangle();

                StreamLine2D.Instance.Visualize();
                FlowObject2DManager.Instance.StartFlowObject();

                dataVisGroup.AddComponent<PersistenceDiagramGenerator>();
            }
        }

        private void MakeTransparent()
        {
            // Find the volume object and adjust its material to make it transparent
            GameObject volumeObject = GameObject.Find("Volume");
            Renderer renderer = volumeObject.GetComponent<Renderer>();

            if (renderer != null && renderer.material != null)
            {
                Material volumeMaterial = renderer.material;
                if (volumeMaterial.HasProperty("_MinVal"))
                    volumeMaterial.SetFloat("_MinVal", 1);
            }
        }
    }
}
