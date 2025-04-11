using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the visualization of 2D vector fields using arrow glyphs
    /// </summary>
    public class Glyph2DVectorField : MonoBehaviour
    {
        public static Glyph2DVectorField Instance;

        // List of arrow GameObjects representing vectors
        private List<GameObject> arrows = new List<GameObject>();

        // Cached gradient data for vector field visualization
        private List<GradientDataset> generatedGradientPoints = new List<GradientDataset>();

        // Container for organizing arrow objects in hierarchy
        private Transform container;

        private static float localScaleRate;
        private static int arrowsPerFrame = 50;
        private bool dontUseHideVolumeObjects = true;
        public bool IsVectorDrawn = false; // for onEnable

        // References to other manager instances
        private static VectorObjectVis arrowObjectVisInstance;
        private static RectangleManager rectangleManager;
        private void Awake()
        {
            // Initialize singleton instance
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            // Get references to required managers
            if (rectangleManager == null)
            {
                rectangleManager = RectangleManager.rectangleManager;
                localScaleRate = rectangleManager.config.Slice2D_VectorSizeRate;
            }

            if (arrowObjectVisInstance == null)
                arrowObjectVisInstance = VectorObjectVis.instance;

        }

        public void ShowArrows()
        {
            bool createNewVectors = !generatedGradientPoints.Any() || !arrows.Any() ||rectangleManager.IsUpdated();
            if (createNewVectors)
                VisualizePoints();
            else
            {
                foreach (var arrow in arrows)
                    arrow.SetActive(true);
            }

            IsVectorDrawn = true;
        }

        public void HideArrows()
        {
            foreach (var arrow in arrows)
                arrow.SetActive(false);

            IsVectorDrawn = false;
        }

        /// <summary>
        /// Creates and displays arrow glyphs to visualize the vector field
        /// </summary>
        private void VisualizePoints()
        {
            //Depends on rectangleManager. Therefore it can not call in start()
            SetContainer();

            DestroyArrows();
            generatedGradientPoints = rectangleManager.GetGradientPoints();

            // Çok sayıda ok varsa Coroutine kullan yoksa CreateArrows'u kullan
            if (generatedGradientPoints.Count > 100)
                StartCoroutine(CreateArrowsCoroutine());
            else
                arrows = arrowObjectVisInstance.CreateArrows(generatedGradientPoints, container, localScaleRate);
        }

        // Disable renderers but keep GameObject active
        public void HideVolumeObjects(bool hideObjects)
        {
            if (container == null || dontUseHideVolumeObjects)
                return;

            Renderer[] renderers = container.parent.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                // Skip streamline objects
                if (renderer.gameObject.name.Contains("2DVectorForce"))
                    continue;

                renderer.enabled = !hideObjects;
            }
        }

        private IEnumerator CreateArrowsCoroutine()
        {
            // Toplam ok sayısı
            int totalArrows = generatedGradientPoints.Count;
            arrows = new List<GameObject>(totalArrows);

            for (int i = 0; i < totalArrows; i += arrowsPerFrame)
            {
                // Bu frame'de işlenecek gradientPoints alt kümesini al
                List<GradientDataset> batchPoints = generatedGradientPoints
                    .Skip(i)
                    .Take(Mathf.Min(arrowsPerFrame, totalArrows - i))
                    .ToList();

                // Bu grup için okları oluştur
                List<GameObject> batchArrows = arrowObjectVisInstance.CreateArrows(batchPoints, container, localScaleRate);

                // Ana listeye ekle
                arrows.AddRange(batchArrows);

                // Bir sonraki frame'e geç
                yield return null;
            }

            Debug.Log($"Created {arrows.Count} arrow glyphs");
        }

        /// <summary>
        /// Creates a new container GameObject under the fiber object to hold all vector visuals.
        /// </summary>
        private void SetContainer()
        {
            if (container != null)
                return;

            GameObject _2DVectorForce = new GameObject("2DVectorForce");
            container = _2DVectorForce.transform;
            container.transform.parent = rectangleManager.GetInteractiveRectangleContainer();
        }

        /// <summary>
        /// Destroys all arrow objects and clears the list
        /// </summary>
        private void DestroyArrows()
        {
            arrows.ForEach(x => Destroy(x));
            arrows.Clear(); // Clear the list
        }
    }
}