using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using System.Data;
using UnityEngine.UIElements;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

public class VolumeRenderedObject
{
    private GameObject containerPrefab;
    private GameObject volumeContainer;

    private GameObject volumePrefab;
    private GameObject volume;

    private Material volumeMaterial;
    private MeshRenderer meshRenderer;

    //Data
    private VoxelDataset voxelDataset;


    public VolumeRenderedObject()
    {
        volumeMaterial = new Material((Material)Resources.Load("Materials/VolumeMaterial", typeof(Material)));

        containerPrefab = (GameObject)Resources.Load("Prefabs/VolumeContainer");
        volumePrefab = (GameObject)Resources.Load("Prefabs/VolumeRenderCube");
    }

    public async Task CreateObject(VoxelDataset dataset)
    {
        voxelDataset = dataset;

        volumeContainer = GameObject.Instantiate(containerPrefab);
        volumeContainer.name = dataset.datasetName;

        volume = GameObject.Instantiate(volumePrefab, volumeContainer.transform);
        volume.name = "Volume";

        meshRenderer = volume.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = volumeMaterial;
        meshRenderer.sharedMaterial.SetTexture("_MainTex", await dataset.GetDataTexture());

        BoxCollider meshColl = volume.GetComponent<BoxCollider>() != null ? volume.GetComponent<BoxCollider>() : volume.AddComponent<BoxCollider>();

        BoundsControl boundsCon = volumeContainer.GetComponent<BoundsControl>() != null ? volumeContainer.GetComponent<BoundsControl>() : volumeContainer.AddComponent<BoundsControl>();
        BoxCollider boundsColl = volumeContainer.GetComponent<BoxCollider>() != null ? volumeContainer.GetComponent<BoxCollider>() : volumeContainer.AddComponent<BoxCollider>();


        //TODO: Save scale change: dataset.scaleZ = maxScale;
        //Maintain Volume Scale Ratio
        GlobalScaleAndPos.ResizeRealtiveObject(volume.transform, 1.0f, new Vector3(dataset.dimX, dataset.dimY, dataset.dimZ));
        GlobalScaleAndPos.ResizeBoxCollider(volume.transform, boundsColl, meshColl.size, meshColl.center);
        //GlobalScaleAndPos.MoveOriginToLowerFrontLeftPoint(volume.transform);
        GlobalScaleAndPos.SetToBestInitialScale(volumeContainer.transform, volumeContainer.transform.localScale);

        GlobalScaleAndPos.SetToBestInitialStartPos(volumeContainer.transform);
    }

    public void ChangeShader(Shader shader)
    {
        Debug.Log("Change to Shadertyp: " + shader.name);
        volume.GetComponent<Renderer>().sharedMaterial.shader = shader;

    }

}
