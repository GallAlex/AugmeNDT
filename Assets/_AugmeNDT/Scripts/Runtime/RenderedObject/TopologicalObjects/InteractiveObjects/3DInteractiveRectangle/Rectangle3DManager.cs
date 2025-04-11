using Assets.Scripts.DataStructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages a 3D rectangular region for visualizing and analyzing vector field data
    /// </summary>
    public class Rectangle3DManager : MonoBehaviour
    {
        public static bool supportedByTTK = false;
        public static Rectangle3DManager rectangle3DManager;
        public TopologyConfigData config;
        public Transform volumeTransform;

        private static TopologicalDataObject topologicalDataObjectInstance;
        private List<GradientDataset> gradientPoints = new List<GradientDataset>();
        private List<CriticalPointDataset> criticalPoints = new List<CriticalPointDataset>();

        private GameObject rectangle;
        private Bounds currentCubeBounds;

        private TTKCalculations ttkCalculations;
        private void Awake()
        {
            // Initialize singleton instance
            rectangle3DManager = this;
        }

        private void Start()
        {
            // Get reference to topological data object
            if (topologicalDataObjectInstance == null)
            {
                topologicalDataObjectInstance = TopologicalDataObject.instance;
                ttkCalculations = topologicalDataObjectInstance.ttkCalculation;
                volumeTransform = topologicalDataObjectInstance.volumeTransform;
                config = topologicalDataObjectInstance.config;
            }
            ShowRectangle();
        }

        /// <summary>
        /// Shows the 3D rectangle wireframe, creating it if needed
        /// </summary>
        public void ShowRectangle()
        {
            if (rectangle == null)
            {
                rectangle3DManager.Create3DRectangle(topologicalDataObjectInstance.min3D,topologicalDataObjectInstance.max3D);
            }
            else
                rectangle.SetActive(true);
        }

        /// <summary>
        /// Hides the 3D rectangle wireframe
        /// </summary>
        public void HideRectangle()
        {
            if (rectangle != null)
                rectangle.SetActive(false);
        }

        /// <summary>
        /// Checks if the rectangle position or size has changed
        /// and updates internal data if necessary
        /// </summary>
        /// <returns>True if the bounds have changed since last update</returns>
        public bool IsUpdated()
        {
            bool IsUpdated = currentCubeBounds != GetRectangleBounds();
            if (IsUpdated)
            {
                UpdateInstance();
            }
            return IsUpdated;
        }

        /// <summary>
        /// Gets the current bounds of the wireframe cube
        /// </summary>
        /// <returns>Bounds representing the position and size of the cube</returns>
        public Bounds GetRectangleBounds()
        {
            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            return rectangleComponent.GetBounds();
        }

        /// <summary>
        /// Gets the gradient points inside the current 3D rectangle
        /// </summary>
        /// <returns>List of gradient points inside the rectangle</returns>
        public List<GradientDataset> GetGradientPoints()
        {
            IsUpdated();
            return gradientPoints;
        }

        /// <summary>
        /// Gets the critical points inside the current 3D rectangle
        /// </summary>
        /// <returns>List of critical points inside the rectangle</returns>
        public List<CriticalPointDataset> GetCriticalPoints()
        {
            IsUpdated();
            return criticalPoints;
        }
        
        public Transform GetRectangleContainer()
        {
            return rectangle.transform;
        }
        
        #region private
        /// <summary>
        /// Updates internal data when the rectangle position or size changes
        /// </summary>
        private void UpdateInstance()
        {
            UpdateCurrentPosition();

            CalculateGradientPoints();
            CalculateCriticalPoints();
        }

        /// <summary>
        /// Updates the current position of the rectangle
        /// </summary>
        private void UpdateCurrentPosition()
        {
            //update position
            currentCubeBounds = GetRectangleBounds();
        }

        /// <summary>
        /// Gets the minimum and maximum values of the box along each axis
        /// </summary>
        /// <returns>List containing [minX, maxX, minY, maxY, minZ, maxZ]</returns>
        private List<int> GetMinMaxValuesOfBox()
        {
            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            if (rectangleComponent != null)
            {
                // Sınırlayıcı kutuyu doğrudan wireframe bileşeninden al
                Bounds bounds = rectangleComponent.GetBounds();
                Vector3 center = bounds.center;
                Vector3 extents = bounds.extents; // Bu size/2'ye eşit

                // Min ve max değerleri hesapla
                int minX = (int)Math.Floor(center.x - extents.x);
                int maxX = (int)Math.Ceiling(center.x + extents.x);
                int minY = (int)Math.Floor(center.y - extents.y);
                int maxY = (int)Math.Ceiling(center.y + extents.y);
                int minZ = (int)Math.Floor(center.z - extents.z);
                int maxZ = (int)Math.Ceiling(center.z + extents.z);

                return new List<int>(new[] { minX, maxX, minY, maxY, minZ, maxZ });
            }
            return new List<int>();
        }

        /// <summary>
        /// Mevcut wireframe sınırları içinde 0.2 birim aralıklarla gradient noktaları oluşturur
        /// </summary>
        private void CalculateGradientPointsNew()
        {
            // Önceki verileri temizle
            gradientPoints.Clear();

            // Dikdörtgenin sınırlarını al
            Bounds bounds = GetRectangleBounds();
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            // Noktalar arası mesafe
            float spacing = 0.1f;

            // X, Y ve Z eksenleri boyunca noktaları oluştur
            for (float x = min.x; x <= max.x; x += spacing)
            {
                for (float y = min.y; y <= max.y; y += spacing)
                {
                    for (float z = min.z; z <= max.z; z += spacing)
                    {
                        Vector3 position = new Vector3(x, y, z);

                        // Sınır kontrolü - tam sınırda sayısal hatalar olabilir
                        if (bounds.Contains(position))
                        {
                            // Yeni bir gradient noktası oluştur
                            GradientDataset gradientPoint = new GradientDataset(
                                gradientPoints.Count,
                                position,
                                Vector3.one,
                                0);

                            gradientPoints.Add(gradientPoint);
                        }
                    }
                }
            }

            Debug.Log($"Oluşturulan toplam gradient nokta sayısı: {gradientPoints.Count}");
            gradientPoints.ForEach(gradientPoint => { TEST(gradientPoint.Position); });
        }

        private void TEST(Vector3 position)
        {
            // Create and configure sphere object
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * 0.003f;
        }
        /// <summary>
        /// Calculates and filters gradient points within the current wireframe bounds
        /// Uses TTK calculations if supported, otherwise filters from topological data
        /// </summary>
        private void CalculateGradientPoints()
        {
            // Clear previous data
            gradientPoints.Clear();

            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            foreach (var data in topologicalDataObjectInstance.gradientList)
            {
                if (rectangleComponent.ContainsPointUsingBounds(data.Position))
                {
                    gradientPoints.Add(data);
                }
            }

            Debug.Log($"Filtered {gradientPoints.Count} points inside the cube.");
        }

        private bool CalculateGradientPointsByTTK()
        {
            bool tkkGradientUsed = false;
            var boxGradientPoints = ttkCalculations.GetGradient3DSubset(GetMinMaxValuesOfBox());
            if (boxGradientPoints.Any())
            {
                var borders = GetRectangleBounds();
                boxGradientPoints = boxGradientPoints.Select(x => x).Where(x => borders.Contains(x.Position)).ToList();
                if (boxGradientPoints.Any())
                {
                    gradientPoints = boxGradientPoints;
                    tkkGradientUsed = true;
                }
            }

            return tkkGradientUsed;
        }

        /// <summary>
        /// Calculates and filters critical points within the current wireframe bounds
        /// Uses TTK calculations if supported, otherwise filters from topological data
        /// </summary>
        private void CalculateCriticalPoints()
        {
            criticalPoints.Clear(); // Clear previous data
            if (supportedByTTK)
            {
                bool isTTKCalculated = CalculateCriticalPointsByTTK();
                if (isTTKCalculated)
                    return;
            }

            // Filter data inside the cube
            Basic3DRectangle rectangleComponent = rectangle.GetComponent<Basic3DRectangle>();
            foreach (var data in topologicalDataObjectInstance.criticalPointList)
            {
                if (rectangleComponent.ContainsPointUsingBounds(data.Position))
                {
                    criticalPoints.Add(data);
                }
            }
        }

        private bool CalculateCriticalPointsByTTK()
        {

            bool tkkcriticalPointsUsed = false;
            var boxcriticalPoints = ttkCalculations.GetCriticalpoint3DSubset(GetMinMaxValuesOfBox());
            if (boxcriticalPoints.Any())
            {
                var borders = GetRectangleBounds();
                boxcriticalPoints = boxcriticalPoints.Select(x => x).Where(x => borders.Contains(x.Position)).ToList();
                if (boxcriticalPoints.Any())
                {
                    criticalPoints = boxcriticalPoints;
                    tkkcriticalPointsUsed = true;
                }
            }
            return tkkcriticalPointsUsed;
        }

        public void Create3DRectangle()
        {
            rectangle = new GameObject("Rectangle3D");
            rectangle.tag = "Rectangle3D";

            rectangle.transform.parent = volumeTransform;
            GameObject volumeObject = volumeTransform.gameObject;

            // Reset transform to align with parent
            rectangle.transform.localPosition = Vector3.zero;
            rectangle.transform.localRotation = Quaternion.identity;
            rectangle.transform.localScale = Vector3.one;
            
            Basic3DRectangle basic3DRectangle = rectangle.AddComponent<Basic3DRectangle>();
            basic3DRectangle.Initialize();

            BoxCollider boxCollider = volumeObject.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Vector3 min = volumeObject.transform.TransformPoint(boxCollider.center - boxCollider.size * 0.5f);
                Vector3 max = volumeObject.transform.TransformPoint(boxCollider.center + boxCollider.size * 0.5f);
                basic3DRectangle.SetBounds(min, max);
            }
        }

        public void Create3DRectangle(Vector3 min, Vector3 max)
        {
            // Yeni GameObject oluştur
            rectangle = new GameObject("Rectangle3D");
            rectangle.tag = "Rectangle3D";

            // Parent olarak volumeTransform'a ayarla
            rectangle.transform.parent = volumeTransform;

            // Transformu sıfırla
            rectangle.transform.localPosition = Vector3.zero;
            rectangle.transform.localRotation = Quaternion.identity;
            rectangle.transform.localScale = Vector3.one;

            // Basic3DRectangle bileşenini ekle ve başlat
            Basic3DRectangle basic3DRectangle = rectangle.AddComponent<Basic3DRectangle>();
            basic3DRectangle.Initialize();

            // Sınırları ayarla
            basic3DRectangle.SetBounds(min, max);
        }

        #endregion private
    }
}