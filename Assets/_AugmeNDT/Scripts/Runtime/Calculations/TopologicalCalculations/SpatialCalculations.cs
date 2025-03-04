using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    public static class SpatialCalculations
    {
        /// <summary>
        /// Determines if a given point lies inside a triangle in 3D space.
        /// Uses barycentric coordinates to check if the point is within the triangle.
        /// </summary>
        /// <param name="p">The point to check.</param>
        /// <param name="point1">First vertex of the triangle.</param>
        /// <param name="point2">Second vertex of the triangle.</param>
        /// <param name="point3">Third vertex of the triangle.</param>
        /// <returns>True if the point is inside the triangle, otherwise false.</returns>
        public static bool IsPointInTriangle(Vector3 p, Vector3 point1, Vector3 point2, Vector3 point3)
        {            
            // Compute vectors
            Vector3 v0 = point3 - point1;
            Vector3 v1 = point2 - point1;
            Vector3 v2 = p - point1;

            // Compute dot products
            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            // Compute inverse denominator for barycentric coordinates
            float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // A point is inside the triangle if u, v are non-negative and u + v <= 1
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }

        /// <summary>
        /// Generates evenly distributed points inside a triangle.
        /// Uses barycentric interpolation to create a uniform grid of points within the triangle.
        /// </summary>
        /// <param name="point1">First vertex of the triangle.</param>
        /// <param name="point2">Second vertex of the triangle.</param>
        /// <param name="point3">Third vertex of the triangle.</param>
        /// <param name="numSteps">Number of subdivisions along each triangle edge.</param>
        /// <returns>List of generated gradient points within the triangle.</returns>
        public static List<GradientDataset> GenerateTrianglePoints(Vector3 point1, Vector3 point2, Vector3 point3, int numSteps)
        {
            List<GradientDataset> generatedGradientPoints = new List<GradientDataset>();

            Vector3 basis1 = point2 - point1;
            Vector3 basis2 = point3 - point1;

            for (int i = 0; i <= numSteps; i++)
            {
                for (int j = 0; j <= numSteps - i; j++)
                {
                    // Compute new point using barycentric interpolation
                    Vector3 newPoint = point1 + (i / (float)numSteps) * basis1 + (j / (float)numSteps) * basis2;
                   
                    // Ensure the new point is inside the triangle
                    if (SpatialCalculations.IsPointInTriangle(newPoint, point1, point2, point3))
                    {
                        generatedGradientPoints.Add(new GradientDataset(generatedGradientPoints.Count, newPoint, Vector3.zero, 0f));
                    }
                }
            }

            return generatedGradientPoints;
        }

        /// <summary>
        /// Generates evenly distributed points inside a triangular prism.
        /// Uses linear interpolation to create multiple triangle slices along the prism height.
        /// </summary>
        /// <param name="points">List of 6 points defining the prism (3 for bottom face, 3 for top face).</param>
        /// <param name="numSteps">Number of subdivisions along the prism height.</param>
        /// <returns>List of generated gradient points within the prism.</returns>
        public static List<GradientDataset> GeneratePrismPoints(List<Vector3> points, int numSteps)
        {
            List<GradientDataset> generatedGradientPoints = new List<GradientDataset>();

            for (int i = 0; i <= numSteps; i++)
            {
                float alpha = i / (float)numSteps;

                // Compute interpolated triangle slice at the current height level
                Vector3 p1 = Vector3.Lerp(points[0], points[3], alpha);
                Vector3 p2 = Vector3.Lerp(points[1], points[4], alpha);
                Vector3 p3 = Vector3.Lerp(points[2], points[5], alpha);

                for (int j = 0; j <= numSteps; j++)
                {
                    for (int k = 0; k <= numSteps - j; k++)
                    {
                        // Compute new point inside the triangle slice
                        Vector3 newPoint = p1 + (j / (float)numSteps) * (p2 - p1) + (k / (float)numSteps) * (p3 - p1);
                        generatedGradientPoints.Add(new GradientDataset(generatedGradientPoints.Count, newPoint, Vector3.zero, 0f));
                    }
                }
            }

            return generatedGradientPoints;
        }

        /// <summary>
        /// Retrieves all gradient points within a specified search radius of a given position.
        /// This is useful for spatial interpolation and smoothing operations.
        /// </summary>
        /// <param name="generatedGradientPoints">List of all available gradient points.</param>
        /// <param name="position">The position to search around.</param>
        /// <param name="searchRadius">The maximum distance a point can be considered "nearby".</param>
        /// <returns>List of gradient points within the search radius.</returns>
        public static List<GradientDataset> GetNearbyPoints(List<GradientDataset> generatedGradientPoints, Vector3 position, float searchRadius)
        {
            return generatedGradientPoints.Where(p => Vector3.Distance(position, p.Position) <= searchRadius).ToList();
        }

        public static Vector3 RungeKutta4(Vector3 position, List<GradientDataset> gradientPoints,float streamlineStepSize)
        {
            float h = streamlineStepSize;

            Vector3 k1 = InterpolateVectorField(position, gradientPoints);
            Vector3 k2 = InterpolateVectorField(position + 0.5f * h * k1, gradientPoints);
            Vector3 k3 = InterpolateVectorField(position + 0.5f * h * k2, gradientPoints);
            Vector3 k4 = InterpolateVectorField(position + h * k3, gradientPoints);

            return (k1 + 2f * k2 + 2f * k3 + k4) / 6f;
        }
        
        public static Vector3 InterpolateVectorField(Vector3 position, List<GradientDataset> gradientPoints)
        {
            List<GradientDataset> nearestPoints = SpatialCalculations.GetNearbyPoints(gradientPoints, position, 1.0f);
            if (nearestPoints.Count == 0) return new Vector3(0, 0, 1); // Default flow direction

            float weightFactor = 0.5f; // Increase interpolation range
            Vector3 interpolatedDirection = Vector3.zero;
            float totalWeight = 0f;

            foreach (var neighbor in nearestPoints)
            {
                float distance = Vector3.Distance(position, neighbor.Position);
                float weight = Mathf.Exp(-Mathf.Pow(distance, 2) / weightFactor);

                interpolatedDirection += neighbor.Direction * weight;
                totalWeight += weight;
            }

            return totalWeight > 0 ? interpolatedDirection / totalWeight : new Vector3(0, 0, 1);
        }

    }
}
