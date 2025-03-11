namespace AugmeNDT
{
    using UnityEngine;

    public class VectorCreator3D : MonoBehaviour
    {
        [Header("Vector Settings")]
        [Tooltip("Direction of the vector")]
        public Vector3 direction = new Vector3(1f, 1f, 1f);

        [Tooltip("Position of the vector's center")]
        public Vector3 position = Vector3.zero;

        [Tooltip("Scale factor for the vector length")]
        public float vectorLength = 0.5f;

        [Tooltip("Width of the vector line")]
        public float lineWidth = 0.1f;

        [Tooltip("Size of the arrow head")]
        public float arrowHeadSize = 0.2f;

        [Header("Magnitude Color Mapping")]
        [Tooltip("Maximum magnitude reference value")]
        public float maxMagnitude = 1.0f;

        [Tooltip("Current vector magnitude (for color mapping)")]
        public float vectorMagnitude = 0.5f;

        [Tooltip("Color at minimum magnitude (0)")]
        public Color minColor = Color.blue;

        [Tooltip("Color at middle magnitude (maxMagnitude/2)")]
        public Color midColor = Color.white;

        [Tooltip("Color at maximum magnitude")]
        public Color maxColor = Color.red;

        [Tooltip("Override automatic color with this color (leave transparent to use magnitude coloring)")]
        public Color overrideColor = new Color(0, 0, 0, 0);

        private GameObject lineObject;
        private GameObject arrowHeadObject;
        private Material lineMaterial;
        private Material arrowMaterial;
        private Mesh mesh;

        private void CreateVector()
        {
            // Clear previous objects if they exist
            if (lineObject != null) Destroy(lineObject);
            if (arrowHeadObject != null) Destroy(arrowHeadObject);

            // Make sure we have a valid direction
            if (direction == Vector3.zero)
            {
                direction = Vector3.forward;
            }

            // Normalize direction and apply length
            Vector3 normalizedDirection = direction.normalized * vectorLength;

            // Set main object position
            transform.position = position;

            // Create cylinder rod
            lineObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lineObject.transform.SetParent(transform);
            lineObject.name = "VectorLine";

            // Position and scale the rod
            float lineLength = normalizedDirection.magnitude * 0.9f; // Leave space for arrow head
            lineObject.transform.localScale = new Vector3(lineWidth, lineLength / 2, lineWidth);
            lineObject.transform.localPosition = normalizedDirection / 2 * 0.9f;

            // Rotate rod according to direction
            lineObject.transform.up = normalizedDirection;

            // Create arrow head (pyramid from cube)
            arrowHeadObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrowHeadObject.transform.SetParent(transform);
            arrowHeadObject.name = "VectorArrowHead";

            // Position and scale the arrow head
            arrowHeadObject.transform.localScale = new Vector3(arrowHeadSize, arrowHeadSize, arrowHeadSize);
            arrowHeadObject.transform.localPosition = normalizedDirection;

            // Transform cube into pyramid shape
            MeshFilter meshFilter = arrowHeadObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.mesh != null)
            {
                mesh = meshFilter.mesh;
                Vector3[] vertices = mesh.vertices;

                // Find vertices on the top face (+y direction)
                // and move them to create a point facing forward
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (vertices[i].y > 0)
                    {
                        // Move top vertices to create a single point
                        vertices[i] = new Vector3(0, vertices[i].y * 2, 0);
                    }
                }

                // Apply modified vertices
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }

            // Rotate arrow head according to direction
            arrowHeadObject.transform.up = normalizedDirection;

            // Determine color based on magnitude
            Color vectorColor = GetColorByMagnitude(vectorMagnitude, maxMagnitude);

            // Apply color to both objects
            Renderer lineRenderer = lineObject.GetComponent<Renderer>();
            Renderer arrowRenderer = arrowHeadObject.GetComponent<Renderer>();

            lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.color = vectorColor;

            arrowMaterial = new Material(Shader.Find("Standard"));
            arrowMaterial.color = vectorColor;

            lineRenderer.material = lineMaterial;
            arrowRenderer.material = arrowMaterial;
        }

        // Get color based on magnitude
        private Color GetColorByMagnitude(float magnitude, float maxMag)
        {
            // If override color is set (alpha > 0), use it instead of calculation
            if (overrideColor.a > 0)
            {
                return overrideColor;
            }

            // Clamp magnitude to 0-maxMag range
            magnitude = Mathf.Clamp(magnitude, 0, maxMag);

            // Normalize magnitude to 0-1 range
            float normalizedMagnitude = magnitude / maxMag;

            // Determine color based on magnitude
            if (normalizedMagnitude <= 0.5f)
            {
                // Interpolate between minColor and midColor
                return Color.Lerp(minColor, midColor, normalizedMagnitude * 2);
            }
            else
            {
                // Interpolate between midColor and maxColor
                return Color.Lerp(midColor, maxColor, (normalizedMagnitude - 0.5f) * 2);
            }
        }

        // Programmatically update the vector
        public void SetVector(Vector3 newPosition, Vector3 directionVector, float vectorMag, float maxMag)
        {
            position = newPosition;
            direction = directionVector;
            vectorMagnitude = vectorMag;
            maxMagnitude = maxMag;
            CreateVector();
        }
    }
}