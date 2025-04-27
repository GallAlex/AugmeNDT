namespace AugmeNDT
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using System;

    /// <summary>
    /// Manages the 2D rectangle for visualization and analysis of vector field data on a plane
    /// </summary>
    public class RectangleManager : MonoBehaviour
    {
        public static RectangleManager rectangleManager;

        public TopologyConfigData config;
        /// <summary>
        /// Controls the density of the grid used for gradient calculations.
        /// Lower values create denser grids with more detail but higher processing cost.
        /// </summary>
        private float defaultInterval;
        /// Scale factor for calculation grid density, automatically adjusted based on volume dimensions.
        public float scaleRateToCalculation;
        public float intervalValue; // defaultInterval*scaleRateToCalculation

        private InteractiveRectangle rectangle;

        private TTKCalculations ttkCalculations;
        public Transform volumeTransform;

        // TDA
        private List<GradientDataset> gradientPoints = new List<GradientDataset>();
        private List<CriticalPointDataset> criticalPoints = new List<CriticalPointDataset>();
        private static TopologicalDataObject topologicalDataObjectInstance;
        private bool supportedByTTK = false; // true means run-time calculation (TTK PARAVIEW API) (slow)

        // Tracking parameters
        private Vector3[] lastKnownCorners;
        private Vector3 lastKnownNormal;
        private Bounds lastKnownBounds;

        /// <summary>
        /// Initializes the singleton instance
        /// </summary>
        private void Awake()
        {
            // Initialize singleton instance
            rectangleManager = this;
        }

        /// <summary>
        /// Sets up references and configures calculation parameters
        /// </summary>
        private void Start()
        {
            // Get reference to topological data object
            if (topologicalDataObjectInstance == null)
            {
                topologicalDataObjectInstance = TopologicalDataObject.instance;
                volumeTransform = topologicalDataObjectInstance.volumeTransform;
                ttkCalculations = topologicalDataObjectInstance.ttkCalculation;
                scaleRateToCalculation = topologicalDataObjectInstance.GetOptimalScaleRateToCalculation();

                config = topologicalDataObjectInstance.config;
                defaultInterval = 0.1f; //default
            }


            intervalValue = defaultInterval * scaleRateToCalculation;
        }

        /// <summary>
        /// Shows the rectangle and its corner handles
        /// </summary>
        public void ShowRectangle()
        {
            if (rectangle == null || rectangle.gameObject == null)
            {
                CreateRectangle();
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
        /// Returns the transform that contains the interactive rectangle for parenting purposes
        /// </summary>
        /// <returns>The parent transform for interactive rectangle objects</returns>
        public Transform GetInteractiveRectangleContainer()
        {
            return volumeTransform;
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

        /// <summary>
        /// Recreates the rectangle when the volume has been scaled
        /// </summary>
        public void UpdateRectangleAfterScaling()
        {
            if (rectangle == null)
                return;

            Destroy(rectangle.gameObject.GetComponent<InteractiveRectangle>());
            Destroy(volumeTransform.Find("Rectangle").gameObject);
            rectangle = null;
            CreateRectangle();

            UpdateInstance();
        }

        #region topological data analysis
        /// <summary>
        /// Updates the instance state and recalculates gradient and critical points
        /// </summary>
        private void UpdateInstance()
        {
            if (rectangle == null)
                return;

            UpdateSaveCurrentState();
            CalculateGradientPoints();
            CalculateCriticalPoints();
        }

        /// <summary>
        /// Returns a grid of points on the rectangle surface with interval
        /// </summary>
        /// <param name="interval">Interval between points (default: 0.5)</param>
        /// <returns>List of gradient data points on the rectangle surface</returns>
        private List<GradientDataset> GetRectangleGridPoints(float interval)
        {

            if (rectangle == null)
                return new List<GradientDataset>();

            // Get the four corners
            Vector3[] corners = rectangle.GetCornerPositions();
            if (corners.Length != 4)
                return new List<GradientDataset>();

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

            List<GradientDataset> gridPoints = new List<GradientDataset>();
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
        /// Creates a test sphere at the specified position for debugging
        /// </summary>
        /// <param name="position">Position to place the sphere</param>
        /// <returns>The created sphere GameObject</returns>
        private GameObject TEST(Vector3 position)
        {
            // Create and configure sphere object
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * 0.003f;
            return sphere;
        }

        /// <summary>
        /// Calculates gradient points on the rectangle plane
        /// </summary>
        private void CalculateGradientPoints()
        {
            List<GradientDataset> pointsOnSurface = GetRectangleGridPoints(intervalValue);

            gradientPoints.Clear();

            List<GradientDataset> sourceGradientPoints = topologicalDataObjectInstance.gradientList;
            gradientPoints = GradientUtils.AssignNewGradientValuesParallel(pointsOnSurface, sourceGradientPoints);
            gradientPoints = GradientUtils.NormalizeGradientsToRectangleParallel(gradientPoints, rectangle.GetCornerPositions());
            gradientPoints = GaussianFilterUtils.ApplyGaussianSmoothingParallel(gradientPoints, intervalValue);
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

                criticalPoints = ttkCalculations.GetCriticalpoint2DSlice(rectangle.transform.position, normal, minMaxVal);
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
        /// <param name="useWorldCornersManuelUpdated"> Only used for 2DStreamline Calculation 
        /// MUST UPDATED before start threads</param>
        /// <returns>True if the point is inside the rectangle</returns>
        public bool IsPointInsideMesh(Vector3 point, bool useWorldCornersManuelUpdated = false)
        {
            return rectangle.IsPointInsideMesh(point, useWorldCornersManuelUpdated);
        }

        /// <summary>
        /// Updates cached corner positions for thread-safe operations
        /// </summary>
        public void UpdateWorldCornersManuel()
        {
            rectangle.UpdateWorldCornersManuel();
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
        /// Old method to create a rectangle - kept for reference
        /// </summary>
        private void CreateRectangleOLD()
        {
            if (rectangle != null)
                return;

            // Get the BoxCollider component
            BoxCollider boxCollider = volumeTransform.gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                UnityEngine.Debug.LogError("BoxCollider not found on the volumetric object!");
                return;
            }

            // Get the world-space center and extents
            Vector3 center = boxCollider.bounds.center;
            Vector3 extents = boxCollider.bounds.extents;

            // Calculate the four corners of the rectangle in the x-z plane
            // We're keeping y constant at the center's y value
            List<Vector3> corners = new List<Vector3>();

            // Bottom-left (min X, center Y, min Z)
            corners.Add(new Vector3(center.x - extents.x, center.y, center.z - extents.z));

            // Bottom-right (max X, center Y, min Z)
            corners.Add(new Vector3(center.x + extents.x, center.y, center.z - extents.z));

            // Top-right (max X, center Y, max Z)
            corners.Add(new Vector3(center.x + extents.x, center.y, center.z + extents.z));

            // Top-left (min X, center Y, max Z)
            corners.Add(new Vector3(center.x - extents.x, center.y, center.z + extents.z));

            // Add the InteractiveRectangle component
            rectangle = volumeTransform.gameObject.AddComponent<InteractiveRectangle>();
            // Initialize the rectangle with our calculated corners
            rectangle.InitializeWithCorners(corners.ToArray());
        }
        /// <summary>
        /// Creates a new interactive rectangle with corners based on the volume's box collider
        /// </summary>
        private void CreateRectangle()
        {
            if (rectangle != null)
                return;

            // Get the BoxCollider component
            BoxCollider boxCollider = volumeTransform.gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                UnityEngine.Debug.LogError("BoxCollider not found on the volumetric object!");
                return;
            }

            // Get the local extents of the box collider
            Vector3 extents = boxCollider.size / 2;

            // Create corners in local space
            List<Vector3> localCorners = new List<Vector3>();

            // Bottom-left (min X, center Y, min Z)
            localCorners.Add(new Vector3(-extents.x, 0, -extents.z));

            // Bottom-right (max X, center Y, min Z)
            localCorners.Add(new Vector3(extents.x, 0, -extents.z));

            // Top-right (max X, center Y, max Z)
            localCorners.Add(new Vector3(extents.x, 0, extents.z));

            // Top-left (min X, center Y, max Z)
            localCorners.Add(new Vector3(-extents.x, 0, extents.z));

            // Convert local corners to world space, respecting rotation
            List<Vector3> worldCorners = new List<Vector3>();
            foreach (Vector3 localCorner in localCorners)
            {
                worldCorners.Add(volumeTransform.TransformPoint(localCorner));
            }

            // Add the InteractiveRectangle component
            rectangle = volumeTransform.gameObject.AddComponent<InteractiveRectangle>();

            // Initialize the rectangle with our calculated corners
            rectangle.InitializeWithCorners(worldCorners.ToArray());
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