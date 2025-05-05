using Microsoft.MixedReality.SceneUnderstanding;
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
                rectangleManagers2D.AddComponent<StreamlineFlowObjectManager2D>();
                rectangleManagers2D.AddComponent<Glyph2DVectorField>();
                rectangleManagers2D.AddComponent<DuplicateStreamLine2D>();
            }

            // AllVectorField: Vectors and CriticalPoints
            // SubRegion: Vectors and CriticalPoints
            if (topologyConfigData.SceneNumber == 3 || topologyConfigData.SceneNumber == 4 ||
                topologyConfigData.SceneNumber == 5 || topologyConfigData.SceneNumber == 6 )
            {
                GameObject rectangleManagers3D = new GameObject("RectangleManagers3D");
                rectangleManagers3D.transform.parent = sceneObjects;
                rectangleManagers3D.AddComponent<Rectangle3DManager>();

                rectangleManagers3D.AddComponent<Glyph3DVectorField>();
                rectangleManagers3D.AddComponent<CriticalPoint3DVis>();
            }

            // AllVectorField: StreamLine, Flow and CriticalPoints
            // SubRegion: StreamLine, Flow and CriticalPoints
            if (topologyConfigData.SceneNumber == 7 || topologyConfigData.SceneNumber == 8 ||
                topologyConfigData.SceneNumber == 9 || topologyConfigData.SceneNumber == 10)
            {
                GameObject rectangleManagers3D = new GameObject("RectangleManagers3D");
                rectangleManagers3D.transform.parent = sceneObjects;
                rectangleManagers3D.AddComponent<Rectangle3DManager>();

                rectangleManagers3D.AddComponent<CriticalPoint3DVis>();
                rectangleManagers3D.AddComponent<StreamLine3D>();
                rectangleManagers3D.AddComponent<FlowObject3DManager>();
            }

            // It will draw a line when you touch the critical points.
            if (topologyConfigData.SceneNumber == 11)
            {
                GameObject rectangleManagers3D = new GameObject("RectangleManagers3D");
                rectangleManagers3D.transform.parent = sceneObjects;
                rectangleManagers3D.AddComponent<Rectangle3DManager>();

                rectangleManagers3D.AddComponent<CriticalPoint3DVis>();
                rectangleManagers3D.AddComponent<StreamLine3D>();
                rectangleManagers3D.AddComponent<FlowObject3DManager>();
            }

            // SUB_REGION_It will draw a line when you touch the critical points.
            if (topologyConfigData.SceneNumber == 12)
            {
                GameObject rectangleManagers3D = new GameObject("RectangleManagers3D");
                rectangleManagers3D.transform.parent = sceneObjects;
                rectangleManagers3D.AddComponent<Rectangle3DManager>();

                rectangleManagers3D.AddComponent<CriticalPoint3DVis>();
                rectangleManagers3D.AddComponent<StreamLine3D>();
                rectangleManagers3D.AddComponent<FlowObject3DManager>();
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

            // AllVectorField: Vectors and CriticalPoints
            if (topologyConfigData.SceneNumber == 3 || topologyConfigData.SceneNumber == 4)
            {
                if (topologyConfigData.SceneNumber == 4)
                    MakeTransparent();

                Rectangle3DManager.rectangle3DManager.InitializeRectangle();
                Glyph3DVectorField.instance.Visualize();
                CriticalPoint3DVis.instance.Visualize();
            }

            // SubRegion: Vectors and CriticalPoints
            if (topologyConfigData.SceneNumber == 5 || topologyConfigData.SceneNumber == 6)
            {
                if (topologyConfigData.SceneNumber == 6)
                    MakeTransparent();

                Rectangle3DManager.rectangle3DManager.useAllData = false;
                Rectangle3DManager.rectangle3DManager.visibleRectangle = true;
                Rectangle3DManager.rectangle3DManager.InitializeRectangle();
                Glyph3DVectorField.instance.Visualize();
                CriticalPoint3DVis.instance.Visualize();
            }

            // AllVectorField: StreamLine, Flow and CriticalPoints
            if (topologyConfigData.SceneNumber == 7 || topologyConfigData.SceneNumber == 8)
            {
                if (topologyConfigData.SceneNumber == 8)
                    MakeTransparent();

                Rectangle3DManager.rectangle3DManager.InitializeRectangle();
                StreamLine3D.Instance.ShowStreamLines();
                CriticalPoint3DVis.instance.Visualize();
                FlowObject3DManager.Instance.StartFlowObject();
            }

            // AllVectorField: StreamLine, Flow and CriticalPoints
            if (topologyConfigData.SceneNumber == 9 || topologyConfigData.SceneNumber == 10)
            {
                if (topologyConfigData.SceneNumber == 10)
                    MakeTransparent();

                Rectangle3DManager.rectangle3DManager.useAllData = false;
                Rectangle3DManager.rectangle3DManager.visibleRectangle = true;
                Rectangle3DManager.rectangle3DManager.InitializeRectangle();
                StreamLine3D.Instance.ShowStreamLines();
                CriticalPoint3DVis.instance.Visualize();
                FlowObject3DManager.Instance.StartFlowObject();
            }

            // It will draw a line when you touch the critical points.
            if (topologyConfigData.SceneNumber == 11 )
            {
                Rectangle3DManager.rectangle3DManager.InitializeRectangle();
            }

            // SUB_REGION_It will draw a line when you touch the critical points.
            if (topologyConfigData.SceneNumber == 12)
            {
                Rectangle3DManager.rectangle3DManager.useAllData = false;
                Rectangle3DManager.rectangle3DManager.visibleRectangle = true;
                Rectangle3DManager.rectangle3DManager.InitializeRectangle();
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
