using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the visualization of critical points in the scene.
    /// Loads critical points and creates interactive GameObjects with appropriate colors.
    /// </summary>
    public class CriticalPoint3DVis : MonoBehaviour
    {
        public static CriticalPoint3DVis instance;
        public Dictionary<int, List<GameObject>> criticalPointDictionary = new Dictionary<int, List<GameObject>>();

        // All visualized points will be parented to this container for better scene organization
        public Transform container;

        private CreateCriticalPoints createCriticalPointsInstance;
        private static Rectangle3DManager rectangle3DManager;

        private GameObject pointPrefab;  // Prefab used for representing a critical point
        private float localScaleRate;
        private Transform sceneObjects;
        private Bounds cubeBounds;
        private int onlyShowThisType = -1;

        private void Awake()
        {
            // Singleton pattern for global access
            instance = this;
            pointPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/TopologicalVis/InteractiveCriticalPointPrefab");
        }

        private void Start()
        {
            if (rectangle3DManager == null)
            {
                // Get reference to the Rectangle3DManager instance
                rectangle3DManager = Rectangle3DManager.rectangle3DManager;
                TopologyConfigData config = rectangle3DManager.config;
                localScaleRate = config.criticalPoints_localScaleRate;
            }

            if (createCriticalPointsInstance == null)
                createCriticalPointsInstance = CreateCriticalPoints.instance;

            // Find the main container for scene objects
            sceneObjects = GameObject.Find("Scene Objects").transform;
        }

        /// <summary>
        /// Filters the critical points displayed based on their type.
        /// </summary>
        /// <param name="buttonIndex">The type index selected from the color bar.</param>
        public void FilterCriticalPointsByType(int buttonIndex)
        {
            Debug.Log($"Color Bar {buttonIndex} pressed!");

            // If the same button is pressed again, show all critical points
            if (onlyShowThisType == buttonIndex)
            {
                SetAllCriticalPointsActive(true);
                onlyShowThisType = -1;
            }
            else
            {
                // Hide all critical points first
                SetAllCriticalPointsActive(false);

                // Then only activate points of the selected type
                if (criticalPointDictionary.ContainsKey(buttonIndex))
                {
                    foreach (var item in criticalPointDictionary[buttonIndex])
                    {
                        item.SetActive(true);
                    }
                }

                onlyShowThisType = buttonIndex;
            }
        }

        /// <summary>
        /// Helper function to set the active state for all critical points.
        /// </summary>
        private void SetAllCriticalPointsActive(bool isActive)
        {
            foreach (var gameObjectList in criticalPointDictionary.Values)
            {
                foreach (var item in gameObjectList)
                {
                    item.SetActive(isActive);
                }
            }
        }

        /// <summary>
        /// Creates and visualizes critical points stored in TopologicalDataObject.
        /// </summary>
        /// <param name="force">Force recreation of critical points even if already created.</param>
        public void Visualize(bool force = false)
        {
            bool createNewObjects = force || criticalPointDictionary.Count == 0 || IsUpdated();
            if (createNewObjects)
            {
                SetContainer();
                criticalPointDictionary = createCriticalPointsInstance.CreateInteractiveCriticalPoint(rectangle3DManager.GetCriticalPoints(), container, pointPrefab, localScaleRate);
                var legend = createCriticalPointsInstance.CreateLegendColorBar(container, FilterCriticalPointsByType, localScaleRate);
            }
        }

        /// <summary>
        /// Checks whether the bounds have changed since last visualization.
        /// </summary>
        private bool IsUpdated()
        {
            Bounds currentCubeBounds = rectangle3DManager.GetRectangleBounds();
            if (cubeBounds == currentCubeBounds)
                return false;

            cubeBounds = currentCubeBounds;
            return true;
        }

        /// <summary>
        /// Creates a new container GameObject under the scene objects to hold all critical point visualizations.
        /// </summary>
        private void SetContainer()
        {
            if (container != null)
            {
                Destroy(container.gameObject);
                Destroy(container);
                container = null;
            }

            container = new GameObject("CriticalPointObjects").transform;
            container.transform.SetParent(sceneObjects.transform, worldPositionStays: true);
        }
    }
}
