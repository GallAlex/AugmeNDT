using UnityEngine;

public class RectangleSlicer : MonoBehaviour
{
    // References to the corner points
    public GameObject[] cornerHandles = new GameObject[4];

    // Reference to the line renderers for edges
    private LineRenderer[] edgeLines = new LineRenderer[4];

    // References to the mesh components
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    // Variables to track which handle is being dragged
    private int activeDragHandleIndex = -1;
    private Camera mainCamera;

    // Initial rectangle size
    public float initialWidth = 1.0f;
    public float initialHeight = 1.0f;

    // Handle properties
    public float handleSize = 0.1f;
    public Color handleColor = Color.red;

    private void Awake()
    {
        mainCamera = Camera.main;

        // Create the rectangle mesh
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Set up material - using default for now, can be customized
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = new Color(1f, 1f, 1f, 0.5f); // Semi-transparent white

        // Make sure we render both sides of the mesh
        meshRenderer.material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);

        // Create the handles if they don't exist
        CreateHandlesIfNeeded();

        // Create edge lines
        CreateEdgeLines();

        // Initialize the rectangle
        UpdateRectangleMesh();
    }

    private void CreateHandlesIfNeeded()
    {
        // Create handles for each corner
        for (int i = 0; i < 4; i++)
        {
            if (cornerHandles[i] == null)
            {
                cornerHandles[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cornerHandles[i].name = "Handle_" + i;
                cornerHandles[i].transform.SetParent(transform);
                cornerHandles[i].transform.localScale = Vector3.one * handleSize;

                // Set red material
                cornerHandles[i].GetComponent<Renderer>().material.color = handleColor;

                // Remove collider and add a smaller one for better interaction
                Destroy(cornerHandles[i].GetComponent<SphereCollider>());
                SphereCollider col = cornerHandles[i].AddComponent<SphereCollider>();
                col.radius = 1.0f; // Unity's default sphere has radius 0.5, so we double it
            }
        }

        // Position the handles at the corners of the initial rectangle
        PositionHandlesAtCorners();
    }

    private void PositionHandlesAtCorners()
    {
        float halfWidth = initialWidth / 2;
        float halfHeight = initialHeight / 2;

        // Position the handles at the corners (clockwise from bottom-left)
        cornerHandles[0].transform.localPosition = new Vector3(-halfWidth, -halfHeight, 0);
        cornerHandles[1].transform.localPosition = new Vector3(halfWidth, -halfHeight, 0);
        cornerHandles[2].transform.localPosition = new Vector3(halfWidth, halfHeight, 0);
        cornerHandles[3].transform.localPosition = new Vector3(-halfWidth, halfHeight, 0);
    }

    private void CreateEdgeLines()
    {
        // Create line renderers for the edges
        for (int i = 0; i < 4; i++)
        {
            GameObject edgeObj = new GameObject("Edge_" + i);
            edgeObj.transform.SetParent(transform);

            LineRenderer line = edgeObj.AddComponent<LineRenderer>();
            line.startWidth = 0.02f;
            line.endWidth = 0.02f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.white;
            line.endColor = Color.white;
            line.positionCount = 2;

            edgeLines[i] = line;
        }

        UpdateEdgeLines();
    }

    private void UpdateEdgeLines()
    {
        // Update the positions of the edge lines to match the handles
        for (int i = 0; i < 4; i++)
        {
            int nextIndex = (i + 1) % 4;

            edgeLines[i].SetPosition(0, cornerHandles[i].transform.position);
            edgeLines[i].SetPosition(1, cornerHandles[nextIndex].transform.position);
        }
    }

    private void UpdateRectangleMesh()
    {
        Mesh mesh = new Mesh();

        // Get the positions of the four corners in clockwise order
        Vector3[] corners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            corners[i] = cornerHandles[i].transform.localPosition;
        }

        // Define mesh vertices - using same vertices for both sides
        Vector3[] vertices = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            vertices[i] = corners[i];
        }

        // Define triangles for both front and back sides
        // Front side = clockwise, back side = counter-clockwise
        int[] triangles = new int[]
        {
            // Front side
            0, 1, 2, // First triangle
            0, 2, 3,  // Second triangle
            
            // Back side (reversed winding order)
            2, 1, 0, // First triangle
            3, 2, 0  // Second triangle
        };

        // Define UVs
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 0), // Bottom-left
            new Vector2(1, 0), // Bottom-right
            new Vector2(1, 1), // Top-right
            new Vector2(0, 1)  // Top-left
        };

        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Recalculate normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Assign the mesh
        meshFilter.mesh = mesh;
    }

    private void Update()
    {
        // Check for mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray to see if we hit any of the handles
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits any of our handles
            for (int i = 0; i < cornerHandles.Length; i++)
            {
                if (cornerHandles[i].GetComponent<Collider>().Raycast(ray, out hit, 100f))
                {
                    activeDragHandleIndex = i;
                    break;
                }
            }
        }

        // Check for mouse drag
        if (Input.GetMouseButton(0) && activeDragHandleIndex != -1)
        {
            // Get the plane that contains the rectangle
            Plane rectanglePlane = new Plane(transform.forward, transform.position);

            // Cast a ray from the mouse position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // Find where the ray intersects the plane
            float enter;
            if (rectanglePlane.Raycast(ray, out enter))
            {
                // Get the world position of the intersection
                Vector3 hitPoint = ray.GetPoint(enter);

                // Convert to local position relative to the rectangle's transform
                Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);

                // Update the position of the active handle
                cornerHandles[activeDragHandleIndex].transform.localPosition = new Vector3(
                    localHitPoint.x,
                    localHitPoint.y,
                    0 // Keep on the plane
                );

                // Update the rectangle mesh and edge lines
                UpdateRectangleMesh();
                UpdateEdgeLines();
            }
        }

        // Check for mouse button up
        if (Input.GetMouseButtonUp(0))
        {
            activeDragHandleIndex = -1;
        }
    }

    // Optional: Add a method to reset the rectangle to initial size
    public void ResetRectangle()
    {
        PositionHandlesAtCorners();
        UpdateRectangleMesh();
        UpdateEdgeLines();
    }
}