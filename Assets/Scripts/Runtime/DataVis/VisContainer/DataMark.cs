using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataMark
{
    public struct Channel
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 facing;
        public Vector3 size; // width, height, depth;
        public Vector4 color;

    }

    [SerializeField]
    private GameObject dataMarkPrefab;
    private GameObject dataMarkInstance;

    private Channel channels;
    private MeshRenderer meshRenderer;


    public GameObject CreateDataMark(Transform visContainer, Channel channel)
    {
        dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
        dataMarkInstance = GameObject.Instantiate(dataMarkPrefab, channel.position, Quaternion.Euler(channel.rotation), visContainer);

        meshRenderer = dataMarkInstance.GetComponent<MeshRenderer>();

        // Get initial data of object
        this.channels = channel;
        meshRenderer.sharedMaterial.color = channel.color;


        //channels = new Channel
        //{
        //    position = this.transform.position,
        //    rotation = this.transform.eulerAngles,
        //    facing = this.transform.forward,
        //    size = meshRenderer.bounds.size,
        //    color = meshRenderer.sharedMaterial.color
        //};

        return dataMarkInstance;
    }

    public void SetPos(Vector3 position)
    {
        channels.position = position;
        dataMarkInstance.transform.position = channels.position;
    }

    public void SetRot(Vector3 rotation)
    {
        channels.rotation = rotation;
        dataMarkInstance.transform.eulerAngles = channels.rotation;
    }

    public void SetSize(Vector3 size)
    {
        channels.size = size;
        dataMarkInstance.transform.localScale = channels.size;
    }

    public void SetColor(Vector4 color)
    {
        channels.color = color;
        meshRenderer.sharedMaterial.color = color;
    }

}
