using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    public static class GradientUtils
    {
        /// <summary>
        /// Computes a new gradient value at a given position by averaging nearby gradients.
        /// Uses a growing search radius to find the nearest gradients.
        /// </summary>
        /// <param name="position">The position where the gradient is needed.</param>
        /// <param name="gradientDatasetList">List of available gradient points.</param>
        /// <returns>A new computed GradientDataset.</returns>
        public static GradientDataset CalculateNewGradientValueByDistanceAvg(Vector3 position,List<GradientDataset> gradientDatasetList)
        {
            List<GradientDataset> nearest = new List<GradientDataset>();
            float minDistance = 0.2f;

            // Expand search radius in steps of 0.2f until a nearby gradient is found (max 4 iterations)
            for (int i = 0; i < 4; i++)
            {
                gradientDatasetList.ForEach(x =>
                {
                    if (Vector3.Distance(position, x.Position) <= minDistance)
                    {
                        nearest.Add(x);
                    }
                });

                if (nearest.Any())
                    break;

                minDistance += 0.2f;
            }

            // If no nearby gradients are found, return a zero vector
            if (nearest.Count == 0)
                return new GradientDataset(-1, position, Vector3.zero, 0f);

            // Compute a weighted gradient based on distance
            return ComputeWeightedGradient(position,nearest);
        }

        /// <summary>
        /// Computes a weighted gradient based on the inverse distance weighting (IDW) method.
        /// Gives more influence to closer gradient points.
        /// </summary>
        /// <param name="position">Target position for interpolation.</param>
        /// <param name="nearest">List of nearest gradient points.</param>
        /// <returns>Interpolated gradient dataset.</returns>
        private static GradientDataset ComputeWeightedGradient(Vector3 position, List<GradientDataset> nearest)
        {
            Vector3 weightedDirection = Vector3.zero;
            float weightedMagnitude = 0f;
            float totalWeight = 0f;
            float alpha = 2f; // Weighting exponent

            foreach (var data in nearest)
            {
                float distance = Vector3.Distance(position, data.Position);

                // If the target position exactly matches a known point, return it directly
                if (distance < 1e-6f)
                {
                    return new GradientDataset(-1, position, data.Direction, data.Magnitude);
                }

                // Compute inverse distance weight
                float weight = 1f / Mathf.Pow(distance, alpha);                
                totalWeight += weight;

                weightedDirection += data.Direction * weight;
                weightedMagnitude += data.Magnitude * weight;
            }

            // If total weight is zero, return zero vector to avoid division by zero
            if (totalWeight < 1e-6f)
            {
                return new GradientDataset(-1, position, Vector3.zero, 0f);
            }

            // Normalize weighted sum
            weightedDirection /= totalWeight;
            weightedMagnitude /= totalWeight;

            return new GradientDataset(-1, position, weightedDirection, weightedMagnitude);
        }

        /// <summary>
        /// Assigns new gradient values to a list of generated gradient points by interpolating
        /// values from the original dataset.
        /// </summary>
        /// <param name="generatedGradientPoints">List of newly generated points.</param>
        /// <param name="orjinalGradientList">Original gradient dataset to use for interpolation.</param>
        /// <returns>A list of gradient points with assigned values.</returns>
        public static List<GradientDataset> AssignNewGradientValues(List<GradientDataset> generatedGradientPoints, List<GradientDataset> orjinalGradientList)
        {
            if (orjinalGradientList.Count == 0)
            {
                Debug.LogError("Original gradient list is empty!");
                return generatedGradientPoints;
            }

            foreach (var point in generatedGradientPoints)
            {
                GradientDataset nearestGradient = GradientUtils.CalculateNewGradientValueByDistanceAvg(point.Position, orjinalGradientList);
                point.Direction = nearestGradient.Direction;
                point.Magnitude = nearestGradient.Magnitude;
            }

            return generatedGradientPoints;
        }

        /// <summary>
        /// Projects gradients onto a plane defined by three points and normalizes them.
        /// Ensures that gradients are consistent with the plane orientation.
        /// </summary>
        /// <param name="generatedGradientPoints">List of gradient points to be normalized.</param>
        /// <param name="point1">First point defining the plane.</param>
        /// <param name="point2">Second point defining the plane.</param>
        /// <param name="point3">Third point defining the plane.</param>
        /// <returns>A list of normalized gradient points.</returns>
        public static List<GradientDataset> NormalizeGradientsToPlane(List<GradientDataset> generatedGradientPoints, Vector3 point1,Vector3 point2,Vector3 point3)
        {
            if (!generatedGradientPoints.Any())
                return generatedGradientPoints;

            // Compute plane normal using cross product
            Vector3 normal = Vector3.Cross(point2 - point1, point3 - point1).normalized;
            float dotGradientNormal = Vector3.Dot(new Vector3(
                generatedGradientPoints.Average(x => x.Direction.x),
                generatedGradientPoints.Average(x => x.Direction.y),
                generatedGradientPoints.Average(x => x.Direction.z)
                ), normal);

            if (dotGradientNormal < 0)
            {
                normal = -normal; // If the dot product is negative, flip the normal direction
            }

            float averageMagnitude = generatedGradientPoints.Average(p => p.Magnitude);
            // Project each gradient onto the plane
            foreach (var point in generatedGradientPoints)
            {
                Vector3 projectedGradient = point.Direction - Vector3.Dot(point.Direction, normal) * normal;

                // If the projected gradient is very small, set it to zero to avoid noise
                if (projectedGradient.magnitude < 1e-6f)
                {
                    point.Direction = Vector3.zero;
                    point.Magnitude = 0f;
                    continue;  // Skip further calculations for this point
                }

                // Normalize direction and scale magnitude proportionally
                float projectedMagnitude = projectedGradient.magnitude; // Correct magnitude
                point.Direction = projectedGradient.normalized;  // Normalize after projection

                // Scale magnitude proportionally based on average
                point.Magnitude = projectedMagnitude / averageMagnitude;
            }

            Debug.Log("Gradients have been projected onto the plane and normalized.");
            return generatedGradientPoints;
        }

    }
}
