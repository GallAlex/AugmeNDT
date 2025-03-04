using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace AugmeNDT
{
    public class InteractiveIntersectionPointVis : MonoBehaviour
    {
        public static InteractiveIntersectionPointVis Instance;

        private static TopologicalDataObject topologicalDataObjectInstance;
        private float gaussianSigma = 0.5f;

        /// For 2D Flow
        private List<GameObject> _2DIntersectionPoints = new List<GameObject>();
        private List<Vector3> _2DPreviousPositions = new List<Vector3>();
        private List<GradientDataset> _2DGeneratedGradientPoints = new List<GradientDataset>();
        private bool are2DSpheresCreated = false;

        /// For 3D Flow
        private List<GameObject> _3DIntersectionPoints = new List<GameObject>();
        private List<Vector3> _3DPreviousPositions = new List<Vector3>();
        private List<GradientDataset> _3DGeneratedGradientPoints = new List<GradientDataset>();
        private bool are3DSpheresCreated = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
        private void Start()
        {
            if (topologicalDataObjectInstance == null)
                topologicalDataObjectInstance = TopologicalDataObject.Instance;
        }
        
        public void Show2DSpheres() => ShowSpheres();
        public void Hide2DSpheres() => HideSpheres();

        public void Show3DSpheres() => ShowSpheres(for2D: false);
        public void Hide3DSpheres() => HideSpheres(for2D: false);

        public List<Vector3> Get2DSpherePositions() => GetSpherePositions();
        public List<GradientDataset> Get2DGeneratedGradientPoints() => GetGeneratedGradientPoints();

        public List<Vector3> Get3DSpherePositions() => GetSpherePositions(getPointsFor2D:false);
        public List<GradientDataset> Get3DGeneratedGradientPoints() => GetGeneratedGradientPoints(getPointsFor2D: false);

        #region private
        private bool TheInteractivePointsChanged(bool for2D = true)
        {
            if (for2D)
            {
                if (!_2DIntersectionPoints.Any() || !_2DPreviousPositions.Any())
                    return true;

                foreach (var item in _2DIntersectionPoints)
                {
                    if (!_2DPreviousPositions.Contains(item.transform.position))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (!_3DIntersectionPoints.Any() || !_3DPreviousPositions.Any())
                    return true;

                foreach (var item in _3DIntersectionPoints)
                {
                    if (!_3DPreviousPositions.Contains(item.transform.position))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ShowSpheres(bool for2D = true)
        {
            if (for2D)
            {
                if (are2DSpheresCreated)
                {
                    foreach (var item in _2DIntersectionPoints)
                    {
                        item.SetActive(true);
                    }
                }
                else
                    CreateSpheres();
            }
            else
            {
                if (are3DSpheresCreated)
                {
                    foreach (var item in _3DIntersectionPoints)
                    {
                        item.SetActive(true);
                    }
                }
                else
                    CreateSpheres(createPointsFor2D:false);
            }
        }
        private void HideSpheres(bool for2D = true)
        {
            if (for2D)
            {
                if (!are2DSpheresCreated)
                    return;

                foreach (var item in _2DIntersectionPoints)
                {
                    item.SetActive(false);
                }
            }
            else
            {
                if (!are3DSpheresCreated)
                    return;

                foreach (var item in _3DIntersectionPoints)
                {
                    item.SetActive(false);
                }
            }
        }

        private List<Vector3> GetSpherePositions(bool getPointsFor2D = true)
        {
            UpdateInstance(getPointsFor2D);

            var intersectionPoints = getPointsFor2D ? _2DIntersectionPoints : _3DIntersectionPoints;

            List <Vector3> positions = new List<Vector3>();

            for (int i = 0; i < intersectionPoints.Count; i++)
            {
                positions.Insert(i, intersectionPoints[i].transform.position);
            }

            return positions;
        }
        private List<GradientDataset> GetGeneratedGradientPoints(bool getPointsFor2D = true)
        {
            UpdateInstance(getPointsFor2D);

            return getPointsFor2D ? _2DGeneratedGradientPoints : _3DGeneratedGradientPoints;
        }

        private void UpdateInstance(bool getPointsFor2D = true)
        {
            if (getPointsFor2D)
                Update2DInstance();
            else
                Update3DInstance();
        }
        private void Update2DInstance()
        {
            if (!are2DSpheresCreated)
            {
                CreateSpheres();
                CreateNewGeneratedGradientPoints(); // yeni alandaki noktaların gradyanları
            }
            else if (TheInteractivePointsChanged())
            {
                CreateNewGeneratedGradientPoints();
            }
        }
        private void Update3DInstance()
        {
            if (!are3DSpheresCreated)
            {
                CreateSpheres(createPointsFor2D:false);
                CreateNewGeneratedGradientPoints(createPointsFor2D: false); // yeni alandaki noktaların gradyanları
            }
            else if (TheInteractivePointsChanged(for2D:false))
            {
                CreateNewGeneratedGradientPoints(createPointsFor2D: false); // yeni alandaki noktaların gradyanları
            }
        }

        private void CreateSpheres(bool createPointsFor2D = true)
        {
            var camPosition = Camera.main.transform.position;
            int limit = createPointsFor2D ? 3 : 6;
            var points = new List<GameObject>();
            for (int i = 0; i < limit; i++)
            {
                var intersectionPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                intersectionPoint.transform.position = new Vector3(camPosition.x + i, camPosition.y + i, camPosition.z); // Küreleri sahneye koy
                intersectionPoint.transform.localScale = Vector3.one * 0.3f;
                intersectionPoint.AddComponent<SphereCollider>();
                intersectionPoint.AddComponent<Rigidbody>().useGravity = false;
                intersectionPoint.GetComponent<Rigidbody>().isKinematic = true; // Sadece elle hareket etsin
                intersectionPoint.AddComponent<DraggableInteractivePoint>(); // Sürükleme scriptini ekle
                if(!createPointsFor2D){
                    MeshRenderer renderer = intersectionPoint.GetComponent<MeshRenderer>();
                    renderer.material = new Material(Shader.Find("Standard")); // Yeni materyal ata
                    if (i < 3)
                    {
                        renderer.material.color = Color.blue; 
                        intersectionPoint.tag = "Triangle1";
                    }
                    else
                    {
                        intersectionPoint.tag = "Triangle2";
                        renderer.material.color = Color.red; // Küreyi kırmızı yap
                    }

                }

                intersectionPoint.transform.position = TTest(i, createPointsFor2D);
                
                points.Add(intersectionPoint);
            }

            if (createPointsFor2D)
            {
                _2DIntersectionPoints.AddRange(points);
                are2DSpheresCreated = true;
            }
            else
            {
                _3DIntersectionPoints.AddRange(points);
                are3DSpheresCreated = true;
            }
        }

        private void CreateNewGeneratedGradientPoints(bool createPointsFor2D = true)
        {
            if (topologicalDataObjectInstance == null)
                topologicalDataObjectInstance = TopologicalDataObject.Instance;

            List<Vector3> points = PreparePointsToGenerateGradient(createPointsFor2D);

            var newGeneratedGradientPoints = createPointsFor2D
                ? SpatialCalculations.GenerateTrianglePoints(points[0], points[1], points[2], 30)
                : SpatialCalculations.GeneratePrismPoints(points, 30);

            newGeneratedGradientPoints = GradientUtils.AssignNewGradientValues(newGeneratedGradientPoints, topologicalDataObjectInstance.gradientList);

            if (createPointsFor2D)
                newGeneratedGradientPoints = GradientUtils.NormalizeGradientsToPlane(newGeneratedGradientPoints, points[0], points[1], points[2]);

            newGeneratedGradientPoints = GaussianFilterUtils.ApplyGaussianSmoothing(newGeneratedGradientPoints, gaussianSigma);

            if (createPointsFor2D)
                _2DGeneratedGradientPoints = newGeneratedGradientPoints;
            else
                _3DGeneratedGradientPoints = newGeneratedGradientPoints;

        }

        private List<Vector3> PreparePointsToGenerateGradient(bool createPointsFor2D)
        {
            List<Vector3> points = new List<Vector3>();
            if (createPointsFor2D)
            {
                _2DIntersectionPoints.ForEach(x =>
                {
                    points.Add(x.transform.position);
                    _2DPreviousPositions.Add(x.transform.position);
                });
            }
            else
            {
                List<Vector3> triangle1 = GameObject.FindGameObjectsWithTag("Triangle1").ToList().Select(x => x.transform.position).ToList();
                List<Vector3> triangle2 = GameObject.FindGameObjectsWithTag("Triangle2").ToList().Select(x => x.transform.position).ToList();
                points = ProcessTriangles(triangle1, triangle2);
                _3DPreviousPositions = points.ToList();
            }
            return points;
        }
        private List<Vector3> ProcessTriangles(List<Vector3> triangle1, List<Vector3> triangle2)
        {
            List<Vector3> points = new List<Vector3>();
            HashSet<Vector3> usedVectors = new HashSet<Vector3>(); // Kullanılmış vektörleri takip et

            // Öncelikle triangle1 içindeki tüm noktaları points listesine ekleyelim
            points.AddRange(triangle1);

            // Şimdi triangle1'deki her vektör için triangle2 içinden en yakın vektörü bul ve ekle
            foreach (var v1 in triangle1)
            {
                Vector3 closest = Vector3.zero;
                float minDistance = float.MaxValue;

                foreach (var v2 in triangle2)
                {
                    if (!usedVectors.Contains(v2)) // Daha önce eklenmemişse kontrol et
                    {
                        float distance = Math.Abs(Vector3.Distance(v1, v2));
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closest = v2;
                        }
                    }
                }

                if (minDistance < float.MaxValue) // Geçerli bir en yakın nokta bulunduysa ekle
                {
                    points.Add(closest);
                    usedVectors.Add(closest); // Aynı vektörü bir daha kullanmamak için işaretle
                }
            }

            return points;
        }

        #endregion private

        //TODO
        ///TESTTT
        ///
        private Vector3 TTest(int i, bool Is2D)
        {
            if (Is2D)
            {
                //TODO FOR TEST
                if (i == 0)
                {
                    return new Vector3(10f, 9f, 22f);
                }
                else if (i == 1)
                {
                    return new Vector3(8f, 9f, 13f);
                }
                else
                {
                    return new Vector3(18f, 10f, 10f);
                }
            }
            else
            {
                if (i == 0)
                {
                    return new Vector3(15f, 9f, 15f);
                }
                else if (i == 1)
                {
                    return new Vector3(17f, 9f, 12f);
                }
                else if (i == 2)
                {
                    return new Vector3(14f, 9f, 12f);
                }
                else if (i == 3)
                {
                    return new Vector3(17f, 7f, 15f);
                }
                else if (i == 4)
                {
                    return new Vector3(19f, 7f, 11f);
                }
                else
                {
                    return new Vector3(14f, 7f, 14f);
                }
            }
        }
    }

}

