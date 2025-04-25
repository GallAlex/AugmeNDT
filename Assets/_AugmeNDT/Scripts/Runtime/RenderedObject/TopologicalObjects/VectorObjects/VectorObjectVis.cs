using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    public class VectorObjectVis : MonoBehaviour
    {
        public static VectorObjectVis instance;

        private void Awake()
        {
            instance = this; // Singleton setup for global access
        }

        /// <summary>
        /// Creates visual arrow representations for a list of gradient vectors.
        /// </summary>
        /// <param name="generatedGradientPoints">List of gradient data</param>
        /// <param name="container">Parent transform to organize created arrows</param>
        /// <returns>List of created arrow GameObjects</returns>
        public List<GameObject> CreateArrows(List<GradientDataset> generatedGradientPoints, Transform container, float scaleFactor = 1.0f)
        {
            List<GameObject> vectors = new List<GameObject>();

            foreach (GradientDataset gradient in generatedGradientPoints)
            {

                if (gradient.Magnitude == 0)
                    continue; // Skip zero-magnitude vectors to avoid unnecessary visuals

                GameObject vector = new GameObject("arrow_" + gradient.Position.ToString());
                vectors.Add(CreateVector(gradient, vector, scaleFactor));
                vector.transform.SetParent(container, true);
            }

            return vectors;
        }

        /// <summary>
        /// Attaches a vector visualization component and initializes it.
        /// </summary>
        private GameObject CreateVector(GradientDataset gradient, GameObject vector, float scaleFactor)
        {
            VectorCreator3D vectorCreator = vector.AddComponent<VectorCreator3D>();
            if (scaleFactor != 1.0f)
            {
                vectorCreator.vectorLength = vectorCreator.vectorLength * scaleFactor;
                vectorCreator.lineWidth = vectorCreator.lineWidth * scaleFactor;
                vectorCreator.arrowHeadSize = vectorCreator.arrowHeadSize * scaleFactor;
            }

            vectorCreator.SetVector(gradient.Position, gradient.Direction); // Configure direction and position
            return vector;
        }
    }
}