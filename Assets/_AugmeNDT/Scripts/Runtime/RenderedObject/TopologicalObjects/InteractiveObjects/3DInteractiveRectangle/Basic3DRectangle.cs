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
        private Vector3 minCorner { get; set; }
        private Vector3 maxCorner { get; set; }
        private Bounds boundsManuelUpdated;

        /// <summary>
        /// Initializes the LineRenderer component
        /// </summary>
        private void Awake()
        {
            // Create LineRenderer component
            rectangleObject = new GameObject("RectangleVisual");
            rectangleObject.transform.SetParent(transform);
        }

        /// <summary>
        /// Sets the boundaries of the rectangle and initializes visualization
        /// </summary>
        /// <param name="min">Minimum corner position (x,y,z)</param>
        /// <param name="max">Maximum corner position (x,y,z)</param>
        public void SetBounds(Vector3 min, Vector3 max)
        {
            minCorner = min;
            maxCorner = max;
            boundsManuelUpdated = GetBounds();

            if (drawBorders)
            {
                lineRenderer = rectangleObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.yellow;
                lineRenderer.endColor = Color.yellow;
                lineRenderer.startWidth = 0.001f;
                lineRenderer.endWidth = 0.001f;
                lineRenderer.positionCount = 24; // 24 points for 12 edges
                lineRenderer.useWorldSpace = false;

                UpdateLinePositions();
            }
        }

        /// <summary>
        /// Calculates and returns a Bounds object that encapsulates the rectangle
        /// </summary>
        /// <returns>Bounds object representing the rectangle</returns>
        public Bounds GetBounds()
        {
            Vector3[] corners = GetCorners();

            // Initialize Bounds with the first corner
            Bounds bounds = new Bounds(corners[0], Vector3.zero);

            // Include all other corners in the Bounds
            for (int i = 1; i < corners.Length; i++)
            {
                bounds.Encapsulate(corners[i]);
            }

            return bounds;
        }

        /// <summary>
        /// Updates the manually cached bounds value
        /// </summary>
        public void SetBoundsManuelUpdated()
        {
            boundsManuelUpdated = GetBounds();
        }

        /// <summary>
        /// Checks if a point is inside the rectangle bounds
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <param name="useBoundsManuelUpdated">Whether to use manually updated bounds or recalculate</param>
        /// <returns>True if the point is inside the bounds</returns>
        public bool ContainsPointUsingBounds(Vector3 point, bool useBoundsManuelUpdated)
        {
            Bounds bounds = useBoundsManuelUpdated ? boundsManuelUpdated : GetBounds();

            // Use epsilon value for boundary checking
            float epsilon = 0.0001f;
            Bounds expandedBounds = new Bounds(bounds.center, bounds.size + new Vector3(epsilon, epsilon, epsilon) * 2);
            return expandedBounds.Contains(point);
        }

        /// <summary>
        /// Updates the LineRenderer positions to display the wireframe cube
        /// </summary>
        private void UpdateLinePositions()
        {
            if (lineRenderer == null) return;

            Vector3[] corners = GetCorners();

            // Configure LineRenderer for 12 separate edges (each edge contains 2 points)
            lineRenderer.positionCount = 24;

            // Bottom surface 4 edges
            int index = 0;
            // Edge 1: corners[0] -> corners[1]
            lineRenderer.SetPosition(index++, corners[0]);
            lineRenderer.SetPosition(index++, corners[1]);

            // Edge 2: corners[1] -> corners[2]
            lineRenderer.SetPosition(index++, corners[1]);
            lineRenderer.SetPosition(index++, corners[2]);

            // Edge 3: corners[2] -> corners[3]
            lineRenderer.SetPosition(index++, corners[2]);
            lineRenderer.SetPosition(index++, corners[3]);

            // Edge 4: corners[3] -> corners[0]
            lineRenderer.SetPosition(index++, corners[3]);
            lineRenderer.SetPosition(index++, corners[0]);

            // Top surface 4 edges
            // Edge 5: corners[4] -> corners[5]
            lineRenderer.SetPosition(index++, corners[4]);
            lineRenderer.SetPosition(index++, corners[5]);

            // Edge 6: corners[5] -> corners[6]
            lineRenderer.SetPosition(index++, corners[5]);
            lineRenderer.SetPosition(index++, corners[6]);

            // Edge 7: corners[6] -> corners[7]
            lineRenderer.SetPosition(index++, corners[6]);
            lineRenderer.SetPosition(index++, corners[7]);

            // Edge 8: corners[7] -> corners[4]
            lineRenderer.SetPosition(index++, corners[7]);
            lineRenderer.SetPosition(index++, corners[4]);

            // Side edges (4 vertical edges)
            // Edge 9: corners[0] -> corners[4]
            lineRenderer.SetPosition(index++, corners[0]);
            lineRenderer.SetPosition(index++, corners[4]);

            // Edge 10: corners[1] -> corners[5]
            lineRenderer.SetPosition(index++, corners[1]);
            lineRenderer.SetPosition(index++, corners[5]);

            // Edge 11: corners[2] -> corners[6]
            lineRenderer.SetPosition(index++, corners[2]);
            lineRenderer.SetPosition(index++, corners[6]);

            // Edge 12: corners[3] -> corners[7]
            lineRenderer.SetPosition(index++, corners[3]);
            lineRenderer.SetPosition(index++, corners[7]);
        }

        /// <summary>
        /// Calculates the 8 corner points of the rectangle - with corrected Z coordinate order
        /// </summary>
        /// <returns>Array of 8 Vector3 corner positions</returns>
        private Vector3[] GetCorners()
        {
            Vector3[] corners = new Vector3[8];
            // Bottom corners (clockwise)
            corners[0] = new Vector3(minCorner.x, minCorner.y, minCorner.z); // front left bottom
            corners[1] = new Vector3(maxCorner.x, minCorner.y, minCorner.z); // front right bottom
            corners[2] = new Vector3(maxCorner.x, minCorner.y, maxCorner.z); // back right bottom
            corners[3] = new Vector3(minCorner.x, minCorner.y, maxCorner.z); // back left bottom

            // Top corners (clockwise)
            corners[4] = new Vector3(minCorner.x, maxCorner.y, minCorner.z); // front left top
            corners[5] = new Vector3(maxCorner.x, maxCorner.y, minCorner.z); // front right top
            corners[6] = new Vector3(maxCorner.x, maxCorner.y, maxCorner.z); // back right top
            corners[7] = new Vector3(minCorner.x, maxCorner.y, maxCorner.z); // back left top

            return corners;
        }
    }
}