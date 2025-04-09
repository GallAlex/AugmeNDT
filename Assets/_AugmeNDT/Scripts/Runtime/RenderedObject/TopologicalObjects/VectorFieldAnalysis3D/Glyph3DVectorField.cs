using Assets.Scripts.DataStructure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the visualization of 3D vector fields and critical points using glyphs
    /// </summary>
    public class Glyph3DVectorField : MonoBehaviour
    {
        public static Glyph3DVectorField instance;

        private static Rectangle3DManager rectangle3DManager;
        private static CreateCriticalPoints createCriticalPointsInstance;

        // Data collection for gradient and critical points
        private List<GradientDataset> gradientPoints = new List<GradientDataset>();
        private List<CriticalPointDataset> criticalPoints = new List<CriticalPointDataset>();

        // Containers for visualization objects
        private Transform container;
        private List<GameObject> arrows = new List<GameObject>();
        private List<GameObject> spheres = new List<GameObject>();
        private static float localScaleRateTo3DVectorVisualize;
        private static float localScaleRateTo3DCriticalPointsVisualize;
        private static int arrowsPerFrame = 50;

        private void Awake()
        {
            // Initialize singleton instance
            if (instance == null)
                instance = this;
        }

        public void Start()
        {
            if (rectangle3DManager == null)
            {
                // Get reference to the rectangle manager
                rectangle3DManager = Rectangle3DManager.rectangle3DManager;
                localScaleRateTo3DVectorVisualize = rectangle3DManager.localScaleRateTo3DVectorVisualize;
                localScaleRateTo3DVectorVisualize = 0.3f;
                localScaleRateTo3DCriticalPointsVisualize = 0.006f;
                
            }
            createCriticalPointsInstance = CreateCriticalPoints.instance;
        }

        /// <summary>
        /// Displays the vector field visualization using arrows
        /// </summary>
        public void ShowVectorField()
        {
            if (!arrows.Any() || rectangle3DManager.IsUpdated())
            {
                SetContainer();
                ClearArrows();
                Initialize();

                // Çok sayıda ok varsa Coroutine kullan
                if (gradientPoints.Count > 800)
                {
                    StartCoroutine(CreateArrowsCoroutine());
                }
                else
                {
                    // Az sayıda ok için normal yöntemi kullan
                    arrows = VectorObjectVis.instance.CreateArrows(gradientPoints, container, localScaleRateTo3DVectorVisualize);
                }
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
                SetContainer();

                ClearCriticalPoints();
                Initialize();
                spheres = createCriticalPointsInstance.CreateBasicCriticalPoint(criticalPoints,container, localScaleRateTo3DCriticalPointsVisualize);
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
        
        /// <summary>
        /// Creates a new container GameObject under the fiber object to hold all vector visuals.
        /// </summary>
        private void SetContainer()
        {
            if (container != null)
                return;

            Transform fibers = GameObject.Find("Rectangle3D").transform;
            container = new GameObject("3DVectorForce").transform;
            container.transform.parent = fibers;
        }

        private IEnumerator CreateArrowsCoroutine()
        {
            // Toplam ok sayısı
            int totalArrows = gradientPoints.Count;
            arrows = new List<GameObject>(totalArrows);

            for (int i = 0; i < totalArrows; i += arrowsPerFrame)
            {
                // Bu frame'de işlenecek gradientPoints alt kümesini al
                List<GradientDataset> batchPoints = gradientPoints
                    .Skip(i)
                    .Take(Mathf.Min(arrowsPerFrame, totalArrows - i))
                    .ToList();

                // Bu grup için okları oluştur
                List<GameObject> batchArrows = VectorObjectVis.instance.CreateArrows(
                    batchPoints, container, localScaleRateTo3DVectorVisualize);

                // Ana listeye ekle
                arrows.AddRange(batchArrows);

                // Bir sonraki frame'e geç
                yield return null;
            }

            Debug.Log($"Created {arrows.Count} arrow glyphs");
        }

        #endregion private

    }
}