using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Handles the visualization of vector fields using arrow prefabs.
    /// Uses gradient data to create 3D arrows.
    /// </summary>
    public class VectorFieldObjectVis : MonoBehaviour
    {
        // Singleton instance
        public static VectorFieldObjectVis instance;

        // Reference to the topological data instance containing gradient data
        private static TopologicalDataObject topologicalDataInstance;

        // Reference to the VectorObjectVis instance containing gradient data
        private static VectorObjectVis arrowObjectVisInstance;

        // List to store all created arrows for toggling visibility
        private List<GameObject> arrows = new List<GameObject>();

        // Parent container for all arrows in the scene
        private Transform container;
        // Volume Transform to set container's parent
        private static Transform volumeTransform;

        //Scale factor for arrow size
        private float localScaleRate = 0.3f; // default
        //Number of arrows to create per frame
        private int arrowsPerFrame = 50;    // default

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void Start()
        {
            if (topologicalDataInstance == null)
            {
                topologicalDataInstance = TopologicalDataObject.instance;
                volumeTransform = topologicalDataInstance.volumeTransform;

                TopologyConfigData config = topologicalDataInstance.config;
                localScaleRate = config.localScaleRate;
                arrowsPerFrame = config.arrowsPerFrame;
            }

            if (arrowObjectVisInstance == null)
                arrowObjectVisInstance = VectorObjectVis.instance;

            SetContainer();
        }

        /// <summary>
        /// Generates and visualizes the vector field using gradient data.
        /// If arrows are already created, it simply makes them visible.
        /// Otherwise, it generates the arrows from `gradientList` in `TopologicalDataObject`.
        /// </summary>
        public void Visualize()
        {
            StartCoroutine(CreateArrowsCoroutine());
        }

        private IEnumerator CreateArrowsCoroutine()
        {
            List<GradientDataset> gradientPoints = topologicalDataInstance.gradientList;
            // Total number of arrows
            int totalArrows = gradientPoints.Count;
            arrows = new List<GameObject>(totalArrows);

            for (int i = 0; i < totalArrows; i += arrowsPerFrame)
            {
                // Get the subset of gradientPoints to process in this frame
                List<GradientDataset> batchPoints = gradientPoints
                    .Skip(i)
                    .Take(Mathf.Min(arrowsPerFrame, totalArrows - i))
                    .ToList();

                // Create arrows for this batch
                List<GameObject> batchArrows = arrowObjectVisInstance.CreateArrows(
                    batchPoints, container, localScaleRate);

                // Add to the main list
                arrows.AddRange(batchArrows);

                // Move to the next frame
                yield return null;
            }

            Debug.Log($"Created {arrows.Count} arrow glyphs");
        }

        private void SetContainer()
        {
            GameObject generalVectorFieldArrows = new GameObject("GeneralVectorFieldArrows");
            container = generalVectorFieldArrows.transform;
            container.SetParent(volumeTransform,true);
        }
    }
}