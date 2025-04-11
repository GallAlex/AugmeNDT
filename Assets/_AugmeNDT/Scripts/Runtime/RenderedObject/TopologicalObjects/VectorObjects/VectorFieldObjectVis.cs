using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Handles the visualization of vector fields using arrow prefabs.
    /// Uses gradient data to create 3D arrows.
    /// </summary>
    public class VectorFieldObjectVis : MonoBehaviour
    {
        // Singleton instance
        public static VectorFieldObjectVis instance;

        // Reference to the topological data instance containing gradient data
        private static TopologicalDataObject topologicalDataInstance;
        
        // Reference to the VectorObjectVis instance containing gradient data
        private static VectorObjectVis arrowObjectVisInstance;
        
        // List to store all created arrows for toggling visibility
        private List<GameObject> arrows = new List<GameObject>();
        
        // Parent container for all arrows in the scene
        private Transform container;
        // Volume Transform to set container's parent
        private static Transform volumeTransform;

        // Flags to check if arrows are already created and if they are currently hidden
        private bool arrowscalculated = false;
        private bool arrowshidden = false;

        //Scale factor for arrow size
        private float localScaleRate = 0.3f; // default
        //Number of arrows to create per frame
        private int arrowsPerFrame = 50;    // default

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void Start()
        {
            if (topologicalDataInstance == null)
            {
                topologicalDataInstance = TopologicalDataObject.instance;
                volumeTransform = topologicalDataInstance.volumeTransform;

                TopologyConfigData config = topologicalDataInstance.config;
                localScaleRate = config.localScaleRate;
                arrowsPerFrame = config.arrowsPerFrame;
            }

            if (arrowObjectVisInstance == null)
                arrowObjectVisInstance = VectorObjectVis.instance;

            SetContainer();
        }

        /// <summary>
        /// Generates and visualizes the vector field using gradient data.
        /// If arrows are already created, it simply makes them visible.
        /// Otherwise, it generates the arrows from `gradientList` in `TopologicalDataObject`.
        /// </summary>
        public void Visualize()
        {
            if (arrowscalculated)
            {
                ShowVectorField();
            }
            else
            {
                arrowscalculated = true;
                arrowshidden = false;
                StartCoroutine(CreateArrowsCoroutine());
            }
        }

        /// <summary>
        /// Makes all arrows in the vector field visible again.
        /// </summary>
        public void ShowVectorField()
        {
            if (!arrowshidden)
                return;

            arrows.ForEach(x => x.SetActive(true));
            arrowshidden = false;
        }

        /// <summary>
        /// Hides all arrows in the vector field without destroying them.
        /// </summary>
        public void HideVectorField()
        {
            if (arrowshidden)
                return;

            arrows.ForEach(x => x.SetActive(false));
            arrowshidden = true;
        }

        private IEnumerator CreateArrowsCoroutine()
        {
            List<GradientDataset> gradientPoints = topologicalDataInstance.gradientList;
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
                List<GameObject> batchArrows = arrowObjectVisInstance.CreateArrows(
                    batchPoints, container, localScaleRate);

                // Ana listeye ekle
                arrows.AddRange(batchArrows);

                // Bir sonraki frame'e geç
                yield return null;
            }

            Debug.Log($"Created {arrows.Count} arrow glyphs");
        }

        private void SetContainer()
        {
            GameObject generalVectorFieldArrows = new GameObject("GeneralVectorFieldArrows");
            container = generalVectorFieldArrows.transform;
            container.SetParent(volumeTransform);
        }
    }
}
