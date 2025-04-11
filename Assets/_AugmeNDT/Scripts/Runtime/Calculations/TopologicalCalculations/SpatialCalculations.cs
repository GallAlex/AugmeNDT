using System;
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
        /// Performs 4th-order Runge-Kutta integration to estimate a flow vector from a position.
        /// </summary>
        public static Vector3 RungeKutta4(Vector3 position, List<GradientDataset> gradientPoints, float stepSize,bool returnPosition = false)
        {
            // RK4 implementasyonu
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
        private static Vector3 InterpolateGradientAtPositionOptimized(Vector3 position, List<GradientDataset> gradientPoints)
        {
            int maxNeighbors = 8;
            List<Tuple<float, GradientDataset>> nearestPointsWithDistance = new List<Tuple<float, GradientDataset>>(maxNeighbors);

            // En yakın 8 noktayı bulmak için daha verimli bir yaklaşım
            foreach (var point in gradientPoints)
            {
                float distance = Vector3.Distance(position, point.Position);

                // Direkt aşırı yakın nokta varsa, hemen döndür
                if (distance < 0.0001f)
                {
                    return point.Direction;
                }

                // Hedef boyutta liste tutuyoruz
                if (nearestPointsWithDistance.Count < maxNeighbors)
                {
                    nearestPointsWithDistance.Add(new Tuple<float, GradientDataset>(distance, point));
                    if (nearestPointsWithDistance.Count == maxNeighbors)
                    {
                        // Listeyi en uzak nokta başta olacak şekilde sırala
                        nearestPointsWithDistance.Sort((a, b) => b.Item1.CompareTo(a.Item1));
                    }
                }
                else if (distance < nearestPointsWithDistance[0].Item1)
                {
                    // Eğer bu nokta listenin en uzak noktasından daha yakınsa, onu değiştir
                    nearestPointsWithDistance[0] = new Tuple<float, GradientDataset>(distance, point);
                    // Yeniden sırala (insertion sort mantığı)
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

            // Ağırlıklı ortalama ile yön hesapla
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
        private static Vector3 InterpolateGradientAtPosition(Vector3 position, List<GradientDataset> gradientPoints)
        {
            var nearestPoints = gradientPoints
                .OrderBy(g => Vector3.Distance(g.Position, position))
                .Take(8)
                .ToList();

            if (!nearestPoints.Any())
                return Vector3.zero;

            // Ağırlıklı ortalama ile yön hesapla
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
