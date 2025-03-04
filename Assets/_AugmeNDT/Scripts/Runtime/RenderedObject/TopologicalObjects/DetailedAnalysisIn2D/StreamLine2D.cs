using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

namespace AugmeNDT
{
    public class StreamLine2D : MonoBehaviour
    {
        public static StreamLine2D Instance;
        public Material streamlineMaterial;
        public StreamLine2DCalculation streamLine2DCalculation;

        private List<Vector3> intersectionPositions = new List<Vector3>();
        public static Vector3 point1, point2, point3;
        public int numStreamlines = 400; // Kaç tane streamline çizileceği
        public float streamLineStepSize = 0.01f; // Adım büyüklüğü
        public int maxStreamlineSteps = 700; // Maksimum adım sayısı

        private List<GameObject> LineObjs = new List<GameObject>();
        private InteractiveIntersectionPointVis interactiveIntersectionPointVisInstance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

        }
        private void Start()
        {
            if (interactiveIntersectionPointVisInstance == null)
            {
                interactiveIntersectionPointVisInstance = InteractiveIntersectionPointVis.Instance;
            }
        }

        public void ShowStreamLines()
        {
            bool isItUpdated = !LineObjs.Any() ? true : IsItUpdated();

            if (isItUpdated)
                DrawStreamlines();
            else
                LineObjs.ForEach(line => { line.SetActive(true); });
        }
        public void HideStreamLines()
        {
            foreach (var line in LineObjs)
            {
                line.SetActive(false);
            }
        }

        #region private
        private void DrawStreamlines()
        {
            DestroyLines();
            UpdateIntersectionPoints();
            var gradientPoints = interactiveIntersectionPointVisInstance.Get2DGeneratedGradientPoints();
            streamLine2DCalculation = new StreamLine2DCalculation(intersectionPositions, numStreamlines,
                    streamLineStepSize, maxStreamlineSteps, gradientPoints);

            List<List<Vector3>> streamLines = streamLine2DCalculation.CalculateStreamlines();
            streamLines.ForEach(lines =>
            {
                CreateLineRenderer(lines);
            });
        }
        private void UpdateIntersectionPoints() 
        { 
            intersectionPositions = interactiveIntersectionPointVisInstance.Get2DSpherePositions();
        }
        private bool IsItUpdated()
        {
            bool isItUpdated = false;
            foreach (var item in interactiveIntersectionPointVisInstance.Get2DSpherePositions())
            {
                if (!intersectionPositions.Contains(item))
                {
                    isItUpdated = true;
                }
            }
            return isItUpdated;
        }
        private void CreateLineRenderer(List<Vector3> points, bool isSaddle = false)
        {
            if (points.Count < 2) return;

            GameObject lineObj = new GameObject(isSaddle ? "Separatrix" : "Streamline");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();

            // Kullanılacak materyali doğru şekilde ayarla
            lr.material = streamlineMaterial;
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.startWidth = 0.01f;
            lr.endWidth = 0.01f;

            if (isSaddle)
            {
                // Separatrix için özel ayarlar
                lr.startColor = Color.green;
                lr.endColor = Color.green;
                lr.startWidth = 0.03f;
                lr.endWidth = 0.03f;
            }

            LineObjs.Add(lineObj);
        }
        private void DestroyLines()
        {
            LineObjs.ForEach(x => Destroy(x));
            LineObjs.Clear(); // Listeyi temizle
        }
        #endregion private
    }
}
