namespace AugmeNDT
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using Assets.Scripts.DataStructure;
    using System;

    /// <summary>
    /// Manages the 2D rectangle for visualization and analysis of vector field data on a plane
    /// </summary>
    public class RectangleManager : MonoBehaviour
    {
        public static RectangleManager rectangleManager;
        public static bool supportedByTTK = false; // TTK PARAVIEW API ERROR

        private Material handleMaterial;
        private float handleRadius = 0.25f;
        private InteractiveRectangle rectangle;
        List<Vector3> defaultCorners = new List<Vector3>() {
                new Vector3(10f, 9f, 22f),
                new Vector3(8f, 9f, 13f),
                new Vector3(18f, 10f, 10f),
                new Vector3(20f, 10f, 19f)
            };


        // TDA
        private List<GradientDataset> gradientPoints = new List<GradientDataset>();
        private List<CriticalPointDataset> criticalPoints = new List<CriticalPointDataset>();
        private static TopologicalDataObject topologicalDataObjectInstance;

        // Tracking parameters
        private Vector3[] lastKnownCorners;
        private Vector3 lastKnownNormal;
        private Bounds lastKnownBounds;

        private void Awake()
        {
            // Initialize singleton instance
            rectangleManager = this;
        }

        private void Start()
        {
            // Get reference to topological data object
            if (topologicalDataObjectInstance == null)
                topologicalDataObjectInstance = TopologicalDataObject.Instance;
        }

        /// <summary>
        /// Shows the rectangle and its corner handles
        /// </summary>
        public void ShowRectangle()
        {
            if (rectangle == null || rectangle.gameObject == null)
            {
                CreateRectangle(defaultCorners);
            }
            else
            {
                IsUpdated();
                rectangle.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hides the rectangle and its corner handles
        /// </summary>
        public void HideRectangle()
        {
            if (rectangle != null && rectangle.gameObject != null)
            {
                rectangle.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Checks if the rectangle has been modified since the last update
        /// </summary>
        /// <returns>True if the rectangle has been updated (position, size, or orientation changed)</returns>
        public bool IsUpdated()
        {
            bool IsUpdated = false;

            if (rectangle == null)
                return IsUpdated;

            // Check if corners have changed
            Vector3[] currentCorners = rectangle.GetCornerPositions();
            if (lastKnownCorners != null && currentCorners.Length == lastKnownCorners.Length)
            {
                for (int i = 0; i < currentCorners.Length; i++)
                {
                    if (Vector3.Distance(currentCorners[i], lastKnownCorners[i]) > 0.001f)
                    {
                        IsUpdated = true;
                    }
                }
            }

            // Check if normal has changed
            Vector3 currentNormal = rectangle.GetNormal();
            if (Vector3.Angle(currentNormal, lastKnownNormal) > 0.1f)
            {
                IsUpdated = true;
            }

            // Check if bounds have changed
            Bounds currentBounds = rectangle.GetBounds();
            if (Vector3.Distance(currentBounds.center, lastKnownBounds.center) > 0.001f ||
                Vector3.Distance(currentBounds.size, lastKnownBounds.size) > 0.001f)
            {
                IsUpdated = true;
            }

            return IsUpdated;
        }

        /// <summary>
        /// Gets the gradient points inside or on the rectangle
        /// </summary>
        /// <returns>List of gradient data points</returns>
        public List<GradientDataset> GetGradientPoints()
        {
            if (!gradientPoints.Any() || IsUpdated())
                UpdateInstance();

            return gradientPoints;
        }

        #region topological data analysis
        /// <summary>
        /// Updates the instance state and recalculates gradient and critical points
        /// </summary>
        private void UpdateInstance()
        {
            UpdateSaveCurrentState();
            CalculateGradientPoints();
            CalculateCriticalPoints();
        }

        /// <summary>
        /// Returns a grid of points on the rectangle surface with 0.5 unit interval
        /// </summary>
        /// <param name="interval">Interval between points (default: 0.5)</param>
        /// <returns>List of gradient data points on the rectangle surface</returns>
        private List<GradientDataset> GetRectangleGridPoints(float interval = 0.5f)
        {
            List<GradientDataset> gridPoints = new List<GradientDataset>();

            if (rectangle == null)
                return gridPoints;

            // Get the four corners
            Vector3[] corners = rectangle.GetCornerPositions();
            if (corners.Length != 4)
                return gridPoints;

            // Calculate the normal of the rectangle
            Vector3 normal = rectangle.GetNormal();

            // Calculate basis vectors for the rectangle plane
            Vector3 edge1 = corners[1] - corners[0];
            Vector3 edge2 = corners[3] - corners[0];

            // Calculate lengths of edges
            float width = edge1.magnitude;
            float height = edge2.magnitude;

            // Calculate normalized direction vectors
            Vector3 dirWidth = edge1.normalized;
            Vector3 dirHeight = edge2.normalized;

            // Calculate number of steps in each direction
            int stepsWidth = Mathf.CeilToInt(width / interval);
            int stepsHeight = Mathf.CeilToInt(height / interval);

            // Generate grid points
            for (int w = 0; w <= stepsWidth; w++)
            {
                float wRatio = (stepsWidth > 0) ? (float)w / stepsWidth : 0;

                for (int h = 0; h <= stepsHeight; h++)
                {
                    float hRatio = (stepsHeight > 0) ? (float)h / stepsHeight : 0;

                    // Bilinear interpolation to handle non-rectangular shapes
                    Vector3 point = corners[0] * (1 - wRatio) * (1 - hRatio) +
                                   corners[1] * wRatio * (1 - hRatio) +
                                   corners[3] * (1 - wRatio) * hRatio +
                                   corners[2] * wRatio * hRatio;

                    GradientDataset gradientDataset = new GradientDataset(gridPoints.Count, point, new Vector3(0, 0, 0), 0);

                    // Add point to result
                    gridPoints.Add(gradientDataset);
                }
            }

            return gridPoints;
        }

        /// <summary>
        /// Calculates gradient points on the rectangle plane
        /// </summary>
        private void CalculateGradientPoints()
        {
            gradientPoints.Clear();
            bool tkkGradientUsed = false;
            List<GradientDataset> sourceGradientPoints = topologicalDataObjectInstance.gradientList;

            if (supportedByTTK)
            {
                var normal = GetRectangleNormal();
                var minMaxVal = GetMinMaxValuesOfBox();
                var sliceGradientPoints = TTKCalculations.GetGradient2DSlice(rectangle.transform.position, normal, minMaxVal);
                if (sliceGradientPoints.Any())
                {
                    sliceGradientPoints = sliceGradientPoints.Select(x => x).Where(x => rectangleManager.IsPointInsideMesh(x.Position)).ToList();
                    sourceGradientPoints = sliceGradientPoints;
                    tkkGradientUsed = true;
                }
            }

            gradientPoints = GradientUtils.AssignNewGradientValues(GetRectangleGridPoints(), sourceGradientPoints);

            if (!tkkGradientUsed)
                gradientPoints = GradientUtils.NormalizeGradientsToRectangle(gradientPoints, rectangle.GetCornerPositions());

            gradientPoints = GaussianFilterUtils.ApplyGaussianSmoothing(gradientPoints, 0.5f);
        }

        /// <summary>
        /// Calculates critical points on the rectangle plane using TTK
        /// </summary>
        private void CalculateCriticalPoints()
        {
            if (supportedByTTK)
            {
                var normal = GetRectangleNormal();
                var minMaxVal = GetMinMaxValuesOfBox();

                criticalPoints = TTKCalculations.GetCriticalpoint2DSlice(rectangle.transform.position, normal, minMaxVal);
                criticalPoints = criticalPoints.Select(x => x).Where(x => rectangleManager.IsPointInsideMesh(x.Position)).ToList();
            }
        }

        #endregion topological data analysis

        #region rectangle utilities
        /// <summary>
        /// Gets the normal vector of the rectangle
        /// </summary>
        /// <returns>Normal vector of the rectangle plane</returns>
        public Vector3 GetRectangleNormal()
        {
            return rectangle.GetNormal();
        }

        /// <summary>
        /// Gets the bounding box of the rectangle
        /// </summary>
        /// <returns>Bounds containing the rectangle</returns>
        public Bounds GetRectangleBounds()
        {
            return rectangle.GetBounds();
        }

        /// <summary>
        /// Checks if a point is inside the rectangle
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <returns>True if the point is inside the rectangle</returns>
        public bool IsPointInsideMesh(Vector3 point)
        {
            return rectangle.IsPointInsideMesh(point);
        }

        /// <summary>
        /// Gets the corner positions of the rectangle
        /// </summary>
        /// <returns>Array of Vector3 positions for the rectangle corners</returns>
        public Vector3[] GetRectangleCorners()
        {
            return rectangle.GetCornerPositions();
        }

        /// <summary>
        /// Save the current state of the rectangle as the reference state
        /// Call this after processing any changes to reset the "updated" flag
        /// </summary>
        private void UpdateSaveCurrentState()
        {
            if (rectangle == null)
                return;

            lastKnownCorners = rectangle.GetCornerPositions();
            lastKnownNormal = rectangle.GetNormal();
            lastKnownBounds = rectangle.GetBounds();
        }

        /// <summary>
        /// Creates a new interactive rectangle with the given corner positions
        /// </summary>
        /// <param name="corners">List of corner positions (should have 4 elements)</param>
        private void CreateRectangle(List<Vector3> corners)
        {
            // Create parent GameObject
            GameObject rectangleObj = new GameObject("InteractiveRectangle");

            // Add the InteractiveRectangle component
            rectangle = rectangleObj.AddComponent<InteractiveRectangle>();

            // Initialize the rectangle
            rectangle.InitializeWithCorners(corners.ToArray(), handleMaterial, handleRadius);
        }

        /// <summary>
        /// Gets the minimum and maximum values of the rectangle in each axis
        /// </summary>
        /// <returns>List containing [minX, maxX, minY, maxY, minZ, maxZ]</returns>
        private List<int> GetMinMaxValuesOfBox()
        {
            if (rectangle != null)
            {
                // Get the corner points
                Vector3[] corners = rectangle.GetCornerPositions();

                // Create bounding box
                Bounds bounds = rectangle.GetBounds();
                Vector3 size = bounds.size;
                Vector3 center = bounds.center;

                // Calculate min and max values for each axis
                int minX = (int)Math.Floor(center.x - (size.x / 2));
                int maxX = (int)Math.Ceiling(center.x + (size.x / 2));
                int minY = (int)Math.Floor(center.y - (size.y / 2));
                int maxY = (int)Math.Ceiling(center.y + (size.y / 2));
                int minZ = (int)Math.Floor(center.z - (size.z / 2));
                int maxZ = (int)Math.Ceiling(center.z + (size.z / 2));

                return new List<int>(new[] { minX, maxX, minY, maxY, minZ, maxZ });
            }
            return new List<int>();
        }

        #endregion rectangle utilities
    }
}