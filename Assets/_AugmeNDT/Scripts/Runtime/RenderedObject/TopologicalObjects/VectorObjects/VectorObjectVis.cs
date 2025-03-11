using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    public class VectorObjectVis : MonoBehaviour
    {
        // Singleton instance of VectorObjectVis
        public static VectorObjectVis Instance;

        private void Awake()
        {
            // Initialize the singleton instance
            Instance = this;
        }

        /// <summary>
        /// Creates multiple arrows for a list of gradient data points
        /// </summary>
        /// <param name="generatedGradientPoints">List of gradient data points to visualize</param>
        /// <param name="container">Parent transform to place arrows under</param>
        /// <param name="vectorScale">Scale factor for arrows (default 1.0)</param>
        /// <returns>List of created arrow GameObjects</returns>
        public List<GameObject> CreateArrows(List<GradientDataset> generatedGradientPoints, Transform container, float vectorScale = 1f)
        {
            List<GameObject> arrows = new List<GameObject>();
            float maxMag = 0;
            float minMag = 0;

            // Find maximum and minimum magnitude values in the dataset
            generatedGradientPoints.ForEach(x =>
            {
                maxMag = maxMag < x.Magnitude ? x.Magnitude : maxMag;
                minMag = minMag < x.Magnitude ? minMag : x.Magnitude;
            });

            // Create an arrow for each non-zero magnitude point
            generatedGradientPoints.ForEach(x =>
            {
                if (x.Magnitude != 0)
                    arrows.Add(CreateArrow(x.Position, x.Direction, x.Magnitude, maxMag, container, vectorScale));
            });

            return arrows;
        }

        /// <summary>
        /// Creates a single arrow GameObject at specified position and direction
        /// </summary>
        /// <param name="position">Position of the arrow</param>
        /// <param name="direction">Direction of the arrow</param>
        /// <param name="magnitude">Magnitude of the vector (used for coloring)</param>
        /// <param name="maxMag">Maximum magnitude in dataset (for color normalization)</param>
        /// <param name="container">Parent transform for the arrow</param>
        /// <param name="vectorScale">Scale factor for the arrow</param>
        /// <param name="use3D">Whether to use 3D or 2D arrow (default true)</param>
        /// <returns>Created arrow GameObject</returns>
        private GameObject CreateArrow(Vector3 position, Vector3 direction,
            float magnitude, float maxMag, Transform container, float vectorScale, bool use3D = true)
        {
            // Create and name the arrow GameObject
            GameObject arrow = new GameObject("arrow_" + position.ToString());
            arrow.transform.SetParent(container);

            // Add vector component and configure properties
            VectorCreator3D vector = arrow.AddComponent<VectorCreator3D>();
            vector.vectorLength = vector.vectorLength * vectorScale;
            vector.arrowHeadSize = vector.arrowHeadSize * vectorScale;

            // Normalize magnitude for color mapping
            float normalizedMagnitude = Mathf.InverseLerp(0, maxMag, magnitude);

            // Initialize the vector with position, direction and magnitude
            vector.SetVector(position, direction, normalizedMagnitude, maxMag);
            return arrow;
        }
    }
}