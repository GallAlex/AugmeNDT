using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// It manages the visualization of critical points in the scene.
    /// Loads critical points and creates interactive GameObjects with appropriate colors.
    /// </summary>
    public class CriticalPointObjectVis : MonoBehaviour
    {
        public static CriticalPointObjectVis instance;
        public Dictionary<int, List<GameObject>> criticalPointDictionary = new Dictionary<int, List<GameObject>>();

        public Transform container; // All visualized points will be parented to this container for better scene organization

        private TopologicalDataObject topologicalDataInstance;
        private CreateCriticalPoints createCriticalPointsInstance;
        private GameObject pointPrefab;  // Prefab for the critical point representation
        private Transform legendColorBar;  
        private static float localScaleRate = 0.006f;

        private void Awake()
        {
            // Singleton assignment for global access
            instance = this;
            pointPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/TopologicalVis/InteractiveCriticalPointPrefab");
        }

        private void Start()
        {
            // Get reference to the singleton data object
            topologicalDataInstance = TopologicalDataObject.instance; 
            createCriticalPointsInstance = CreateCriticalPoints.instance;
        }

        /// <summary>
        /// Filters critical points based on type and user selection.
        /// </summary>
        /// <param name="type">Type ID of the critical point</param>
        /// <param name="state">True to show, False to hide</param>
        public void FilterCriticalPointsByType(int type, bool state)
        {
            if (criticalPointDictionary.ContainsKey(type))
            {
                foreach (GameObject point in criticalPointDictionary[type])
                {
                    point.SetActive(state);
                }
            }
        }

        /// <summary>
        /// Returns the name of the critical point type based on its ID.
        /// </summary>
        /// <param name="type">Integer ID of the critical point type</param>
        /// <returns>String representation of the critical point type</returns>
        public string GetCriticalTypeName(int type)
        {
            switch (type)
            {
                case 0: return "Sink";
                case 1: return "1-Saddle";
                case 2: return "2-Saddle";
                case 3: return "Source";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Creates visualization for all critical points stored in `TopologicalDataObject`.
        /// </summary>
        public void Visualize()
        {
            // Ensure container exists before drawing
            if (container == null)
                SetContainer();

            criticalPointDictionary = createCriticalPointsInstance.CreateInteractiveCriticalPoint(topologicalDataInstance.criticalPointList,container, pointPrefab, localScaleRate);
        }

        public void ShowLegend(bool isOn)
        {
            if (legendColorBar == null)
                legendColorBar = container.Find("Color Scheme");
            
            legendColorBar.gameObject.SetActive(isOn);
        }

        /// <summary>
        /// Creates a new container GameObject under the fiber object to hold all critical point visuals.
        /// </summary>
        private void SetContainer()
        {
            Transform fibers = GameObject.Find("DataVisGroup_0/fibers.raw").transform;

            GameObject criticalPointsObj = new GameObject("CriticalPointObjects");
            container = criticalPointsObj.transform;
            container.SetParent(fibers);
        }
    }

}
