using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages a 3D rectangular region for visualizing and analyzing vector field data.
    /// </summary>
    public class Rectangle3DManager : MonoBehaviour
    {
        public static Rectangle3DManager rectangle3DManager;

        public bool useAllData = true; // Whether to use the full volume data for the rectangle
        public bool visibleRectangle = false; // Toggle visibility of the rectangle borders

        public TopologyConfigData config;
        public Transform volumeTransform;

        /// <summary>
        /// Default spacing interval used for grid generation.
        /// </summary>
        private float defaultInterval;
        /// <summary>
        /// Scale factor for adjusting calculation density based on volume size.
        /// </summary>
        public float scaleRateToCalculation;
        /// <summary>
        /// Actual interval value after applying scale rate.
        /// </summary>
        public float intervalValue;

        private static TopologicalDataObject topologicalDataObjectInstance;
        private List<GradientDataset> gradientPoints = new List<GradientDataset>();
        private List<CriticalPointDataset> criticalPoints = new List<CriticalPointDataset>();
        private GameObject rectangle;

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
        private void Awake()
        {
            rectangle3DManager = this;
        }

        /// <summary>
        /// Initializes references and loads configuration values.
        /// </summary>
        private void Start()
        {
            if (topologicalDataObjectInstance == null)
            {
                topologicalDataObjectInstance = TopologicalDataObject.instance;
                volumeTransform = topologicalDataObjectInstance.volumeTransform;
                config = topologicalDataObjectInstance.config;

                scaleRateToCalculation = topologicalDataObjectInstance.GetOptimalScaleRateToCalculation();
                defaultInterval = 0.1f;
                intervalValue = defaultInterval * scaleRateToCalculation;
            }
        }

        /// <summary>
        /// Initializes the 3D rectangle if it has not been created.
        /// </summary>
        public void InitializeRectangle()
        {
            if (rectangle != null)
                return;

            rectangle3DManager.Create3DRectangle();
            UpdateInstance();
        }

        /// <summary>
        /// Returns the bounds of the current rectangle.
        /// </summary>
        public Bounds GetRectangleBounds()
        {
            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            return rectangleComponent.GetBounds();
        }

        /// <summary>
        /// Returns the list of gradient points within the rectangle.
        /// </summary>
        public List<GradientDataset> GetGradientPoints()
        {
            return gradientPoints;
        }

        /// <summary>
        /// Returns the list of critical points within the rectangle.
        /// </summary>
        public List<CriticalPointDataset> GetCriticalPoints()
        {
            return criticalPoints;
        }

        /// <summary>
        /// Updates rectangle content after scaling.
        /// </summary>
        public void UpdateRectangleAfterScaling()
        {
            if (rectangle == null)
                return;

            UpdateInstance();
        }

        #region private

        /// <summary>
        /// Updates internal data when the rectangle changes.
        /// </summary>
        private void UpdateInstance()
        {
            CalculateGradientPointsWithSpacing();
            CalculateCriticalPointsWithSpacing();
        }

        /// <summary>
        /// Calculates gradient points within the rectangle using a spaced grid to avoid overcrowding.
        /// </summary>
        private void CalculateGradientPointsWithSpacing()
        {
            gradientPoints.Clear();

            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            Bounds bounds = rectangleComponent.GetBounds();

            float spacing = intervalValue;

            ConcurrentDictionary<Vector3Int, GradientDataset> gridPoints = new ConcurrentDictionary<Vector3Int, GradientDataset>();

            rectangleComponent.SetBoundsManuelUpdated();
            Parallel.ForEach(topologicalDataObjectInstance.gradientList, data =>
            {
                if (rectangleComponent.ContainsPointUsingBounds(data.Position, true))
                {
                    Vector3Int gridPos = new Vector3Int(
                        Mathf.FloorToInt(data.Position.x / spacing),
                        Mathf.FloorToInt(data.Position.y / spacing),
                        Mathf.FloorToInt(data.Position.z / spacing)
                    );

                    gridPoints.TryAdd(gridPos, data);
                }
            });

            gradientPoints.AddRange(gridPoints.Values);

            Debug.Log($"Filtered {gradientPoints.Count} points inside the cube with interval {spacing}.");
        }

        /// <summary>
        /// Calculates critical points within the rectangle using a spaced grid to avoid overcrowding.
        /// </summary>
        private void CalculateCriticalPointsWithSpacing()
        {
            criticalPoints.Clear();

            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            Bounds bounds = rectangleComponent.GetBounds();

            float spacing = intervalValue;

            ConcurrentDictionary<Vector3Int, CriticalPointDataset> gridPoints = new ConcurrentDictionary<Vector3Int, CriticalPointDataset>();

            rectangleComponent.SetBoundsManuelUpdated();
            Parallel.ForEach(topologicalDataObjectInstance.criticalPointList, data =>
            {
                if (rectangleComponent.ContainsPointUsingBounds(data.Position, true))
                {
                    Vector3Int gridPos = new Vector3Int(
                        Mathf.FloorToInt(data.Position.x / spacing),
                        Mathf.FloorToInt(data.Position.y / spacing),
                        Mathf.FloorToInt(data.Position.z / spacing)
                    );

                    gridPoints.TryAdd(gridPos, data);
                }
            });

            criticalPoints.AddRange(gridPoints.Values);

            Debug.Log($"Filtered {criticalPoints.Count} critical points inside the cube with interval {spacing}.");
        }

        /// <summary>
        /// Creates a new 3D rectangle in the scene.
        /// </summary>
        public bool Create3DRectangle()
        {
            if (rectangle == null)
            {
                rectangle = new GameObject("Rectangle3D");
                rectangle.tag = "Rectangle3D";
                rectangle.transform.parent = volumeTransform;
                rectangle.transform.localPosition = Vector3.zero;
                rectangle.transform.localRotation = Quaternion.identity;
                rectangle.transform.localScale = Vector3.one;
            }

            Vector3 worldMin;
            Vector3 worldMax;

            if (useAllData)
            {
                GameObject volumeObject = volumeTransform.gameObject;
                BoxCollider boxCollider = volumeObject.GetComponent<BoxCollider>();
                if (boxCollider == null)
                {
                    Debug.LogError("BoxCollider for fiber.raw not found.");
                    return false;
                }

                worldMin = volumeObject.transform.TransformPoint(boxCollider.center - boxCollider.size * 0.5f);
                worldMax = volumeObject.transform.TransformPoint(boxCollider.center + boxCollider.size * 0.5f);
            }
            else
            {
                worldMin = topologicalDataObjectInstance.min3D;
                worldMax = topologicalDataObjectInstance.max3D;

                if (worldMin == Vector3.zero && worldMax == Vector3.zero)
                {
                    Debug.LogError("Please insert min and max values in config.file.");
                    return false;
                }
            }

            Vector3 localMin = volumeTransform.InverseTransformPoint(worldMin);
            Vector3 localMax = volumeTransform.InverseTransformPoint(worldMax);

            Basic3DRectangle basic3DRectangle = rectangle.AddComponent<Basic3DRectangle>();
            basic3DRectangle.drawBorders = visibleRectangle;
            basic3DRectangle.InitializeBoundsLocal(localMin, localMax);

            return true;
        }

        #endregion private
    }
}
