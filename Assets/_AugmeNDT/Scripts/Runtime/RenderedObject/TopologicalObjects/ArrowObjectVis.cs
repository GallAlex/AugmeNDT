using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    public class ArrowObjectVis: MonoBehaviour
    {
        public static ArrowObjectVis Instance;
        
        private void Awake()
        {
            Instance = this;
        }

        public List<GameObject> CreateArrows(List<GradientDataset> generatedGradientPoints, GameObject arrowPrefab, Transform container)
        {
            List<GameObject> arrows = new List<GameObject>();
            float maxMag = generatedGradientPoints.Select(x => x.Magnitude).Max();
            generatedGradientPoints.ForEach(x =>
            {
                if (x.Magnitude != 0)
                {
                    arrows.Add(CreateArrow(x.Position, x.Direction, x.Magnitude, maxMag, arrowPrefab, container));
                }
                else
                {
                    //TODO
                }
            });

            return arrows;
        }

        private GameObject CreateArrow(Vector3 position, Vector3 direction, float magnitude,float maxMag, GameObject arrowPrefab, Transform container)
        {
            GameObject arrow = Instantiate(arrowPrefab, position, Quaternion.identity, container);
            arrow.transform.forward = direction.normalized;

            float normalizedMagnitude = Mathf.InverseLerp(0, maxMag, magnitude);
            Renderer[] renderers = arrow.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.material.color = Color.Lerp(Color.blue, Color.red, normalizedMagnitude);
            }
            return arrow;
        }
    }
}
