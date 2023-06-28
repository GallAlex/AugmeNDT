using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AugmeNDT{
    /// <summary>
    /// Used to combined multiple meshes to one big mesh to save draw calls.
    /// Class is responisble to create the combined mesh, store a uniques id of the individual mesh and its vertices & triangle indices
    /// </summary>
    public class MeshManager
    {
        // Can work with GameObjects and extract its mesh or directly by adding meshes

        // Combined Mesh
        private List<Mesh> combinedMeshes;

        /// Dictionary which mesh is saved in which combined mesh
        private Dictionary<int, int> meshIdToCombinedMeshMap;

        // Dictionary of start-/end vertice ID for every mesh
        private Dictionary<int, int[]> startEndVerticeMap;

        // Dictionary with the original (uncombined) Meshes 
        private Dictionary<int, Mesh> meshIdToOriginalMeshMap;


        //Index buffer can either be 16 bit (supports up to 65535 vertices in a mesh), or 32 bit (supports up to 4 billion vertices).
        private IndexFormat meshIndexFormat = IndexFormat.UInt16;
        //How many vertices per combined mesh (16 bit - max 65535) (Smaller meshes are faster to modify and GPU support for 32 bit indices is not guaranteed)
        private int vertexLimit = 32760;

        private bool setDefaultVertexColor = true;
        private Color defaultVertexColor = Color.white;

        public MeshManager()
        {
            combinedMeshes = new List<Mesh>();
            meshIdToCombinedMeshMap = new Dictionary<int, int>();
            startEndVerticeMap = new Dictionary<int, int[]>();
            meshIdToOriginalMeshMap = new Dictionary<int, Mesh>();
        }

        /// <summary>
        /// Method creates a combined mesh from a list of meshes
        /// </summary>
        /// <param name="meshes"></param>
        /// <returns></returns>
        public List<Mesh> CreateCombinedMesh(List<Mesh> meshes)
        {
            int verticesSoFar = 0; // Note that vertices ID start at zero!
            int currenCombinedMeshID = 0; // ID in which the mesh is saved

            // Combine all meshes
            List<CombineInstance> combineList = new List<CombineInstance>(meshes.Count);
            CombineInstance combine = new CombineInstance();
        
            for (int meshID = 0; meshID < meshes.Count; meshID++)
            {

                //Do we reached the vertex limit?
                if ((verticesSoFar + meshes[meshID].vertexCount) > vertexLimit)
                {
                    combinedMeshes.Add(CombineAllMeshes(combineList, verticesSoFar)); //Save the already combined meshes
                    currenCombinedMeshID++;
                    //Reset Storage/Counters
                    combineList.Clear();
                    verticesSoFar = 0;
                }

                // Save to Dictionary
                meshIdToOriginalMeshMap.Add(meshID, meshes[meshID]);
                meshIdToCombinedMeshMap.Add(meshID, currenCombinedMeshID);
                startEndVerticeMap.Add(meshID, new int[] { verticesSoFar, verticesSoFar + meshes[meshID].vertexCount - 1 });

                combine.mesh = meshes[meshID];
                combine.transform = Matrix4x4.identity;
                combineList.Add(combine);
            
                // Add vertices to counter
                verticesSoFar += meshes[meshID].vertexCount;
            }
            combinedMeshes.Add(CombineAllMeshes(combineList, verticesSoFar)); //Save the remaining combined meshes

            return combinedMeshes;
        }

        /// <summary>
        /// Method creates a combined mesh from a list of gameObjects
        /// Gameobjects are expected to have a mesh filter with a mesh
        /// Calls internally the method CreateCombinedMesh(List<Mesh> meshes)
        /// </summary>
        /// <param name="gameObjects"></param>
        /// <returns></returns>
        public List<Mesh> CreateCombinedMesh(List<GameObject> gameObjects)
        {
            // Get all meshes from gameObjects
            List<Mesh> meshes = new List<Mesh>();
            foreach (GameObject obj in gameObjects)
            {
                meshes.Add(obj.GetComponent<MeshFilter>().mesh);
            }

            return CreateCombinedMesh(meshes);
        }

        private Mesh CombineAllMeshes(List<CombineInstance> meshesToCombine, int vertices)
        {
            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = meshIndexFormat;
            combinedMesh.CombineMeshes(meshesToCombine.ToArray(), true, true);
            combinedMesh.name = "Vertices_" + vertices;

            //Set Vertex colors for combined mesh
            if (setDefaultVertexColor)
            {
                SetVertexColorArray(combinedMesh, defaultVertexColor);
            }

            return combinedMesh;
        }


        /// <summary>
        /// Method gets the start and end indices for the vertices of a specific mesh.
        /// Array[0] has the first index and Array[1] the of last index for the vertices of the selected mesh.
        /// </summary>
        /// <param name="meshID"></param>
        /// <returns></returns>
        public int[] GetMeshVerticeIndices(int meshID)
        {
            return startEndVerticeMap[meshID];
        }

        /// <summary>
        /// Methods returns the index where a specific mesh is stored in the combined mesh list.
        /// The selected mesh can then be found in the at the returned index in List combinedMeshes.
        /// </summary>
        /// <param name="meshID"></param>
        /// <returns></returns>
        public int GetIndexOfCombinedMesh(int meshID)
        {
            return meshIdToCombinedMeshMap[meshID];
        }

        /// <summary>
        /// Methods returns the selected CombinedMesh.
        /// </summary>
        /// <param name="meshID"></param>
        /// <returns></returns>
        public Mesh GetCombinedMesh(int meshID)
        {
            return combinedMeshes[meshID];
        }

        // Method to add mesh to combined mesh

        // Method to remove mesh from combined mesh

        private void SetVertexColorArray(Mesh mesh, Color color)
        {
            //Iterate through all vertices and set the color
            Color[] colors = new Color[mesh.vertexCount];

            //List<Color> newColor = Enumerable.Repeat(Color.white, combinedMesh.vertexCount).ToList();
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                colors[i] = color;
            }
        
            mesh.SetColors(colors);
        }
    }
}
