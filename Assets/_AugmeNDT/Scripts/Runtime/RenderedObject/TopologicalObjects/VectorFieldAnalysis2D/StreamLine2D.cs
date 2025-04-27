using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the visualization of 2D streamlines based on gradient vector field data
    /// </summary>
    public class StreamLine2D : MonoBehaviour
    {
        public static StreamLine2D Instance;

        public int numStreamlines; // Number of streamlines
        public float streamLineStepSize; // Step size for integration
        public int maxStreamlineSteps; // Maximum number of steps per streamline
        public float minDistanceToGeneratePoissonDiskSeeds;

        [Header("Visual Settings")]
        public float streamlineWidth; // Reduced width for all streamlines

        public List<GradientDataset> gradientPoints = new List<GradientDataset>();
        private List<GameObject> lineObjs = new List<GameObject>();
        private static RectangleManager rectangleManager;
        private static Transform container;
        private Material streamlineMaterial;
        private StreamLineObjectPool streamLinePool;

        /// <summary>
        /// Initializes the singleton instance and default values for visualization
        /// </summary>
        private void Awake()
        {
            // Initialize singleton instance
            if (Instance == null)
                Instance = this;

            streamlineMaterial = (Material)Resources.Load("Materials/StreamLine");
            numStreamlines = 200;                      // Fewer streamlines
            streamLineStepSize = 0.007f;              // Smaller step size
            maxStreamlineSteps = 300;                // More steps
            streamlineWidth = 0.0009f;                // Much thinner lines
            minDistanceToGeneratePoissonDiskSeeds = 0.01f; // Slightly larger minimum distance

            // Object pool referansını al veya oluştur
            streamLinePool = FindObjectOfType<StreamLineObjectPool>();
            if (streamLinePool == null)
            {
                GameObject poolObj = new GameObject("StreamLineObjectPool");
                poolObj.transform.SetParent(GameObject.Find("Scene Objects").transform);
                streamLinePool = poolObj.AddComponent<StreamLineObjectPool>();
            }
        }

        /// <summary>
        /// Gets references to required managers
        /// </summary>
        private void Start()
        {
            // Get references to required managers
            if (rectangleManager == null)
            {
                rectangleManager = RectangleManager.rectangleManager;
            }
        }

        /// <summary>
        /// Shows streamlines by creating them if needed or making existing ones visible
        /// </summary>
        public void ShowStreamLines(bool forced = false)
        {
            if (forced || !lineObjs.Any() || rectangleManager.IsUpdated())
            {
                DrawStreamlines();
            }
            else
                lineObjs.ForEach(line => { line.SetActive(true); });
        }

        /// <summary>
        /// Hides all streamlines by setting them inactive
        /// </summary>
        public void HideStreamLines()
        {
            foreach (var line in lineObjs)
                line.SetActive(false);
        }

        #region private
        /// <summary>
        /// Calculates and draws all streamlines
        /// </summary>
        private void DrawStreamlines()
        {
            //Depends on rectangleManager. Therefore it can not call in start()
            SetContainer();

            //Return to pool instead of Destroy the lines
            ReturnAllLinesToPool();

            gradientPoints.Clear();
            gradientPoints = rectangleManager.GetGradientPoints();

            List<List<Vector3>> streamLines = CalculateStreamlinesParallel();
            StartCoroutine(CreateStreamlinesInBatches(streamLines, 10));
        }

        /// <summary>
        /// Calculates streamlines in parallel to improve performance
        /// </summary>
        /// <returns>List of streamline point sequences</returns>
        private List<List<Vector3>> CalculateStreamlinesParallel()
        {
            rectangleManager.UpdateWorldCornersManuel();
            Vector3[] corners = rectangleManager.GetRectangleCorners();

            List<Vector3> seedPoints = GeneratePoissonDiskSeeds(minDistanceToGeneratePoissonDiskSeeds, numStreamlines);

            // Using concurrent collection to safely collect results from multiple threads
            ConcurrentBag<Tuple<int, List<Vector3>>> concurrentResults = new ConcurrentBag<Tuple<int, List<Vector3>>>();

            // Calculate streamlines in parallel for each seed point
            Parallel.For(0, seedPoints.Count, i =>
            {
                Vector3 seed = seedPoints[i];
                List<Vector3> streamline = GenerateStreamline(seed, corners);
                concurrentResults.Add(new Tuple<int, List<Vector3>>(i, streamline));
            });

            // ConcurrentBag doesn't guarantee order, so sort results by index
            var sortedResults = concurrentResults.OrderBy(t => t.Item1).Select(t => t.Item2).ToList();

            return sortedResults;
        }

        /// <summary>
        /// Generates Poisson disk sampling points within the rectangle
        /// </summary>
        /// <param name="minDistance">Minimum distance between points</param>
        /// <param name="maxAttempts">Maximum sampling attempts</param>
        /// <returns>List of Vector3 points distributed according to Poisson disk sampling</returns>
        private List<Vector3> GeneratePoissonDiskSeeds(float minDistance, int maxAttempts)
        {
            HashSet<Vector3> seedPoints = new HashSet<Vector3>();

            // Get rectangle corners
            Vector3[] corners = rectangleManager.GetRectangleCorners();
            if (corners == null || corners.Length != 4)
            {
                Debug.LogError("Rectangle corners not available for Poisson disk sampling");
                return seedPoints.ToList();
            }

            // Get rectangle normal
            Vector3 normal = rectangleManager.GetRectangleNormal();

            // Compute rectangle bounds
            float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float minZ = Mathf.Min(corners[0].z, corners[1].z, corners[2].z, corners[3].z);
            float maxZ = Mathf.Max(corners[0].z, corners[1].z, corners[2].z, corners[3].z);

            // Poisson Disk Sampling
            int attemptsPerPoint = 50; // Number of attempts to find a valid point position

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                bool foundValidPoint = false;

                for (int innerAttempt = 0; innerAttempt < attemptsPerPoint; innerAttempt++)
                {
                    // Generate a random point within the bounding box
                    Vector3 candidate = new Vector3(
                        UnityEngine.Random.Range(minX, maxX),
                        UnityEngine.Random.Range(minY, maxY),
                        UnityEngine.Random.Range(minZ, maxZ)
                    );

                    // Project the point onto the rectangle plane
                    candidate = SpatialCalculations.ProjectPointOntoRectanglePlane(candidate, corners, normal);

                    // Check if the point is inside the rectangle
                    if (!rectangleManager.IsPointInsideMesh(candidate))
                    {
                        continue;
                    }

                    // Ensure minimum distance constraint
                    bool valid = true;
                    foreach (Vector3 point in seedPoints)
                    {
                        if (Vector3.Distance(point, candidate) < minDistance)
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                    {
                        seedPoints.Add(candidate);
                        foundValidPoint = true;
                        break;
                    }
                }

                // If we couldn't find a valid point after multiple attempts, 
                // the space might be too densely filled
                if (!foundValidPoint && seedPoints.Count > 0)
                {
                    // Reduce attempts if we're struggling to find valid positions
                    attemptsPerPoint = Mathf.Max(5, attemptsPerPoint - 2);
                }

                // Early exit if we've reached the maximum samples
                if (seedPoints.Count >= maxAttempts)
                {
                    break;
                }
            }

            return seedPoints.ToList();
        }

        /// <summary>
        /// Generates a streamline starting from the given position by tracing through the gradient field.
        /// </summary>
        /// <param name="startPosition">Starting position for the streamline</param>
        /// <param name="corners">Array of rectangle corner points</param>
        /// <returns>List of points representing the streamline path</returns>
        private List<Vector3> GenerateStreamline(Vector3 startPosition, Vector3[] corners)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3 currentPosition = startPosition;

            // Get rectangle corners and normal for boundary checking
            Vector3 normal = rectangleManager.GetRectangleNormal();

            // Add the starting position to the streamline
            points.Add(currentPosition);

            for (int i = 0; i < maxStreamlineSteps; i++)
            {
                // Calculate the next position with using RungeKutta4
                Vector3 nextPosition = SpatialCalculations.RungeKutta4(
                    currentPosition,
                    gradientPoints,
                    streamLineStepSize,
                    true
                );

                // Project the point back onto the rectangle plane to prevent drift
                nextPosition = SpatialCalculations.ProjectPointOntoRectanglePlane(nextPosition, corners, normal);

                // Check if the new position is still inside the rectangle
                if (!rectangleManager.IsPointInsideMesh(nextPosition, true))
                {
                    // Find the intersection with the rectangle boundary
                    Vector3 boundaryPoint = FindIntersectionWithRectangleBoundary(currentPosition, nextPosition, corners);
                    if (boundaryPoint != Vector3.zero)
                        points.Add(boundaryPoint);
                    break;
                }

                // Add the new position to our streamline
                points.Add(nextPosition);
                currentPosition = nextPosition;
            }

            return points;
        }

        /// <summary>
        /// Finds the intersection point between a line segment and the rectangle boundary
        /// </summary>
        /// <param name="lineStart">Start point of the line segment</param>
        /// <param name="lineEnd">End point of the line segment</param>
        /// <param name="corners">Array of rectangle corner points</param>
        /// <returns>Intersection point or Vector3.zero if no intersection found</returns>
        private Vector3 FindIntersectionWithRectangleBoundary(Vector3 lineStart, Vector3 lineEnd, Vector3[] corners)
        {
            // For each edge of the rectangle, check for intersection
            for (int i = 0; i < 4; i++)
            {
                Vector3 edgeStart = corners[i];
                Vector3 edgeEnd = corners[(i + 1) % 4];

                // Check if the line segments intersect
                Vector3 intersection = LineSegmentIntersection(lineStart, lineEnd, edgeStart, edgeEnd);
                if (intersection != Vector3.zero)
                {
                    return intersection;
                }
            }

            return Vector3.zero; // No intersection found
        }

        /// <summary>
        /// Finds the intersection between two line segments in 3D space
        /// </summary>
        /// <param name="line1Start">First line segment start point</param>
        /// <param name="line1End">First line segment end point</param>
        /// <param name="line2Start">Second line segment start point</param>
        /// <param name="line2End">Second line segment end point</param>
        /// <returns>Intersection point or Vector3.zero if no intersection found</returns>
        private Vector3 LineSegmentIntersection(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
        {
            // Get rectangle normal to define the plane
            Vector3 normal = rectangleManager.GetRectangleNormal();

            // Project all points onto a plane defined by the rectangle normal
            // This simplifies the 3D intersection problem to 2D

            // Create a coordinate system in the plane
            Vector3 origin = line2Start;
            Vector3 xAxis = (line2End - line2Start).normalized;
            Vector3 yAxis = Vector3.Cross(normal, xAxis).normalized;

            // Convert 3D points to 2D coordinates in this plane
            Vector2 l1s = ConvertTo2D(line1Start - origin, xAxis, yAxis);
            Vector2 l1e = ConvertTo2D(line1End - origin, xAxis, yAxis);
            Vector2 l2s = ConvertTo2D(line2Start - origin, xAxis, yAxis);
            Vector2 l2e = ConvertTo2D(line2End - origin, xAxis, yAxis);

            // Check for intersection in 2D
            Vector2 intersection2D;
            if (LineIntersection(l1s, l1e, l2s, l2e, out intersection2D))
            {
                // Convert back to 3D
                Vector3 intersection3D = origin + xAxis * intersection2D.x + yAxis * intersection2D.y;
                return intersection3D;
            }

            return Vector3.zero; // No intersection
        }

        /// <summary>
        /// Converts a 3D point to 2D coordinates using the given basis vectors
        /// </summary>
        /// <param name="point">3D point to convert</param>
        /// <param name="xAxis">X axis basis vector</param>
        /// <param name="yAxis">Y axis basis vector</param>
        /// <returns>2D coordinates in the plane defined by xAxis and yAxis</returns>
        private Vector2 ConvertTo2D(Vector3 point, Vector3 xAxis, Vector3 yAxis)
        {
            float x = Vector3.Dot(point, xAxis);
            float y = Vector3.Dot(point, yAxis);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Determines if two 2D line segments intersect and calculates the intersection point
        /// </summary>
        /// <param name="line1Start">First line segment start point</param>
        /// <param name="line1End">First line segment end point</param>
        /// <param name="line2Start">Second line segment start point</param>
        /// <param name="line2End">Second line segment end point</param>
        /// <param name="intersection">Output parameter for the intersection point</param>
        /// <returns>True if the line segments intersect, false otherwise</returns>
        private bool LineIntersection(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            Vector2 d1 = line1End - line1Start;
            Vector2 d2 = line2End - line2Start;

            float denominator = d1.x * d2.y - d1.y * d2.x;

            // Lines are parallel if denominator is zero
            if (Mathf.Abs(denominator) < 0.0001f)
                return false;

            Vector2 r = line1Start - line2Start;
            float t1 = (d2.x * r.y - d2.y * r.x) / denominator;
            float t2 = (d1.x * r.y - d1.y * r.x) / denominator;

            // Check if intersection is within both line segments
            if (t1 < 0 || t1 > 1 || t2 < 0 || t2 > 1)
                return false;

            // Calculate the intersection point
            intersection = line1Start + t1 * d1;
            return true;
        }

        /// <summary>
        /// Creates streamlines in batches over multiple frames to prevent performance spikes
        /// </summary>
        /// <param name="streamLines">List of streamline point sequences</param>
        /// <param name="batchSize">Number of streamlines to create per frame</param>
        /// <returns>IEnumerator for coroutine processing</returns>
        private IEnumerator CreateStreamlinesInBatches(List<List<Vector3>> streamLines, int batchSize)
        {
            int totalCount = streamLines.Count;
            int processedCount = 0;

            while (processedCount < totalCount)
            {
                // Create batchSize streamlines each frame
                int currentBatchSize = Mathf.Min(batchSize, totalCount - processedCount);

                for (int i = 0; i < currentBatchSize; i++)
                {
                    CreateLineRenderer(streamLines[processedCount + i]);
                }

                processedCount += currentBatchSize;

                // Wait for the next frame
                yield return null;
            }
        }

        /// <summary>
        /// Creates a LineRenderer GameObject for visualizing a streamline
        /// </summary>
        /// <param name="points">List of points defining the streamline path</param>
        private void CreateLineRenderer(List<Vector3> points)
        {
            if (points.Count < 2)
                return;

            // Get line object from the pool
            GameObject lineObj = streamLinePool.GetPooledObject();
            lineObj.transform.SetParent(container);

            LineRenderer lr = lineObj.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;

            // Adjust line renderer to 2D specifications
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.startWidth = lr.endWidth = streamlineWidth;
            lr.alignment = LineAlignment.TransformZ;
            lr.numCapVertices = 2;
            lr.numCornerVertices = 2;

            lineObjs.Add(lineObj);
        }

        /// <summary>
        /// Creates and initializes the container for streamline objects
        /// </summary>
        private void SetContainer()
        {
            if (container != null)
                return;

            container = new GameObject("2DStreamLines").transform;
            container.transform.parent = rectangleManager.GetInteractiveRectangleContainer();
        }

        private void ReturnAllLinesToPool()
        {
            if (streamLinePool != null)
            {
                foreach (var lineObj in lineObjs)
                {
                    streamLinePool.ReturnToPool(lineObj);
                }
                lineObjs.Clear();
            }
        }

        #endregion private
    }
}