using Assets.Scripts.DataStructure;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the visualization of 3D vector fields and critical points using glyphs
    /// </summary>
    public class Glyph3DVectorField : MonoBehaviour
    {
        public static Glyph3DVectorField instance;
        private static Rectangle3DManager rectangle3DManager;

        // Data collection for gradient and critical points
        private List<GradientDataset> gradientPoints = new List<GradientDataset>();
        private List<CriticalPointDataset> criticalPoints = new List<CriticalPointDataset>();

        // Containers for visualization objects
        private Transform container;
        private List<GameObject> arrows = new List<GameObject>();
        private List<GameObject> spheres = new List<GameObject>();

        private void Awake()
        {
            // Initialize singleton instance
            if (instance == null)
                instance = this;

            // Create a container for all vector field objects
            container = new GameObject("3DVectorForce").transform;
        }

        public void Start()
        {
            // Get reference to the rectangle manager
            rectangle3DManager = Rectangle3DManager.rectangle3DManager;
        }

        /// <summary>
        /// Displays the vector field visualization using arrows
        /// </summary>
        public void ShowVectorField()
        {
            if (!arrows.Any() || rectangle3DManager.IsUpdated())
            {
                ClearArrows();
                Initialize();
                arrows = VectorObjectVis.instance.CreateArrows(gradientPoints, container);
            }
            else
            {
                arrows.ForEach(x => x.SetActive(true));
            }
        }

        /// <summary>
        /// Hides the vector field visualization
        /// </summary>
        public void HideVectorField()
        {
            arrows.ForEach(x => x.SetActive(false));
        }

        /// <summary>
        /// Displays the critical points using colored spheres
        /// </summary>
        public void ShowCriticalPoints()
        {
            if (!spheres.Any() || rectangle3DManager.IsUpdated())
            {
                ClearCriticalPoints();
                Initialize();
                criticalPoints.ForEach(x =>
                {
                    spheres.Add(DrawCriticalPoints(x.Position, x.Type));
                });
            }
            else
            {
                spheres.ForEach(x => x.SetActive(true));
            }

        }

        /// <summary>
        /// Hides the critical points visualization
        /// </summary>
        public void HideCriticalPoints()
        {
            spheres.ForEach(x => x.SetActive(false));
        }

        #region private
        /// <summary>
        /// Initializes data by fetching current gradient and critical points
        /// </summary>
        private void Initialize()
        {
            gradientPoints = rectangle3DManager.GetGradientPoints();
            criticalPoints = rectangle3DManager.GetCriticalPoints();
        }

        /// <summary>
        /// Creates a sphere to represent a critical point with color coding by type
        /// </summary>
        /// <param name="position">Position of the critical point</param>
        /// <param name="typeId">Type identifier of the critical point</param>
        /// <returns>GameObject representing the critical point</returns>
        private GameObject DrawCriticalPoints(Vector3 position, int typeId)
        {
            // Color coding for different critical point types
            Dictionary<int, Color> typeColors = new Dictionary<int, Color>()
            {
                { 0, Color.blue },   // Minimum
                { 1, Color.yellow }, // 1-Saddle
                { 2, Color.yellow }, // 2-Saddle
                { 3, Color.red },    // Maximum
            };

            // Create and configure sphere object
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * 0.3f;
            sphere.GetComponent<Renderer>().material.color = typeColors.ContainsKey(typeId) ? typeColors[typeId] : Color.gray;

            return sphere;
        }

        /// <summary>
        /// Clears all arrow objects from the scene
        /// </summary>
        private void ClearArrows()
        {
            arrows.ForEach(x => Destroy(x));
            arrows.Clear();
        }

        /// <summary>
        /// Clears all critical point sphere objects from the scene
        /// </summary>
        private void ClearCriticalPoints()
        {
            spheres.ForEach(x => Destroy(x));
            spheres.Clear();
        }
        #endregion private

    }
}