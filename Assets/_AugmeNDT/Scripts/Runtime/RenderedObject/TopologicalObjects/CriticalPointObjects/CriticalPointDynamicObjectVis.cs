using Assets.Scripts.DataStructure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Dynamically manages the visualization of critical points based on volume scale.
    /// Adjusts visibility and number of points shown as the volume changes size.
    /// </summary>
    public class CriticalPointDynamicObjectVis : MonoBehaviour
    {
        // Singleton instance
        public static CriticalPointDynamicObjectVis instance;

        // Dictionary to store critical points by type
        private Dictionary<int, List<GameObject>> criticalPointDictionary = new Dictionary<int, List<GameObject>>();

        // Container for all critical point visualizations
        private Transform container;

        // References to required components
        private TopologicalDataObject topologicalDataInstance;
        private CreateCriticalPoints createCriticalPointsInstance;
        private GameObject pointPrefab;

        // Display settings
        [Header("Dynamic Visualization Settings")]
        [Tooltip("Maximum number of critical points to display at full scale")]
        private int maxPointCount = 10000;

        [Tooltip("Minimum number of critical points to display at smallest scale")]
        private int minPointCount = 400;

        [Tooltip("Scale threshold at which to update critical point visualization")]
        private float scaleChangeThreshold = 0.1f;

        [Tooltip("Base scale factor for critical points")]
        private float localScaleFactor = 0.006f;

        // Runtime variables
        private Vector3 lastScale;
        private Transform volumeTransform;
        private bool isInitialized = false;
        private bool isLegendColorBarCreated = false;
        private bool pointsVisible = false;
        private Coroutine updateCoroutine;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);

            // Load the point prefab
            pointPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/TopologicalVis/InteractiveCriticalPointPrefab");
        }

        private void Start()
        {
            // Get reference to the singleton data object
            if (topologicalDataInstance == null)
            {
                topologicalDataInstance = TopologicalDataObject.instance;
                volumeTransform = topologicalDataInstance.volumeTransform;
                lastScale = volumeTransform.lossyScale;

                TopologyConfigData config = topologicalDataInstance.config;
                localScaleFactor = config.cp_dynamic_localScaleFactor;
                maxPointCount = config.cp_dynamic_maxPointCount;
                minPointCount = config.cp_dynamic_minPointCount;
                scaleChangeThreshold = config.cp_dynamic_scaleChangeThreshold;
            }

            if (createCriticalPointsInstance == null)
                createCriticalPointsInstance = CreateCriticalPoints.instance;

            // Create the container
            SetupContainer();

            // Start monitoring scale changes
            StartCoroutine(MonitorVolumeScale());
        }

        /// <summary>
        /// Shows all critical points, initializing visualization if needed
        /// </summary>
        public void ShowCriticalPoints()
        {
            if (!isInitialized)
            {
                UpdateCriticalPointVisualization();
                isInitialized = true;
            }
            else
            {
                // Simply show existing points
                foreach (var pointList in criticalPointDictionary.Values)
                {
                    foreach (GameObject point in pointList)
                    {
                        if (point != null)
                            point.SetActive(true);
                    }
                }
            }

            pointsVisible = true;
        }

        /// <summary>
        /// Hides all critical points without destroying them
        /// </summary>
        public void HideCriticalPoints()
        {
            foreach (var pointList in criticalPointDictionary.Values)
            {
                foreach (GameObject point in pointList)
                {
                    if (point != null)
                        point.SetActive(false);
                }
            }

            pointsVisible = false;
        }

        /// <summary>
        /// Filters critical points by type
        /// </summary>
        public void FilterCriticalPointsByType(int type, bool state)
        {
            if (criticalPointDictionary.ContainsKey(type))
            {
                foreach (GameObject point in criticalPointDictionary[type])
                {
                    if (point != null)
                        point.SetActive(state);
                }
            }
        }

        /// <summary>
        /// Sets up the container for critical point visualization objects
        /// </summary>
        private void SetupContainer()
        {
            container = new GameObject("DynamicCriticalPointObjects").transform;
            container.transform.parent = volumeTransform;
        }

        /// <summary>
        /// Monitors volume scale changes and updates visualization accordingly
        /// </summary>
        private IEnumerator MonitorVolumeScale()
        {
            while (true)
            {
                if (volumeTransform != null && isInitialized)
                {
                    // Check if scale has changed significantly
                    Vector3 currentScale = volumeTransform.lossyScale;
                    float scaleDifference = Vector3.Distance(currentScale, lastScale) / lastScale.magnitude;

                    if (scaleDifference > scaleChangeThreshold)
                    {
                        // Scale has changed enough to warrant updating the visualization
                        Debug.Log($"Volume scale changed significantly: {lastScale} -> {currentScale}");
                        lastScale = currentScale;

                        // Only update if points are visible
                        if (pointsVisible)
                        {
                            Debug.Log("Updating critical point visualization due to scale change");
                            UpdateCriticalPointVisualization();
                        }
                        else
                        {
                            Debug.Log("Critical points not currently visible, scale change noted but visualization not updated");
                        }
                    }
                }

                yield return new WaitForSeconds(0.2f); // Check 5 times per second
            }
        }

        /// <summary>
        /// Updates the critical point visualization based on current volume scale
        /// </summary>
        private void UpdateCriticalPointVisualization()
        {
            if (updateCoroutine != null)
                StopCoroutine(updateCoroutine);

            updateCoroutine = StartCoroutine(UpdateVisualizationCoroutine());
        }

        /// <summary>
        /// Coroutine to update critical point visualization
        /// </summary>
        private IEnumerator UpdateVisualizationCoroutine()
        {
            // Clear existing critical points
            ClearCriticalPoints();

            // Calculate how many points to show based on volume size
            float volumeSize = volumeTransform.lossyScale.magnitude;
            int pointCount = CalculatePointCount(volumeSize);

            // Select critical points to display
            List<CriticalPointDataset> selectedPoints = SelectPointsToDisplay(pointCount);

            // Simply use the existing CreateCriticalPoints functionality
            criticalPointDictionary = createCriticalPointsInstance.CreateInteractiveCriticalPoint(
                selectedPoints,
                container,
                pointPrefab,
                localScaleFactor,
                !isLegendColorBarCreated);

            isLegendColorBarCreated = true;

            yield return null;

            Debug.Log($"Critical point visualization updated with {selectedPoints.Count} points");
        }

        /// <summary>
        /// Calculates how many critical points to show based on volume size
        /// </summary>
        private int CalculatePointCount(float volumeSize)
        {
            // Normalize volume size to 0-1 range (assuming a reasonable range of scales)
            float normalizedSize = Mathf.Clamp01(volumeSize / 10f);

            // Calculate point count that increases with size
            return Mathf.FloorToInt(Mathf.Lerp(minPointCount, maxPointCount, normalizedSize));
        }

        /// <summary>
        /// Selects which critical points to display
        /// </summary>
        private List<CriticalPointDataset> SelectPointsToDisplay(int count)
        {
            if (topologicalDataInstance == null || topologicalDataInstance.criticalPointList == null ||
                topologicalDataInstance.criticalPointList.Count == 0)
            {
                Debug.LogWarning("No critical point data available for selection");
                return new List<CriticalPointDataset>();
            }

            List<CriticalPointDataset> allPoints = topologicalDataInstance.criticalPointList;

            // If we need fewer points than available, select a subset
            if (count < allPoints.Count)
            {
                // Strategy: Group by type and select points evenly across types

                // Group points by type
                Dictionary<int, List<CriticalPointDataset>> pointsByType = new Dictionary<int, List<CriticalPointDataset>>();
                foreach (var point in allPoints)
                {
                    if (!pointsByType.ContainsKey(point.Type))
                        pointsByType[point.Type] = new List<CriticalPointDataset>();

                    pointsByType[point.Type].Add(point);
                }

                int typeCount = pointsByType.Count;
                if (typeCount == 0)
                    return new List<CriticalPointDataset>();

                // Calculate points per type, making sure each type gets at least one point
                int pointsPerType = Mathf.FloorToInt((float)count / typeCount);
                int remainingPoints = count - (pointsPerType * typeCount);

                // Final selection
                List<CriticalPointDataset> selectedPoints = new List<CriticalPointDataset>();

                foreach (var typeGroup in pointsByType)
                {
                    int typePointCount = pointsPerType;

                    // Distribute any remaining points
                    if (remainingPoints > 0)
                    {
                        typePointCount++;
                        remainingPoints--;
                    }

                    List<CriticalPointDataset> typePoints = typeGroup.Value;

                    // If we need fewer points than available for this type, sample them evenly
                    if (typePointCount < typePoints.Count)
                    {
                        // Evenly sample points from this type
                        float step = (float)typePoints.Count / typePointCount;

                        for (float i = 0; i < typePoints.Count && selectedPoints.Count < count; i += step)
                        {
                            selectedPoints.Add(typePoints[Mathf.FloorToInt(i)]);
                        }
                    }
                    else
                    {
                        // Use all points for this type
                        selectedPoints.AddRange(typePoints);
                    }
                }

                return selectedPoints;
            }

            // If we need all points or more than available, return all
            return new List<CriticalPointDataset>(allPoints);
        }

        /// <summary>
        /// Clears all critical point visualization objects
        /// </summary>
        private void ClearCriticalPoints()
        {
            foreach (var pointList in criticalPointDictionary.Values)
            {
                foreach (GameObject point in pointList)
                {
                    if (point != null)
                        Destroy(point);
                }
            }

            criticalPointDictionary.Clear();
        }
    }
}