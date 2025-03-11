using Assets.Scripts.DataStructure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the visualization of 3D streamlines based on gradient vector field data
    /// </summary>
    public class StreamLine3D : MonoBehaviour
    {
        public static StreamLine3D Instance;
        public Material streamlineMaterial; // Material for LineRenderer

        public int numStreamlines = 1000; // Number of streamlines to draw
        public float streamlineStepSize = 0.2f; // Step size for integration
        public int maxStreamlineSteps = 100; // Maximum number of steps per streamline

        private static Rectangle3DManager rectangle3DManager;

        private List<GradientDataset> gradientPoints = new List<GradientDataset>();
        private List<CriticalPointDataset> criticalPoints = new List<CriticalPointDataset>();
        private Bounds cubeBounds;
        private List<GameObject> LineObjs = new List<GameObject>();

        private void Awake()
        {
            // Initialize singleton instance
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            // Get reference to rectangle manager
            rectangle3DManager = Rectangle3DManager.rectangle3DManager;
        }

        /// <summary>
        /// Shows streamlines by creating them if needed or making existing ones visible
        /// </summary>
        public void ShowStreamLines()
        {
            if (!LineObjs.Any() || rectangle3DManager.IsUpdated())
                DrawStreamlines();
            else
                LineObjs.ForEach(line => line.SetActive(true));
        }

        /// <summary>
        /// Hides all streamlines by setting them inactive
        /// </summary>
        public void HideStreamLines()
        {
            foreach (var line in LineObjs)
            {
                line.SetActive(false);
            }
        }

        #region private
        /// <summary>
        /// Calculates and draws all streamlines
        /// </summary>
        private void DrawStreamlines()
        {
            DestroyLines();

            gradientPoints = rectangle3DManager.GetGradientPoints();
            if (!gradientPoints.Any())
                return;

            criticalPoints = rectangle3DManager.GetCriticalPoints();
            cubeBounds = rectangle3DManager.GetWireframeCubeBounds();

            // Use better seeding strategies
            List<Vector3> seedPoints = new List<Vector3>();

            if (gradientPoints.Count > 0)
            {
                // If we have critical points, use those as a basis
                seedPoints.AddRange(GenerateCriticalPointBasedSeeds(numStreamlines / 2));
            }

            // Fill the rest with evenly distributed seeds
            int remainingSeeds = numStreamlines - seedPoints.Count;
            if (remainingSeeds > 0)
            {
                seedPoints.AddRange(GenerateEvenlyDistributedSeeds(remainingSeeds));
            }

            // Now generate streamlines from our seed points
            foreach (Vector3 seedPoint in seedPoints)
            {
                // Generate in both directions for better coverage
                List<Vector3> forwardPoints = GenerateStreamline(seedPoint, 1f);
                List<Vector3> backwardPoints = GenerateStreamline(seedPoint, -1f);

                // Combine the points (reverse the backward points and add them first)
                backwardPoints.Reverse();
                backwardPoints.AddRange(forwardPoints);

                // Create the line renderer
                if (backwardPoints.Count >= 2)
                {
                    CreateLineRenderer(backwardPoints);
                }
            }
        }

        /// <summary>
        /// Generates evenly distributed seed points within the volume
        /// </summary>
        /// <param name="count">Number of seed points to generate</param>
        /// <returns>List of seed points</returns>
        private List<Vector3> GenerateEvenlyDistributedSeeds(int count)
        {
            List<Vector3> seeds = new List<Vector3>();
            Vector3 min = cubeBounds.min;
            Vector3 max = cubeBounds.max;

            // Calculate the number of points along each axis
            int numX = Mathf.CeilToInt(Mathf.Pow(count, 1f / 3f));
            int numY = numX;
            int numZ = numX;

            float stepX = (max.x - min.x) / (numX - 1);
            float stepY = (max.y - min.y) / (numY - 1);
            float stepZ = (max.z - min.z) / (numZ - 1);

            for (int i = 0; i < numX; i++)
            {
                for (int j = 0; j < numY; j++)
                {
                    for (int k = 0; k < numZ; k++)
                    {
                        Vector3 seed = new Vector3(
                            min.x + i * stepX,
                            min.y + j * stepY,
                            min.z + k * stepZ
                        );

                        // Add a small random offset to avoid grid-like patterns
                        seed += new Vector3(
                            Random.Range(-stepX / 4, stepX / 4),
                            Random.Range(-stepY / 4, stepY / 4),
                            Random.Range(-stepZ / 4, stepZ / 4)
                        );

                        if (cubeBounds.Contains(seed))
                        {
                            seeds.Add(seed);
                        }
                    }
                }
            }

            // If we have too many seeds, randomly select the desired number
            if (seeds.Count > count)
            {
                List<Vector3> finalSeeds = new List<Vector3>();
                for (int i = 0; i < count; i++)
                {
                    int index = Random.Range(0, seeds.Count);
                    finalSeeds.Add(seeds[index]);
                    seeds.RemoveAt(index);
                }
                return finalSeeds;
            }

            return seeds;
        }

        /// <summary>
        /// Generates seed points based on critical points and high gradient areas
        /// </summary>
        /// <param name="count">Number of seed points to generate</param>
        /// <returns>List of seed points</returns>
        private List<Vector3> GenerateCriticalPointBasedSeeds(int count)
        {
            List<Vector3> seeds = new List<Vector3>();

            // First add seeds near critical points
            foreach (var cp in criticalPoints)
            {
                // Add the critical point itself
                seeds.Add(cp.Position);

                // Add some points around it
                for (int i = 0; i < 3; i++)
                {
                    Vector3 offset = Random.insideUnitSphere * 0.5f;
                    Vector3 seed = cp.Position + offset;
                    if (cubeBounds.Contains(seed))
                    {
                        seeds.Add(seed);
                    }
                }
            }

            // Fill remaining seeds with areas of high gradient magnitude
            List<GradientDataset> sortedGradients = gradientPoints
                .OrderByDescending(g => g.Magnitude)
                .ToList();

            int remainingSeeds = count - seeds.Count;
            int step = Mathf.Max(1, sortedGradients.Count / remainingSeeds);

            for (int i = 0; i < sortedGradients.Count && seeds.Count < count; i += step)
            {
                seeds.Add(sortedGradients[i].Position);
            }

            return seeds;
        }

        /// <summary>
        /// Generates a single streamline starting from the given position
        /// </summary>
        /// <param name="startPosition">Starting position for the streamline</param>
        /// <param name="direction">Direction multiplier (1 for forward, -1 for backward)</param>
        /// <returns>List of points representing the streamline path</returns>
        private List<Vector3> GenerateStreamline(Vector3 startPosition, float direction = 1f)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3 currentPosition = startPosition;
            points.Add(currentPosition);

            for (int i = 0; i < maxStreamlineSteps; i++)
            {
                //currentPosition += EulerStep(currentPosition);

                Vector3 step = SpatialCalculations.RungeKutta4(currentPosition, gradientPoints, streamlineStepSize) * direction;

                if (step.magnitude < 0.001f) break; // Stop if movement is too small

                currentPosition += step.normalized * streamlineStepSize;

                if (!cubeBounds.Contains(currentPosition))
                    break;

                points.Add(currentPosition);
            }

            return points;
        }

        /// <summary>
        /// Destroys all line objects and clears the list
        /// </summary>
        private void DestroyLines()
        {
            LineObjs.ForEach(x => Destroy(x));
            LineObjs.Clear();
        }

        /// <summary>
        /// Performs a simple Euler integration step
        /// </summary>
        /// <param name="position">Current position</param>
        /// <returns>Vector representing the step to take</returns>
        private Vector3 EulerStep(Vector3 position)
        {
            return InterpolateVectorField(position).normalized * streamlineStepSize;
        }

        /// <summary>
        /// Interpolates the vector field at a given position using nearby points
        /// </summary>
        /// <param name="position">Position to interpolate at</param>
        /// <returns>Interpolated direction vector</returns>
        private Vector3 InterpolateVectorField(Vector3 position)
        {
            List<GradientDataset> nearestPoints = SpatialCalculations.GetNearbyPoints(gradientPoints, position, 1.0f);
            if (nearestPoints.Count == 0) return Vector3.zero;

            Vector3 interpolatedDirection = Vector3.zero;
            float totalWeight = 0f;

            foreach (var neighbor in nearestPoints)
            {
                float distance = Vector3.Distance(position, neighbor.Position);
                float weight = Mathf.Exp(-Mathf.Pow(distance, 2) / 0.2f); // Gaussian weighting

                interpolatedDirection += neighbor.Direction * weight;
                totalWeight += weight;
            }

            return totalWeight > 0 ? interpolatedDirection / totalWeight : Vector3.zero;
        }

        /// <summary>
        /// Creates a LineRenderer GameObject for visualizing a streamline
        /// </summary>
        /// <param name="points">List of points defining the streamline path</param>
        private void CreateLineRenderer(List<Vector3> points)
        {
            if (points.Count < 2) return;

            GameObject lineObj = new GameObject("Streamline");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();

            lr.material = streamlineMaterial;
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;

            LineObjs.Add(lineObj);
        }
        #endregion private
    }
}