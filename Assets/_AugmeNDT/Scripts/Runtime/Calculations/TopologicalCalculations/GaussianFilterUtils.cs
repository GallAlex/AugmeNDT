using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{

    public static class GaussianFilterUtils
    {
        /// <summary>
        /// Applies Gaussian smoothing to the given list of gradient points.
        /// This method reduces noise by averaging nearby gradients with a Gaussian weight function.
        /// </summary>
        /// <param name="generatedGradientPoints">List of gradient points to be smoothed.</param>
        /// <param name="gaussianSigma">The standard deviation (sigma) of the Gaussian kernel.</param>
        /// <returns>A new list of smoothed gradient points.</returns>
        public static List<GradientDataset> ApplyGaussianSmoothing(List<GradientDataset> generatedGradientPoints, float gaussianSigma)
        {
            List<GradientDataset> smoothedGradientPoints = new List<GradientDataset>();

            foreach (var point in generatedGradientPoints)
            {
                // Find nearby points within a radius of 1.0f using a search method.
                List<GradientDataset> neighbors = SpatialCalculations.GetNearbyPoints(generatedGradientPoints, point.Position, 1.0f);

                // Skip if no neighbors are found (avoid division by zero later).
                if (neighbors.Count == 0) continue;

                Vector3 weightedDirection = Vector3.zero;
                float weightedMagnitude = 0f;
                float totalWeight = 0f;

                // Apply Gaussian weight function to each neighboring point.
                foreach (var neighbor in neighbors)
                {
                    // Compute Euclidean distance between the point and its neighbor.
                    float distance = Vector3.Distance(point.Position, neighbor.Position);

                    // Gaussian weight function: exp(-d^2 / (2 * sigma^2))
                    float weight = Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * Mathf.Pow(gaussianSigma, 2)));
                    
                    // Accumulate weighted values
                    weightedDirection += neighbor.Direction * weight;
                    weightedMagnitude += neighbor.Magnitude * weight;
                    totalWeight += weight;
                }

                // Normalize weighted sum to prevent bias
                if (totalWeight > 0)
                {
                    weightedDirection /= totalWeight;
                    weightedMagnitude /= totalWeight;
                }

                // Add the smoothed gradient point to the new list
                smoothedGradientPoints.Add(new GradientDataset(point.ID, point.Position, weightedDirection.normalized, weightedMagnitude));
            }

            Debug.Log("Gaussian Kernel Smoothing completed.");
            return smoothedGradientPoints;
        }
    }
}
