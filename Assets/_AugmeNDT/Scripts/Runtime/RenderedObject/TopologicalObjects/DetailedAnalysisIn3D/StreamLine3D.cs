using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    public class StreamLine3D:MonoBehaviour
    {
        public static StreamLine3D Instance;
        private List<Vector3> intersectipnPositions = new List<Vector3>();

        public int numStreamlines = 100; // Çizilecek streamline sayısı
        public float streamlineStepSize = 0.2f; // Adım büyüklüğü
        public int maxStreamlineSteps = 100; // Maksimum adım sayısı
        public Material streamlineMaterial; // LineRenderer için malzeme
        public GameObject spherePrefab; // Küre prefabı
        public int numSpheres = 5; // Aynı anda kaç küre olacak
        public bool enableSpheres = true;
        public float sphereSpeed = 0.5f;
        private InteractiveIntersectionPointVis interactiveIntersectionPointVisInstance;
        private bool stopFlowObjects = false;
        private List<GameObject> LineObjs = new List<GameObject>();
        public float lifetime = 15f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            if (interactiveIntersectionPointVisInstance == null)
            {
                interactiveIntersectionPointVisInstance = InteractiveIntersectionPointVis.Instance;
            }
        }

        public void DrawStreamlines()
        {
            bool firstVisualization = !LineObjs.Any();
            bool isItUpdated = false;

            if (!firstVisualization)
            {
                foreach (var item in interactiveIntersectionPointVisInstance.Get3DSpherePositions())
                {
                    if (!intersectipnPositions.Contains(item))
                    {
                        isItUpdated = true;
                    }
                }
            }

            if (firstVisualization || isItUpdated)
            {
                intersectipnPositions = interactiveIntersectionPointVisInstance.Get3DSpherePositions();
                DestroyLines();
                for (int i = 0; i < numStreamlines; i++)
                {
                    Vector3 startPosition = GetRandomPointInPrism();
                    List<Vector3> streamlinePoints = GenerateStreamline(startPosition);
                    CreateLineRenderer(streamlinePoints);
                }
            }
            else
                ShowStreamLines();

        }

        public void HideStreamLines()
        {
            foreach (var line in LineObjs)
            {
                line.SetActive(false);
            }
        }

        public void ShowStreamLines()
        {
            foreach (var line in LineObjs)
            {
                line.SetActive(true);
            }
        }

        public void StartFlowObject()
        {
            stopFlowObjects = false;
            if (enableSpheres)
            {
                StartCoroutine(SpawnMovingSpheres());
            }
        }

        public void PauseFlowObject()
        {
            stopFlowObjects = true;
            GameObject.FindGameObjectsWithTag("Moving3DSphere").ToList().ForEach(x => Destroy(x));
        }

        #region private
        private void CreateLineRenderer(List<Vector3> points)
        {
            if (points.Count < 2) return;

            GameObject lineObj = new GameObject("Streamline");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();

            lr.material = streamlineMaterial;
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;

            LineObjs.Add(lineObj);
        }
        private Vector3 GetRandomPointInPrism()
        {
            var points = interactiveIntersectionPointVisInstance.Get3DGeneratedGradientPoints();
            int index = Random.Range(0, points.Count);
            return points[index].Position;
        }
        private List<Vector3> GenerateStreamline(Vector3 startPosition)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3 currentPosition = startPosition;

            for (int i = 0; i < maxStreamlineSteps; i++)
            {
                currentPosition += EulerStep(currentPosition);
                Vector3 direction = RungeKutta4(currentPosition);
                if (direction.magnitude < 0.001f) break; // Küçük hareket varsa durdur

                points.Add(currentPosition);
                currentPosition += direction.normalized * streamlineStepSize;

                if (!IsPointInPrism(currentPosition)) break;
            }

            return points;
        }
        private Vector3 EulerStep(Vector3 position)
        {
            return InterpolateVectorField(position).normalized * streamlineStepSize;
        }

        private Vector3 RungeKutta4(Vector3 position)
        {
            float h = streamlineStepSize;

            Vector3 k1 = InterpolateVectorField(position);
            Vector3 k2 = InterpolateVectorField(position + 0.5f * h * k1);
            Vector3 k3 = InterpolateVectorField(position + 0.5f * h * k2);
            Vector3 k4 = InterpolateVectorField(position + h * k3);

            return (k1 + 2f * k2 + 2f * k3 + k4) / 6f;
        }
        private Vector3 InterpolateVectorField(Vector3 position)
        {
            List<GradientDataset> nearestPoints = SpatialCalculations.GetNearbyPoints(interactiveIntersectionPointVisInstance.Get3DGeneratedGradientPoints(), position, 1.0f);
            if (nearestPoints.Count == 0) return Vector3.zero;

            Vector3 interpolatedDirection = Vector3.zero;
            float totalWeight = 0f;

            foreach (var neighbor in nearestPoints)
            {
                float distance = Vector3.Distance(position, neighbor.Position);
                float weight = Mathf.Exp(-Mathf.Pow(distance, 2) / 0.2f); // Gaussian ağırlık

                interpolatedDirection += neighbor.Direction * weight;
                totalWeight += weight;
            }

            return totalWeight > 0 ? interpolatedDirection / totalWeight : Vector3.zero;
        }
        public bool IsPointInPrism(Vector3 p)
        {
            var vertices = intersectipnPositions;

            float minX = Mathf.Min(vertices[0].x, vertices[1].x, vertices[2].x, vertices[3].x, vertices[4].x, vertices[5].x);
            float maxX = Mathf.Max(vertices[0].x, vertices[1].x, vertices[2].x, vertices[3].x, vertices[4].x, vertices[5].x);
            float minY = Mathf.Min(vertices[0].y, vertices[1].y, vertices[2].y, vertices[3].y, vertices[4].y, vertices[5].y);
            float maxY = Mathf.Max(vertices[0].y, vertices[1].y, vertices[2].y, vertices[3].y, vertices[4].y, vertices[5].y);
            float minZ = Mathf.Min(vertices[0].z, vertices[1].z, vertices[2].z, vertices[3].z, vertices[4].z, vertices[5].z);
            float maxZ = Mathf.Max(vertices[0].z, vertices[1].z, vertices[2].z, vertices[3].z, vertices[4].z, vertices[5].z);

            return (p.x >= minX && p.x <= maxX) &&
                   (p.y >= minY && p.y <= maxY) &&
                   (p.z >= minZ && p.z <= maxZ);
        }
        private IEnumerator SpawnMovingSpheres()
        {
            while (true)
            {
                if (stopFlowObjects)
                    break;

                int currentSpheres = GameObject.FindGameObjectsWithTag("MovingSphere").Length;

                if (currentSpheres < numSpheres)
                {
                    int spheresToSpawn = numSpheres - currentSpheres;

                    for (int i = 0; i < spheresToSpawn; i++)
                    {
                        Vector3 startPosition = GetRandomPointInPrism();
                        GameObject sphere = Instantiate(spherePrefab, startPosition, Quaternion.identity);
                        sphere.tag = "MovingSphere"; // Küreleri takip etmek için
                        FlowObjects movingSphere = sphere.AddComponent<FlowObjects>();
                        movingSphere.Initialize(sphereSpeed, lifetime);
                        movingSphere.StartMoveSphere(borders2D: intersectipnPositions, is2DField: false);
                    }
                }

                yield return new WaitForSeconds(1f); // Daha kısa aralıklarla yeni küreler oluştur
            }
        }
        private void DestroyLines()
        {
            LineObjs.ForEach(x => Destroy(x));
            LineObjs.Clear(); // Listeyi temizle
        }
        #endregion private

    }
}
