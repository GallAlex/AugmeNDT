using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace AugmeNDT
{
    public class FlowObjects : MonoBehaviour
    {
        private StreamLine2D streamLine2DInstance;
        private float sphereSpeed;
        private float lifetime;
        private static InteractiveIntersectionPointVis interactiveIntersectionPointVisInstance;

        public void Initialize(float speed, float time)
        {
            sphereSpeed = speed;
            lifetime = time;
        }

        private void Start()
        {
            streamLine2DInstance = StreamLine2D.Instance;
            interactiveIntersectionPointVisInstance = InteractiveIntersectionPointVis.Instance;
            StartCoroutine(StartMoveSphere(
                borders2D: interactiveIntersectionPointVisInstance.Get2DSpherePositions(),
                streamlineStepSize: streamLine2DInstance.streamLine2DCalculation.streamlineStepSize,
                gradientPoints: streamLine2DInstance.streamLine2DCalculation.gradientPoints,
                is2DField: true));
        }

        public IEnumerator StartMoveSphere(List <Vector3> borders2D = null, List<Vector3> borders3D = null,
            List<GradientDataset> gradientPoints = null, float? streamlineStepSize = null ,bool is2DField = true)
        {
            if (gradientPoints == null || !streamlineStepSize.HasValue)
                yield return null;

            if (borders2D == null && borders3D == null)
                yield return null;

            Vector3 currentPosition = transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < lifetime)
            {
                Vector3 direction = RungeKutta4(currentPosition, gradientPoints, (float)streamlineStepSize);
                if (direction.magnitude < 0.01f) break;

                Vector3 nextPosition = currentPosition + direction.normalized * sphereSpeed * Time.deltaTime;

                if (is2DField && !IsPointInTriangle(nextPosition, borders2D[0], borders2D[1], borders2D[2])) break;
                if (!is2DField && !IsPointInPrism(nextPosition)) break;

                transform.position = nextPosition;
                currentPosition = nextPosition;
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            Destroy(gameObject);
        }

        private bool IsPointInTriangle(Vector3 pCurrent, Vector3 p1,Vector3 p2,Vector3 p3) => SpatialCalculations.IsPointInTriangle(pCurrent,p1,p2,p3);
        private bool IsPointInPrism(Vector3 pCurrent) => StreamLine3D.Instance.IsPointInPrism(pCurrent);

        private Vector3 RungeKutta4(Vector3 pCurrent, List<GradientDataset> gradientPoints, float streamlineStepSize) => SpatialCalculations.RungeKutta4(pCurrent, gradientPoints, streamlineStepSize);
    }
}
