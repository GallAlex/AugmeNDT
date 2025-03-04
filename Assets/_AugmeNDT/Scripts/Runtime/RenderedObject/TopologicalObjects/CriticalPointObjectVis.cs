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
        public static CriticalPointObjectVis Instance;
        public Dictionary<int, List<GameObject>> criticalPointDictionary = new Dictionary<int, List<GameObject>>();

        private Transform container; // All objects drawn will be inside the "1"CriticalPointContainer" in the scene. Makes the management easier
        private TopologicalDataObject topologicalDataInstance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            topologicalDataInstance = TopologicalDataObject.Instance;
            container = new GameObject("CriticalPointContainer").transform;
        }

        /// <summary>
        /// Creates visualization for all critical points stored in `TopologicalDataObject`.
        /// </summary>
        public void Visualize()
        {
            foreach (var item in topologicalDataInstance.criticalPointList)
            {
                CreatePoint(item.ID, item.Type, item.Position);
            };
        }

        /// <summary>
        /// Instantiates a critical point in the scene.
        /// </summary>
        private void CreatePoint(int id, int type, Vector3 position)
        {
            GameObject point = Instantiate(pointPrefab, position, Quaternion.identity, container);
            point.name = $"InteractiveCriticalPoint_{id}";
            point.GetComponent<Renderer>().material.color = GetColorByType(type);
            point.tag = "InteractiveCriticalPoint";
            InteractiveCriticalPoint cp = point.GetComponent<InteractiveCriticalPoint>();

            // CriticalPoint scriptini ekle 
            cp.pointID = id;
            cp.pointType = type;
            cp.pointPosition = position;

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
            Dictionary<int, Color> typeColors = new Dictionary<int, Color>()
        {
            { 0, Color.blue },   // Minimum
            { 1, Color.yellow }, // 1-Saddle
            { 2, new Color(1.0f, 0.5f, 0.0f) }, // 2-Saddle (Turuncu)
            { 3, Color.red },    // Maximum
            { 4, Color.green },  // Degenerate
            { 5, Color.white }   // Regular
        };

            return typeColors.ContainsKey(type) ? typeColors[type] : Color.gray; // Bilinmeyen tip için gri
        }
    }

}
