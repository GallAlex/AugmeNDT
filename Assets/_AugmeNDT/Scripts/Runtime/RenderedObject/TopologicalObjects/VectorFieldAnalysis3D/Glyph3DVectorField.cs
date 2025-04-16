using Assets.Scripts.DataStructure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the visualization of 3D vector fields and critical points using glyphs
    /// </summary>
    public class Glyph3DVectorField : MonoBehaviour
    {
        public static Glyph3DVectorField instance;

        private static Rectangle3DManager rectangle3DManager;
        private static CreateCriticalPoints createCriticalPointsInstance;
        public bool onlyVisCriticalPoints = false;

        // Containers for visualization objects
        private Transform container;
        private List<GameObject> arrows = new List<GameObject>();
        private List<GameObject> spheres = new List<GameObject>();
        private static float localScaleRateTo3DVectorVisualize;
        private static float localScaleRateTo3DCriticalPointsVisualize;
        private static int arrowsPerFrame = 50;

        private Bounds cubeBounds;
        private bool IsUpdated()
        {
            Bounds currentCubeBounds = rectangle3DManager.GetRectangleBounds();
            if (cubeBounds == currentCubeBounds)
                return false;

            cubeBounds = currentCubeBounds;
            return true;
        }

        private void Awake()
        {
            // Initialize singleton instance
            if (instance == null)
                instance = this;
        }

        public void Start()
        {
            if (rectangle3DManager == null)
            {
                // Get reference to the rectangle manager
                rectangle3DManager = Rectangle3DManager.rectangle3DManager;

                localScaleRateTo3DVectorVisualize = 0.3f;
                localScaleRateTo3DCriticalPointsVisualize = 0.006f;
            }

            createCriticalPointsInstance = CreateCriticalPoints.instance;
        }

        /// <summary>
        /// Displays the vector field visualization using arrows
        /// </summary>
        private void ShowVectorField()
        {
            if (!arrows.Any() || IsUpdated())
            {
                SetContainer();
                ClearArrows();

                List<GradientDataset> gradientPoints = rectangle3DManager.GetGradientPoints();
                if (gradientPoints.Count > 200)
                {
                    StartCoroutine(CreateArrowsCoroutine(gradientPoints));
                }
                else
                {
                    // For a small number of arrows, use the normal method
                    arrows = VectorObjectVis.instance.CreateArrows(gradientPoints, container, localScaleRateTo3DVectorVisualize);
                }
            }
            else
            {
                arrows.ForEach(x => x.SetActive(true));
            }
        }
        /// <summary>
        /// Hides the vector field visualization
        /// </summary>
        private void HideVectorField()
        {
            arrows.ForEach(x => x.SetActive(false));
        }

        private void ShowCriticalPoints(bool force = false)
        {
            if (force || !spheres.Any() || IsUpdated())
            {
                SetContainer();

                ClearCriticalPoints();

                spheres = createCriticalPointsInstance.CreateBasicCriticalPoint(rectangle3DManager.GetCriticalPoints(), container, localScaleRateTo3DCriticalPointsVisualize);
            }
            else
            {
                spheres.ForEach(x => x.SetActive(true));
            }
        }

        public void ShowVectorsAndCriticalPoints(bool force = false)
        {
            if (onlyVisCriticalPoints)
            {
                ShowCriticalPoints(force);
                return;
            }

            bool createNewObjects = force || !arrows.Any() || !spheres.Any() || IsUpdated();
            if (createNewObjects)
            {
                container = null;
                SetContainer();

                ClearArrows();
                ClearCriticalPoints();

                spheres = createCriticalPointsInstance.CreateBasicCriticalPoint(rectangle3DManager.GetCriticalPoints(), container, localScaleRateTo3DCriticalPointsVisualize);
                arrows = VectorObjectVis.instance.CreateArrows(rectangle3DManager.GetGradientPoints(), container, localScaleRateTo3DVectorVisualize);
            }
        }

        #region private

        /// <summary>
        /// Creates a new container GameObject under the fiber object to hold all vector visuals.
        /// </summary>
        private void SetContainer()
        {
            Destroy(GameObject.Find("3DVectorForce"));

            container = new GameObject("3DVectorForce").transform;
            container.transform.parent = rectangle3DManager.volumeTransform;
        }

        /// <summary>
        /// Clears all arrow objects from the scene
        /// </summary>
        private void ClearArrows()
        {
            arrows.ForEach(x => Destroy(x));
            arrows.Clear();
        }

        /// <summary>
        /// Clears all critical point sphere objects from the scene
        /// </summary>
        private void ClearCriticalPoints()
        {
            spheres.ForEach(x => Destroy(x));
            spheres.Clear();
        }

        private IEnumerator CreateArrowsCoroutine(List<GradientDataset> gradientPoints)
        {
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
                List<GameObject> batchArrows = VectorObjectVis.instance.CreateArrows(
                    batchPoints, container, localScaleRateTo3DVectorVisualize);

                // Add to the main list
                arrows.AddRange(batchArrows);

                // Move to the next frame
                yield return null;
            }

            Debug.Log($"Created {arrows.Count} arrow glyphs");
        }

        #endregion private

    }
}