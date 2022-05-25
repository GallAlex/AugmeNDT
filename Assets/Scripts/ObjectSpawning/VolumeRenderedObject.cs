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


        if (dataset.scaleX != 0.0f && dataset.scaleY != 0.0f && dataset.scaleZ != 0.0f)
        {
            float maxScale = Mathf.Max(dataset.scaleX, dataset.scaleY, dataset.scaleZ);
            volume.transform.localScale = new Vector3(dataset.scaleX / maxScale, dataset.scaleY / maxScale, dataset.scaleZ / maxScale);
        }

        return volume;
    }
}
