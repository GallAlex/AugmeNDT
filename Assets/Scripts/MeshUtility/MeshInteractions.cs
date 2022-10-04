using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Class enables the manipulation of Meshes
/// </summary>
[ExecuteAlways]
public class MeshInteractions : MonoBehaviour
{
    //private Mesh clonedMesh;
    //private List<Vector3> vertices;
    //private List<int> triangles;
    //private List<Vector3> normals;

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh originalMesh = meshFilter.sharedMesh;
        TranslateMesh(new Vector3(60, 0, -60), originalMesh);
        ColorMesh(Color.black, originalMesh);
    }


    public void TranslateMesh(Vector3 translation, Mesh mesh)
    {
        // https://docs.unity3d.com/ScriptReference/Mesh.html
        Vector3[] vertices = mesh.vertices;

        for (int vertexID = 0; vertexID < vertices.Length; vertexID++)
        {
            vertices[vertexID] = vertices[vertexID] + translation;
        }

        mesh.vertices = vertices;
        //meshFilter.mesh.RecalculateNormals();

    }

    public void RotateMesh(Vector3 rotation)
    {

    }

    public void ColorMesh(Color color, Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color[] verticeColors = mesh.colors;

        for (int vertexID = 0; vertexID < verticeColors.Length; vertexID++)
        {
            verticeColors[vertexID] = Color.green;
        }
        //for (int vertexID = verticeColors.Length / 2; vertexID < verticeColors.Length; vertexID++)
        //{
        //    verticeColors[vertexID] = Color.red;
        //}

        mesh.colors = verticeColors;
    }

}
