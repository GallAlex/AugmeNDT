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

            // Paralel olarak her bir point için hesaplama yapacağız
            Parallel.ForEach(generatedGradientPoints, point =>
            {
                // CalculateNewGradientValueByDistanceAvg metodunun içeriğini doğrudan burada uygulayalım
                List<GradientDataset> nearest = new List<GradientDataset>();
                float minDistance = 0.2f;

                // Expand search radius in steps of 0.2f until a nearby gradient is found (max 4 iterations)
                for (int i = 0; i < 4; i++)
                {
                    // ForEach yerine LINQ kullanabiliriz, çünkü originalGradientList sadece okunuyor
                    var nearbyPoints = originalGradientList.Where(x =>
                        Vector3.Distance(point.Position, x.Position) <= minDistance);

                    nearest.AddRange(nearbyPoints);

                    if (nearest.Any())
                        break;

                    minDistance += 0.2f;
                }

                // If no nearby gradients are found, set zero vector
                if (nearest.Count == 0)
                {
                    point.Direction = Vector3.zero;
                    point.Magnitude = 0f;
                    return; // continue in Parallel.ForEach için return kullanılır
                }

                // Compute weighted gradient
                Vector3 weightedDirection = Vector3.zero;
                float weightedMagnitude = 0f;
                float totalWeight = 0f;
                float alpha = 2f; // Weighting exponent

                foreach (var data in nearest)
                {
                    float distance = Vector3.Distance(point.Position, data.Position);

                    // If the target position exactly matches a known point, use it directly
                    if (distance < 1e-6f)
                    {
                        point.Direction = data.Direction;
                        point.Magnitude = data.Magnitude;
                        return; // continue in Parallel.ForEach için return kullanılır
                    }

                    // Compute inverse distance weight
                    float weight = 1f / Mathf.Pow(distance, alpha);
                    totalWeight += weight;

                    weightedDirection += data.Direction * weight;
                    weightedMagnitude += data.Magnitude * weight;
                }

                // If total weight is zero, set zero vector
                if (totalWeight < 1e-6f)
                {
                    point.Direction = Vector3.zero;
                    point.Magnitude = 0f;
                    return; // continue in Parallel.ForEach için return kullanılır
                }

                // Normalize weighted sum
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

            // Calculate the normal of the rectangle using multiple corner combinations for robustness
            Vector3 normal1 = Vector3.Cross(rectangleCorners[1] - rectangleCorners[0], rectangleCorners[3] - rectangleCorners[0]).normalized;
            Vector3 normal2 = Vector3.Cross(rectangleCorners[2] - rectangleCorners[1], rectangleCorners[0] - rectangleCorners[1]).normalized;
            Vector3 normal3 = Vector3.Cross(rectangleCorners[3] - rectangleCorners[2], rectangleCorners[1] - rectangleCorners[2]).normalized;
            Vector3 normal4 = Vector3.Cross(rectangleCorners[0] - rectangleCorners[3], rectangleCorners[2] - rectangleCorners[3]).normalized;

            // Average the normals for a more accurate representation
            Vector3 normal = (normal1 + normal2 + normal3 + normal4).normalized;

            // Calculate average gradient direction
            Vector3 averageGradientDirection = new Vector3(
                generatedGradientPoints.Average(x => x.Direction.x),
                generatedGradientPoints.Average(x => x.Direction.y),
                generatedGradientPoints.Average(x => x.Direction.z)
            );

            // Check if we need to flip the normal to align with average gradient direction
            float dotGradientNormal = Vector3.Dot(averageGradientDirection, normal);
            if (dotGradientNormal < 0)
            {
                normal = -normal; // If the dot product is negative, flip the normal direction
            }

            // Calculate average magnitude for scaling
            float averageMagnitude = generatedGradientPoints.Average(p => p.Magnitude);

            // Project each gradient onto the rectangle plane - PARALLEL IMPLEMENTATION
            Parallel.ForEach(generatedGradientPoints, point =>
            {
                // Project the gradient onto the plane defined by the rectangle
                Vector3 projectedGradient = point.Direction - Vector3.Dot(point.Direction, normal) * normal;

                // If the projected gradient is very small, set it to zero to avoid noise
                if (projectedGradient.magnitude < 1e-6f)
                {
                    point.Direction = Vector3.zero;
                    point.Magnitude = 0f;
                    return; // Using return instead of continue in Parallel.ForEach
                }

                // Normalize direction and scale magnitude proportionally
                float projectedMagnitude = projectedGradient.magnitude;
                point.Direction = projectedGradient.normalized;

                // Scale magnitude proportionally based on average
                point.Magnitude = projectedMagnitude / averageMagnitude;
            });

            Debug.Log("Gradients have been projected onto the rectangle plane and normalized (using parallel processing).");
            return generatedGradientPoints;
        }
    }
}
