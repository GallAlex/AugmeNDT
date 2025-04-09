namespace AugmeNDT
{
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class VectorCreator2D : MonoBehaviour
    {
        [Header("Arrow Settings")]
        private float stemLength = 0.3f;
        private float stemWidth = 0.1f;
        private float tipLength = 0.2f;
        private float tipWidth = 0.2f;
        public float  tipScale = 1f;

        [Header("Vector Settings")]
        [Tooltip("Direction of the vector")]
        public Vector3 direction = Vector3.right;

        [Tooltip("Position of the vector's center")]
        public Vector3 position = Vector3.zero;

        [Header("Color Settings")]
        [Tooltip("Color of the arrow")]
        public Color arrowColor = Color.white;

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

        [System.NonSerialized]
        public List<Vector3> verticesList;
        [System.NonSerialized]
        public List<int> trianglesList;

        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private Material arrowMaterial;

        private void CreateVector()
        {
            stemLength = stemLength * tipScale;
            stemWidth = stemWidth * tipScale;
            tipLength = tipLength * tipScale;
            tipWidth = tipWidth * tipScale;

            // Update position - adjust transform's real world position
            transform.position = position;

            if (mesh == null)
            {
                mesh = new Mesh();
                GetComponent<MeshFilter>().mesh = mesh;
            }

            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();

                // Create material
                arrowMaterial = new Material(Shader.Find("Standard"));
                meshRenderer.material = arrowMaterial;
            }

            // Reset lists
            verticesList = new List<Vector3>();
            trianglesList = new List<int>();

            // Make sure we have a valid direction
            if (direction == Vector3.zero)
            {
                direction = Vector3.right;
            }

            // Normalize direction
            Vector3 normalizedDirection = direction.normalized;

            // Calculate arrow stem length and width
            float stemHalfWidth = stemWidth / 2f;
            float tipHalfWidth = tipWidth / 2f;

            // Calculate arrow end points
            Vector3 stemOrigin = Vector3.zero;
            Vector3 tipOrigin = stemOrigin + (stemLength * normalizedDirection);

            // Find vector perpendicular to arrow direction (for width left/right)
            Vector3 perpendicular;

            if (normalizedDirection != Vector3.up && normalizedDirection != Vector3.down)
            {
                perpendicular = Vector3.Cross(normalizedDirection, Vector3.up).normalized;
            }
            else
            {
                perpendicular = Vector3.Cross(normalizedDirection, Vector3.forward).normalized;
            }

            // Front face vertices
            verticesList.Add(stemOrigin + (stemHalfWidth * perpendicular)); // 0
            verticesList.Add(stemOrigin - (stemHalfWidth * perpendicular)); // 1
            verticesList.Add(tipOrigin + (stemHalfWidth * perpendicular));  // 2
            verticesList.Add(tipOrigin - (stemHalfWidth * perpendicular));  // 3
            verticesList.Add(tipOrigin + (tipHalfWidth * perpendicular));   // 4
            verticesList.Add(tipOrigin - (tipHalfWidth * perpendicular));   // 5
            verticesList.Add(tipOrigin + (tipLength * normalizedDirection)); // 6

            // Front face triangles
            trianglesList.Add(0); trianglesList.Add(1); trianglesList.Add(3);
            trianglesList.Add(0); trianglesList.Add(3); trianglesList.Add(2);
            trianglesList.Add(4); trianglesList.Add(6); trianglesList.Add(5);

            // Create offset on Z axis for thickness
            Vector3 depthOffset = Vector3.Cross(normalizedDirection, perpendicular).normalized * stemWidth * 0.1f;

            // Create back face (reversed copy of the same points)
            int vertexCount = verticesList.Count;
            for (int i = 0; i < vertexCount; i++)
            {
                verticesList.Add(verticesList[i] + depthOffset);
            }

            // Add back face triangles (reversed direction of front face)
            trianglesList.Add(1 + vertexCount); trianglesList.Add(0 + vertexCount); trianglesList.Add(3 + vertexCount);
            trianglesList.Add(3 + vertexCount); trianglesList.Add(0 + vertexCount); trianglesList.Add(2 + vertexCount);
            trianglesList.Add(6 + vertexCount); trianglesList.Add(4 + vertexCount); trianglesList.Add(5 + vertexCount);

            // Add side faces (sides of the stem)
            trianglesList.Add(0); trianglesList.Add(0 + vertexCount); trianglesList.Add(1 + vertexCount);
            trianglesList.Add(0); trianglesList.Add(1 + vertexCount); trianglesList.Add(1);

            trianglesList.Add(2); trianglesList.Add(2 + vertexCount); trianglesList.Add(3 + vertexCount);
            trianglesList.Add(2); trianglesList.Add(3 + vertexCount); trianglesList.Add(3);

            trianglesList.Add(0); trianglesList.Add(2); trianglesList.Add(2 + vertexCount);
            trianglesList.Add(0); trianglesList.Add(2 + vertexCount); trianglesList.Add(0 + vertexCount);

            trianglesList.Add(1); trianglesList.Add(1 + vertexCount); trianglesList.Add(3 + vertexCount);
            trianglesList.Add(1); trianglesList.Add(3 + vertexCount); trianglesList.Add(3);

            // Arrow tip side faces
            trianglesList.Add(2); trianglesList.Add(4); trianglesList.Add(4 + vertexCount);
            trianglesList.Add(2); trianglesList.Add(4 + vertexCount); trianglesList.Add(2 + vertexCount);

            trianglesList.Add(3); trianglesList.Add(3 + vertexCount); trianglesList.Add(5 + vertexCount);
            trianglesList.Add(3); trianglesList.Add(5 + vertexCount); trianglesList.Add(5);

            // Arrow tip back faces
            trianglesList.Add(4); trianglesList.Add(5); trianglesList.Add(5 + vertexCount);
            trianglesList.Add(4); trianglesList.Add(5 + vertexCount); trianglesList.Add(4 + vertexCount);

            trianglesList.Add(4); trianglesList.Add(6); trianglesList.Add(6 + vertexCount);
            trianglesList.Add(4); trianglesList.Add(6 + vertexCount); trianglesList.Add(4 + vertexCount);

            trianglesList.Add(5); trianglesList.Add(6); trianglesList.Add(6 + vertexCount);
            trianglesList.Add(5); trianglesList.Add(6 + vertexCount); trianglesList.Add(5 + vertexCount);

            // Assign to mesh
            mesh.Clear();
            mesh.vertices = verticesList.ToArray();
            mesh.triangles = trianglesList.ToArray();
            mesh.RecalculateNormals();

            // Determine color based on magnitude and apply it
            Color vectorColor = GetColorByMagnitude(vectorMagnitude, maxMagnitude);
            arrowMaterial.color = vectorColor;
        }

        // Get color based on magnitude (adapted from VectorCreator3D)
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

        // Programmatically update the vector (adapted from VectorCreator3D)
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