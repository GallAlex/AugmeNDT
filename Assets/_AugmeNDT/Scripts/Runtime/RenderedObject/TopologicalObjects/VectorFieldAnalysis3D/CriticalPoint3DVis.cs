using System.Collections.Generic;
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

        private float localScaleRate;
        private Transform sceneObjects;
        private Bounds cubeBounds;
        private int onlyShowThisType = -1;
        private bool legendCreated = false;
        private GameObject legend;
        
        [Tooltip("Offset position for the duplicate container")]
        // Determines where the duplicate streamlines appear relative to the original
        private Vector3 positionOffset = new Vector3(0.5f, 0f, 0f);

        private void Awake()
        {
            // Singleton pattern for global access
            instance = this;

            // Find the main container for scene objects
            sceneObjects = GameObject.Find("Scene Objects").transform;

            GameObject poolObj = new GameObject("CriticalPointPoolManager");
            poolObj.AddComponent<CriticalPointObjectPool>();
            poolObj.transform.SetParent(sceneObjects);
        }

        private void Start()
        {
            if (rectangle3DManager == null)
            {
                // Get reference to the Rectangle3DManager instance
                rectangle3DManager = Rectangle3DManager.rectangle3DManager;
                TopologyConfigData config = rectangle3DManager.config;
                localScaleRate = 0.004f;
            }

            if (createCriticalPointsInstance == null)
                createCriticalPointsInstance = CreateCriticalPoints.instance;
        }

        public void Visualize(bool force = false)
        {
            bool needsUpdate = force || criticalPointDictionary.Count == 0 || IsUpdated();

            if (needsUpdate)
            {
                // Clear existing visible objects and return them to the pool
                ClearVisualization();

                // Set up the container (update its position instead of recreating)
                SetupContainer();

                // Visualize critical points (using the pool)
                criticalPointDictionary = CreateCriticalPointsUsingPool(rectangle3DManager.GetCriticalPoints(), container, localScaleRate);

                if (legendCreated)
                {
                    // Calculate the right edge based on position and scale
                    float rightEdge = rectangle3DManager.volumeTransform.position.x +
                                     (rectangle3DManager.volumeTransform.localScale.x / 2);

                    legend.transform.position = new Vector3(rightEdge,
                                       rectangle3DManager.volumeTransform.position.y,
                                       rectangle3DManager.volumeTransform.position.z) + positionOffset;

                }
                else
                {
                    legend = createCriticalPointsInstance.CreateLegendColorBar(container, FilterCriticalPointsByType, localScaleRate*2);
                    legendCreated = true;
                }
            }
        }

        private void ClearVisualization()
        {
            // Return all active critical points back to the pool
            foreach (var pointList in criticalPointDictionary.Values)
            {
                foreach (var point in pointList)
                {
                    CriticalPointObjectPool.Instance.ReturnToPool(point);
                }
            }

            criticalPointDictionary.Clear();
        }

        private void SetupContainer()
        {
            if (container == null)
            {
                container = new GameObject("CriticalPointObjects").transform;
                container.SetParent(rectangle3DManager.volumeTransform, worldPositionStays: true);


                Dictionary<int, Color> typeColors = new Dictionary<int, Color>()
                    {
                        { 0, rectangle3DManager.config.sinkColor },              // Minimum
                        { 1, rectangle3DManager.config.saddle1_PointColor },     // 1-Saddle
                        { 2, rectangle3DManager.config.saddle2_PointColor },     // 2-Saddle
                        { 3, rectangle3DManager.config.sourcePointColor },       // Maximum
                    };

                createCriticalPointsInstance.CustomizeTypeColors(typeColors);
            }
        }

        private Dictionary<int, List<GameObject>> CreateCriticalPointsUsingPool(List<CriticalPointDataset> criticalPoints, Transform container, float localScaleRate)
        {
            Dictionary<int, List<GameObject>> result = new Dictionary<int, List<GameObject>>();

            foreach (var point in criticalPoints)
            {
                // Get an object from the pool
                GameObject obj = CriticalPointObjectPool.Instance.GetPooledObject();

                // Configure the object
                obj.transform.SetParent(container, true);
                obj.transform.position = point.Position;  // world position olarak ayarla
                obj.transform.localScale = Vector3.one * localScaleRate;
                obj.name = $"InteractiveCriticalPoint_{point.ID}";

                // Set the renderer
                obj.GetComponent<Renderer>().material.color = createCriticalPointsInstance.GetColorByType(point.Type);

                // Set the critical point component
                InteractiveCriticalPoint cp = obj.GetComponent<InteractiveCriticalPoint>();
                cp.pointID = point.ID;
                cp.pointType = point.Type;
                cp.pointPosition = point.Position;

                // Add to the dictionary
                if (!result.ContainsKey(point.Type))
                {
                    result[point.Type] = new List<GameObject>();
                }
                result[point.Type].Add(obj);
            }

            return result;
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

    }
}