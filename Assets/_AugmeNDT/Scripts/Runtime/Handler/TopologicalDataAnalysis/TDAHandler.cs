using System.Collections;
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

            //Loads necessary classes for all scenes
            GameObject commonObjectsForTopologicalVis = new GameObject("CommonObjectsForTopologicalVis");
            commonObjectsForTopologicalVis.transform.parent = sceneObjects;
            commonObjectsForTopologicalVis.AddComponent<VectorObjectVis>();
            commonObjectsForTopologicalVis.AddComponent<CreateCriticalPoints>();

            ManageScenes(sceneObjects);
            StartCoroutine(InitializeAfterStart());
        }

        private void ManageScenes(Transform sceneObjects)
        {
            // Only for fibers.raw dataset
            if (topologyConfigData.SceneNumber == -1)
            {
                return;
            }

            // Dynamic All VectorField
            if (topologyConfigData.SceneNumber == 0)
            {
                GameObject vectorFieldObject = new GameObject("VectorFieldObject");
                vectorFieldObject.transform.parent = sceneObjects;
                vectorFieldObject.AddComponent<VectorFieldDynamicObjectVis>();
            }

            // AllVectorField: Vectors and CriticalPoints
            if (topologyConfigData.SceneNumber == 1 || topologyConfigData.SceneNumber == 2)
            {
                GameObject rectangleManagers3D = new GameObject("RectangleManagers3D");
                rectangleManagers3D.transform.parent = sceneObjects;
                rectangleManagers3D.AddComponent<Rectangle3DManager>();

                rectangleManagers3D.AddComponent<Glyph3DVectorField>();
                rectangleManagers3D.AddComponent<CriticalPoint3DVis>();
            }

            // AllVectorField: StreamLine, Flow and CriticalPoints
            if (topologyConfigData.SceneNumber == 3 || topologyConfigData.SceneNumber == 4)
            {
                GameObject rectangleManagers3D = new GameObject("RectangleManagers3D");
                rectangleManagers3D.transform.parent = sceneObjects;
                rectangleManagers3D.AddComponent<Rectangle3DManager>();

                rectangleManagers3D.AddComponent<Glyph3DVectorField>();
                rectangleManagers3D.AddComponent<CriticalPoint3DVis>();
                rectangleManagers3D.AddComponent<StreamLine3D>();
                rectangleManagers3D.AddComponent<FlowObject3DManager>();
            }

            // Predetermined area: StreamLine, Flow and CriticalPoints
            if (topologyConfigData.SceneNumber == 5 || topologyConfigData.SceneNumber == 6)
            {
                GameObject rectangleManagers3D = new GameObject("RectangleManagers3D");
                rectangleManagers3D.transform.parent = sceneObjects;
                rectangleManagers3D.AddComponent<Rectangle3DManager>();

                rectangleManagers3D.AddComponent<Glyph3DVectorField>();
                rectangleManagers3D.AddComponent<CriticalPoint3DVis>();
                rectangleManagers3D.AddComponent<StreamLine3D>();
                rectangleManagers3D.AddComponent<FlowObject3DManager>();
            }

            // 2D visualizations
            if (topologyConfigData.SceneNumber == 7 || topologyConfigData.SceneNumber == 8)
            {
                GameObject rectangleManagers2D = new GameObject("RectangleManagers2D");
                rectangleManagers2D.transform.parent = sceneObjects;
                rectangleManagers2D.AddComponent<RectangleManager>();

                rectangleManagers2D.AddComponent<Glyph2DVectorField>();
                rectangleManagers2D.AddComponent<StreamLine2D>();
                rectangleManagers2D.AddComponent<FlowObject2DManager>();
            }
        }

        private IEnumerator InitializeAfterStart()
        {
            yield return null;

            // Dynamic All VectorField
            if (topologyConfigData.SceneNumber == 0)
            {
                VectorFieldDynamicObjectVis.instance.ShowVectorField();
            }

            // AllVectorField and CriticalPoints
            if (topologyConfigData.SceneNumber == 1 || topologyConfigData.SceneNumber == 2)
            {
                if (topologyConfigData.SceneNumber == 2)
                    MakeTransparent();

                Rectangle3DManager.rectangle3DManager.InitializeRectangle();
                Glyph3DVectorField.instance.Visualize();
                CriticalPoint3DVis.instance.Visualize(true);
            }

            // AllVectorField: StreamLine, Flow and CriticalPoints
            if (topologyConfigData.SceneNumber == 3 || topologyConfigData.SceneNumber == 4)
            {
                if (topologyConfigData.SceneNumber == 4)
                    MakeTransparent();

                Rectangle3DManager.rectangle3DManager.InitializeRectangle();

                Glyph3DVectorField.instance.Visualize();
                CriticalPoint3DVis.instance.Visualize(true);
                StreamLine3D.Instance.ShowStreamLines(true);
                FlowObject3DManager.Instance.StartFlowObject();
            }

            // Predetermined area: StreamLine, Flow and CriticalPoints
            if (topologyConfigData.SceneNumber == 5 || topologyConfigData.SceneNumber == 6)
            {
                if (topologyConfigData.SceneNumber == 6)
                    MakeTransparent();

                Rectangle3DManager.rectangle3DManager.useAllData = false;
                Rectangle3DManager.rectangle3DManager.visibleRectangle = true;
                Rectangle3DManager.rectangle3DManager.InitializeRectangle();

                Glyph3DVectorField.instance.Visualize();
                CriticalPoint3DVis.instance.Visualize(true);
                StreamLine3D.Instance.ShowStreamLines(true);
                FlowObject3DManager.Instance.StartFlowObject();
            }

            // 2D visualization
            if (topologyConfigData.SceneNumber == 7 || topologyConfigData.SceneNumber == 8)
            {
                if (topologyConfigData.SceneNumber == 8)
                    MakeTransparent();

                RectangleManager.rectangleManager.ShowRectangle();
                Glyph2DVectorField.Instance.ShowArrows();
                StreamLine2D.Instance.ShowStreamLines();
                FlowObject2DManager.Instance.StartFlowObject();
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
