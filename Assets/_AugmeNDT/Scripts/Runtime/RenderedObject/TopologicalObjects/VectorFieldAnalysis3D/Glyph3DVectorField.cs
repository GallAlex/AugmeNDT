using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the visualization of 3D vector fields and critical points using glyphs.
    /// </summary>
    public class Glyph3DVectorField : MonoBehaviour
    {
        public static Glyph3DVectorField instance;

        private static Rectangle3DManager rectangle3DManager;

        // Containers for visualization objects
        private Transform container;
        private List<GameObject> arrows = new List<GameObject>();
        private List<GameObject> spheres = new List<GameObject>();

        private static float localScaleRateTo3DVectorVisualize;
        private Bounds cubeBounds;

        private void Awake()
        {
            // Initialize singleton instance
            if (instance == null)
                instance = this;
        }

        private void Start()
        {
            if (rectangle3DManager == null)
            {
                // Get reference to the Rectangle3DManager instance
                rectangle3DManager = Rectangle3DManager.rectangle3DManager;

                // Set default local scale rate for vector field glyphs
                localScaleRateTo3DVectorVisualize = 0.3f;
            }
        }

        /// <summary>
        /// Visualizes the 3D vector field using arrow glyphs.
        /// </summary>
        public void Visualize()
        {
            List<GradientDataset> gradients = rectangle3DManager.GetGradientPoints();
            ClearArrows();
            SetContainer();

            // Create new arrow glyphs based on gradient points
            arrows = VectorObjectVis.instance.CreateArrows(gradients, container, 
                localScaleRateTo3DVectorVisualize, rectangle3DManager.config.ColorOfVectorObject);

        }

        #region private methods

        /// <summary>
        /// Creates a new container GameObject under the scene objects to hold all 3D vector glyphs.
        /// </summary>
        private void SetContainer()
        {
            if (container != null)
            {
                Destroy(GameObject.Find("3DVectorForce"));
                container = null;
            }
            
            container = new GameObject("3DVectorForce").transform;
            container.transform.SetParent(rectangle3DManager.volumeTransform.parent.transform, worldPositionStays: true);
        }

        /// <summary>
        /// Clears all previously generated arrow glyphs from the scene.
        /// </summary>
        private void ClearArrows()
        {
            arrows.ForEach(x => Destroy(x));
            arrows.Clear();
        }

        #endregion private methods

    }
}
