using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the visualization of 2D vector fields using arrow glyphs
    /// </summary>
    public class Glyph2DVectorField : MonoBehaviour
    {
        public static Glyph2DVectorField Instance;

        // List of arrow GameObjects representing vectors
        private List<GameObject> arrows = new List<GameObject>();

        // Cached gradient data for vector field visualization
        private List<GradientDataset> generatedGradientPoints = new List<GradientDataset>();

        // Container for organizing arrow objects in hierarchy
        private Transform container;

        private static float localScaleRate;
        private static int arrowsPerFrame = 50;

        // References to other manager instances
        private static VectorObjectVis arrowObjectVisInstance;
        private static RectangleManager rectangleManager;

        /// <summary>
        /// Initializes the singleton instance
        /// </summary>
        private void Awake()
        {
            // Initialize singleton instance
            if (Instance == null)
                Instance = this;
        }

        /// <summary>
        /// Gets references to required managers and initializes configuration values
        /// </summary>
        private void Start()
        {
            // Get references to required managers
            if (rectangleManager == null)
            {
                rectangleManager = RectangleManager.rectangleManager;
                localScaleRate = rectangleManager.config.Slice2D_VectorSizeRate;
            }

            if (arrowObjectVisInstance == null)
                arrowObjectVisInstance = VectorObjectVis.instance;

        }

        /// <summary>
        /// Makes vector field arrows visible, creating them if necessary
        /// </summary>
        public void ShowArrows()
        {
            bool createNewVectors = !generatedGradientPoints.Any() || !arrows.Any() || rectangleManager.IsUpdated();
            if (createNewVectors)
                VisualizePoints();
            else
            {
                foreach (var arrow in arrows)
                    arrow.SetActive(true);
            }

        }

        /// <summary>
        /// Creates and displays arrow glyphs to visualize the vector field
        /// </summary>
        private void VisualizePoints()
        {
            //Depends on rectangleManager. Therefore it can not call in start()
            SetContainer();

            DestroyArrows();
            generatedGradientPoints = rectangleManager.GetGradientPoints();

            // For a large number of arrows use Coroutine, otherwise use CreateArrows directly
            if (generatedGradientPoints.Count > 100)
                StartCoroutine(CreateArrowsCoroutine());
            else
                arrows = arrowObjectVisInstance.CreateArrows(generatedGradientPoints, container, localScaleRate);
        }

        /// <summary>
        /// Creates arrows over multiple frames to prevent performance spikes
        /// </summary>
        private IEnumerator CreateArrowsCoroutine()
        {
            // Total number of arrows
            int totalArrows = generatedGradientPoints.Count;
            arrows = new List<GameObject>(totalArrows);

            for (int i = 0; i < totalArrows; i += arrowsPerFrame)
            {
                // Get the subset of gradientPoints to process in this frame
                List<GradientDataset> batchPoints = generatedGradientPoints
                    .Skip(i)
                    .Take(Mathf.Min(arrowsPerFrame, totalArrows - i))
                    .ToList();

                // Create arrows for this batch
                List<GameObject> batchArrows = arrowObjectVisInstance.CreateArrows(batchPoints, container, localScaleRate);

                // Add to the main list
                arrows.AddRange(batchArrows);

                // Move to the next frame
                yield return null;
            }

            Debug.Log($"Created {arrows.Count} arrow glyphs");
        }

        /// <summary>
        /// Creates a new container GameObject under the fiber object to hold all vector visuals.
        /// </summary>
        private void SetContainer()
        {
            if (container != null)
                return;

            GameObject _2DVectorForce = new GameObject("2DVectorForce");
            container = _2DVectorForce.transform;
            container.transform.parent = rectangleManager.GetInteractiveRectangleContainer();
        }

        /// <summary>
        /// Destroys all arrow objects and clears the list
        /// </summary>
        private void DestroyArrows()
        {
            arrows.ForEach(x => Destroy(x));
            arrows.Clear(); // Clear the list
        }
    }
}