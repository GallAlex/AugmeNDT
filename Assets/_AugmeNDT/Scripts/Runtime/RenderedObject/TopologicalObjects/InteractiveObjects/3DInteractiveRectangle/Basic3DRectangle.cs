namespace AugmeNDT
{
    using Newtonsoft.Json.Bson;
    using UnityEngine;

    /// <summary>
    /// Simplified wireframe cube creation and management class
    /// </summary>
    public class Basic3DRectangle : MonoBehaviour
    {
        private GameObject rectangleObject;
        private LineRenderer lineRenderer;
        public bool drawBorders = true;
        private Matrix4x4 worldToLocalMatrix; // Thread-safe kullanım için

        /// <summary>
        /// Initializes the LineRenderer component
        /// </summary>
        private void Awake()
        {
            // Create LineRenderer component
            rectangleObject = new GameObject("RectangleVisual");
            rectangleObject.transform.SetParent(transform);
        }

        /// <summary>
        /// Local space koordinatlarında dikdörtgen sınırlarını ayarlar
        /// </summary>
        /// <param name="localMin">Local space'teki minimum köşe</param>
        /// <param name="localMax">Local space'teki maximum köşe</param>
        public void InitializeBoundsLocal(Vector3 localMin, Vector3 localMax)
        {
            // Local space'te merkez hesapla
            Vector3 localCenter = (localMin + localMax) * 0.5f;

            // Local space'te boyut hesapla
            Vector3 localSize = localMax - localMin;

            // Objenin local pozisyonunu ayarla
            transform.localPosition = localCenter;

            // Objenin local ölçeğini ayarla
            transform.localScale = localSize;

            // Manuel bounds'u güncelle
            worldToLocalMatrix = transform.worldToLocalMatrix;

            // LineRenderer ekle ve ayarla
            if (drawBorders)
            {
                // Mevcut LineRenderer'ı temizle (varsa)
                if (lineRenderer != null)
                {
                    Destroy(lineRenderer);
                }

                lineRenderer = rectangleObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.yellow;
                lineRenderer.endColor = Color.yellow;
                lineRenderer.startWidth = 0.001f; // Daha görünür olması için kalınlığı artırıyorum
                lineRenderer.endWidth = 0.001f;
                lineRenderer.positionCount = 24; // 12 kenar için 24 nokta
                lineRenderer.useWorldSpace = false; // Local space'te çalış!

                // RectangleVisual objesi transformunu sıfırla
                rectangleObject.transform.localPosition = Vector3.zero;
                rectangleObject.transform.localRotation = Quaternion.identity;
                rectangleObject.transform.localScale = Vector3.one;

                // LineRenderer pozisyonlarını güncelle
                UpdateLinePositions();
            }
        }

        /// Calculates and returns a Bounds object that encapsulates the rectangle
        public Bounds GetBounds()
        {
            // Birim küpün köşelerini tanımla (-0.5, -0.5, -0.5) ile (0.5, 0.5, 0.5) arası
            Vector3[] unitCorners = new Vector3[8]
            {
                new Vector3(-0.5f, -0.5f, -0.5f), // sol alt ön
                new Vector3(0.5f, -0.5f, -0.5f),  // sağ alt ön
                new Vector3(0.5f, -0.5f, 0.5f),   // sağ alt arka
                new Vector3(-0.5f, -0.5f, 0.5f),  // sol alt arka
                new Vector3(-0.5f, 0.5f, -0.5f),  // sol üst ön
                new Vector3(0.5f, 0.5f, -0.5f),   // sağ üst ön
                new Vector3(0.5f, 0.5f, 0.5f),    // sağ üst arka
                new Vector3(-0.5f, 0.5f, 0.5f)    // sol üst arka
            };

            // Transform'un world matrix'ini kullanarak tüm dönüşümleri uygula
            Matrix4x4 worldMatrix = transform.localToWorldMatrix;

            // İlk köşe ile bounds'u başlat
            Vector3 firstCorner = worldMatrix.MultiplyPoint(unitCorners[0]);
            Bounds bounds = new Bounds(firstCorner, Vector3.zero);

            // Diğer tüm köşeleri ekleyerek bounds'u genişlet
            for (int i = 1; i < unitCorners.Length; i++)
            {
                bounds.Encapsulate(worldMatrix.MultiplyPoint(unitCorners[i]));
            }

            return bounds;
        }

        public void SetBoundsManuelUpdated()
        {
            // Transform matrisini önden hesaplayıp saklayalım
            worldToLocalMatrix = transform.worldToLocalMatrix;
        }

        // Thread-safe içinde/dışında kontrolü
        public bool ContainsPointUsingBounds(Vector3 worldPoint, bool useCachedBounds)
        {
            if (!useCachedBounds)
            {
                // Thread-safe olmayan yol, sadece ana thread'de çağrılabilir
                Vector3 lp = transform.InverseTransformPoint(worldPoint);
                return IsPointInUnitCube(lp);
            }

            // Thread-safe yol, cached matrix kullanır
            Vector3 localPoint = worldToLocalMatrix.MultiplyPoint3x4(worldPoint);
            return IsPointInUnitCube(localPoint);
        }

        // Yardımcı metod
        private bool IsPointInUnitCube(Vector3 localPoint)
        {
            // Epsilon değeri ekleyerek sınır değerlerde daha toleranslı olabiliriz
            float epsilon = 0.0001f;
            return (localPoint.x >= -0.5f - epsilon && localPoint.x <= 0.5f + epsilon &&
                    localPoint.y >= -0.5f - epsilon && localPoint.y <= 0.5f + epsilon &&
                    localPoint.z >= -0.5f - epsilon && localPoint.z <= 0.5f + epsilon);
        }

        /// <summary>
        /// Updates the LineRenderer positions to display the wireframe cube more efficiently
        /// </summary>
        private void UpdateLinePositions()
        {
            if (lineRenderer == null) return;

            // Configure LineRenderer for 12 separate edges (each edge contains 2 points)
            lineRenderer.positionCount = 24;

            // Birim küpün (-0.5, -0.5, -0.5) ile (0.5, 0.5, 0.5) arasındaki köşeleri
            // Bu değerler, transform.localScale tarafından ölçeklendirilecek
            Vector3[] unitCorners = new Vector3[8]
            {
                new Vector3(-0.5f, -0.5f, -0.5f), // sol alt ön
                new Vector3(0.5f, -0.5f, -0.5f),  // sağ alt ön
                new Vector3(0.5f, -0.5f, 0.5f),   // sağ alt arka
                new Vector3(-0.5f, -0.5f, 0.5f),  // sol alt arka
                new Vector3(-0.5f, 0.5f, -0.5f),  // sol üst ön
                new Vector3(0.5f, 0.5f, -0.5f),   // sağ üst ön
                new Vector3(0.5f, 0.5f, 0.5f),    // sağ üst arka
                new Vector3(-0.5f, 0.5f, 0.5f)    // sol üst arka
            };

            // LineRenderer'ın parent objesi ile aynı dönüşüme sahip olduğunu varsayarsak,
            // yerel koordinatlarda çizim yaparız (useWorldSpace = false ise)

            int index = 0;

            // Alt yüzey (0-1-2-3)
            lineRenderer.SetPosition(index++, unitCorners[0]);
            lineRenderer.SetPosition(index++, unitCorners[1]);

            lineRenderer.SetPosition(index++, unitCorners[1]);
            lineRenderer.SetPosition(index++, unitCorners[2]);

            lineRenderer.SetPosition(index++, unitCorners[2]);
            lineRenderer.SetPosition(index++, unitCorners[3]);

            lineRenderer.SetPosition(index++, unitCorners[3]);
            lineRenderer.SetPosition(index++, unitCorners[0]);

            // Üst yüzey (4-5-6-7)
            lineRenderer.SetPosition(index++, unitCorners[4]);
            lineRenderer.SetPosition(index++, unitCorners[5]);

            lineRenderer.SetPosition(index++, unitCorners[5]);
            lineRenderer.SetPosition(index++, unitCorners[6]);

            lineRenderer.SetPosition(index++, unitCorners[6]);
            lineRenderer.SetPosition(index++, unitCorners[7]);

            lineRenderer.SetPosition(index++, unitCorners[7]);
            lineRenderer.SetPosition(index++, unitCorners[4]);

            // Yan kenarlar (0-4, 1-5, 2-6, 3-7)
            lineRenderer.SetPosition(index++, unitCorners[0]);
            lineRenderer.SetPosition(index++, unitCorners[4]);

            lineRenderer.SetPosition(index++, unitCorners[1]);
            lineRenderer.SetPosition(index++, unitCorners[5]);

            lineRenderer.SetPosition(index++, unitCorners[2]);
            lineRenderer.SetPosition(index++, unitCorners[6]);

            lineRenderer.SetPosition(index++, unitCorners[3]);
            lineRenderer.SetPosition(index++, unitCorners[7]);
        }

    }
}