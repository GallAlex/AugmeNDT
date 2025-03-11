using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    public static class SpatialCalculations
    {
        #region 2D Triangle

        /// <summary>
        /// Determines if a given point lies inside a triangle in 3D space.
        /// Uses barycentric coordinates to check if the point is within the triangle.
        /// </summary>
        public static bool IsPointInTriangle(Vector3 p, Vector3 point1, Vector3 point2, Vector3 point3)
        {
            // Compute vectors from triangle vertices
            Vector3 v0 = point3 - point1;
            Vector3 v1 = point2 - point1;
            Vector3 v2 = p - point1;

            // Compute dot products
            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            // Compute barycentric coordinates
            float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // Check if point is inside triangle
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }

        /// <summary>
        /// Generates evenly distributed points inside a triangle.
        /// Uses barycentric interpolation to create a uniform grid of points within the triangle.
        /// </summary>
        public static List<GradientDataset> GenerateTrianglePoints(Vector3 point1, Vector3 point2, Vector3 point3, int numSteps)
        {
            List<GradientDataset> generatedGradientPoints = new List<GradientDataset>();

            Vector3 basis1 = point2 - point1;
            Vector3 basis2 = point3 - point1;

            for (int i = 0; i <= numSteps; i++)
            {
                for (int j = 0; j <= numSteps - i; j++)
                {
                    // Compute point using barycentric coordinates
                    Vector3 newPoint = point1 + (i / (float)numSteps) * basis1 + (j / (float)numSteps) * basis2;

                    // Add point only if it lies within the triangle
                    if (SpatialCalculations.IsPointInTriangle(newPoint, point1, point2, point3))
                    {
                        generatedGradientPoints.Add(new GradientDataset(generatedGradientPoints.Count, newPoint, Vector3.zero, 0f));
                    }
                }
            }

            return generatedGradientPoints;
        }

        #endregion

        #region 2D Rectangle

        /// <summary>
        /// Checks if a point is inside a 2D rectangle in 3D space.
        /// </summary>
        public static bool IsPointInRectangle(Vector3 point, Vector3[] corners)
        {
            // Get rectangle's plane normal
            Vector3 normal = Vector3.Cross(corners[1] - corners[0], corners[3] - corners[0]).normalized;

            // Create 2D basis vectors in rectangle plane
            Vector3 u = (corners[1] - corners[0]).normalized;
            Vector3 perpV = Vector3.Cross(normal, u).normalized;

            // Project point into the 2D basis
            Vector3 relativePoint = point - corners[0];
            float projU = Vector3.Dot(relativePoint, u);
            float projV = Vector3.Dot(relativePoint, perpV);

            // Compute bounds
            float maxU = Vector3.Dot(corners[1] - corners[0], u);
            float maxV = Vector3.Dot(corners[3] - corners[0], perpV);

            return projU >= 0 && projU <= maxU && projV >= 0 && projV <= maxV;
        }

        /// <summary>
        /// Projects a 3D point onto the plane defined by a rectangle.
        /// </summary>
        public static Vector3 ProjectPointOntoRectanglePlane(Vector3 point, Vector3[] corners, Vector3 normal)
        {
            Vector3 planePoint = corners[0];
            float distance = Vector3.Dot(normal, point - planePoint);
            return point - distance * normal;
        }

        #endregion

        #region 3D Rectangle / Triangular Prism

        /// <summary>
        /// Gets a random point from a list of gradient points.
        /// </summary>
        public static Vector3 GetRandomPointInRectangularPrism(List<GradientDataset> gradientPoints)
        {
            // Randomly select a point from the list
            return gradientPoints[Random.Range(0, gradientPoints.Count)].Position;
        }

        /// <summary>
        /// Generates evenly spaced points inside a triangular prism.
        /// </summary>
        public static List<GradientDataset> GeneratePrismPoints(List<Vector3> points, int numSteps)
        {
            List<GradientDataset> generatedGradientPoints = new List<GradientDataset>();

            for (int i = 0; i <= numSteps; i++)
            {
                float alpha = i / (float)numSteps;

                // Interpolate between bottom and top triangle faces
                Vector3 p1 = Vector3.Lerp(points[0], points[3], alpha);
                Vector3 p2 = Vector3.Lerp(points[1], points[4], alpha);
                Vector3 p3 = Vector3.Lerp(points[2], points[5], alpha);

                for (int j = 0; j <= numSteps; j++)
                {
                    for (int k = 0; k <= numSteps - j; k++)
                    {
                        Vector3 newPoint = p1 + (j / (float)numSteps) * (p2 - p1) + (k / (float)numSteps) * (p3 - p1);
                        generatedGradientPoints.Add(new GradientDataset(generatedGradientPoints.Count, newPoint, Vector3.zero, 0f));
                    }
                }
            }

            return generatedGradientPoints;
        }

        #endregion

        #region 3D Space Utilities

        /// <summary>
        /// Retrieves all gradient points within a specified radius from a position.
        /// </summary>
        public static List<GradientDataset> GetNearbyPoints(List<GradientDataset> generatedGradientPoints, Vector3 position, float searchRadius)
        {
            return generatedGradientPoints
                .Where(p => Vector3.Distance(position, p.Position) <= searchRadius)
                .ToList();
        }

        /// <summary>
        /// Performs 4th-order Runge-Kutta integration to estimate a flow vector from a position.
        /// </summary>
        public static Vector3 RungeKutta4(Vector3 position, List<GradientDataset> gradientPoints, float streamlineStepSize)
        {
            float h = streamlineStepSize;

            Vector3 k1 = InterpolateVectorField(position, gradientPoints);
            Vector3 k2 = InterpolateVectorField(position + 0.5f * h * k1, gradientPoints);
            Vector3 k3 = InterpolateVectorField(position + 0.5f * h * k2, gradientPoints);
            Vector3 k4 = InterpolateVectorField(position + h * k3, gradientPoints);

            return (k1 + 2f * k2 + 2f * k3 + k4) / 6f;
        }

        /// <summary>
        /// Estimates the directional vector at a position by interpolating nearby gradient vectors.
        /// </summary>
        public static Vector3 InterpolateVectorField(Vector3 position, List<GradientDataset> gradientPoints)
        {
            List<GradientDataset> nearestPoints = GetNearbyPoints(gradientPoints, position, 1.0f);
            if (nearestPoints.Count == 0) return new Vector3(0, 0, 1); // Default direction

            float weightFactor = 0.5f;
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

        #endregion
    }
}
