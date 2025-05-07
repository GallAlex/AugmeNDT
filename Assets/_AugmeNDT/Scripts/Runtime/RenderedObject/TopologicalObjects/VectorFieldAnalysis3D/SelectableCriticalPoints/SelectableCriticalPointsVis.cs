using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    public class SelectableCriticalPointsVis : MonoBehaviour
    {
        public static SelectableCriticalPointsVis instance;

        private CreateCriticalPoints createCriticalPointsInstance;
        private static Rectangle3DManager rectangle3DManager;
        private float localScaleRate;
        private Bounds cubeBounds;
        private int onlyShowThisType = -1;

        private Dictionary<int, List<GameObject>> criticalPointDictionary = new Dictionary<int, List<GameObject>>();
        // All visualized points will be parented to this container for better scene organization
        private Transform container;
        private float basedPositionOffset = -0.2f;
        private Vector3 currentPositionOffset;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (rectangle3DManager == null)
            {
                // Get reference to the Rectangle3DManager instance
                rectangle3DManager = Rectangle3DManager.rectangle3DManager;
                TopologyConfigData config = rectangle3DManager.config;
                localScaleRate = 0.004f;
            }

            if (createCriticalPointsInstance == null)
                createCriticalPointsInstance = CreateCriticalPoints.instance;
        }

        public void Visualize()
        {
            // Clear existing visible objects and return them to the pool
            ClearVisualization();

            // Set up the container (update its position instead of recreating)
            SetupContainer();

            var criticalPoints = PrepareNewCriticalPositions();

            // Visualize critical points (using the pool)
            criticalPointDictionary = CreateCriticalPointsUsingPool(criticalPoints, container, localScaleRate);

        }

        private List<CriticalPointDataset> PrepareNewCriticalPositions()
        {
            var criticalPoints = rectangle3DManager.GetCriticalPoints();
            List<CriticalPointDataset> criticalPointDatasets = new List<CriticalPointDataset>();

            // Calculate the right edge based on position and scale
            float leftEdge = rectangle3DManager.volumeTransform.position.x -
                             (rectangle3DManager.volumeTransform.localScale.x / 2);
            currentPositionOffset = new Vector3(leftEdge + basedPositionOffset, 0, 0);

            foreach (var item in criticalPoints)
            {
                var temp = item;
                temp.Position = temp.Position + currentPositionOffset;
                criticalPointDatasets.Add(temp);
            }

            return criticalPointDatasets;
        }

        private List<GradientDataset> PrepareNewGradientPositions()
        {
            var gradients = rectangle3DManager.GetGradientPoints();
            List<GradientDataset> gradientDatasets = new List<GradientDataset>();

            // Calculate the right edge based on position and scale
            float leftEdge = rectangle3DManager.volumeTransform.position.x -
                             (rectangle3DManager.volumeTransform.localScale.x / 2);
            currentPositionOffset = new Vector3(leftEdge + basedPositionOffset, 0, 0);

            foreach (var item in gradients)
            {
                var temp = item;
                temp.Position = temp.Position + currentPositionOffset;
                gradientDatasets.Add(temp);
            }

            return gradientDatasets;
        }
        private Dictionary<int, List<GameObject>> CreateCriticalPointsUsingPool(List<CriticalPointDataset> criticalPoints, Transform container, float localScaleRate)
        {
            Dictionary<int, List<GameObject>> result = new Dictionary<int, List<GameObject>>();

            foreach (var point in criticalPoints)
            {
                // Get an object from the pool
                GameObject obj = CriticalPointObjectPool.Instance.GetPooledObject();

                // Configure the object
                obj.transform.SetParent(container, true);
                obj.transform.position = point.Position;  // world position olarak ayarla
                obj.transform.localScale = Vector3.one * localScaleRate;
                obj.name = $"DuplicatedInteractiveCriticalPoint_{point.ID}";

                // Set the renderer
                obj.GetComponent<Renderer>().material.color = createCriticalPointsInstance.GetColorByType(point.Type);

                // Set the critical point component
                InteractiveCriticalPoint cp = obj.GetComponent<InteractiveCriticalPoint>();
                cp.pointID = point.ID;
                cp.pointType = point.Type;
                cp.pointPosition = point.Position;

                // Add to the dictionary
                if (!result.ContainsKey(point.Type))
                {
                    result[point.Type] = new List<GameObject>();
                }
                result[point.Type].Add(obj);
            }

            return result;
        }
        /// <summary>
        /// Filters the critical points displayed based on their type.
        /// </summary>
        /// <param name="buttonIndex">The type index selected from the color bar.</param>
        public void FilterCriticalPointsByType(int buttonIndex)
        {
            Debug.Log($"Color Bar {buttonIndex} pressed!");

            // If the same button is pressed again, show all critical points
            if (onlyShowThisType == buttonIndex)
            {
                SetAllCriticalPointsActive(true);
                onlyShowThisType = -1;
            }
            else
            {
                // Hide all critical points first
                SetAllCriticalPointsActive(false);

                // Then only activate points of the selected type
                if (criticalPointDictionary.ContainsKey(buttonIndex))
                {
                    foreach (var item in criticalPointDictionary[buttonIndex])
                    {
                        item.SetActive(true);
                    }
                }

                onlyShowThisType = buttonIndex;
            }
        }

        /// <summary>
        /// Helper function to set the active state for all critical points.
        /// </summary>
        private void SetAllCriticalPointsActive(bool isActive)
        {
            foreach (var gameObjectList in criticalPointDictionary.Values)
            {
                foreach (var item in gameObjectList)
                {
                    item.SetActive(isActive);
                }
            }
        }

        private void ClearVisualization()
        {
            // Return all active critical points back to the pool
            foreach (var pointList in criticalPointDictionary.Values)
            {
                foreach (var point in pointList)
                {
                    CriticalPointObjectPool.Instance.ReturnToPool(point);
                }
            }

            criticalPointDictionary.Clear();
        }

        private void SetupContainer()
        {
            if (container == null)
            {
                container = new GameObject("DuplicatedCriticalPointObjects").transform;
                container.SetParent(rectangle3DManager.volumeTransform, worldPositionStays: true);
            }
        }

    }
}
