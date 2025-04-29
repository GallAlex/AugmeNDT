using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the visualization of 2D streamlines based on gradient vector field data
    /// Uses Jobard-Lefer algorithm for streamline placement
    /// </summary>
    public class StreamLine2D : MonoBehaviour
    {
        public static StreamLine2D Instance;
        private static Transform container;

        [Header("Jobard-Lefer Parameters")]
        private float dsep;                  // Minimum separation distance between streamlines
        private float dtest;                 // Distance between consecutive tests along a streamline
        public float streamLineStepSize;     // Step size for integration
        private int maxStreamlineSteps;      // Maximum number of steps per streamline
        private float streamlineWidth;       // Thickness of streamlines

        private Color streamlineColor;       // Single color for all streamlines

        private List<GradientDataset> gradientPoints = new List<GradientDataset>();
        public List<GameObject> lineObjs = new List<GameObject>();

        private static RectangleManager rectangleManager;
        private static Glyph2DVectorField glyphInstance;
        private static DuplicateStreamLine2D duplicateStreamLine2D;

        public StreamLineObjectPool streamLinePool;

        // For Jobard-Lefer algorithm
        private List<List<Vector3>> allStreamlines = new List<List<Vector3>>();
        private List<Vector3> candidateList = new List<Vector3>();
        private HashSet<Vector3> processedPoints = new HashSet<Vector3>();

        /// <summary>
        /// Initializes the singleton instance and default values for visualization
        /// </summary>
        private void Awake()
        {
            // Initialize singleton instance
            if (Instance == null)
                Instance = this;

            streamLineStepSize = 0.005f;          // Daha hassas adımlar
            maxStreamlineSteps = 2000;             // Daha uzun streamline'lar
            streamlineWidth = 0.0009f;            // Daha ince çizgiler
            dsep = 0.008f;                        // Çok daha yoğun streamline'lar
            dtest = dsep / 2.0f;                  // Daha sık kontroller


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

            if (streamLinePool == null)
            {
                streamLinePool = StreamLineObjectPool.Instance;
            }

            if (glyphInstance == null && Glyph2DVectorField.Instance != null)
            {
                glyphInstance = Glyph2DVectorField.Instance;
            }

            if (duplicateStreamLine2D == null && DuplicateStreamLine2D.instance != null)
            {
                duplicateStreamLine2D = DuplicateStreamLine2D.instance;
            }

            var config = rectangleManager.config;
            streamlineColor = config.ColorOfStreamLines;
        }

        /// <summary>
        /// Calculates and draws all streamlines using Jobard-Lefer algorithm
        /// </summary>
        public void Visualize()
        {
            // Depends on rectangleManager, we set container in here
            if (container == null)
            {
                container = new GameObject("2DStreamLines").transform;
                container.transform.parent = rectangleManager.volumeTransform;
            }

            // Return to pool instead of Destroy the lines
            ReturnAllLinesToPool();

            gradientPoints.Clear();
            gradientPoints = rectangleManager.GetGradientPoints();

            // Reset collections for Jobard-Lefer algorithm
            allStreamlines.Clear();
            candidateList.Clear();
            processedPoints.Clear();

            // Generate streamlines using Jobard-Lefer algorithm
            List<List<Vector3>> streamLines = GenerateJobardLeferStreamlines();
            StartCoroutine(CreateStreamlinesInBatches(streamLines, 10));
        }

        #region private

        /// <summary>
        /// Implements the Jobard-Lefer algorithm for streamline placement
        /// </summary>
        /// <returns>List of streamline point sequences</returns>
        private List<List<Vector3>> GenerateJobardLeferStreamlines()
        {
            rectangleManager.UpdateWorldCornersManuel();
            Vector3[] corners = rectangleManager.GetRectangleCorners();
            Vector3 normal = rectangleManager.GetRectangleNormal();

            // Compute rectangle bounds for seed placement
            float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float minZ = Mathf.Min(corners[0].z, corners[1].z, corners[2].z, corners[3].z);
            float maxZ = Mathf.Max(corners[0].z, corners[1].z, corners[2].z, corners[3].z);

            // Start with a seed point in the middle of the rectangle
            Vector3 centerSeed = new Vector3(
                (minX + maxX) / 2f,
                (minY + maxY) / 2f,
                (minZ + maxZ) / 2f
            );
            centerSeed = SpatialCalculations.ProjectPointOntoRectanglePlane(centerSeed, corners, normal);

            // Try to find a valid first seed if the center doesn't have significant gradient
            Vector3 firstSeed = centerSeed;
            bool validSeedFound = false;

            // If gradient at center is too small, try a grid of points to find a better seed
            Vector3 gradientAtCenter = SpatialCalculations.GetInterpolatedGradient(firstSeed, gradientPoints);
            if (gradientAtCenter.magnitude < 0.01f)
            {
                int gridSize = 5;
                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        Vector3 testSeed = new Vector3(
                            Mathf.Lerp(minX, maxX, (float)i / (gridSize - 1)),
                            Mathf.Lerp(minY, maxY, (float)j / (gridSize - 1)),
                            (minZ + maxZ) / 2f
                        );
                        testSeed = SpatialCalculations.ProjectPointOntoRectanglePlane(testSeed, corners, normal);

                        if (rectangleManager.IsPointInsideMesh(testSeed))
                        {
                            Vector3 gradient = SpatialCalculations.GetInterpolatedGradient(testSeed, gradientPoints);
                            if (gradient.magnitude > 0.01f)
                            {
                                firstSeed = testSeed;
                                validSeedFound = true;
                                break;
                            }
                        }
                    }
                    if (validSeedFound) break;
                }
            }
            else
            {
                validSeedFound = true;
            }

            if (!validSeedFound)
            {
                Debug.LogWarning("Could not find a good seed point with significant gradient. Using center anyway.");
            }

            // Create the first streamline
            List<Vector3> firstStreamline = GenerateStreamline(firstSeed, corners);
            allStreamlines.Add(firstStreamline);

            // Add candidate points from the first streamline
            AddCandidatesFromStreamline(firstStreamline, dsep);

            // Process all candidate points until no more valid candidates are found
            while (candidateList.Count > 0)
            {
                // Get a random candidate from the list (randomizing improves coverage)
                int randomIndex = UnityEngine.Random.Range(0, candidateList.Count);
                Vector3 seed = candidateList[randomIndex];
                candidateList.RemoveAt(randomIndex);

                // Skip if this point has already been processed
                if (processedPoints.Contains(seed))
                    continue;

                processedPoints.Add(seed);

                // Check if this seed point is valid (far enough from existing streamlines)
                if (IsValidSeedPoint(seed, dsep / 3f))
                {
                    // Generate a new streamline
                    List<Vector3> newStreamline = GenerateStreamline(seed, corners);

                    if (newStreamline.Count > 5) // Only add if streamline has enough points
                    {
                        allStreamlines.Add(newStreamline);
                        // Add new candidate points from this streamline
                        AddCandidatesFromStreamline(newStreamline, dsep);
                    }
                }
            }

            Debug.Log($"Generated {allStreamlines.Count} streamlines using Jobard-Lefer algorithm");
            return allStreamlines;
        }

        /// <summary>
        /// Adds candidate seed points along a streamline at distance dsep
        /// </summary>
        /// <param name="streamline">The streamline to generate candidates from</param>
        /// <param name="dsep">Separation distance</param>
        private void AddCandidatesFromStreamline(List<Vector3> streamline, float dsep)
        {
            if (streamline.Count < 2)
                return;

            // For each point in the streamline
            for (int i = 0; i < streamline.Count; i += Mathf.Max(1, Mathf.FloorToInt(dsep / streamLineStepSize / 2)))
            {
                if (i >= streamline.Count) break;

                Vector3 point = streamline[i];

                // Get the tangent vector (direction of the streamline at this point)
                Vector3 tangent;
                if (i < streamline.Count - 1)
                    tangent = (streamline[i + 1] - point).normalized;
                else if (i > 0)
                    tangent = (point - streamline[i - 1]).normalized;
                else
                    continue;

                // Get the normal vector (perpendicular to the tangent and the rectangle normal)
                Vector3 rectNormal = rectangleManager.GetRectangleNormal();
                Vector3 normal = Vector3.Cross(tangent, rectNormal).normalized;

                // Create candidate points on both sides of the streamline
                Vector3 candidate1 = point + normal * dsep;
                Vector3 candidate2 = point - normal * dsep;

                // Project candidates onto the rectangle plane
                Vector3[] corners = rectangleManager.GetRectangleCorners();
                candidate1 = SpatialCalculations.ProjectPointOntoRectanglePlane(candidate1, corners, rectNormal);
                candidate2 = SpatialCalculations.ProjectPointOntoRectanglePlane(candidate2, corners, rectNormal);

                // Add candidates if they're inside the rectangle
                if (rectangleManager.IsPointInsideMesh(candidate1))
                {
                    candidateList.Add(candidate1);
                }

                if (rectangleManager.IsPointInsideMesh(candidate2))
                {
                    candidateList.Add(candidate2);
                }
            }
        }

        /// <summary>
        /// Checks if a seed point is valid (far enough from existing streamlines)
        /// </summary>
        /// <param name="seedPoint">The point to check</param>
        /// <param name="minDist">Minimum distance required from existing streamlines</param>
        /// <returns>True if the point is valid, false otherwise</returns>
        private bool IsValidSeedPoint(Vector3 seedPoint, float minDist)
        {
            // Check distance to all existing streamlines
            foreach (var streamline in allStreamlines)
            {
                foreach (var point in streamline)
                {
                    float distance = Vector3.Distance(seedPoint, point);
                    if (distance < minDist)
                        return false;
                }
            }
            return true;
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

            // Generate the streamline in forward direction
            List<Vector3> forwardPoints = GenerateStreamlineInDirection(currentPosition, corners, true);

            // Generate the streamline in backward direction
            List<Vector3> backwardPoints = GenerateStreamlineInDirection(currentPosition, corners, false);

            // Combine the two parts
            backwardPoints.Reverse();
            points.AddRange(backwardPoints);

            // Don't add the start position twice
            if (forwardPoints.Count > 0)
                points.AddRange(forwardPoints);

            return points;
        }

        /// <summary>
        /// Generates a streamline in one direction (forward or backward)
        /// </summary>
        /// <param name="startPosition">Starting position</param>
        /// <param name="corners">Rectangle corners</param>
        /// <param name="forward">Direction (true for forward, false for backward)</param>
        /// <returns>List of points for the streamline segment</returns>
        private List<Vector3> GenerateStreamlineInDirection(Vector3 startPosition, Vector3[] corners, bool forward)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3 currentPosition = startPosition;

            // Add the starting position to the streamline
            points.Add(currentPosition);

            Vector3 normal = rectangleManager.GetRectangleNormal();
            float directionMultiplier = forward ? 1.0f : -1.0f;

            for (int i = 0; i < maxStreamlineSteps / 2; i++)
            {
                // Calculate the next position with using RungeKutta4
                Vector3 nextPosition = SpatialCalculations.RungeKutta4(
                    currentPosition,
                    gradientPoints,
                    streamLineStepSize * directionMultiplier,
                    true
                );

                // Project the point back onto the rectangle plane to prevent drift
                nextPosition = SpatialCalculations.ProjectPointOntoRectanglePlane(nextPosition, corners, normal);

                // Check if movement is too small (stagnation point)
                if (Vector3.Distance(nextPosition, currentPosition) < streamLineStepSize * 0.1f)
                    break;

                // Check if the new position is still inside the rectangle
                if (!rectangleManager.IsPointInsideMesh(nextPosition, true))
                {
                    // Find the intersection with the rectangle boundary
                    Vector3 boundaryPoint = FindIntersectionWithRectangleBoundary(currentPosition, nextPosition, corners);
                    if (boundaryPoint != Vector3.zero)
                        points.Add(boundaryPoint);
                    break;
                }

                // Check distance to other streamlines for early termination
                bool tooClose = false;
                foreach (var streamline in allStreamlines)
                {
                    if (IsPointTooCloseToStreamline(nextPosition, streamline, dsep / 3f))
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                    break;

                // Add the new position to our streamline
                points.Add(nextPosition);
                currentPosition = nextPosition;
            }

            return points;
        }

        /// <summary>
        /// Checks if a point is too close to any point in a streamline
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <param name="streamline">The streamline to check against</param>
        /// <param name="minDist">Minimum acceptable distance</param>
        /// <returns>True if the point is too close, false otherwise</returns>
        private bool IsPointTooCloseToStreamline(Vector3 point, List<Vector3> streamline, float minDist)
        {
            foreach (var streamlinePoint in streamline)
            {
                if (Vector3.Distance(point, streamlinePoint) < minDist)
                    return true;
            }
            return false;
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

            if(duplicateStreamLine2D != null)
            {
                DuplicateStreamLine2D.instance.UpdateDuplicateStreamlines(rectangleManager.GetRectangleNormal(),
                    rectangleManager.GetRectangleBounds(), rectangleManager.GetRectangleCorners(),
                    streamLines,streamlineWidth,streamlineColor,gradientPoints);
            }

            if (glyphInstance != null)
            {
                Glyph2DVectorField.Instance.Visualize(rectangleManager.GetRectangleNormal(),
                    rectangleManager.GetRectangleBounds(),
                    rectangleManager.GetRectangleCorners(),
                    gradientPoints);
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
            lr.alignment = LineAlignment.View;
            lr.numCapVertices = lr.numCornerVertices = 5;

            lr.startColor = lr.endColor = streamlineColor;

            lineObjs.Add(lineObj);
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