using System.Collections;
using UnityEngine;

namespace AugmeNDT
{
    public class TDAMenu : MonoBehaviour
    {
        public static TDAMenu instance;
        public GameObject infoPanel;  // Main menu panel
        private TopologyConfigData topologyConfigData;

        private void Awake()
        {
            instance = this;
        }

        public void ActivateTDAInfoPanel(TopologyConfigData config)
        {
            // SetActive=false to hide LoadPanel
            GameObject dataLoadingMenu = GameObject.Find("DataLoadingMenu");
            dataLoadingMenu.SetActive(false);

            topologyConfigData = config;
            InitialTopologicalGameObjects();
        }

        private void InitialTopologicalGameObjects()
        {
            if (topologyConfigData.showTopologicalUI)
                infoPanel.SetActive(true);

            Transform sceneObjects = GameObject.Find("Scene Objects").transform;

            GameObject commonObjectsForTopologicalVis = new GameObject("CommonObjectsForTopologicalVis");
            commonObjectsForTopologicalVis.transform.parent = sceneObjects;
            commonObjectsForTopologicalVis.AddComponent<VectorObjectVis>();
            commonObjectsForTopologicalVis.AddComponent<CreateCriticalPoints>();

            GameObject vectorFieldObject = new GameObject("VectorFieldObject");
            vectorFieldObject.transform.parent = sceneObjects;
            vectorFieldObject.AddComponent<VectorFieldObjectVis>();
            vectorFieldObject.AddComponent<VectorFieldDynamicObjectVis>();
            vectorFieldObject.AddComponent<CriticalPointObjectVis>();
            vectorFieldObject.AddComponent<CriticalPointDynamicObjectVis>();

            GameObject rectangleManagers2D = new GameObject("RectangleManagers2D");
            rectangleManagers2D.transform.parent = sceneObjects;
            rectangleManagers2D.AddComponent<RectangleManager>();
            rectangleManagers2D.AddComponent<Glyph2DVectorField>();
            rectangleManagers2D.AddComponent<StreamLine2D>();
            rectangleManagers2D.AddComponent<FlowObject2DManager>();

            GameObject rectangleManagers3D = new GameObject("RectangleManagers3D");
            rectangleManagers3D.transform.parent = sceneObjects;
            rectangleManagers3D.AddComponent<Rectangle3DManager>();
            rectangleManagers3D.AddComponent<Glyph3DVectorField>();
            rectangleManagers3D.AddComponent<StreamLine3D>();
            rectangleManagers3D.AddComponent<FlowObject3DManager>();

            StartCoroutine(InitializeAfterStart(vectorFieldObject, rectangleManagers2D, rectangleManagers3D));
        }

        private IEnumerator InitializeAfterStart(GameObject vectorFieldObject, GameObject rectangleManagers2D, GameObject rectangleManagers3D)
        {
            // Wait for one frame (all Start methods will be called)
            yield return null;
            //if (topologyConfigData.UseDynamicVectorField)
            //{
            //    vectorFieldObject.GetComponent<CriticalPointDynamicObjectVis>().ShowCriticalPoints();
            //}
            //CriticalPointDynamicObjectVis.instance.ShowCriticalPoints();
            //VectorFieldDynamicObjectVis.instance.ShowVectorField();
        }

    }
}
