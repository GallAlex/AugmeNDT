using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// It manages the visualization of critical points in the scene.
    /// Loads critical points and creates interactive GameObjects with appropriate colors.
    /// </summary>
    public class CriticalPointObjectVis : MonoBehaviour
    {
        public GameObject pointPrefab;  // Prefab for the critical point representation
        public static CriticalPointObjectVis instance;
        public Dictionary<int, List<GameObject>> criticalPointDictionary = new Dictionary<int, List<GameObject>>();

        public Transform container; // All visualized points will be parented to this container for better scene organization
        private TopologicalDataObject topologicalDataInstance;

        public Dictionary<int, Color> typeColors = new Dictionary<int, Color>()
            {
                { 0, Color.blue },   // Minimum
                { 1, Color.yellow }, // 1-Saddle
                { 2, new Color(1.0f, 0.5f, 0.0f) }, // 2-Saddle (Orange)
                { 3, Color.red },    // Maximum
            };

        private void Awake()
        {
            instance = this; // Singleton assignment for global access
        }

        private void Start()
        {
            topologicalDataInstance = TopologicalDataObject.instance; // Get reference to the singleton data object
        }

        /// <summary>
        /// Creates visualization for all critical points stored in `TopologicalDataObject`.
        /// </summary>
        public void Visualize()
        {
            if (container == null)
                SetContainer(); // Ensure container exists before drawing

            foreach (var item in topologicalDataInstance.GetCriticalPointList())
            {
                CreatePoint(item.ID, item.Type, item.Position); // Instantiate each critical point
            };
        }

        /// <summary>
        /// Creates a new container GameObject under the fiber object to hold all critical point visuals.
        /// </summary>
        private void SetContainer()
        {
            Transform fibers = GameObject.Find("DataVisGroup_0/fibers.raw").transform;

            GameObject criticalPointsObj = new GameObject("CriticalPointObjects");
            container = criticalPointsObj.transform;
            container.SetParent(fibers);

            // Reset transform to align with parent
            container.localPosition = Vector3.zero;
            container.localRotation = Quaternion.identity;
            container.localScale = Vector3.one;
        }

        /// <summary>
        /// Instantiates a critical point in the scene with specific ID, type, and position.
        /// </summary>
        private void CreatePoint(int id, int type, Vector3 position)
        {
            GameObject point = Instantiate(pointPrefab, container); // Instantiate under container
            point.transform.localPosition = position;

            point.name = $"InteractiveCriticalPoint_{id}";
            point.tag = "InteractiveCriticalPoint";

            // Assign color based on point type
            point.GetComponent<Renderer>().material.color = GetColorByType(type);
            point.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f); // Uniform small scale

            // Assign metadata to script component
            InteractiveCriticalPoint cp = point.GetComponent<InteractiveCriticalPoint>();
            cp.pointID = id;
            cp.pointType = type;
            cp.pointPosition = position;

            // Store in dictionary categorized by type
            if (!criticalPointDictionary.ContainsKey(cp.pointType))
            {
                criticalPointDictionary[cp.pointType] = new List<GameObject>();
            }
            criticalPointDictionary[cp.pointType].Add(point);
        }

        /// <summary>
        /// Returns the appropriate color for the given critical point type.
        /// </summary>
        private Color GetColorByType(int type)
        {
            return typeColors.ContainsKey(type) ? typeColors[type] : Color.gray; // Fallback color for unknown types
        }
    }

}
