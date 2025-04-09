using UnityEngine;

namespace AugmeNDT
{
    public class TDAMenu : MonoBehaviour
    {
        public static TDAMenu instance;
        public GameObject infoPanel;  // Main menu panel

        private void Awake()
        {
            instance = this;
        }

        public void ActivateTDAInfoPanel()
        {
            // SetActive=false to hide LoadPanel
            GameObject dataLoadingMenu = GameObject.Find("DataLoadingMenu");
            dataLoadingMenu.SetActive(false);

            InitialTopologicalGameObjects();

            infoPanel.SetActive(true);
        }

        private void InitialTopologicalGameObjects()
        {
            Transform sceneObjects = GameObject.Find("Scene Objects").transform;

            GameObject vectorFieldObject = new GameObject("VectorFieldObject");
            vectorFieldObject.transform.parent = sceneObjects;
            vectorFieldObject.AddComponent<VectorObjectVis>();
            vectorFieldObject.AddComponent<VectorFieldObjectVis>();
            vectorFieldObject.AddComponent<CreateCriticalPoints>();
            vectorFieldObject.AddComponent<CriticalPointObjectVis>();

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
        }
    }
}
