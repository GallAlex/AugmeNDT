using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace AugmeNDT
{
    public class Detailed2DVectorFieldObjectVis: MonoBehaviour
    {
        public static Detailed2DVectorFieldObjectVis Instance;

        public GameObject arrowPrefab;

        private List<GameObject> arrows = new List<GameObject>();
        private List<GradientDataset> generatedGradientPoints = new List<GradientDataset>();
        private Transform container;
        private List<Vector3> intersectipnPositions = new List<Vector3>();
        private static ArrowObjectVis arrowObjectVisInstance;
        private static InteractiveIntersectionPointVis interactiveIntersectionPointVisInstance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            container = new GameObject("2DVectorForce").transform;
        }

        private void Start()
        {
            if (arrowPrefab == null)
                Debug.LogWarning("arrowPrefab tanımlı degil");
            
            if (interactiveIntersectionPointVisInstance == null)
                interactiveIntersectionPointVisInstance = InteractiveIntersectionPointVis.Instance;

            if(arrowObjectVisInstance == null)
                arrowObjectVisInstance = ArrowObjectVis.Instance;
        }

        public void VisualizePoints()
        {
            bool firstVisualization = !generatedGradientPoints.Any();
            bool isItUpdated = false;

            if (!firstVisualization)
            {
                foreach (var item in interactiveIntersectionPointVisInstance.Get2DSpherePositions())
                {
                    if (!intersectipnPositions.Contains(item))
                    {
                        isItUpdated = true;
                    }
                }
            }

            if (firstVisualization || isItUpdated)
            {
                intersectipnPositions = interactiveIntersectionPointVisInstance.Get2DSpherePositions();
                generatedGradientPoints = interactiveIntersectionPointVisInstance.Get2DGeneratedGradientPoints();
                DestroyArrows();
                arrows = arrowObjectVisInstance.CreateArrows(generatedGradientPoints, arrowPrefab, container);
            }
            else
                ShowHideArrows(true);
        }

        public void ShowHideArrows(bool showArrows)
        {
            foreach(var arrow in arrows)
                arrow.SetActive(showArrows);
        }

        private void DestroyArrows()
        {
            arrows.ForEach(x => Destroy(x));
            arrows.Clear(); // Listeyi temizle
        }
    }
}
