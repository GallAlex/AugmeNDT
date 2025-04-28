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
        private static Rectangle3DManager rectangle3DManager;

        public List<GradientDataset> gradientPoints = new List<GradientDataset>();
        public Dictionary<Vector3Int, List<GradientDataset>> spatialGrid = new Dictionary<Vector3Int, List<GradientDataset>>();

        private List<GameObject> LineObjs = new List<GameObject>();
        public Transform container;

        [Header("Streamline Parameters")]
        public int numStreamlines = 350; // Number of streamlines to draw
        public float streamlineStepSize = 0.0033f; // Step size for integration
        public int maxStreamlineSteps = 142; // Maximum number of steps per streamline
        public float cellSize = 0.01f; // Spatial grid cell size
        public float streamlineDensity = 1f; // Controls the density of streamlines

        [Header("Visual Settings")]
        public float streamlineWidth = 0.001f; // Reduced width for all streamlines
        [Range(0f, 1f)]
        public Color streamlineColor = Color.white; // Single color for all streamlines
        public bool useAdaptiveStepSize = true;

        public int streamlinesPerFrame = 10;

        private float averageMagnitude = 50f; // Will be updated from data
        private List<Vector3> criticalPointsPositions = new List<Vector3>();
        public Bounds cubeBounds;

        private StreamLineObjectPool streamLinePool;

        private void Awake()
        {
            // Initialize singleton instance
            Instance = this;

            // Get or create a reference to the object pool
            streamLinePool = FindObjectOfType<StreamLineObjectPool>();
            if (streamLinePool == null)
            {
                GameObject poolObj = new GameObject("StreamLineObjectPool");
                poolObj.transform.SetParent(GameObject.Find("Scene Objects").transform);
                
                streamLinePool = poolObj.AddComponent<StreamLineObjectPool>();
            }
        }

        private void Start()
        {
            // Get reference to rectangle manager
            rectangle3DManager = Rectangle3DManager.rectangle3DManager;
            TopologyConfigData config = rectangle3DManager.config;
            streamlineColor = config.ColorOfStreamLines;
        }

        /// <summary>
        /// Shows streamlines by creating them if needed or making existing ones visible
        /// </summary>
        public void ShowStreamLines(bool forced = false)
        {
            if (forced || !LineObjs.Any() || IsUpdated())
            {
                if (!PrepareInstance())
                    return;

                StartCoroutine(DrawStreamlinesCoroutine());
            }
            else
            {
                LineObjs.ForEach(line => line.SetActive(true));
            }
        }

        #region private
        private bool IsUpdated()
        {
            Bounds currentCubeBounds = rectangle3DManager.GetRectangleBounds();
            if (cubeBounds == currentCubeBounds)
                return false;

            cubeBounds = currentCubeBounds;
            return true;
        }

        private bool PrepareInstance()
        {
            ReturnAllLinesToPool();

            gradientPoints = rectangle3DManager.GetGradientPoints();
            if (!gradientPoints.Any())
                return false;

            // If the 3DStreamLines container does not exist, create it
            if (container != null)
            {
                Destroy(GameObject.Find("3DStreamLines"));
            }

            container = new GameObject("3DStreamLines").transform;
            container.transform.SetParent(rectangle3DManager.volumeTransform, true);

            cubeBounds = rectangle3DManager.GetRectangleBounds();
            averageMagnitude = gradientPoints.Average(gradient => gradient.Magnitude);

            // Build spatial grid for faster lookup
            BuildSpatialGrid();

            // Detect critical points in the vector field
            DetectInterestingRegions();

            return true;
        }

        /// <summary>
        /// Gets the grid cell index for a position
        /// </summary>
        private Vector3Int GetGridCell(Vector3 position)
        {
            return new Vector3Int(
                Mathf.FloorToInt(position.x / cellSize),
                Mathf.FloorToInt(position.y / cellSize),
                Mathf.FloorToInt(position.z / cellSize)
            );
        }

        /// <summary>
        /// Builds a spatial grid for faster neighbor lookups
        /// </summary>
        private void BuildSpatialGrid()
        {
            spatialGrid.Clear();
            foreach (var gradient in gradientPoints)
            {
                Vector3Int cell = GetGridCell(gradient.Position);
                if (!spatialGrid.ContainsKey(cell))
                    spatialGrid[cell] = new List<GradientDataset>();
                spatialGrid[cell].Add(gradient);
            }

            Debug.Log($"Built spatial grid with {spatialGrid.Count} cells");
        }

        /// <summary>
        /// Detects interesting regions in the vector field like critical points
        /// </summary>
        private void DetectInterestingRegions()
        {
            criticalPointsPositions.Clear();

            var globalCriticalPointsPositions = rectangle3DManager.GetCriticalPoints().Select(x => x.Position).ToList();
            List<Vector3> localCriticalPointsPositions = new List<Vector3>();

            // Find local minima in magnitude as potential critical points
            float thresholdMagnitude = averageMagnitude * 0.1f;

            foreach (var gradient in gradientPoints)
            {
                if (gradient.Magnitude < thresholdMagnitude)
                {
                    // Check if this is a local minimum
                    bool isLocalMin = true;
                    Vector3Int cell = GetGridCell(gradient.Position);

                    // Check neighboring cells
                    for (int x = -1; x <= 1 && isLocalMin; x++)
                    {
                        for (int y = -1; y <= 1 && isLocalMin; y++)
                        {
                            for (int z = -1; z <= 1 && isLocalMin; z++)
                            {
                                if (x == 0 && y == 0 && z == 0) continue;

                                Vector3Int neighborCell = new Vector3Int(cell.x + x, cell.y + y, cell.z + z);
                                if (spatialGrid.TryGetValue(neighborCell, out List<GradientDataset> neighbors))
                                {
                                    foreach (var neighbor in neighbors)
                                    {
                                        if (neighbor.Magnitude < gradient.Magnitude)
                                        {
                                            isLocalMin = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (isLocalMin)
                    {
                        localCriticalPointsPositions.Add(gradient.Position);
                    }
                }
            }


            int separator = (int)numStreamlines / 2;
            if (globalCriticalPointsPositions.Count > 0)
                criticalPointsPositions.AddRange(globalCriticalPointsPositions.Take(separator));

            if (localCriticalPointsPositions.Count > 0)
                criticalPointsPositions.AddRange(localCriticalPointsPositions.Take(separator));

            Debug.Log($"Detected {criticalPointsPositions.Count} potential critical points");
        }

        #region Streamline Generation

        /// <summary>
        /// Calculates and draws all streamlines using coroutines for better performance
        /// </summary>
        private IEnumerator DrawStreamlinesCoroutine()
        {
            // Generate seed points
            List<Vector3> seedPoints = GenerateSeeds();
            Debug.Log($"Generated {seedPoints.Count} seed points");

            // Process in batches to avoid freezing the UI
            int batchSize = streamlinesPerFrame;

            for (int i = 0; i < seedPoints.Count; i += batchSize)
            {
                for (int j = 0; j < batchSize && i + j < seedPoints.Count; j++)
                {
                    GenerateAndRenderStreamline(seedPoints[i + j]);
                }
                yield return null; // Wait for next frame
            }

            Debug.Log($"Completed streamline generation with {LineObjs.Count} streamlines");
        }

        /// <summary>
        /// Generates and renders a streamline from a seed point
        /// </summary>
        private void GenerateAndRenderStreamline(Vector3 seedPoint)
        {
            // Generate in both directions for better coverage
            List<Vector3> forwardPoints = GenerateStreamline(seedPoint, 1f);
            List<Vector3> backwardPoints = GenerateStreamline(seedPoint, -1f);

            // Combine the points (reverse the backward points and add them first)
            backwardPoints.Reverse();
            backwardPoints.AddRange(forwardPoints);

            // Create the line renderer if we have enough points
            if (backwardPoints.Count >= 2)
            {
                CreateLineRenderer(backwardPoints);
            }
        }

        /// <summary>
        /// Generates seed points for streamlines
        /// </summary>
        private List<Vector3> GenerateSeeds()
        {
            List<Vector3> seeds = new List<Vector3>();
            int actualStreamlineCount = Mathf.RoundToInt(numStreamlines * streamlineDensity);

            // First add seeds at critical points if any
            if (criticalPointsPositions.Count > 0)
            {
                foreach (var cp in criticalPointsPositions)
                {
                    // Add the critical point itself
                    seeds.Add(cp);

                    // Add points around critical points
                    for (int i = 0; i < Mathf.Min(5, actualStreamlineCount / criticalPointsPositions.Count / 4); i++)
                    {
                        Vector3 offset = Random.insideUnitSphere * streamlineStepSize * 4f;
                        Vector3 seed = cp + offset;
                        if (cubeBounds.Contains(seed))
                        {
                            seeds.Add(seed);
                        }
                    }
                }
            }

            // Then add magnitude-weighted random seeds
            int remainingSeeds = actualStreamlineCount - seeds.Count;
            if (remainingSeeds > 0)
            {
                seeds.AddRange(GenerateAdaptiveSeeds(remainingSeeds));
            }

            return seeds;
        }

        /// <summary>
        /// Generates seed points with adaptive distribution based on magnitude
        /// </summary>
        private List<Vector3> GenerateAdaptiveSeeds(int count)
        {
            List<Vector3> seeds = new List<Vector3>();

            // Create a magnitude-weighted distribution
            List<float> weights = gradientPoints.Select(g => Mathf.Pow(g.Magnitude + 0.1f, 0.5f)).ToList();
            float totalWeight = weights.Sum();

            // Generate seeds by weighted selection
            for (int i = 0; i < count; i++)
            {
                // Select a gradient point based on its magnitude weight
                float randomValue = Random.Range(0, totalWeight);
                float cumulativeWeight = 0;

                for (int j = 0; j < gradientPoints.Count; j++)
                {
                    cumulativeWeight += weights[j];
                    if (cumulativeWeight >= randomValue)
                    {
                        // Add some randomness to seed position
                        Vector3 offset = Random.insideUnitSphere * streamlineStepSize * 3f;
                        Vector3 seed = gradientPoints[j].Position + offset;

                        if (cubeBounds.Contains(seed))
                        {
                            seeds.Add(seed);
                        }
                        break;
                    }
                }
            }

            // If we still need more seeds, add evenly distributed ones
            if (seeds.Count < count / 2)
            {
                seeds.AddRange(GenerateEvenlyDistributedSeeds(count - seeds.Count));
            }

            return seeds;
        }

        /// <summary>
        /// Generates evenly distributed seed points within the volume
        /// </summary>
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
                            Random.Range(-stepX / 3, stepX / 3),
                            Random.Range(-stepY / 3, stepY / 3),
                            Random.Range(-stepZ / 3, stepZ / 3)
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

            // Keep track of previous positions to detect cycles
            HashSet<Vector3Int> visitedCells = new HashSet<Vector3Int>();
            visitedCells.Add(GetGridCell(currentPosition));

            for (int i = 0; i < maxStreamlineSteps; i++)
            {
                // Get interpolated vector at current position
                Vector3 currentVector = SpatialCalculations.InterpolateVectorField(currentPosition, cellSize, spatialGrid);

                // Skip if the vector is too small
                if (currentVector.magnitude < 0.0005f) break;

                // Use adaptive step size if enabled
                float actualStepSize = streamlineStepSize;
                if (useAdaptiveStepSize)
                {
                    // Adjust step size based on vector magnitude
                    float magnitudeFactor = Mathf.Clamp01(currentVector.magnitude / averageMagnitude);
                    actualStepSize = streamlineStepSize * Mathf.Lerp(0.5f, 1.5f, magnitudeFactor);
                }

                // Calculate step using Runge-Kutta integration
                Vector3 step = RungeKutta4Integration(currentPosition, actualStepSize) * direction;

                // Apply the step
                Vector3 newPosition = currentPosition + step.normalized * actualStepSize;

                // Check for boundaries
                if (!cubeBounds.Contains(newPosition))
                    break;

                // Check for cycles (if we've visited this cell before)
                Vector3Int newCell = GetGridCell(newPosition);
                if (visitedCells.Contains(newCell) && i > 10)
                    break;

                visitedCells.Add(newCell);
                currentPosition = newPosition;
                points.Add(currentPosition);
            }

            return points;
        }

        /// <summary>
        /// Performs 4th-order Runge-Kutta integration at a position
        /// </summary>
        private Vector3 RungeKutta4Integration(Vector3 position, float stepSize)
        {
            float h = stepSize;

            Vector3 k1 = SpatialCalculations.InterpolateVectorField(position, cellSize, spatialGrid);
            Vector3 k2 = SpatialCalculations.InterpolateVectorField(position + 0.5f * h * k1, cellSize, spatialGrid);
            Vector3 k3 = SpatialCalculations.InterpolateVectorField(position + 0.5f * h * k2, cellSize, spatialGrid);
            Vector3 k4 = SpatialCalculations.InterpolateVectorField(position + h * k3, cellSize, spatialGrid);

            return (k1 + 2f * k2 + 2f * k3 + k4) / 6f;
        }

        private void CreateLineRenderer(List<Vector3> points)
        {
            if (points.Count < 2) return;

            // Get a line object from the object pool
            GameObject lineObj = streamLinePool.GetPooledObject();
            lineObj.transform.SetParent(container, true);

            LineRenderer lr = lineObj.GetComponent<LineRenderer>();

            // Convert world coordinates to local coordinates
            List<Vector3> localPoints = new List<Vector3>(points.Count);
            foreach (var point in points)
            {
                // Convert each point to the line object's local coordinate system
                localPoints.Add(lineObj.transform.InverseTransformPoint(point));
            }

            // Set up the line renderer
            lr.positionCount = localPoints.Count;
            lr.SetPositions(localPoints.ToArray());

            // Color and width settings
            lr.startColor = lr.endColor = streamlineColor;
            lr.startWidth = lr.endWidth = streamlineWidth;

            // Add to the list of active used objects
            LineObjs.Add(lineObj);
        }

        private void ReturnAllLinesToPool()
        {
            if (streamLinePool != null)
            {
                foreach (var lineObj in LineObjs)
                {
                    // Instead of destroying, return to the object pool
                    streamLinePool.ReturnToPool(lineObj);
                }
                LineObjs.Clear();
            }
        }

        #endregion
        #endregion

    }
}