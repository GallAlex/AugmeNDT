namespace AugmeNDT
{
    using UnityEngine;

    /// <summary>
    /// Responsible for creating a 3D vector arrow consisting of a cylinder (line) and a pyramid-like head.
    /// Allows setting position and direction for visualization in 3D space.
    /// </summary>
    public class VectorCreator3D : MonoBehaviour
    {
        [Header("Vector Settings")]
        [Tooltip("Direction of the vector")]
        public Vector3 direction = new Vector3(1f, 1f, 1f);

        [Tooltip("Position of the vector's center")]
        public Vector3 position = Vector3.zero;

        [Tooltip("Constant length for all vectors")]
        public float vectorLength = 0.02f;

        [Tooltip("Width of the vector line")]
        public float lineWidth = 0.008f;

        [Tooltip("Size of the arrow head")]
        public float arrowHeadSize = 0.009f;

        [Tooltip("Line portion of the vector (0-1)")]
        [Range(0f, 1f)]
        public float linePortion = 0.7f;  // The percentage of the vector dedicated to the line (rest is arrow head)

        private GameObject lineObject;
        private GameObject arrowHeadObject;

        /// <summary>
        /// Initializes the vector's visual components based on the position and direction.
        /// </summary>
        private void CreateVector()
        {
            // Clear previous vector components if they exist
            if (lineObject != null) Destroy(lineObject);
            if (arrowHeadObject != null) Destroy(arrowHeadObject);

            // Calculate proportional lengths for the line and the arrow head
            float lineLength = vectorLength * linePortion;
            float arrowLength = vectorLength * (1 - linePortion);

            Vector3 normalizedDirection = direction.normalized;

            // Calculate start and end positions relative to the center (position)
            Vector3 vectorStart = position - (normalizedDirection * vectorLength / 2f);
            Vector3 lineEnd = vectorStart + (normalizedDirection * lineLength);

            // Calculate local positioning and scaling for the line object
            Vector3 localPosition = vectorStart + (normalizedDirection * lineLength / 2f);
            Vector3 localScale = new Vector3(lineWidth, lineLength / 2f, lineWidth);

            CreateLineObject(normalizedDirection, localPosition, localScale);  // Create cylinder
            CreateVectorHead(normalizedDirection, lineEnd);                    // Create arrow head

            SetColor(); // Apply color to the vector parts
        }

        /// <summary>
        /// Creates the cylindrical line part of the vector.
        /// </summary>
        private void CreateLineObject(Vector3 normalizedDirection, Vector3 localPosition, Vector3 localScale)
        {
            lineObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lineObject.transform.SetParent(transform,true);
            lineObject.name = "VectorLine";

            // Position and orient the cylinder in local space
            lineObject.transform.position = localPosition;
            lineObject.transform.localScale = localScale;

            // Align the cylinder's Y-axis with the vector direction
            lineObject.transform.up = normalizedDirection;
        }

        /// <summary>
        /// Creates the arrow head using a cube modified to appear as a pyramid.
        /// </summary>
        private void CreateVectorHead(Vector3 normalizedDirection, Vector3 lineEnd)
        {
            arrowHeadObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrowHeadObject.transform.SetParent(transform,true);
            arrowHeadObject.name = "VectorArrowHead";

            arrowHeadObject.transform.position = lineEnd;
            arrowHeadObject.transform.localScale = new Vector3(arrowHeadSize, arrowHeadSize, arrowHeadSize);

            // Morph cube into a pyramid by manipulating vertices
            MeshFilter meshFilter = arrowHeadObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.mesh != null)
            {
                Mesh mesh = meshFilter.mesh;
                Vector3[] vertices = mesh.vertices;

                for (int i = 0; i < vertices.Length; i++)
                {
                    if (vertices[i].y > 0)
                    {
                        // Collapse top face vertices into a single point (makes it pointy)
                        vertices[i] = new Vector3(0, vertices[i].y * 2, 0);
                    }
                }

                // Update mesh with new geometry
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }

            // Orient the arrow head in the correct direction
            arrowHeadObject.transform.up = normalizedDirection;
        }

        /// <summary>
        /// Assigns a standard blue material to both the line and arrow head.
        /// </summary>
        private void SetColor()
        {
            Material lineMaterial = new Material(Shader.Find("Standard"));
            Material arrowMaterial = new Material(Shader.Find("Standard"));

            lineMaterial.color = Color.blue;
            arrowMaterial.color = Color.blue;

            Renderer lineRenderer = lineObject.GetComponent<Renderer>();
            Renderer arrowRenderer = arrowHeadObject.GetComponent<Renderer>();

            lineRenderer.material = lineMaterial;
            arrowRenderer.material = arrowMaterial;
        }

        /// <summary>
        /// Public method to set the vector's position and direction, then (re)create its visuals.
        /// </summary>
        /// <param name="newPosition">Center position of the vector</param>
        /// <param name="directionVector">Direction in which the vector points</param>
        public void SetVector(Vector3 newPosition, Vector3 directionVector)
        {
            position = newPosition;
            direction = directionVector;
            CreateVector(); // Trigger creation or update of the arrow
        }
    }
}