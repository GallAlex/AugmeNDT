using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AugmeNDT
{
    public static class SpatialCalculations
    {
        #region 2D Rectangle
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
            if (nearestPoints.Count == 0) return Vector3.zero;

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

        #region 
        /// <summary>
        /// Gets the grid cell index for a position
        /// </summary>
        public static Vector3Int GetGridCell(Vector3 position, float cellSize)
        {
            return new Vector3Int(
                Mathf.FloorToInt(position.x / cellSize),
                Mathf.FloorToInt(position.y / cellSize),
                Mathf.FloorToInt(position.z / cellSize)
            );
        }

        public static Vector3 InterpolateVectorField(Vector3 position, float cellSize, Dictionary<Vector3Int, List<GradientDataset>> spatialGrid)
        {
            Vector3Int cell = GetGridCell(position, cellSize);
            Vector3 interpolatedDirection = Vector3.zero;
            float totalWeight = 0f;

            // Search in current cell and neighboring cells
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Vector3Int neighborCell = new Vector3Int(cell.x + x, cell.y + y, cell.z + z);
                        if (spatialGrid.TryGetValue(neighborCell, out List<GradientDataset> cellPoints))
                        {
                            foreach (var point in cellPoints)
                            {
                                float distance = Vector3.Distance(position, point.Position);
                                float weight = Mathf.Exp(-distance * distance * 100f); // Higher falloff for more local interpolation

                                interpolatedDirection += point.Direction * weight;
                                totalWeight += weight;
                            }
                        }
                    }
                }
            }

            // If no weights found, return zero vector
            if (totalWeight < 0.0001f)
                return Vector3.zero;

            return interpolatedDirection / totalWeight;
        }
        #endregion
    }
}
