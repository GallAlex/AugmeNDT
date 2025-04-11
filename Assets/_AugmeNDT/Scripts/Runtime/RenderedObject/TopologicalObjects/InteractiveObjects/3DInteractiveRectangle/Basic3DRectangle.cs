namespace AugmeNDT
{
    using UnityEngine;

    /// <summary>
    /// Basitleştirilmiş wireframe küp oluşturma ve yönetme sınıfı
    /// </summary>
    public class Basic3DRectangle : MonoBehaviour
    {
        private GameObject rectangleObject;
        private LineRenderer lineRenderer;
        private bool drawBorders = true;
        private Vector3 minCorner { get; set; }
        private Vector3 maxCorner { get; set; }

        public void Initialize()
        {
            // LineRenderer bileşeni oluştur
            rectangleObject = new GameObject("RectangleVisual");
            rectangleObject.transform.SetParent(transform);

            // Varsayılan bounds değerleri
            minCorner = Vector3.one * -1f;
            maxCorner = Vector3.one;


            if (drawBorders)
            {
                lineRenderer = rectangleObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.yellow;
                lineRenderer.endColor = Color.yellow;
                lineRenderer.startWidth = 0.001f;
                lineRenderer.endWidth = 0.001f;
                lineRenderer.positionCount = 24; // 12 kenar için 24 nokta
                lineRenderer.useWorldSpace = false;

                UpdateLinePositions();
            }
        }

        // Dikdörtgenin sınırlarını ayarla
        public void SetBounds(Vector3 min, Vector3 max)
        {
            minCorner = min;
            maxCorner = max;
            if (drawBorders)
            {
                UpdateLinePositions();
            }
        }

        public Bounds GetBounds()
        {
            Vector3[] corners = GetCorners();

            // İlk köşeyi kullanarak Bounds başlat
            Bounds bounds = new Bounds(corners[0], Vector3.zero);

            // Diğer tüm köşeleri Bounds'a dahil et
            for (int i = 1; i < corners.Length; i++)
            {
                bounds.Encapsulate(corners[i]);
            }

            return bounds;
        }

        // Alternatif olarak Bounds kullanarak da kontrol edebilirsiniz
        public bool ContainsPointUsingBounds(Vector3 point)
        {
            Bounds bounds = GetBounds();
            // Epsilon değeri kullanarak sınır kontrolü
            float epsilon = 0.0001f;
            Bounds expandedBounds = new Bounds(bounds.center, bounds.size + new Vector3(epsilon, epsilon, epsilon) * 2);
            return expandedBounds.Contains(point);
        }

        // LineRenderer pozisyonlarını güncelle
        private void UpdateLinePositions()
        {
            if (lineRenderer == null) return;

            Vector3[] corners = GetCorners();

            // LineRenderer'ı 12 ayrı kenar için yapılandır (her kenar 2 nokta içerir)
            lineRenderer.positionCount = 24;

            // Alt yüzeyin 4 kenarı
            int index = 0;
            // Kenar 1: corners[0] -> corners[1]
            lineRenderer.SetPosition(index++, corners[0]);
            lineRenderer.SetPosition(index++, corners[1]);

            // Kenar 2: corners[1] -> corners[2]
            lineRenderer.SetPosition(index++, corners[1]);
            lineRenderer.SetPosition(index++, corners[2]);

            // Kenar 3: corners[2] -> corners[3]
            lineRenderer.SetPosition(index++, corners[2]);
            lineRenderer.SetPosition(index++, corners[3]);

            // Kenar 4: corners[3] -> corners[0]
            lineRenderer.SetPosition(index++, corners[3]);
            lineRenderer.SetPosition(index++, corners[0]);

            // Üst yüzeyin 4 kenarı
            // Kenar 5: corners[4] -> corners[5]
            lineRenderer.SetPosition(index++, corners[4]);
            lineRenderer.SetPosition(index++, corners[5]);

            // Kenar 6: corners[5] -> corners[6]
            lineRenderer.SetPosition(index++, corners[5]);
            lineRenderer.SetPosition(index++, corners[6]);

            // Kenar 7: corners[6] -> corners[7]
            lineRenderer.SetPosition(index++, corners[6]);
            lineRenderer.SetPosition(index++, corners[7]);

            // Kenar 8: corners[7] -> corners[4]
            lineRenderer.SetPosition(index++, corners[7]);
            lineRenderer.SetPosition(index++, corners[4]);

            // Yan kenarlar (4 dikey kenar)
            // Kenar 9: corners[0] -> corners[4]
            lineRenderer.SetPosition(index++, corners[0]);
            lineRenderer.SetPosition(index++, corners[4]);

            // Kenar 10: corners[1] -> corners[5]
            lineRenderer.SetPosition(index++, corners[1]);
            lineRenderer.SetPosition(index++, corners[5]);

            // Kenar 11: corners[2] -> corners[6]
            lineRenderer.SetPosition(index++, corners[2]);
            lineRenderer.SetPosition(index++, corners[6]);

            // Kenar 12: corners[3] -> corners[7]
            lineRenderer.SetPosition(index++, corners[3]);
            lineRenderer.SetPosition(index++, corners[7]);
        }

        // Köşe noktalarını hesapla - Z koordinatını düzeltilmiş sırayla
        private Vector3[] GetCorners()
        {
            Vector3[] corners = new Vector3[8];
            // Alt köşeler (saat yönünde)
            corners[0] = new Vector3(minCorner.x, minCorner.y, minCorner.z); // sol ön alt
            corners[1] = new Vector3(maxCorner.x, minCorner.y, minCorner.z); // sağ ön alt
            corners[2] = new Vector3(maxCorner.x, minCorner.y, maxCorner.z); // sağ arka alt
            corners[3] = new Vector3(minCorner.x, minCorner.y, maxCorner.z); // sol arka alt

            // Üst köşeler (saat yönünde)
            corners[4] = new Vector3(minCorner.x, maxCorner.y, minCorner.z); // sol ön üst
            corners[5] = new Vector3(maxCorner.x, maxCorner.y, minCorner.z); // sağ ön üst
            corners[6] = new Vector3(maxCorner.x, maxCorner.y, maxCorner.z); // sağ arka üst
            corners[7] = new Vector3(minCorner.x, maxCorner.y, maxCorner.z); // sol arka üst

            return corners;
        }
    }
}