using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.UI.GridLayoutGroup;
using Unity.VisualScripting;

/// <summary>
/// Creates and manages an interactive rectangle that can be resized and repositioned by manipulating its corners
/// </summary>
public class InteractiveRectangle : MonoBehaviour
{
    [Header("Rectangle Settings")]
    [SerializeField] private float width = 5f; // Default width: 5
    [SerializeField] private float height = 3f; // Default height: 3
    [SerializeField] private Color handleColor = Color.white;
    [SerializeField] private float handleRadius = 0.25f;

    private Vector3[] corners = new Vector3[4];
    private GameObject rectangleObject;
    private List<GameObject> cornerHandles = new List<GameObject>();
    private int selectedCornerIndex = -1;
    private Camera mainCamera;
    private GameObject centerPoint;

    private void Awake()
    {
        // Get reference to main camera
        mainCamera = Camera.main;
    }

    private void Start()
    {
        // Only auto-initialize if not being managed by RectangleManager
        if (corners[0] == Vector3.zero && corners[1] == Vector3.zero)
        {
            // Default initialization
            InitializeDefault();
        }
    }

    /// <summary>
    /// Initialize with default settings at origin
    /// </summary>
    public void InitializeDefault()
    {
        // Ensure we start at origin (0,0,0)
        transform.position = Vector3.zero;

        // Initial corner positions (rectangle in XY plane)
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        corners[0] = new Vector3(-halfWidth, -halfHeight, 0);
        corners[1] = new Vector3(halfWidth, -halfHeight, 0);
        corners[2] = new Vector3(halfWidth, halfHeight, 0);
        corners[3] = new Vector3(-halfWidth, halfHeight, 0);

        CreateCornerHandles();
        InitializeRectangle();
        CreateCenterPoint();
    }

    /// <summary>
    /// Initialize with specific corner positions
    /// </summary>
    /// <param name="cornerPositions">Array of 4 positions for the corners (world space)</param>
    /// <param name="material">Optional material for handles</param>
    /// <param name="radius">Optional radius for handle spheres</param>
    public void InitializeWithCorners(Vector3[] cornerPositions)
    {
        this.handleRadius = 0.02f;

        // Calculate center as average of corners
        Vector3 center = Vector3.zero;
        for (int i = 0; i < 4; i++)
        {
            center += cornerPositions[i];
        }
        center /= 4;

        // Set position to calculated center
        transform.position = center;

        // Store corners relative to center
        for (int i = 0; i < 4; i++)
        {
            corners[i] = cornerPositions[i] - center;
        }

        // Create handles at the specified positions
        //If you want to change the corners interactively, you can use it
        //CreateCornerHandles();
        InitializeRectangle();
        //CreateCenterPoint();
    }

    /// <summary>
    /// Creates the rectangle mesh and material
    /// </summary>
    private void InitializeRectangle()
    {
        // Destroy existing rectangle if it exists
        if (rectangleObject != null)
        {
            Destroy(rectangleObject);
        }

        // Create a new GameObject for the rectangle
        rectangleObject = new GameObject("Rectangle");
        rectangleObject.transform.parent = transform;
        rectangleObject.transform.localPosition = Vector3.zero;

        // Add a MeshFilter and MeshRenderer
        MeshFilter meshFilter = rectangleObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = rectangleObject.AddComponent<MeshRenderer>();

        // Create unlit material with flat gray color that doesn't respond to lighting
        Material material = new Material(Shader.Find("Unlit/Color"));

        // Set a flat gray color
        material.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Medium gray with some transparency

        // Apply material
        meshRenderer.material = material;

        // Create mesh from current corner positions
        UpdateRectangleMesh(meshFilter);

        // We don't add any collider to the rectangle
        // This prevents any interaction with the rectangle surface
    }

    private Material handleMaterial;

