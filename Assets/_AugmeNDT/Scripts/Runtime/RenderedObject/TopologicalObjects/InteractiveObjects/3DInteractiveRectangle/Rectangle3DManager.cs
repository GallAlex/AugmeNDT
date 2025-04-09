using Assets.Scripts.DataStructure;
using System;
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
        public static bool supportedByTTK = false;
        public static Rectangle3DManager rectangle3DManager;

        public float localScaleRateTo3DVectorVisualize;

        private static TopologicalDataObject topologicalDataObjectInstance;
        private List<GradientDataset> gradientPoints = new List<GradientDataset>();
        private List<CriticalPointDataset> criticalPoints = new List<CriticalPointDataset>();

        private GameObject rectangle;
        private Bounds currentCubeBounds;


        private void Awake()
        {
            // Initialize singleton instance
            rectangle3DManager = this;
            localScaleRateTo3DVectorVisualize = 0.6f;
        }

        private void Start()
        {
            // Get references
            topologicalDataObjectInstance = TopologicalDataObject.instance;
        }

        /// <summary>
        /// Shows the 3D rectangle wireframe, creating it if needed
        /// </summary>
        public void ShowRectangle()
        {
            if (rectangle == null)
            {
                Create3DRectangle();
            }
            else
                rectangle.SetActive(true);
        }

        /// <summary>
        /// Hides the 3D rectangle wireframe
        /// </summary>
        public void HideRectangle()
        {
            if (rectangle != null)
                rectangle.SetActive(false);
        }

        /// <summary>
        /// Checks if the rectangle position or size has changed
        /// and updates internal data if necessary
        /// </summary>
        /// <returns>True if the bounds have changed since last update</returns>
        public bool IsUpdated()
        {
            bool IsUpdated = currentCubeBounds != GetRectangleBounds();
            if (IsUpdated)
            {
                UpdateInstance();
            }
            return IsUpdated;
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
            IsUpdated();
            return gradientPoints;
        }

        /// <summary>
        /// Gets the critical points inside the current 3D rectangle
        /// </summary>
        /// <returns>List of critical points inside the rectangle</returns>
        public List<CriticalPointDataset> GetCriticalPoints()
        {
            IsUpdated();
            return criticalPoints;
        }

        #region private
        /// <summary>
        /// Updates internal data when the rectangle position or size changes
        /// </summary>
        private void UpdateInstance()
        {
            UpdateCurrentPosition();

            CalculateGradientPoints();
            CalculateCriticalPoints();
        }

        /// <summary>
        /// Updates the current position of the rectangle
        /// </summary>
        private void UpdateCurrentPosition()
        {
            //update position
            currentCubeBounds = GetRectangleBounds();
        }

        /// <summary>
        /// Gets the minimum and maximum values of the box along each axis
        /// </summary>
        /// <returns>List containing [minX, maxX, minY, maxY, minZ, maxZ]</returns>
        private List<int> GetMinMaxValuesOfBox()
        {
            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            if (rectangleComponent != null)
            {
                // Sınırlayıcı kutuyu doğrudan wireframe bileşeninden al
                Bounds bounds = rectangleComponent.GetBounds();
                Vector3 center = bounds.center;
                Vector3 extents = bounds.extents; // Bu size/2'ye eşit

                // Min ve max değerleri hesapla
                int minX = (int)Math.Floor(center.x - extents.x);
                int maxX = (int)Math.Ceiling(center.x + extents.x);
                int minY = (int)Math.Floor(center.y - extents.y);
                int maxY = (int)Math.Ceiling(center.y + extents.y);
                int minZ = (int)Math.Floor(center.z - extents.z);
                int maxZ = (int)Math.Ceiling(center.z + extents.z);

                return new List<int>(new[] { minX, maxX, minY, maxY, minZ, maxZ });
            }
            return new List<int>();
        }

        /// <summary>
        /// Calculates and filters gradient points within the current wireframe bounds
        /// Uses TTK calculations if supported, otherwise filters from topological data
        /// </summary>
        private void CalculateGradientPoints()
        {
            gradientPoints.Clear(); // Clear previous data
            
            if (supportedByTTK)
            {
                bool isTTKCalculated = CalculateGradientPointsByTTK();
                if (isTTKCalculated)
                    return;
            }

            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            foreach (var data in topologicalDataObjectInstance.gradientList)
            {
                if (rectangleComponent.ContainsPointUsingBounds(data.Position))
                {
                    gradientPoints.Add(data);
                }
            }

            Debug.Log($"Filtered {gradientPoints.Count} points inside the cube.");
        }

        private bool CalculateGradientPointsByTTK()
        {
            bool tkkGradientUsed = false;

            var boxGradientPoints = TTKCalculations.GetGradient3DSubset(GetMinMaxValuesOfBox());
            if (boxGradientPoints.Any())
            {
                var borders = GetRectangleBounds();
                boxGradientPoints = boxGradientPoints.Select(x => x).Where(x => borders.Contains(x.Position)).ToList();
                if (boxGradientPoints.Any())
                {
                    gradientPoints = boxGradientPoints;
                    tkkGradientUsed = true;
                }
            }

            return tkkGradientUsed;
        }

        /// <summary>
        /// Calculates and filters critical points within the current wireframe bounds
        /// Uses TTK calculations if supported, otherwise filters from topological data
        /// </summary>
        private void CalculateCriticalPoints()
        {
            criticalPoints.Clear(); // Clear previous data
            if (supportedByTTK)
            {
                bool isTTKCalculated = CalculateCriticalPointsByTTK();
                if (isTTKCalculated)
                    return;
            }

            // Filter data inside the cube
            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            foreach (var data in topologicalDataObjectInstance.criticalPointList)
            {
                if (rectangleComponent.ContainsPointUsingBounds(data.Position))
                {
                    criticalPoints.Add(data);
                }
            }
        }

        private bool CalculateCriticalPointsByTTK()
        {

            bool tkkcriticalPointsUsed = false;
            var boxcriticalPoints = TTKCalculations.GetCriticalpoint3DSubset(GetMinMaxValuesOfBox());
            if (boxcriticalPoints.Any())
            {
                var borders = GetRectangleBounds();
                boxcriticalPoints = boxcriticalPoints.Select(x => x).Where(x => borders.Contains(x.Position)).ToList();
                if (boxcriticalPoints.Any())
                {
                    criticalPoints = boxcriticalPoints;
                    tkkcriticalPointsUsed = true;
                }
            }
            return tkkcriticalPointsUsed;
        }

        public void Create3DRectangle()
        {
            rectangle = new GameObject("Rectangle3D");
            rectangle.tag = "Rectangle3D";

            GameObject fibers = GameObject.Find("DataVisGroup_0/fibers.raw");
            rectangle.transform.parent = fibers.transform;

            // Reset transform to align with parent
            rectangle.transform.localPosition = Vector3.zero;
            rectangle.transform.localRotation = Quaternion.identity;
            rectangle.transform.localScale = Vector3.one;
            
            Basic3DRectangle basic3DRectangle = rectangle.AddComponent<Basic3DRectangle>();
            basic3DRectangle.Initialize();

            BoxCollider boxCollider = fibers.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Vector3 min = fibers.transform.TransformPoint(boxCollider.center - boxCollider.size * 0.5f);
                Vector3 max = fibers.transform.TransformPoint(boxCollider.center + boxCollider.size * 0.5f);
                basic3DRectangle.SetBounds(min, max);
            }
        }

        #endregion private
    }
}