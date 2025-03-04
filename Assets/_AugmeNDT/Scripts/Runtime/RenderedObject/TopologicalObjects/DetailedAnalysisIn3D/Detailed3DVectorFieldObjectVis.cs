using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace AugmeNDT
{
    public class Detailed3DVectorFieldObjectVis: MonoBehaviour
    {
        public static Detailed3DVectorFieldObjectVis Instance;

        public GameObject arrowPrefab;

        private List<GameObject> arrows = new List<GameObject>();
        private List<GradientDataset> generatedGradientPoints = new List<GradientDataset>();
        private Transform container;

        private static ArrowObjectVis arrowObjectVisInstance;
        private static InteractiveIntersectionPointVis interactiveIntersectionPointVisInstance;
        private List<Vector3> intersectionPositions = new List<Vector3>();


        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            container = new GameObject("3DVectorForce").transform;
        }
        public void Start()
        {
            if (arrowPrefab == null)
                Debug.LogWarning("arrowPrefab tanımlı degil");

            if (interactiveIntersectionPointVisInstance == null)
                interactiveIntersectionPointVisInstance = InteractiveIntersectionPointVis.Instance;

            if (arrowObjectVisInstance == null)
                arrowObjectVisInstance = ArrowObjectVis.Instance;
        }

        public void VisualizePoints()
        {
            bool firstVisualization = !generatedGradientPoints.Any();
            bool isItUpdated = false;

            if (!firstVisualization)
            {
                foreach (var item in interactiveIntersectionPointVisInstance.Get3DSpherePositions())
                {
                    if (!intersectionPositions.Contains(item))
                    {
                        isItUpdated = true;
                    }
                }
            }

            if (firstVisualization || isItUpdated)
            {
                intersectionPositions = interactiveIntersectionPointVisInstance.Get3DSpherePositions();
                generatedGradientPoints = interactiveIntersectionPointVisInstance.Get3DGeneratedGradientPoints();
                DestroyArrows();
                Deneme();
                //arrows = arrowObjectVisInstance.CreateArrows(generatedGradientPoints, arrowPrefab, container);
            }
            else
                ShowHideArrows(true);
        }

        public void ShowHideArrows(bool showArrows)
        {
            foreach (var arrow in arrows)
                arrow.SetActive(showArrows);
        }

        private void DestroyArrows()
        {
            arrows.ForEach(x => Destroy(x));
            arrows.Clear(); // Listeyi temizle
        }
        private void Deneme()
        {
            foreach (var item in generatedGradientPoints)
            {
                var intersectionPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                intersectionPoint.transform.position = new Vector3(item.Position.x, item.Position.y, item.Position.z); // Küreleri sahneye koy
                intersectionPoint.transform.localScale = Vector3.one * 0.1f;
                intersectionPoint.AddComponent<SphereCollider>();
                intersectionPoint.AddComponent<Rigidbody>().useGravity = false;
                intersectionPoint.GetComponent<Rigidbody>().isKinematic = true; // Sadece elle hareket etsin
                intersectionPoint.AddComponent<DraggableInteractivePoint>(); // Sürükleme scriptini ekle
                MeshRenderer renderer = intersectionPoint.GetComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Standard")); // Yeni materyal ata
                renderer.material.color = Color.yellow; // Küreyi kırmızı yap
            }
        }

        //public void GeneratePrismPoints()
        //{
        //    generatedGradientPoints.Clear();

        //    for (int i = 0; i <= numSteps; i++)
        //    {
        //        float alpha = i / (float)numSteps;

        //        Vector3 p1 = Vector3.Lerp(bottomP1, upP1, alpha);
        //        Vector3 p2 = Vector3.Lerp(bottomP2, upP2, alpha);
        //        Vector3 p3 = Vector3.Lerp(bottomP3, upP3, alpha);

        //        for (int j = 0; j <= numSteps; j++)
        //        {
        //            for (int k = 0; k <= numSteps - j; k++)
        //            {
        //                Vector3 newPoint = p1 + (j / (float)numSteps) * (p2 - p1) + (k / (float)numSteps) * (p3 - p1);
        //                generatedGradientPoints.Add(new GradientData(generatedGradientPoints.Count, newPoint, Vector3.zero, 0f));
        //            }
        //        }
        //    }

        //    Debug.Log($"Prizma içinde {generatedGradientPoints.Count} nokta olu?turuldu.");
        //}

        //public void AssignGradientValues()
        //{
        //    if (gradientLoader == null || gradientLoader.vectorField.Count == 0)
        //    {
        //        Debug.LogError("Gradient verisi yüklenmedi!");
        //        return;
        //    }

        //    foreach (var point in generatedGradientPoints)
        //    {
        //        GradientData nearestGradient = FindNearestGradient(point.Position);
        //        point.Direction = nearestGradient.Direction;
        //        point.Magnitude = nearestGradient.Magnitude;
        //    }

        //    Debug.Log("Gradient verileri prizma içindeki noktalara atand?.");
        //}

        //public void ApplyGaussianSmoothing()
        //{
        //    List<GradientData> smoothedGradientPoints = new List<GradientData>();

        //    foreach (var point in generatedGradientPoints)
        //    {
        //        List<GradientData> neighbors = GetNearbyPoints(point.Position, 1.0f);
        //        if (neighbors.Count == 0) continue;

        //        Vector3 weightedDirection = Vector3.zero;
        //        float weightedMagnitude = 0f;
        //        float totalWeight = 0f;

        //        foreach (var neighbor in neighbors)
        //        {
        //            float distance = Vector3.Distance(point.Position, neighbor.Position);
        //            float weight = Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * Mathf.Pow(gaussianSigma, 2)));

        //            weightedDirection += neighbor.Direction * weight;
        //            weightedMagnitude += neighbor.Magnitude * weight;
        //            totalWeight += weight;
        //        }

        //        if (totalWeight > 0)
        //        {
        //            weightedDirection /= totalWeight;
        //            weightedMagnitude /= totalWeight;
        //        }

        //        smoothedGradientPoints.Add(new GradientData(point.ID, point.Position, weightedDirection.normalized, weightedMagnitude));
        //    }

        //    generatedGradientPoints = smoothedGradientPoints;
        //    Debug.Log("Gaussian Kernel Smoothing i?lemi tamamland?.");
        //}

        //public void DrawPrismEdges()
        //{
        //    foreach (GameObject edge in edgeObjects)
        //    {
        //        Destroy(edge);
        //    }
        //    edgeObjects.Clear();

        //    List<Vector3[]> edges = new List<Vector3[]>
        //{
        //    new Vector3[] { upP1, upP2 }, new Vector3[] { upP2, upP3 }, new Vector3[] { upP3, upP1 },
        //    new Vector3[] { bottomP1, bottomP2 }, new Vector3[] { bottomP2, bottomP3 }, new Vector3[] { bottomP3, bottomP1 },
        //    new Vector3[] { upP1, bottomP1 }, new Vector3[] { upP2, bottomP2 }, new Vector3[] { upP3, bottomP3 }
        //};

        //    foreach (var edge in edges)
        //    {
        //        GameObject lineObj = new GameObject("PrismEdge");
        //        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        //        lr.material = lineMaterial;
        //        lr.positionCount = 2;
        //        lr.SetPositions(edge);
        //        lr.startWidth = 0.05f;
        //        lr.endWidth = 0.05f;
        //        edgeObjects.Add(lineObj);
        //    }
        //}

        //public void VisualizeVectorField()
        //{
        //    foreach (GameObject arrow in arrowObjects)
        //    {
        //        Destroy(arrow);
        //    }
        //    arrowObjects.Clear();

        //    float maxMag = generatedGradientPoints.Select(x => x.Magnitude).Max();
        //    generatedGradientPoints.ForEach(x =>
        //    {
        //        CreateArrow(x.Position, x.Direction.normalized, x.Magnitude, maxMag);
        //    });
        //}

        //public void CreateArrow(Vector3 position, Vector3 direction, float magnitude, float maxMag)
        //{
        //    if (magnitude == 0) return;

        //    GameObject arrow = Instantiate(arrowPrefab, position, Quaternion.identity);
        //    arrow.transform.forward = direction.normalized;

        //    float normalizedMagnitude = Mathf.InverseLerp(0, maxMag, magnitude);
        //    Renderer[] renderers = arrow.GetComponentsInChildren<Renderer>();
        //    foreach (Renderer r in renderers)
        //    {
        //        r.material.color = Color.Lerp(Color.blue, Color.red, normalizedMagnitude);
        //    }
        //    arrowObjects.Add(arrow);
        //}

        //public GradientData FindNearestGradient(Vector3 position)
        //{
        //    List<GradientData> nearest = new List<GradientData>();
        //    float minDistance = 0.2f;

        //    for (int i = 0; i < 4; i++)
        //    {
        //        gradientLoader.vectorField.ForEach(x =>
        //        {
        //            if (Vector3.Distance(position, x.Position) <= minDistance)
        //            {
        //                nearest.Add(x);
        //            }
        //        });

        //        if (nearest.Any())
        //            break;

        //        minDistance *= 2;
        //    }

        //    if (nearest.Count == 0)
        //        return new GradientData(-1, position, Vector3.zero, 0f);

        //    Vector3 avgDirection = Vector3.zero;
        //    float avgMagnitude = 0f;

        //    foreach (var data in nearest)
        //    {
        //        avgDirection += data.Direction;
        //        avgMagnitude += data.Magnitude;
        //    }

        //    avgDirection.Normalize();
        //    avgMagnitude = avgMagnitude / nearest.Count;

        //    return new GradientData(-1, position, avgDirection, avgMagnitude);
        //}

        //public List<GradientData> GetNearbyPoints(Vector3 position, float searchRadius)
        //{
        //    return generatedGradientPoints.Where(p => Vector3.Distance(position, p.Position) <= searchRadius).ToList();
        //}


        //#region Interactive
        //private List<GameObject> controlPoints = new List<GameObject>();
        //public GameObject spherePrefab;
        //public void CreateControlSpheres()
        //{
        //    List<Vector3> positions = new List<Vector3> { upP1, upP2, upP3, bottomP1, bottomP2, bottomP3 };

        //    foreach (var pos in positions)
        //    {
        //        GameObject sphere = Instantiate(spherePrefab, pos, Quaternion.identity);
        //        sphere.AddComponent<DraggableSphere>();
        //        controlPoints.Add(sphere);
        //    }
        //}
        //public void GeneratePrism()
        //{
        //    UpdateControlPoints();
        //    GeneratePrismPoints();
        //    AssignGradientValues();
        //    ApplyGaussianSmoothing();
        //    DrawPrismEdges();
        //    VisualizeVectorField();
        //}
        //private void UpdateControlPoints()
        //{
        //    upP1 = controlPoints[0].transform.position;
        //    upP2 = controlPoints[1].transform.position;
        //    upP3 = controlPoints[2].transform.position;
        //    bottomP1 = controlPoints[3].transform.position;
        //    bottomP2 = controlPoints[4].transform.position;
        //    bottomP3 = controlPoints[5].transform.position;
        //}
        //#endregion Interactive
    }
}
