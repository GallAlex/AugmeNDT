using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Performs 4th-order Runge-Kutta integration to estimate a flow vector from a position.
        /// </summary>
        public static Vector3 RungeKutta4(Vector3 position, List<GradientDataset> gradientPoints, float stepSize, bool returnPosition = false)
        {
            // 4th-order Runge-Kutta integration implementation
            Vector3 k1 = InterpolateGradientAtPositionOptimized(position, gradientPoints);
            Vector3 k2 = InterpolateGradientAtPositionOptimized(position + k1 * stepSize * 0.5f, gradientPoints);
            Vector3 k3 = InterpolateGradientAtPositionOptimized(position + k2 * stepSize * 0.5f, gradientPoints);
            Vector3 k4 = InterpolateGradientAtPositionOptimized(position + k3 * stepSize, gradientPoints);

            Vector3 direction = (k1 + 2 * k2 + 2 * k3 + k4) / 6f;

            if (returnPosition)
                return position + direction * stepSize;
            else
                return direction;
        }

        /// <summary>
        /// Optimized method to interpolate a gradient at a given position using nearest neighbors.
        /// </summary>
        private static Vector3 InterpolateGradientAtPositionOptimized(Vector3 position, List<GradientDataset> gradientPoints)
        {
            int maxNeighbors = 8;
            List<Tuple<float, GradientDataset>> nearestPointsWithDistance = new List<Tuple<float, GradientDataset>>(maxNeighbors);

            // Efficient nearest neighbor search
            foreach (var point in gradientPoints)
            {
                float distance = Vector3.Distance(position, point.Position);

                // If a very close point is found, return its direction immediately
                if (distance < 0.0001f)
                {
                    return point.Direction;
                }

                // Maintain a list of the closest N points
                if (nearestPointsWithDistance.Count < maxNeighbors)
                {
                    nearestPointsWithDistance.Add(new Tuple<float, GradientDataset>(distance, point));
                    if (nearestPointsWithDistance.Count == maxNeighbors)
                    {
                        // Sort by distance in descending order (farthest first)
                        nearestPointsWithDistance.Sort((a, b) => b.Item1.CompareTo(a.Item1));
                    }
                }
                else if (distance < nearestPointsWithDistance[0].Item1)
                {
                    // If this point is closer than the farthest in the list, replace and re-sort
                    nearestPointsWithDistance[0] = new Tuple<float, GradientDataset>(distance, point);
                    for (int i = 0; i < maxNeighbors - 1; i++)
                    {
                        if (nearestPointsWithDistance[i].Item1 > nearestPointsWithDistance[i + 1].Item1)
                        {
                            var temp = nearestPointsWithDistance[i];
                            nearestPointsWithDistance[i] = nearestPointsWithDistance[i + 1];
                            nearestPointsWithDistance[i + 1] = temp;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            // Compute weighted average direction
            Vector3 weightedDirection = Vector3.zero;
            float totalWeight = 0f;

            foreach (var tuple in nearestPointsWithDistance)
            {
                float distance = tuple.Item1;
                GradientDataset point = tuple.Item2;

                float weight = (distance < 0.0001f) ? 1000f : 1f / (distance * distance); // Inverse distance weighting

                weightedDirection += point.Direction * weight;
                totalWeight += weight;
            }

            if (totalWeight > 0)
                return weightedDirection / totalWeight;

            return Vector3.zero;
        }

        /// <summary>
        /// Basic interpolation of gradient direction using the 8 nearest neighbors.
        /// </summary>
        private static Vector3 InterpolateGradientAtPosition(Vector3 position, List<GradientDataset> gradientPoints)
        {
            var nearestPoints = gradientPoints
                .OrderBy(g => Vector3.Distance(g.Position, position))
                .Take(8)
                .ToList();

            if (!nearestPoints.Any())
                return Vector3.zero;

            // Compute weighted average direction
            Vector3 weightedDirection = Vector3.zero;
            float totalWeight = 0f;

            foreach (var point in nearestPoints)
            {
                float distance = Vector3.Distance(position, point.Position);
                float weight = (distance < 0.0001f) ? 1000f : 1f / (distance * distance); // Inverse distance weighting

                weightedDirection += point.Direction * weight;
                totalWeight += weight;
            }

            if (totalWeight > 0)
                return weightedDirection / totalWeight;

            return Vector3.zero;
        }

        #endregion

        #region Grid-Based Spatial Interpolation

        /// <summary>
        /// Gets the grid cell index for a given 3D position based on cell size.
        /// </summary>
        public static Vector3Int GetGridCell(Vector3 position, float cellSize)
        {
            return new Vector3Int(
                Mathf.FloorToInt(position.x / cellSize),
                Mathf.FloorToInt(position.y / cellSize),
                Mathf.FloorToInt(position.z / cellSize)
            );
        }

        /// <summary>
        /// Interpolates a vector field using a spatial grid of precomputed gradients.
        /// </summary>
        public static Vector3 InterpolateVectorField(Vector3 position, float cellSize, Dictionary<Vector3Int, List<GradientDataset>> spatialGrid)
        {
            Vector3Int cell = GetGridCell(position, cellSize);
            Vector3 interpolatedDirection = Vector3.zero;
            float totalWeight = 0f;

            // Search in current and surrounding cells (3x3x3)
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
                                float weight = Mathf.Exp(-distance * distance * 100f); // Fast falloff for local influence

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
