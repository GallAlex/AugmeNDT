using UnityEngine;

public class Basic2DRectangle : MonoBehaviour
{
    public void Create()
    {
        GameObject volumetricObject = GameObject.Find("DataVisGroup_0/fibers.raw");
        if (volumetricObject == null)
        {
            Debug.LogError("Volumetric object reference not set!");
            return;
        }

        // Get the BoxCollider component
        BoxCollider boxCollider = volumetricObject.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider not found on the volumetric object!");
            return;
        }

        // Get the bounds and center from the BoxCollider
        Vector3 extents = boxCollider.size / 2; // Local extents

        // Create a new GameObject for the rectangle as a child of the volumetric object
        GameObject rectangle = new GameObject("2DRectangle");
        rectangle.transform.SetParent(volumetricObject.transform, false);

        // Set the position relative to the parent (local position)
        rectangle.transform.localPosition = Vector3.zero; // Center in local space

        // Add a MeshFilter and MeshRenderer for visualization
        MeshFilter meshFilter = rectangle.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = rectangle.AddComponent<MeshRenderer>();

        // Create the rectangle mesh (aligned with x-z plane)
        Mesh mesh = new Mesh();

        // Define the four corners of the rectangle on the x-z plane (y remains constant at 0)
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-extents.x, 0, -extents.z), // bottom-left
            new Vector3(extents.x, 0, -extents.z),  // bottom-right
            new Vector3(extents.x, 0, extents.z),   // top-right
            new Vector3(-extents.x, 0, extents.z)   // top-left
        };

        // Define the triangles (two triangles to form a quad)
        int[] triangles = new int[6]
        {
            0, 1, 2,
            0, 2, 3
        };

        // Define UVs
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        // Assign mesh to the MeshFilter
        meshFilter.mesh = mesh;

        // Assign a material
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red
    }
}