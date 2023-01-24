using System.Collections.Generic;
using UnityEngine;

public class DataGrid
{
    private float worldSpaceWidth = 1.0f;
    private float worldSpaceHeight = 1.0f;

    public int xGridSize = 4;
    public int yGridSize = 4;
    private Direction[] gridOrientation = {Direction.X, Direction.Y};

    private float xAxisTickSpacing;
    private float yAxisTickSpacing;

    private GameObject gridInstance;
    private MeshFilter filter;

    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> indicies;


    public void CreateGrid(Transform visContainer, float containerWidth, float containerHeight, Direction[] axisDirections, int xDivision, int yDivision)
    {
        //Init Values
        worldSpaceWidth = containerWidth;
        worldSpaceHeight = containerHeight;
        gridOrientation = axisDirections;
        xGridSize = xDivision;
        yGridSize = yDivision;
        
        gridInstance = new GameObject("Grid" + axisDirections[0].ToString() + axisDirections[1].ToString());
        gridInstance.transform.parent = visContainer.transform;

        // Add mesh filter if not present
        filter = gridInstance.GetComponent<MeshFilter>();
        filter = filter != null ? filter : gridInstance.AddComponent<MeshFilter>();

        // Add mesh renderer if not present
        MeshRenderer meshRenderer = filter.GetComponent<MeshRenderer>();
        meshRenderer = meshRenderer != null ? meshRenderer : gridInstance.AddComponent<MeshRenderer>();

        mesh = new Mesh();

        meshRenderer.material = new Material(Shader.Find("Mixed Reality Toolkit/Standard"));
        //meshRenderer.material.color = Color.white;

        Build();
    }

    /// <summary>
    /// Return the Gameobject of the grid
    /// </summary>
    /// <returns></returns>
    public GameObject GetGridObject()
    {
        return gridInstance;
    }

    /// <summary>
    /// Builds a grid mesh with lines
    /// </summary>
    private void Build()
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

                vertices.Add(CreateGridInDirection(xPos, yPos));

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
    /// Methods creates vector for the vertices of the grid. The position depends on the two axes between the grid should be drawn.
    /// </summary>
    /// <param name="xPos"></param>
    /// <param name="yPos"></param>
    /// <returns></returns>
    private Vector3 CreateGridInDirection(float xPos, float yPos)
    {
        Direction axisDir1 = gridOrientation[0];
        Direction axisDir2 = gridOrientation[1];

        //Grid XY
        if ( (axisDir1 == Direction.X && axisDir2 == Direction.Y) || (axisDir2 == Direction.X && axisDir1 == Direction.Y) )
        {
            return new Vector3(xPos, yPos, 0);
        }
        //Grid XZ
        if ((axisDir1 == Direction.X && axisDir2 == Direction.Z) || (axisDir2 == Direction.X && axisDir1 == Direction.Z))
        {
            return new Vector3(xPos, 0 , yPos);
        }
        //Grid ZY
        if ((axisDir1 == Direction.Z && axisDir2 == Direction.Y) || (axisDir2 == Direction.Z && axisDir1 == Direction.Y))
        {
            return new Vector3(0, xPos, yPos);
        }

        //Use Grid XY
        return new Vector3(xPos, yPos, 0);
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
