using Assets.Scripts.DataStructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages a 3D rectangular region for visualizing and analyzing vector field data
    /// </summary>
    public class Rectangle3DManager : MonoBehaviour
    {
        public static Rectangle3DManager rectangle3DManager;

        public bool useAllData = true;
        public bool visibleRectangle = false;

        public TopologyConfigData config;
        public Transform volumeTransform;

        /// <summary>
        /// Controls the density of the grid used for gradient calculations.
        /// Lower values create denser grids with more detail but higher processing cost.
        /// </summary>
        private float defaultInterval;
        /// Scale factor for calculation grid density, automatically adjusted based on volume dimensions.
        public float scaleRateToCalculation;
        public float intervalValue; // defaultInterval*scaleRateToCalculation


        private static TopologicalDataObject topologicalDataObjectInstance;
        private List<GradientDataset> gradientPoints = new List<GradientDataset>();
        private List<CriticalPointDataset> criticalPoints = new List<CriticalPointDataset>();
        private GameObject rectangle;

        /// <summary>
        /// Initializes the singleton instance
        /// </summary>
        private void Awake()
        {
            // Initialize singleton instance
            rectangle3DManager = this;
        }

        /// <summary>
        /// Initializes references and configuration values
        /// </summary>
        private void Start()
        {
            // Get reference to topological data object
            if (topologicalDataObjectInstance == null)
            {
                topologicalDataObjectInstance = TopologicalDataObject.instance;
                volumeTransform = topologicalDataObjectInstance.volumeTransform;
                config = topologicalDataObjectInstance.config;

                scaleRateToCalculation = topologicalDataObjectInstance.GetOptimalScaleRateToCalculation();
                defaultInterval = 0.1f; //default
                intervalValue = defaultInterval * scaleRateToCalculation;
            }
        }

        /// <summary>
        /// Creates the 3D rectangle visualization if it doesn't exist yet
        /// </summary>
        public void InitializeRectangle()
        {
            if (rectangle == null)
            {
                if (useAllData)
                    rectangle3DManager.Create3DRectangle();
                else
                    rectangle3DManager.Create3DRectangle(topologicalDataObjectInstance.min3D, topologicalDataObjectInstance.max3D);

                UpdateInstance();
            }
        }

        /// <summary>
        /// Gets the current bounds of the wireframe cube
        /// </summary>
        /// <returns>Bounds representing the position and size of the cube</returns>
        public Bounds GetRectangleBounds()
        {
            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            return rectangleComponent.GetBounds();
        }

        /// <summary>
        /// Gets the gradient points inside the current 3D rectangle
        /// </summary>
        /// <returns>List of gradient points inside the rectangle</returns>
        public List<GradientDataset> GetGradientPoints()
        {
            return gradientPoints;
        }

        /// <summary>
        /// Gets the critical points inside the current 3D rectangle
        /// </summary>
        /// <returns>List of critical points inside the rectangle</returns>
        public List<CriticalPointDataset> GetCriticalPoints()
        {
            return criticalPoints;
        }

        /// <summary>
        /// Recreates the rectangle after the volume has been scaled
        /// </summary>
        public void UpdateRectangleAfterScaling()
        {
            if (rectangle == null)
                return;

            Destroy(rectangle.GetComponent<Basic3DRectangle>());
            Destroy(GameObject.Find("RectangleVisual"));
            Destroy(rectangle);
            rectangle = null;

            if (useAllData)
                rectangle3DManager.Create3DRectangle();
            else
                rectangle3DManager.Create3DRectangle(topologicalDataObjectInstance.min3D, topologicalDataObjectInstance.max3D);

            UpdateInstance();
        }

        #region private
        /// <summary>
        /// Updates internal data when the rectangle position or size changes
        /// </summary>
        private void UpdateInstance()
        {
            CalculateGradientPointsWithSpacing();
            CalculateCriticalPointsWithSpacing();
        }

        /// <summary>
        /// Calculates and filters gradient points within the rectangle using spatial grid spacing
        /// </summary>
        private void CalculateGradientPointsWithSpacing()
        {
            // Clear previous data
            gradientPoints.Clear();

            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            Bounds bounds = rectangleComponent.GetBounds();

            // Get rectangle bounds
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            // Use interval value
            float spacing = intervalValue;

            // Use thread-safe dictionary to manage concurrent access
            ConcurrentDictionary<Vector3Int, GradientDataset> gridPoints = new ConcurrentDictionary<Vector3Int, GradientDataset>();

            rectangleComponent.SetBoundsManuelUpdated();
            Parallel.ForEach(topologicalDataObjectInstance.gradientList, data =>
            {
                if (rectangleComponent.ContainsPointUsingBounds(data.Position, true))
                {
                    // Convert position to grid cell
                    Vector3Int gridPos = new Vector3Int(
                        Mathf.FloorToInt(data.Position.x / spacing),
                        Mathf.FloorToInt(data.Position.y / spacing),
                        Mathf.FloorToInt(data.Position.z / spacing)
                    );

                    // Take only one point from each grid cell (in a thread-safe way)
                    gridPoints.TryAdd(gridPos, data);
                }
            });

            // Add selected points to gradientPoints list
            gradientPoints.AddRange(gridPoints.Values);

            Debug.Log($"Filtered {gradientPoints.Count} points inside the cube with interval {spacing}.");
        }

        /// <summary>
        /// Calculates and filters critical points within the rectangle using spatial grid spacing
        /// </summary>
        private void CalculateCriticalPointsWithSpacing()
        {
            criticalPoints.Clear(); // Clear previous data

            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            Bounds bounds = rectangleComponent.GetBounds();

            // Use interval value - you can use a different interval value for critical points
            float spacing = intervalValue;

            // Use thread-safe dictionary to manage concurrent access
            ConcurrentDictionary<Vector3Int, CriticalPointDataset> gridPoints = new ConcurrentDictionary<Vector3Int, CriticalPointDataset>();

            rectangleComponent.SetBoundsManuelUpdated();
            Parallel.ForEach(topologicalDataObjectInstance.criticalPointList, data =>
            {
                if (rectangleComponent.ContainsPointUsingBounds(data.Position, true))
                {
                    // Convert position to grid cell
                    Vector3Int gridPos = new Vector3Int(
                        Mathf.FloorToInt(data.Position.x / spacing),
                        Mathf.FloorToInt(data.Position.y / spacing),
                        Mathf.FloorToInt(data.Position.z / spacing)
                    );

                    // Take only one point from each grid cell (in a thread-safe way)
                    gridPoints.TryAdd(gridPos, data);
                }
            });

            // Add selected points to criticalPoints list
            criticalPoints.AddRange(gridPoints.Values);

            Debug.Log($"Filtered {criticalPoints.Count} critical points inside the cube with interval {spacing}.");
        }

        /// <summary>
        /// Creates a 3D rectangle using the volume's bounds
        /// </summary>
        public void Create3DRectangle()
        {
            rectangle = new GameObject("Rectangle3D");
            rectangle.tag = "Rectangle3D";

            rectangle.transform.parent = volumeTransform;
            GameObject volumeObject = volumeTransform.gameObject;

            // Reset transform to align with parent
            rectangle.transform.localPosition = Vector3.zero;
            rectangle.transform.localRotation = Quaternion.identity;
            rectangle.transform.localScale = Vector3.one;

            Basic3DRectangle basic3DRectangle = rectangle.AddComponent<Basic3DRectangle>();
            basic3DRectangle.drawBorders = visibleRectangle;

            BoxCollider boxCollider = volumeObject.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Vector3 min = volumeObject.transform.TransformPoint(boxCollider.center - boxCollider.size * 0.5f);
                Vector3 max = volumeObject.transform.TransformPoint(boxCollider.center + boxCollider.size * 0.5f);
                basic3DRectangle.SetBounds(min, max);
            }
        }

        /// <summary>
        /// Creates a 3D rectangle with specified minimum and maximum corners
        /// </summary>
        /// <param name="min">Minimum corner position (x,y,z)</param>
        /// <param name="max">Maximum corner position (x,y,z)</param>
        public void Create3DRectangle(Vector3 min, Vector3 max)
        {
            // Create new GameObject
            rectangle = new GameObject("Rectangle3D");
            rectangle.tag = "Rectangle3D";

            // Set volumeTransform as parent
            rectangle.transform.parent = volumeTransform;

            // Reset transform
            rectangle.transform.localPosition = Vector3.zero;
            rectangle.transform.localRotation = Quaternion.identity;
            rectangle.transform.localScale = Vector3.one;

            // Add and initialize Basic3DRectangle component
            Basic3DRectangle basic3DRectangle = rectangle.AddComponent<Basic3DRectangle>();
            basic3DRectangle.drawBorders = visibleRectangle;
            // Set bounds
            basic3DRectangle.SetBounds(min, max);
        }

        #endregion private
    }
}