using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class VolumeRenderedObject : MonoBehaviour
{
    private GameObject volume;
    private Renderer renderer;

    public VolumeRenderedObject()
    {

    }

    public async Task CreateObject(GameObject container, VoxelDataset dataset)
    {
        volume = Instantiate((GameObject)Resources.Load("Prefabs/VolumePrefab"));
        volume.transform.SetParent(container.transform);

        //Instantiate(volumePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        //Renderer renderer = volume.AddComponent<Renderer>();

        renderer = volume.GetComponent<Renderer>();

        MeshRenderer meshRenderer = volume.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial.SetTexture("_MainTex", await dataset.GetDataTexture());

        float maxScale = Mathf.Max(dataset.dimX, dataset.dimY, dataset.dimZ);
        maxScale = 0.1f / maxScale;
        dataset.scaleX = maxScale;
        dataset.scaleY = maxScale;
        dataset.scaleZ = maxScale;

        volume.transform.localScale = new Vector3(dataset.dimX * maxScale, dataset.dimY * maxScale, dataset.dimZ * maxScale);

    }

    public void ChangeShader(Shader shader)
    {
        Debug.Log("Change to Shadertyp: " + shader.name);
        //switch (type)
        //{
        //    case 0:
        //        volume.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Volume Rendering/RaymarchingShader/DVR");
        //        break;
        //    case 1:
        //        volume.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Volume Rendering/RaymarchingShader/MIP");
        //        break;
        //    case 2:
        //        volume.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Volume Rendering/RaymarchingShader/Isosurface");
        //        break;
        //    default:
        //        volume.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Volume Rendering/RaymarchingShader/DVR");
        //        break;

        //}
        renderer.sharedMaterial.shader = shader;

    }
}
