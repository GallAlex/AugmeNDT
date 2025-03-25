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

        // References to other manager instances
        private static VectorObjectVis arrowObjectVisInstance;
        private static RectangleManager rectangleManager;

        private void Awake()
        {
            // Initialize singleton instance
            if (Instance == null)
                Instance = this;

            // Create container for organizing arrow objects
            container = new GameObject("2DVectorForce").transform;
        }

        private void Start()
        {
            // Get references to required managers
            if (rectangleManager == null)
                rectangleManager = RectangleManager.rectangleManager;

            if (arrowObjectVisInstance == null)
                arrowObjectVisInstance = VectorObjectVis.instance;
        }

        /// <summary>
        /// Creates and displays arrow glyphs to visualize the vector field
        /// </summary>
        public void VisualizePoints()
        {
            // Recreate arrows if none exist or if underlying data has been updated
            if (!generatedGradientPoints.Any() || rectangleManager.IsUpdated())
            {
                generatedGradientPoints = rectangleManager.GetGradientPoints();
                DestroyArrows();
                arrows = arrowObjectVisInstance.CreateArrows(generatedGradientPoints, container);
            }
            else
                ShowHideArrows(true);
        }

        /// <summary>
        /// Shows or hides all arrow glyphs
        /// </summary>
        /// <param name="showArrows">True to show arrows, false to hide them</param>
        public void ShowHideArrows(bool showArrows)
        {
            foreach (var arrow in arrows)
                arrow.SetActive(showArrows);
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