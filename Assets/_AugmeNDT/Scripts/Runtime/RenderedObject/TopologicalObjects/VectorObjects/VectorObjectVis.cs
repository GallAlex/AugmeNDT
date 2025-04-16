using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
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

                GameObject vector = CreateGameObject(gradient.Position.ToString(), container);
                vectors.Add(CreateVector(gradient, vector, scaleFactor));
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

        /// <summary>
        /// Creates a new empty GameObject for a vector and sets transform under the container.
        /// </summary>
        private GameObject CreateGameObject(string position, Transform parentContainer)
        {
            GameObject vector = new GameObject("arrow_" + position.ToString());
            vector.transform.parent = parentContainer;

            // Reset local transform
            //vector.transform.localPosition = Vector3.zero;
            //vector.transform.localRotation = Quaternion.identity;
            //vector.transform.localScale = Vector3.one;

            return vector;
        }
    }
}