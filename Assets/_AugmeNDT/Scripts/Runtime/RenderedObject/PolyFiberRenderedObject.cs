using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public class PolyFiberRenderedObject : MonoBehaviour
{

    private GameObject containerPrefab;
    private GameObject polyModelContainer;

    //private GameObject polyModelPrefab;
    private GameObject polyModel;

    private Material cylinderMaterial;
    private MeshFilter currentMeshFilter;

    //Data
    private PolyFiberData polyFiberDataset;


    //private CombineInstance[] combine;
    private CylinderObjectVis cylinderVis;


    //How many vertices per combined mesh (max 65535)
    //Should sometimes be smaller because smaller meshes are faster to modify
    [System.NonSerialized]
    public int vertexLimit = 30000;
    int verticesSoFar = 0;

    public PolyFiberRenderedObject()
    {
        cylinderMaterial = new Material((Material)Resources.Load("Materials/PolyMaterial", typeof(Material)));

        containerPrefab = (GameObject)Resources.Load("Prefabs/PolyModelContainer");
    }

    public async Task CreateObject(PolyFiberData dataset)
    {
        polyFiberDataset = dataset;

        polyModelContainer = GameObject.Instantiate(containerPrefab, containerPrefab.transform.position, Quaternion.identity);
        polyModelContainer.name = "FiberModel";

        polyModel = new GameObject("Model");
        polyModel.transform.SetParent(polyModelContainer.transform);


        //TODO:
        // Mesh is bigger then start - end point (because of radius)
        // Save real size - either by mhd or by min/max

        // CREATE MESH

        //CreateCylinderRepresentation(polyFiberDataset);
        GameObject fiberMesh = CreateCombinedCylinderRepresentation(dataset);
        BoxCollider meshColl = fiberMesh.GetComponent<BoxCollider>() != null ? fiberMesh.GetComponent<BoxCollider>() : fiberMesh.AddComponent<BoxCollider>();

        //BoundsControl boundsCon = polyModelContainer.GetComponent<BoundsControl>() != null ? polyModelContainer.GetComponent<BoundsControl>() : polyModelContainer.AddComponent<BoundsControl>();
        BoxCollider boundsColl = polyModelContainer.GetComponent<BoxCollider>() != null ? polyModelContainer.GetComponent<BoxCollider>() : polyModelContainer.AddComponent<BoxCollider>();

        GlobalScaleAndPos.ResizeAbsolutMeshObject(fiberMesh.transform, 1.0f, meshColl.size);
        GlobalScaleAndPos.ResizeBoxCollider(fiberMesh.transform, boundsColl, meshColl.size, meshColl.center);
        GlobalScaleAndPos.SetToBestInitialScale(polyModelContainer.transform, polyModelContainer.transform.localScale);


        GlobalScaleAndPos.SetToBestInitialStartPos(polyModelContainer.transform);

    }

    private void CreateCylinderRepresentation(PolyFiberData dataset)
    {
        cylinderVis = new CylinderObjectVis();
        //combine = new CombineInstance[polyFiberDataset.NumberOfFibers];

        for (int fiber = 0; fiber < dataset.NumberOfFibers; fiber++)
        {
            GameObject fiberObject = new GameObject("Fiber_"+ fiber);
            fiberObject.transform.SetParent(polyModel.transform);


            Mesh mesh = cylinderVis.CreateMesh(dataset.Label[fiber].ToString(), dataset.GetFiberRadius(fiber), dataset.GetFiberCoordinates(fiber));
            currentMeshFilter = fiberObject.AddComponent<MeshFilter>();
            currentMeshFilter.mesh = mesh;
            MeshRenderer currentMeshRenderer = fiberObject.AddComponent<MeshRenderer>();
            //currentMeshRenderer.material = new Material(Shader.Find("Mixed Reality Toolkit/Standard"));
            currentMeshRenderer.material = new Material(cylinderMaterial);
            //currentMeshRenderer.material.SetFloat("Vertex Colors", 1.0f);
            //ResizeToBound(polyFiberObject, fiberObject);

            //combine[fiber].mesh = mesh;
        }

        //polyFiberObject.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        //polyFiberObject.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
    }

    private GameObject CreateCombinedCylinderRepresentation(PolyFiberData dataset)
    {
        cylinderVis = new CylinderObjectVis();
        CombineInstance[] combine = new CombineInstance[dataset.NumberOfFibers];

        GameObject fiberObject = new GameObject("AllFibers_" + dataset.NumberOfFibers);
        fiberObject.transform.SetParent(polyModel.transform);

        currentMeshFilter = fiberObject.AddComponent<MeshFilter>();
        MeshRenderer currentMeshRenderer = fiberObject.AddComponent<MeshRenderer>();
        

        for (int fiber = 0; fiber < dataset.NumberOfFibers; fiber++)
        {
            Mesh mesh = cylinderVis.CreateMesh(dataset.Label[fiber].ToString(), dataset.GetFiberRadius(fiber), dataset.GetFiberCoordinates(fiber));
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
        currentMeshRenderer.material = new Material(cylinderMaterial);

        return fiberObject;
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