    /// <summary>
    /// Creates the corner handles for resizing the rectangle
    /// </summary>
    private void CreateCornerHandles()
    {
        // Clear existing handles
        foreach (var handle in cornerHandles)
        {
            Destroy(handle);
        }
        cornerHandles.Clear();

        // Create new handles at corner positions
        for (int i = 0; i < 4; i++)
        {
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handle.name = "Corner_" + i;
            handle.transform.parent = transform;
            handle.transform.localScale = Vector3.one * handleRadius * 2;

            // Set position in world space
            handle.transform.position = transform.position + corners[i];

            // Set material
            Renderer renderer = handle.GetComponent<Renderer>();

            // Use provided material or create a new one
            if (handleMaterial != null)
            {
                renderer.material = handleMaterial;
            }
            else
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = handleColor;
            }

            // Set up collider
            SphereCollider collider = handle.GetComponent<SphereCollider>();
            collider.radius = 0.5f;

            // Store handle
            cornerHandles.Add(handle);
        }
    }

    /// <summary>
    /// Updates the rectangle mesh based on current corner positions
    /// </summary>
    /// <param name="meshFilter">MeshFilter to update</param>
    private void UpdateRectangleMesh(MeshFilter meshFilter)
    {
        Mesh mesh = new Mesh();

        // Use the corners directly as vertices
        Vector3[] vertices = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            vertices[i] = corners[i];
        }

        // Define triangles (two triangles making a quad)
        // Create triangles for both front and back faces for visibility from both sides
        int[] triangles = new int[] { 
            // Front face
            0, 1, 2,
            0, 2, 3,
            // Back face (reversed winding order)
            2, 1, 0,
            3, 2, 0
        };

        // Define UVs
        Vector2[] uvs = new Vector2[] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        // Set mesh properties
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // No need to recalculate normals for unlit shader, but useful for mesh bounds
        mesh.RecalculateBounds();

        // Set the mesh to the MeshFilter
        meshFilter.mesh = mesh;
    }

    private void Update()
    {
        // Handle mouse input and dragging operations
        HandleInput();

        // Only update the corner position in the array while dragging
        // (but don't recreate the mesh until release)
        if (selectedCornerIndex != -1)
        {
            corners[selectedCornerIndex] = cornerHandles[selectedCornerIndex].transform.position - transform.position;
        }
    }

    /// <summary>
    /// Handles mouse input for corner selection and dragging
    /// </summary>
    private void HandleInput()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Mouse Down: Check what was clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Check if we're clicking on a corner handle
            selectedCornerIndex = -1;
            if (Physics.Raycast(ray, out hit))
            {
                for (int i = 0; i < cornerHandles.Count; i++)
                {
                    if (hit.collider.gameObject == cornerHandles[i])
                    {
                        selectedCornerIndex = i;
                        break;
                    }
                }

                // Completely ignore clicks on the rectangle surface
                // No dragging functionality for the rectangle itself
            }
        }

        // Mouse Drag: Handle movement
        if (Input.GetMouseButton(0))
        {
            // If dragging a corner handle - allow free movement in 3D space
            if (selectedCornerIndex != -1)
            {
                // Find where the ray intersects with a plane defined by the current view
                Plane movementPlane;

                // Create a plane perpendicular to the camera view
                movementPlane = new Plane(-mainCamera.transform.forward, cornerHandles[selectedCornerIndex].transform.position);

                float enter;
                if (movementPlane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);

                    // Move the handle to that position
                    cornerHandles[selectedCornerIndex].transform.position = hitPoint;
                }
            }
            // Rectangle dragging has been completely disabled
        }
    }

    /// <summary>
    /// Get the normal vector of the rectangle
    /// </summary>
    /// <returns>Normalized normal vector</returns>
    public Vector3 GetNormal()
    {
        // Calculate normal using three corners
        Vector3 side1 = corners[1] - corners[0];
        Vector3 side2 = corners[3] - corners[0];
        return Vector3.Cross(side1, side2).normalized;
    }

    /// <summary>
    /// Get the bounds of the rectangle in world space
    /// </summary>
    /// <returns>Bounds object representing the rectangle</returns>
    public Bounds GetBounds()
    {
        // Create bounds from corner positions
        Bounds bounds = new Bounds(transform.position, Vector3.zero);

        for (int i = 0; i < 4; i++)
        {
            bounds.Encapsulate(transform.position + corners[i]);
        }

        return bounds;
    }

    /// <summary>
    /// Check if a point is inside the mesh boundaries
    /// </summary>
    /// <param name="point">The world space point to check</param>
    /// <returns>True if the point is inside the rectangle mesh</returns>
    public bool IsPointInsideMesh(Vector3 point)
    {
        // Get the plane of the mesh
        Plane meshPlane = GetMeshPlane();

        // Project the point onto the plane
        Vector3 projectedPoint = point - meshPlane.normal * meshPlane.GetDistanceToPoint(point);

        // Get corners in world space
        Vector3[] corners = GetCornerPositions();

        // Use a cross product method to determine if the point is inside the quadrilateral
        int sign = 0;
        for (int i = 0; i < 4; i++)
        {
            Vector3 v1 = corners[i] - projectedPoint;
            Vector3 v2 = corners[(i + 1) % 4] - projectedPoint;

            // Calculate cross product
            Vector3 cross = Vector3.Cross(v1, v2);

            // Check the sign of the z-component 
            // (assuming the rectangle is mostly in the XY plane)
            float crossZ = Vector3.Dot(cross, meshPlane.normal);

            if (i == 0)
            {
                sign = (crossZ > 0) ? 1 : -1;
            }
            else if ((crossZ > 0 && sign < 0) || (crossZ < 0 && sign > 0))
            {
                // Sign changed, point is outside
                return false;
            }
        }

        // If we get here, the point is inside
        return true;
    }

    /// <summary>
    /// Get plane representation of the mesh
    /// </summary>
    /// <returns>A Plane object representing the mesh surface</returns>
    private Plane GetMeshPlane()
    {
        // Get corner positions
        Vector3[] corners = GetCornerPositions();

        // Calculate normal using three corners
        Vector3 side1 = corners[1] - corners[0];
        Vector3 side2 = corners[3] - corners[0];
        Vector3 normal = Vector3.Cross(side1, side2).normalized;

        // Create plane at first corner with normal
        return new Plane(normal, corners[0]);
    }

    /// <summary>
    /// Update the corner positions of the rectangle
    /// </summary>
    /// <param name="newCorners">Array of 4 world-space positions</param>
    public void UpdateCorners(Vector3[] newCorners)
    {
        if (newCorners == null || newCorners.Length != 4)
        {
            Debug.LogError("UpdateCorners requires exactly 4 corner positions");
            return;
        }

        // Calculate center as average of corners
        Vector3 center = Vector3.zero;
        for (int i = 0; i < 4; i++)
        {
            center += newCorners[i];
        }
        center /= 4;

        // Set position to calculated center
        transform.position = center;

        // Store corners relative to center
        for (int i = 0; i < 4; i++)
        {
            corners[i] = newCorners[i] - center;

            // Update handle positions if they exist
            if (cornerHandles != null && cornerHandles.Count > i && cornerHandles[i] != null)
            {
                cornerHandles[i].transform.position = transform.position + corners[i];
            }
        }

        // Update the rectangle mesh if it exists
        if (rectangleObject != null)
        {
            MeshFilter meshFilter = rectangleObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                UpdateRectangleMesh(meshFilter);
            }
        }

        // Update center point
        if (centerPoint != null)
        {
            centerPoint.transform.position = transform.position;
        }
    }

    /// <summary>
    /// Get the corner positions in world space
    /// </summary>
    /// <returns>Array of 4 Vector3 positions</returns>
    public Vector3[] GetCornerPositions()
    {
        Vector3[] worldCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = transform.position + corners[i];
        }
        return worldCorners;
    }

    /// <summary>
    /// Updates the center position of the rectangle based on the current corner positions
    /// </summary>
    private void UpdateRectangleCenter()
    {
        // Calculate the centroid of the corners
        Vector3 centroid = Vector3.zero;
        for (int i = 0; i < 4; i++)
        {
            centroid += cornerHandles[i].transform.position;
        }
        centroid /= 4;

        // Move the transform to the new centroid
        Vector3 delta = centroid - transform.position;
        transform.position = centroid;

        // Update corner positions relative to the new center
        for (int i = 0; i < 4; i++)
        {
            corners[i] = cornerHandles[i].transform.position - transform.position;
        }

        // Update center point
        if (centerPoint != null)
        {
            centerPoint.transform.position = transform.position;
        }

        // Update the rectangle mesh
        UpdateRectangleMesh(rectangleObject.GetComponent<MeshFilter>());
    }

    /// <summary>
    /// Creates a visual indicator for the center point of the rectangle
    /// </summary>
    private void CreateCenterPoint()
    {
        centerPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        centerPoint.name = "CenterPoint";
        centerPoint.transform.parent = transform;
        centerPoint.transform.localPosition = Vector3.zero;
        centerPoint.transform.localScale = Vector3.one * 0.05f;

        // Set material
        Renderer renderer = centerPoint.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = Color.white;

        // Remove collider
        Destroy(centerPoint.GetComponent<Collider>());
    }
}