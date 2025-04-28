using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Handles dynamic visualization of vector fields based on volume scale,
    /// displaying vectors proportionally to their magnitude and the current scale of the volume.
    /// </summary>
    public class VectorFieldDynamicObjectVis : MonoBehaviour
    {
        // Singleton instance
        public static VectorFieldDynamicObjectVis instance;

        // Reference to the topological data instance containing gradient data
        private TopologicalDataObject topologicalDataInstance;

        // Reference to the VectorObjectVis instance for arrow creation
        private VectorObjectVis arrowObjectVisInstance;

        // List to store all created arrows
        private List<GameObject> arrows = new List<GameObject>();

        // Parent container for all arrows in the scene
        private Transform container;

        // Flags to track visualization state
        private bool arrowsVisible = false;
        private bool isInitialized = false;

        // Vector field display parameters
        [Header("Vector Field Settings")]
        [Tooltip("Scale factor for arrow size")]
        private float localScaleRate = 0.3f;

        [Tooltip("Maximum number of vectors to display at full scale")]
        private int maxVectorCount = 20000;

        [Tooltip("Minimum number of vectors to display at smallest scale")]
        private int minVectorCount = 1000;

        [Tooltip("Scale threshold at which to update vector visualization")]
        private float scaleChangeThreshold = 0.1f;

        [Tooltip("Density of vectors (higher value = more vectors)")]
        [Range(0.1f, 1.0f)]
        private float vectorDensity = 0.6f;

        // Performance parameters
        [Header("Performance Settings")]
        [Tooltip("Number of arrows to create per frame")]
        private int arrowsPerFrame = 50;

        // Runtime variables
        private Vector3 lastScale;
        private Transform volumeTransform;
        private Coroutine updateCoroutine;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
        }

        void Start()
        {
            if (topologicalDataInstance == null)
            {
                topologicalDataInstance = TopologicalDataObject.instance;
                volumeTransform = topologicalDataInstance.volumeTransform;
                lastScale = volumeTransform.lossyScale;

                TopologyConfigData config = topologicalDataInstance.config;
                localScaleRate = config.localScaleRate;
                arrowsPerFrame = config.arrowsPerFrame;

                maxVectorCount = config.maxVectorCount;
                minVectorCount = config.minVectorCount;
                scaleChangeThreshold = config.scaleChangeThreshold;
                vectorDensity = config.vectorDensity;
            }

            if (arrowObjectVisInstance == null)
                arrowObjectVisInstance = VectorObjectVis.instance;

            // Create the container for arrows
            SetupContainer();

            // Start the scale monitoring coroutine
            StartCoroutine(MonitorVolumeScale());
        }

        /// <summary>
        /// Makes the vector field visible and initializes it if needed.
        /// </summary>
        public void ShowVectorField()
        {
            if (!isInitialized)
            {
                // First-time initialization
                UpdateVectorVisualization();
                isInitialized = true;
            }
            else
            {
                // Just make existing arrows visible
                foreach (var arrow in arrows)
                {
                    if (arrow != null)
                        arrow.SetActive(true);
                }
            }

            arrowsVisible = true;
        }

        /// <summary>
        /// Hides the vector field without destroying the arrows.
        /// </summary>
        public void HideVectorField()
        {
            foreach (var arrow in arrows)
            {
                if (arrow != null)
                    arrow.SetActive(false);
            }

            arrowsVisible = false;
        }

        private void SetupContainer()
        {
            GameObject arrowsContainer = new GameObject("DynamicVectorFieldArrows");
            container = arrowsContainer.transform;
            container.SetParent(volumeTransform);
        }

        /// <summary>
        /// Monitors the fiber.raw object scale and updates the vector field when significant changes occur.
        /// </summary>
        private IEnumerator MonitorVolumeScale()
        {
            while (true)
            {
                if (volumeTransform != null && isInitialized && arrowsVisible)
                {
                    // Check if scale has changed significantly
                    Vector3 currentScale = volumeTransform.lossyScale;
                    float scaleDifference = Vector3.Distance(currentScale, lastScale) / lastScale.magnitude;

                    if (scaleDifference > scaleChangeThreshold)
                    {
                        // Scale has changed enough to warrant updating the visualization
                        Debug.Log($"Object scale changed significantly: {lastScale} -> {currentScale}");
                        lastScale = currentScale;

                        // Cancel any ongoing update
                        if (updateCoroutine != null)
                            StopCoroutine(updateCoroutine);

                        // Only update if arrows are visible
                        if (arrowsVisible)
                        {
                            Debug.Log("Updating vector field visualization due to scale change");
                            // Start a new update
                            updateCoroutine = StartCoroutine(UpdateVectorVisualizationCoroutine());
                        }
                    }
                }

                yield return new WaitForSeconds(0.5f); // Check more frequently (2 times per second)
            }
        }

        /// <summary>
        /// Updates the vector field visualization based on current volume scale.
        /// </summary>
        private void UpdateVectorVisualization()
        {
            if (updateCoroutine != null)
                StopCoroutine(updateCoroutine);

            updateCoroutine = StartCoroutine(UpdateVectorVisualizationCoroutine());
        }

        /// <summary>
        /// Coroutine to update vector visualization over multiple frames.
        /// </summary>
        private IEnumerator UpdateVectorVisualizationCoroutine()
        {
            // Clear existing arrows
            ClearArrows();

            // Calculate how many vectors to show based on current scale
            float volumeSize = volumeTransform.lossyScale.magnitude;
            int vectorCount = CalculateVectorCount(volumeSize);

            Debug.Log($"Updating vector field. Volume size: {volumeSize}, Vector count: {vectorCount}");

            // Select vectors based on magnitude and spatial distribution
            List<GradientDataset> selectedVectors = SelectVectorsToDisplay(vectorCount);

            // Create new arrows over multiple frames
            yield return CreateArrowsOverTime(selectedVectors);

            Debug.Log($"Vector field updated with {arrows.Count} vectors");
        }

        /// <summary>
        /// Calculates how many vectors to show based on volume size.
        /// </summary>
        private int CalculateVectorCount(float volumeSize)
        {
            // Normalize the volume size to a 0-1 range (assuming a reasonable range of scales)
            float normalizedSize = Mathf.Clamp01(volumeSize / 10f);

            // Calculate vector count using a curve that increases with size
            return Mathf.FloorToInt(Mathf.Lerp(minVectorCount, maxVectorCount, normalizedSize * vectorDensity));
        }

        /// <summary>
        /// Selects which vectors to display based on magnitude and spatial distribution.
        /// </summary>
        private List<GradientDataset> SelectVectorsToDisplay(int count)
        {
            if (topologicalDataInstance == null || topologicalDataInstance.gradientList == null || topologicalDataInstance.gradientList.Count == 0)
            {
                Debug.LogWarning("No gradient data available for vector selection");
                return new List<GradientDataset>();
            }

            List<GradientDataset> allVectors = topologicalDataInstance.gradientList;

            // If we need fewer vectors than available, select a subset
            if (count < allVectors.Count)
            {
                // Strategy: Combine magnitude-based selection with spatial distribution

                // Step 1: Sort by magnitude (descending)
                List<GradientDataset> sortedByMagnitude = allVectors
                    .OrderByDescending(v => v.Magnitude)
                    .ToList();

                // Step 2: Take top 25% by magnitude
                int topMagnitudeCount = Mathf.Max(1, count / 4);
                List<GradientDataset> selectedByMagnitude = sortedByMagnitude
                    .Take(topMagnitudeCount)
                    .ToList();

                // Step 3: For remaining vectors, choose spatially distributed ones
                int remainingCount = count - topMagnitudeCount;
                if (remainingCount > 0)
                {
                    // Simple spatial sampling approach: take every Nth vector from the sorted list
                    // after skipping the ones we already took
                    float skipRatio = (float)(sortedByMagnitude.Count - topMagnitudeCount) / remainingCount;
                    List<GradientDataset> spatiallyDistributed = new List<GradientDataset>();

                    for (int i = 0; i < remainingCount; i++)
                    {
                        int index = topMagnitudeCount + Mathf.FloorToInt(i * skipRatio);
                        if (index < sortedByMagnitude.Count)
                            spatiallyDistributed.Add(sortedByMagnitude[index]);
                    }

                    // Combine the two sets
                    selectedByMagnitude.AddRange(spatiallyDistributed);
                }

                return selectedByMagnitude;
            }

            // If we need all vectors or more than available, return all
            return new List<GradientDataset>(allVectors);
        }

        /// <summary>
        /// Creates arrows over multiple frames to prevent performance spikes.
        /// </summary>
        private IEnumerator CreateArrowsOverTime(List<GradientDataset> vectors)
        {
            arrows = new List<GameObject>(vectors.Count);

            for (int i = 0; i < vectors.Count; i += arrowsPerFrame)
            {
                // Process a batch of vectors
                List<GradientDataset> batch = vectors
                    .Skip(i)
                    .Take(Mathf.Min(arrowsPerFrame, vectors.Count - i))
                    .ToList();

                // Create arrows for this batch
                List<GameObject> batchArrows = arrowObjectVisInstance.CreateArrows(
                    batch, container, localScaleRate, topologicalDataInstance.config.ColorOfVectorObject);

                // Add to main list
                arrows.AddRange(batchArrows);

                // Wait for next frame
                yield return null;
            }
        }

        /// <summary>
        /// Clears all existing arrows from the scene.
        /// </summary>
        private void ClearArrows()
        {
            foreach (var arrow in arrows)
            {
                if (arrow != null)
                    Destroy(arrow);
            }

            arrows.Clear();
        }
    }
}