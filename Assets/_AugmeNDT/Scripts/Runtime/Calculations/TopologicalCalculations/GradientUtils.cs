using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    public static class GradientUtils
    {
        /// <summary>
        /// Parallel implementation: Assigns new gradient values to a list of generated gradient points by interpolating
        /// values from the original dataset. Combines the functionality of AssignNewGradientValues and 
        /// CalculateNewGradientValueByDistanceAvg for better performance.
        /// </summary>
        /// <param name="generatedGradientPoints">List of newly generated points.</param>
        /// <param name="originalGradientList">Original gradient dataset to use for interpolation.</param>
        /// <returns>A list of gradient points with assigned values.</returns>
        public static List<GradientDataset> AssignNewGradientValuesParallel(List<GradientDataset> generatedGradientPoints, List<GradientDataset> originalGradientList)
        {
            if (originalGradientList.Count == 0)
            {
                Debug.LogError("Original gradient list is empty!");
                return generatedGradientPoints;
            }

            // Perform computation for each point in parallel
            Parallel.ForEach(generatedGradientPoints, point =>
            {
                // Re-implementation of CalculateNewGradientValueByDistanceAvg inline
                List<GradientDataset> nearest = new List<GradientDataset>();
                float minDistance = 0.2f;

                // Expand search radius in steps of 0.2f until nearby points are found (max 4 iterations)
                for (int i = 0; i < 4; i++)
                {
                    // Use LINQ instead of ForEach for clarity and efficiency
                    var nearbyPoints = originalGradientList.Where(x =>
                        Vector3.Distance(point.Position, x.Position) <= minDistance);

                    nearest.AddRange(nearbyPoints);

                    if (nearest.Any())
                        break;

                    minDistance += 0.2f;
                }

                // If no nearby gradients are found, set to zero vector
                if (nearest.Count == 0)
                {
                    point.Direction = Vector3.zero;
                    point.Magnitude = 0f;
                    return; // Use return instead of continue in Parallel.ForEach
                }

                // Compute weighted average of neighboring gradients
                Vector3 weightedDirection = Vector3.zero;
                float weightedMagnitude = 0f;
                float totalWeight = 0f;
                float alpha = 2f; // Weighting exponent

                foreach (var data in nearest)
                {
                    float distance = Vector3.Distance(point.Position, data.Position);

                    // If exact match, use it directly
                    if (distance < 1e-6f)
                    {
                        point.Direction = data.Direction;
                        point.Magnitude = data.Magnitude;
                        return;
                    }

                    // Compute inverse-distance weighting
                    float weight = 1f / Mathf.Pow(distance, alpha);
                    totalWeight += weight;

                    weightedDirection += data.Direction * weight;
                    weightedMagnitude += data.Magnitude * weight;
                }

                // If weights sum to zero, assign zero values
                if (totalWeight < 1e-6f)
                {
                    point.Direction = Vector3.zero;
                    point.Magnitude = 0f;
                    return;
                }

                // Normalize weighted results
                point.Direction = (weightedDirection / totalWeight);
                point.Magnitude = (weightedMagnitude / totalWeight);
            });

            return generatedGradientPoints;
        }

        /// <summary>
        /// Projects gradients onto a plane defined by a rectangle and normalizes them using parallel processing.
        /// Ensures that gradients are consistent with the rectangle orientation.
        /// </summary>
        /// <param name="generatedGradientPoints">List of gradient points to be normalized.</param>
        /// <param name="rectangleCorners">Array of four corners defining the rectangle.</param>
        /// <returns>A list of normalized gradient points.</returns>
        public static List<GradientDataset> NormalizeGradientsToRectangleParallel(List<GradientDataset> generatedGradientPoints, Vector3[] rectangleCorners)
        {
            if (!generatedGradientPoints.Any() || rectangleCorners == null || rectangleCorners.Length != 4)
                return generatedGradientPoints;

            // Calculate the normal of the rectangle using different corner pairs for better accuracy
            Vector3 normal1 = Vector3.Cross(rectangleCorners[1] - rectangleCorners[0], rectangleCorners[3] - rectangleCorners[0]).normalized;
            Vector3 normal2 = Vector3.Cross(rectangleCorners[2] - rectangleCorners[1], rectangleCorners[0] - rectangleCorners[1]).normalized;
            Vector3 normal3 = Vector3.Cross(rectangleCorners[3] - rectangleCorners[2], rectangleCorners[1] - rectangleCorners[2]).normalized;
            Vector3 normal4 = Vector3.Cross(rectangleCorners[0] - rectangleCorners[3], rectangleCorners[2] - rectangleCorners[3]).normalized;

            // Average the normals to improve plane estimation
            Vector3 normal = (normal1 + normal2 + normal3 + normal4).normalized;

            // Calculate average gradient direction to determine orientation
            Vector3 averageGradientDirection = new Vector3(
                generatedGradientPoints.Average(x => x.Direction.x),
                generatedGradientPoints.Average(x => x.Direction.y),
                generatedGradientPoints.Average(x => x.Direction.z)
            );

            // Flip the normal if it's pointing in the opposite direction
            float dotGradientNormal = Vector3.Dot(averageGradientDirection, normal);
            if (dotGradientNormal < 0)
            {
                normal = -normal;
            }

            // Compute average magnitude to normalize projection magnitude
            float averageMagnitude = generatedGradientPoints.Average(p => p.Magnitude);

            // Project each gradient onto the plane and normalize it — in parallel
            Parallel.ForEach(generatedGradientPoints, point =>
            {
                // Remove the component of the gradient that's perpendicular to the plane
                Vector3 projectedGradient = point.Direction - Vector3.Dot(point.Direction, normal) * normal;

                // If the projection is too small, discard it
                if (projectedGradient.magnitude < 1e-6f)
                {
                    point.Direction = Vector3.zero;
                    point.Magnitude = 0f;
                    return;
                }

                // Normalize direction and scale magnitude proportionally
                float projectedMagnitude = projectedGradient.magnitude;
                point.Direction = projectedGradient.normalized;

                // Scale magnitude to maintain relative strength
                point.Magnitude = projectedMagnitude / averageMagnitude;
            });

            Debug.Log("Gradients have been projected onto the rectangle plane and normalized (using parallel processing).");
            return generatedGradientPoints;
        }
    }
}
