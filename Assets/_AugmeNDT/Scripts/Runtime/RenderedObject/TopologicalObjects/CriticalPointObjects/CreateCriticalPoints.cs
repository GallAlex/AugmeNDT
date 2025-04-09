using Assets.Scripts.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace AugmeNDT
{
    public class CreateCriticalPoints : MonoBehaviour
    {
        public static CreateCriticalPoints instance;

        // Define text labels for each critical point type
        private string[] labels = { "Minimum", "1-Saddle", "2-Saddle", "Maximum" };

        private Dictionary<int, Color> typeColors = new Dictionary<int, Color>()
        {
            { 0, Color.blue },   // Minimum
            { 1, Color.yellow }, // 1-Saddle
            { 2, new Color(1.0f, 0.5f, 0.0f) }, // 2-Saddle (Orange)
            { 3, Color.red },    // Maximum
        };

        private void Awake()
        {
            instance = this; // Singleton setup for global access
        }

        public Dictionary<int, List<GameObject>> CreateInteractiveCriticalPoint(List<CriticalPointDataset> criticalPoints,Transform container, GameObject pointPrefab, float localScaleRate = 1.0f)
        {
            Dictionary<int, List<GameObject>> criticalPointDictionary = new Dictionary<int, List<GameObject>>();
            criticalPoints.ForEach(point => {

                CreateInteractiveCriticalPoint(point.ID, point.Type, point.Position, container, pointPrefab, localScaleRate, criticalPointDictionary);
            });
            CreateLegendColorBar(container, localScaleRate * 10);
            return criticalPointDictionary;
        }

        public List<GameObject> CreateBasicCriticalPoint(List<CriticalPointDataset> criticalPoints, Transform container, float localScaleRate = 1.0f)
        {
            List<GameObject> spheres = new List<GameObject>();
            criticalPoints.ForEach(cp => {
                // Create and configure sphere object
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.parent = container;
                sphere.transform.position = cp.Position;
                sphere.transform.localScale = Vector3.one * localScaleRate;
                sphere.GetComponent<Renderer>().material.color = GetColorByType(cp.Type);
                spheres.Add(sphere);
            });

            CreateLegendColorBar(container, localScaleRate * 10);
            return spheres;
        }

        /// <summary>
        /// Instantiates a critical point in the scene with specific ID, type, and position.
        /// </summary>
        private void CreateInteractiveCriticalPoint(int id, int type, Vector3 position, Transform container, GameObject pointPrefab, float localScaleRate, Dictionary<int, List<GameObject>> criticalPointDictionary)
        {
            GameObject point = Instantiate(pointPrefab, container); // Instantiate under container
            point.transform.localPosition = position;

            point.name = $"InteractiveCriticalPoint_{id}";
            point.tag = "InteractiveCriticalPoint";

            // Assign color based on point type
            point.GetComponent<Renderer>().material.color = GetColorByType(type);
            point.transform.localScale = Vector3.one * localScaleRate;

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

        /// <summary>
        /// Creates and places the color legend bar that explains critical point types with corresponding colors and labels.
        /// </summary>
        private void CreateLegendColorBar(Transform container,float localScale)
        {
            // Extract colors from the dictionary into a color array (ordered by key)
            Color[] colors = new Color[typeColors.Count];
            for (int i = 0; i < typeColors.Count; i++)
            {
                colors[i] = typeColors[i];
            }

            // Create an instance of the LegendColorBar class to generate the visual legend
            LegendColorBar legend = new LegendColorBar();

            // Generate the color bar using the new method that places labels on all sides
            GameObject legendObject = legend.CreateColorScalarBar(
                new Vector3(0f, 0f, 0f),   // World position where the legend will appear
                "Critical Points",         // Title of the legend
                labels,                    // Labels to display for each color segment
                colors,                    // Corresponding colors for each label
                2                          // Percentage spacing between blocks
            );

            // Attach the legend under the main container so it moves/scales with the data visualization
            legendObject.transform.parent = container;
            legendObject.transform.localScale = Vector3.one * localScale;

            // Make it interactable in immersive systems
            legendObject.AddComponent<BoxCollider>();
            legendObject.AddComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();
            legendObject.AddComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionGrabbable>();
        }


    }
}
