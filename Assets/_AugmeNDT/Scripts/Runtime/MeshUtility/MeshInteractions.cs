// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using UnityEngine;

namespace AugmeNDT{
    /// <summary>
    /// Class enables methods to manipulation a mesh
    /// </summary>
    public class MeshInteractions
    {

        public static void TranslateMesh(Mesh mesh, int[] indexRange, Vector3 translation)
        {
            Vector3[] modifiedVertices = mesh.vertices;

            // Range is expected to be inclusive startIndex and endIndex (objectID * vertexCount -1)
            for (int vertexID = indexRange[0]; vertexID <= indexRange[1]; vertexID++)
            {
                modifiedVertices[vertexID] = modifiedVertices[vertexID] + translation;
            }
        
            mesh.vertices = modifiedVertices;
        }

        /// <summary>
        /// Colors a part of the vertices (range from startIndex indexRange[0] to endIndex indexRange[1]) in the mesh in color[0] and the whole remaining mesh with color[1]
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="indexRange"></param>
        /// <param name="color"></param>
        public static void ColorMesh(Mesh mesh, int[] indexRange, Color[] color)
        {
            Color[] newColor = mesh.colors;

            if (newColor.Length != mesh.vertexCount)
            {
                Debug.LogError("Mesh has no or too few vertex colors");
                return;
            }
        
            // Range is expected to be inclusive startIndex and endIndex (objectID * vertexCount -1)
            for (int vertexID = indexRange[0]; vertexID <= indexRange[1]; vertexID++)
            {
                newColor[vertexID] = color[0];
            }

            mesh.SetColors(newColor);
        }

    }
}
