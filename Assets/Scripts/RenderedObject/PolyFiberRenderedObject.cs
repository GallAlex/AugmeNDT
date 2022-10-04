using System.Data;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public class PolyFiberRenderedObject : MonoBehaviour
{
    private GameObject polyFiberObject;
    //private CombineInstance[] combine;
    private CylinderObjectVis cylinderVis;

    //How many vertices per combined mesh (max 65535)
    //Should sometimes be smaller because smaller meshes are faster to modify
    [System.NonSerialized]
    public int vertexLimit = 30000;
    int verticesSoFar = 0;

    public async Task CreateObject(GameObject container, PolyFiberData polyFiberDataset)
    {
        //polyFiberObject = new GameObject("PolyObject");
        polyFiberObject = Instantiate((GameObject)Resources.Load("Prefabs/PolyPrefab"));
        polyFiberObject.transform.SetParent(container.transform);
        //MeshFilter currentMeshFilter = polyFiberObject.AddComponent<MeshFilter>();
        //MeshRenderer currentMeshRenderer = polyFiberObject.AddComponent<MeshRenderer>();

        //CreateCylinderRepresentation(polyFiberDataset);
        CreateCombinedCylinderRepresentation(polyFiberDataset);

        //polyFiberObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //polyFiberObject.AddComponent<BoundsControl>();
        polyFiberObject.GetComponent<BoundsControl>().UpdateBounds();
        polyFiberObject.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
    }

    private void CreateCylinderRepresentation(PolyFiberData polyFiberDataset)
    {
        cylinderVis = new CylinderObjectVis();
        //combine = new CombineInstance[polyFiberDataset.NumberOfFibers];

        for (int fiber = 0; fiber < polyFiberDataset.NumberOfFibers; fiber++)
        {
            GameObject fiberObject = new GameObject("Fiber_"+ fiber);
            fiberObject.transform.SetParent(polyFiberObject.transform);


            Mesh mesh = cylinderVis.CreateMesh(polyFiberDataset.Label[fiber].ToString(), polyFiberDataset.GetFiberRadius(fiber), polyFiberDataset.GetFiberCoordinates(fiber));
            MeshFilter currentMeshFilter = fiberObject.AddComponent<MeshFilter>();
            currentMeshFilter.mesh = mesh;
            MeshRenderer currentMeshRenderer = fiberObject.AddComponent<MeshRenderer>();
            currentMeshRenderer.material = new Material(Shader.Find("Mixed Reality Toolkit/Standard"));
            currentMeshRenderer.material.SetFloat("Vertex Colors", 1.0f);
            //ResizeToBound(polyFiberObject, fiberObject);

            //combine[fiber].mesh = mesh;
        }

        //polyFiberObject.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        //polyFiberObject.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
    }

    private void CreateCombinedCylinderRepresentation(PolyFiberData polyFiberDataset)
    {
        cylinderVis = new CylinderObjectVis();
        CombineInstance[] combine = new CombineInstance[polyFiberDataset.NumberOfFibers];

        GameObject fiberObject = new GameObject("AllFibers_" + polyFiberDataset.NumberOfFibers);
        fiberObject.transform.SetParent(polyFiberObject.transform);

        MeshFilter currentMeshFilter = fiberObject.AddComponent<MeshFilter>();
        MeshRenderer currentMeshRenderer = fiberObject.AddComponent<MeshRenderer>();
        

        for (int fiber = 0; fiber < polyFiberDataset.NumberOfFibers; fiber++)
        {
            Mesh mesh = cylinderVis.CreateMesh(polyFiberDataset.Label[fiber].ToString(), polyFiberDataset.GetFiberRadius(fiber), polyFiberDataset.GetFiberCoordinates(fiber));
            combine[fiber].mesh = mesh;
            combine[fiber].transform = Matrix4x4.identity;


            verticesSoFar += mesh.vertexCount;

            //Have we reached the limit?
            if (verticesSoFar > vertexLimit)
            {
                Debug.Log("Until Fiber [" + fiber + "] the number of vertices is [" + verticesSoFar + "]");
            }
        }

        currentMeshFilter.mesh = new Mesh();
        currentMeshFilter.mesh.indexFormat = IndexFormat.UInt32;
        currentMeshFilter.mesh.CombineMeshes(combine, true, true);
        currentMeshRenderer.material = new Material(Shader.Find("Mixed Reality Toolkit/Standard"));
        //currentMeshRenderer.material.EnableKeyword("INSTANCING_ON");

    }

    public void ResizeToBound(GameObject targetGameObject, GameObject newGameObject)
    {
        var targetCollider = targetGameObject.GetComponent<BoxCollider>();
        var modelMesh = newGameObject.GetComponent<MeshFilter>().mesh;

        var targetScale = targetCollider.bounds.size;
        var modelScale = modelMesh.bounds.size;

        var xFraction = modelScale.x / targetScale.x;
        var yFraction = modelScale.y / targetScale.y;
        var zFraction = modelScale.z / targetScale.z;

        var fraction = Mathf.Min(xFraction, yFraction, zFraction);

        newGameObject.transform.localScale *= fraction;
    }

    private Bounds GetMaxBounds(GameObject g)
    {
        var b = new Bounds(g.transform.position, Vector3.zero);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }
}
