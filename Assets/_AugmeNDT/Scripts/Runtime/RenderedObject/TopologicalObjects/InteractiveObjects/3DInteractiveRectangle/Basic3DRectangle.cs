namespace AugmeNDT
{
    using Newtonsoft.Json.Bson;
    using UnityEngine;

    /// <summary>
    /// Simplified wireframe cube creation and management class
    /// </summary>
    public class Basic3DRectangle : MonoBehaviour
    {
        private GameObject rectangleObject;
        private LineRenderer lineRenderer;
        public bool drawBorders = true;
        private Matrix4x4 worldToLocalMatrix; // For thread-safe usage

        /// <summary>
        /// Initializes the LineRenderer component
        /// </summary>
        private void Awake()
        {
            // Create LineRenderer holder object
            rectangleObject = new GameObject("RectangleVisual");
            rectangleObject.transform.SetParent(transform);
        }

        /// <summary>
        /// Sets rectangle bounds in local space
        /// </summary>
        /// <param name="localMin">Minimum corner in local space</param>
        /// <param name="localMax">Maximum corner in local space</param>
        public void InitializeBoundsLocal(Vector3 localMin, Vector3 localMax)
        {
            // Calculate center in local space
            Vector3 localCenter = (localMin + localMax) * 0.5f;

            // Calculate size in local space
            Vector3 localSize = localMax - localMin;

            // Set object's local position
            transform.localPosition = localCenter;

            // Set object's local scale
            transform.localScale = localSize;

            // Manually update cached matrix
            worldToLocalMatrix = transform.worldToLocalMatrix;

            // Add and configure LineRenderer if borders are requested
            if (drawBorders)
            {
                // Clear existing LineRenderer (if exists)
                if (lineRenderer != null)
                {
                    Destroy(lineRenderer);
                }

                lineRenderer = rectangleObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.yellow;
                lineRenderer.endColor = Color.yellow;
                lineRenderer.startWidth = 0.001f; // Increase thickness for better visibility
                lineRenderer.endWidth = 0.001f;
                lineRenderer.positionCount = 24; // 12 edges, 2 points per edge
                lineRenderer.useWorldSpace = false; // Work in local space

                // Reset RectangleVisual transform
                rectangleObject.transform.localPosition = Vector3.zero;
                rectangleObject.transform.localRotation = Quaternion.identity;
                rectangleObject.transform.localScale = Vector3.one;

                // Update LineRenderer positions
                UpdateLinePositions();
            }
        }

        /// <summary>
        /// Calculates and returns a Bounds object that encapsulates the rectangle
        /// </summary>
        public Bounds GetBounds()
        {
            // Define the corners of a unit cube (-0.5, -0.5, -0.5) to (0.5, 0.5, 0.5)
            Vector3[] unitCorners = new Vector3[8]
            {
                new Vector3(-0.5f, -0.5f, -0.5f), // bottom-left-front
                new Vector3(0.5f, -0.5f, -0.5f),  // bottom-right-front
                new Vector3(0.5f, -0.5f, 0.5f),   // bottom-right-back
                new Vector3(-0.5f, -0.5f, 0.5f),  // bottom-left-back
                new Vector3(-0.5f, 0.5f, -0.5f),  // top-left-front
                new Vector3(0.5f, 0.5f, -0.5f),   // top-right-front
                new Vector3(0.5f, 0.5f, 0.5f),    // top-right-back
                new Vector3(-0.5f, 0.5f, 0.5f)    // top-left-back
            };

            // Apply local to world transformation
            Matrix4x4 worldMatrix = transform.localToWorldMatrix;

            // Initialize bounds using the first corner
            Vector3 firstCorner = worldMatrix.MultiplyPoint(unitCorners[0]);
            Bounds bounds = new Bounds(firstCorner, Vector3.zero);

            // Expand bounds to include all corners
            for (int i = 1; i < unitCorners.Length; i++)
            {
                bounds.Encapsulate(worldMatrix.MultiplyPoint(unitCorners[i]));
            }

            return bounds;
        }

        /// <summary>
        /// Updates the cached world-to-local matrix manually
        /// </summary>
        public void SetBoundsManuelUpdated()
        {
            worldToLocalMatrix = transform.worldToLocalMatrix;
        }

        /// <summary>
        /// Checks if a given world point is inside the bounds of the rectangle
        /// </summary>
        /// <param name="worldPoint">World space point to check</param>
        /// <param name="useCachedBounds">Use cached matrix if true (thread-safe)</param>
        /// <returns>True if inside bounds, otherwise false</returns>
        public bool ContainsPointUsingBounds(Vector3 worldPoint, bool useCachedBounds)
        {
            if (!useCachedBounds)
            {
                // Non-thread-safe path, must be called from the main thread
                Vector3 lp = transform.InverseTransformPoint(worldPoint);
                return IsPointInUnitCube(lp);
            }

            // Thread-safe path using cached matrix
            Vector3 localPoint = worldToLocalMatrix.MultiplyPoint3x4(worldPoint);
            return IsPointInUnitCube(localPoint);
        }

        /// <summary>
        /// Helper method to check if a point is inside a unit cube with small epsilon tolerance
        /// </summary>
        private bool IsPointInUnitCube(Vector3 localPoint)
        {
            //float epsilon = 0.0001f;
            float epsilon = 0f;
            return (localPoint.x >= -0.5f - epsilon && localPoint.x <= 0.5f + epsilon &&
                    localPoint.y >= -0.5f - epsilon && localPoint.y <= 0.5f + epsilon &&
                    localPoint.z >= -0.5f - epsilon && localPoint.z <= 0.5f + epsilon);
        }

        /// <summary>
        /// Updates the LineRenderer positions to visualize the cube edges
        /// </summary>
        private void UpdateLinePositions()
        {
            if (lineRenderer == null) return;

            // Configure LineRenderer for 12 edges (2 points per edge)
            lineRenderer.positionCount = 24;

            // Define corners of a unit cube
            Vector3[] unitCorners = new Vector3[8]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f)
            };

            int index = 0;

            // Bottom face edges (0-1-2-3)
            lineRenderer.SetPosition(index++, unitCorners[0]);
            lineRenderer.SetPosition(index++, unitCorners[1]);

            lineRenderer.SetPosition(index++, unitCorners[1]);
            lineRenderer.SetPosition(index++, unitCorners[2]);

            lineRenderer.SetPosition(index++, unitCorners[2]);
            lineRenderer.SetPosition(index++, unitCorners[3]);

            lineRenderer.SetPosition(index++, unitCorners[3]);
            lineRenderer.SetPosition(index++, unitCorners[0]);

            // Top face edges (4-5-6-7)
            lineRenderer.SetPosition(index++, unitCorners[4]);
            lineRenderer.SetPosition(index++, unitCorners[5]);

            lineRenderer.SetPosition(index++, unitCorners[5]);
            lineRenderer.SetPosition(index++, unitCorners[6]);

            lineRenderer.SetPosition(index++, unitCorners[6]);
            lineRenderer.SetPosition(index++, unitCorners[7]);

            lineRenderer.SetPosition(index++, unitCorners[7]);
            lineRenderer.SetPosition(index++, unitCorners[4]);

            // Side edges (0-4, 1-5, 2-6, 3-7)
            lineRenderer.SetPosition(index++, unitCorners[0]);
            lineRenderer.SetPosition(index++, unitCorners[4]);

            lineRenderer.SetPosition(index++, unitCorners[1]);
            lineRenderer.SetPosition(index++, unitCorners[5]);

            lineRenderer.SetPosition(index++, unitCorners[2]);
            lineRenderer.SetPosition(index++, unitCorners[6]);

            lineRenderer.SetPosition(index++, unitCorners[3]);
            lineRenderer.SetPosition(index++, unitCorners[7]);
        }

    }
}
