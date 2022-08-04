using System;
using UnityEngine;

public class VolumeRenderedObject : MonoBehaviour
{

    public VolumeRenderedObject()
    {

    }

    public GameObject CreateObject(VoxelDataset dataset)
    {
        GameObject volume = Instantiate((GameObject)Resources.Load("Prefabs/VolumePrefab"));

        //Instantiate(volumePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        //Renderer renderer = volume.AddComponent<Renderer>();

        MeshRenderer meshRenderer = volume.GetComponent<MeshRenderer>();
        //meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);

        meshRenderer.sharedMaterial.SetTexture("_MainTex", dataset.GetDataTexture());

        MeshFilter viewedModelFilter = (MeshFilter)volume.GetComponent("MeshFilter");
        Debug.Log("VertexCount for Volume: " + viewedModelFilter.mesh.vertexCount);

        float maxScale = Mathf.Max(dataset.dimX, dataset.dimY, dataset.dimZ);
        maxScale = 0.1f / maxScale;
        dataset.scaleX = maxScale;
        dataset.scaleY = maxScale;
        dataset.scaleZ = maxScale;

        volume.transform.localScale = new Vector3(dataset.dimX * maxScale, dataset.dimY * maxScale, dataset.dimZ * maxScale);

        return volume;
    }
}
