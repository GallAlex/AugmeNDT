using System.Collections.Generic;
using UnityEngine;

public class DataGrid : MonoBehaviour
{
    private float worldSpaceWidth = 1.0f;
    private float worldSpaceHeight = 1.0f;

    public int xGridSize = 4;
    public int yGridSize = 4;

    private float xAxisTickSpacing;
    private float yAxisTickSpacing;

    private MeshFilter filter;
    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> indicies;

    void Start()
    {
        // Add mesh filter if not present
        filter = gameObject.GetComponent<MeshFilter>();
        filter = filter != null ? filter : gameObject.AddComponent<MeshFilter>();
        mesh = new Mesh();

        // Add mesh renderer if not present
        MeshRenderer meshRenderer = filter.GetComponent<MeshRenderer>();
        meshRenderer = meshRenderer != null ? meshRenderer : gameObject.AddComponent<MeshRenderer>();

        meshRenderer.material = new Material(Shader.Find("Mixed Reality Toolkit/Standard"));
        //meshRenderer.material.color = Color.white;
        Build();
    }

    /// <summary>
    /// Builds a grid mesh with lines
    /// </summary>
    public void Build()
    {
        xAxisTickSpacing = GetXAxisTickSpacing();
        yAxisTickSpacing = GetYAxisTickSpacing();

        vertices = new List<Vector3>();
        indicies = new List<int>();

        int currentVertexId = 0;
        
        for (int y = 0; y <= yGridSize; y++)
        {
            for (int x = 0; x <= xGridSize; x++)
            {
                float xPos = x * xAxisTickSpacing;
                float yPos = y * yAxisTickSpacing;

                vertices.Add(new Vector3(xPos, yPos, 0));

                if (x != xGridSize)
                {
                    indicies.Add(currentVertexId);
                    indicies.Add(currentVertexId + 1);
                }
                if (y != yGridSize)
                {
                    indicies.Add(currentVertexId);
                    indicies.Add(currentVertexId + xGridSize + 1);
                }

                currentVertexId++;
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Lines, 0);
        filter.mesh = mesh;

    }

    /// <summary>
    /// Calculates the number of vertices in the mesh where adjacent quads share the same vertex.
    /// </summary>
    /// <returns>Number of vertices in the grid mesh</returns>
    public int GetNumberOfVertices()
    {
        return (xGridSize + 1) * (yGridSize + 1); 
    }

    private float GetXAxisTickSpacing()
    {
        return worldSpaceWidth / (float)xGridSize;
    }

    private float GetYAxisTickSpacing()
    {
        return worldSpaceHeight / (float)yGridSize;
    }

}
