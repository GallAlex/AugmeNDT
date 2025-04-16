using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.UI.GridLayoutGroup;
using Unity.VisualScripting;

/// <summary>
/// Creates and manages an interactive rectangle that can be resized and repositioned by manipulating its corners
/// </summary>
public class InteractiveRectangle : MonoBehaviour
{
    private Vector3[] corners = new Vector3[4];
    private GameObject rectangleObject;


    /// <summary>
    /// Only used for 2DStreamline Calculation 
    /// MUST UPDATED before start threads
    /// </summary>
    private Vector3[] worldCornersManuelUpdated = new Vector3[4];

    /// <summary>
    /// Initialize with specific corner positions
    /// </summary>
    /// <param name="cornerPositions">Array of 4 positions for the corners (world space)</param>
    /// <param name="material">Optional material for handles</param>
    /// <param name="radius">Optional radius for handle spheres</param>
    public void InitializeWithCorners(Vector3[] cornerPositions)
    {
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
        InitializeRectangle();
    }

    /// <summary>
    /// Creates the rectangle mesh and material
    /// </summary>
    private void InitializeRectangle()
    {
        // Create a new GameObject for the rectangle
        rectangleObject = new GameObject("Rectangle");
        rectangleObject.transform.parent = transform;
        rectangleObject.transform.localPosition = Vector3.zero;

        // Add a MeshFilter and MeshRenderer
        MeshFilter meshFilter = rectangleObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = rectangleObject.AddComponent<MeshRenderer>();

        // Create unlit material with flat gray color that doesn't respond to lighting
        Material material = new Material(Shader.Find("Transparent/Diffuse"));

        // Set a flat gray color
        material.color = new Color(0.5f, 0.5f, 0.5f, 0.0f);
        material.renderQueue = 3000; // Transparent render queue

        // Apply material
        meshRenderer.material = material;

        // Create mesh from current corner positions
        UpdateRectangleMesh(meshFilter);
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
    /// <param name="useWorldCornersManuelUpdated"> Only used for 2DStreamline Calculation MUST UPDATED before start threads</param>
    /// <returns>True if the point is inside the rectangle mesh</returns>
    public bool IsPointInsideMesh(Vector3 point, bool useWorldCornersManuelUpdated = false)
    {
        // Get corners in world space
        Vector3[] corners = useWorldCornersManuelUpdated ? worldCornersManuelUpdated : GetCornerPositions();

        // Get the plane of the mesh
        Plane meshPlane = GetMeshPlane(corners);

        // Project the point onto the plane
        Vector3 projectedPoint = point - meshPlane.normal * meshPlane.GetDistanceToPoint(point);


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
        // Update the rectangle mesh if it exists
        if (rectangleObject != null)
        {
            MeshFilter meshFilter = rectangleObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                UpdateRectangleMesh(meshFilter);
            }
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
    /// Updates the cached world corner positions
    /// Used for thread-safe operations in 2D streamline calculations
    /// </summary>
    public void UpdateWorldCornersManuel()
    {
        worldCornersManuelUpdated = GetCornerPositions();
    }

    /// <summary>
    /// Get plane representation of the mesh
    /// </summary>
    /// <returns>A Plane object representing the mesh surface</returns>
    private Plane GetMeshPlane(Vector3[] corners)
    {
        // Calculate normal using three corners
        Vector3 side1 = corners[1] - corners[0];
        Vector3 side2 = corners[3] - corners[0];
        Vector3 normal = Vector3.Cross(side1, side2).normalized;

        // Create plane at first corner with normal
        return new Plane(normal, corners[0]);
    }

}