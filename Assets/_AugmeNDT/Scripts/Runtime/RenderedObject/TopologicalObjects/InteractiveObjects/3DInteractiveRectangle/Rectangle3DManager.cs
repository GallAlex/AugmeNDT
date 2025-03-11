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
        public GameObject cubePrefab; // Assign a cube prefab in Unity Inspector
        private GameObject wireframeCube;

        public static bool supportedByTTK = false;

        public static Rectangle3DManager rectangle3DManager;
        private static TopologicalDataObject topologicalDataObjectInstance;

        private List<GradientDataset> gradientPoints = new List<GradientDataset>();
        private List<CriticalPointDataset> criticalPoints = new List<CriticalPointDataset>();
        public Bounds currentCubeBounds;

        private void Awake()
        {
            // Initialize singleton instance
            rectangle3DManager = this;
        }

        private void Start()
        {
            // Get reference to topological data object
            topologicalDataObjectInstance = TopologicalDataObject.Instance;
        }

        /// <summary>
        /// Shows the 3D rectangle wireframe, creating it if needed
        /// </summary>
        public void ShowRectangle()
        {
            if (wireframeCube == null)
            {
                CreateInteractiveRectangle();
            }
            else
                wireframeCube.SetActive(true);
        }

        /// <summary>
        /// Hides the 3D rectangle wireframe
        /// </summary>
        public void HideRectangle()
        {
            if (wireframeCube != null)
                wireframeCube.SetActive(false);
        }

        /// <summary>
        /// Checks if the rectangle position or size has changed
        /// and updates internal data if necessary
        /// </summary>
        /// <returns>True if the bounds have changed since last update</returns>
        public bool IsUpdated()
        {
            bool IsUpdated = currentCubeBounds != GetWireframeCubeBounds();
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
        public Bounds GetWireframeCubeBounds()
        {
            WireframeCube wireframeComponent = wireframeCube.GetComponent<WireframeCube>();
            if (wireframeComponent != null)
            {
                // Create bounds from the center and size of the cube
                Vector3 center = wireframeCube.transform.position;
                Vector3 size = wireframeComponent.cubeSize;

                // Create and return bounds with appropriate transformation
                Bounds bounds = new Bounds(center, size);
                return bounds;
            }

            return wireframeCube.GetComponents<Renderer>().FirstOrDefault(x => x is LineRenderer).bounds;
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
            currentCubeBounds = GetWireframeCubeBounds();
        }

        /// <summary>
        /// Gets the minimum and maximum values of the box along each axis
        /// </summary>
        /// <returns>List containing [minX, maxX, minY, maxY, minZ, maxZ]</returns>
        private List<int> GetMinMaxValuesOfBox()
        {
            WireframeCube wireframeComponent = wireframeCube.GetComponent<WireframeCube>();
            if (wireframeComponent != null)
            {
                Vector3 center = wireframeCube.transform.position;
                Vector3 size = wireframeComponent.cubeSize;

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

        /// <summary>
        /// Calculates and filters gradient points within the current wireframe bounds
        /// Uses TTK calculations if supported, otherwise filters from topological data
        /// </summary>
        private void CalculateGradientPoints()
        {
            gradientPoints.Clear(); // Clear previous data

            bool tkkGradientUsed = false;
            if (supportedByTTK)
            {
                var boxGradientPoints = TTKCalculations.GetGradient3DSubset(GetMinMaxValuesOfBox());
                if (boxGradientPoints.Any())
                {
                    var borders = GetWireframeCubeBounds();
                    boxGradientPoints = boxGradientPoints.Select(x => x).Where(x => borders.Contains(x.Position)).ToList();
                    if (boxGradientPoints.Any())
                    {
                        gradientPoints = boxGradientPoints;
                        tkkGradientUsed = true;
                    }
                }
            }

            if (!tkkGradientUsed)
            {
                Bounds cubeBounds = GetWireframeCubeBounds();
                if (topologicalDataObjectInstance == null)
                {
                    Debug.LogError("topologicalDataObjectInstance is missing!");
                    return;
                }

                // Filter data inside the cube
                foreach (var data in topologicalDataObjectInstance.gradientList)
                {
                    if (cubeBounds.Contains(data.Position))
                    {
                        gradientPoints.Add(data);
                    }
                }
            }

            Debug.Log($"Filtered {gradientPoints.Count} points inside the cube.");
        }

        /// <summary>
        /// Calculates and filters critical points within the current wireframe bounds
        /// Uses TTK calculations if supported, otherwise filters from topological data
        /// </summary>
        private void CalculateCriticalPoints()
        {
            criticalPoints.Clear(); // Clear previous data

            bool tkkcriticalPointsUsed = false;
            if (supportedByTTK)
            {
                var boxcriticalPoints = TTKCalculations.GetCriticalpoint3DSubset(GetMinMaxValuesOfBox());
                if (boxcriticalPoints.Any())
                {
                    var borders = GetWireframeCubeBounds();
                    boxcriticalPoints = boxcriticalPoints.Select(x => x).Where(x => borders.Contains(x.Position)).ToList();
                    if (boxcriticalPoints.Any())
                    {
                        criticalPoints = boxcriticalPoints;
                        tkkcriticalPointsUsed = true;
                    }
                }
            }

            if (!tkkcriticalPointsUsed)
            {
                if (topologicalDataObjectInstance == null)
                {
                    Debug.LogError("topologicalDataObjectInstance is missing!");
                    return;
                }

                // Filter data inside the cube
                foreach (var data in topologicalDataObjectInstance.criticalPointList)
                {
                    // Get the cube's boundaries
                    Bounds cubeBounds = GetWireframeCubeBounds();
                    if (cubeBounds.Contains(data.Position))
                    {
                        criticalPoints.Add(data);
                    }
                }
            }
        }

        /// <summary>
        /// Creates an interactive wireframe cube at the default position
        /// </summary>
        private void CreateInteractiveRectangle()
        {
            wireframeCube = Instantiate(cubePrefab, new Vector3(9.081296f, 6.504251f, 11.54561f), Quaternion.identity);
            wireframeCube.tag = "BoxWireframeLine";

            // Ensure no solid cube is rendered
            MeshRenderer meshRenderer = wireframeCube.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;

            // Add necessary wireframe behavior
            if (wireframeCube.GetComponent<WireframeCube>() == null)
                wireframeCube.AddComponent<WireframeCube>();

            if (wireframeCube.GetComponent<DraggableResizableWireframe>() == null)
                wireframeCube.AddComponent<DraggableResizableWireframe>();
        }

        /// <summary>
        /// Destroys the wireframe cube
        /// </summary>
        private void DestroyWireframe()
        {
            Destroy(wireframeCube);
        }
        #endregion private
    }
}