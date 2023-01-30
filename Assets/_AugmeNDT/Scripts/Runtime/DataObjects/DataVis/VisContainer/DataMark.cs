using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private Channel dataChannel;
    private MeshRenderer meshRenderer;

    
    public DataMark()
    {
        if (this.dataMarkPrefab == null) this.dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Sphere");
    }
    
    public DataMark(GameObject dataMarkPrefab)
    {
        if (dataMarkPrefab == null)
        {
            this.dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Sphere");
        }
        else this.dataMarkPrefab = dataMarkPrefab;
    }

    public GameObject CreateDataMark(Transform visContainer, Channel channel)
    {
        dataMarkInstance = GameObject.Instantiate(dataMarkPrefab, channel.position, Quaternion.Euler(channel.rotation), visContainer);

        //Todo: Set Color
        meshRenderer = dataMarkInstance.GetComponent<MeshRenderer>();
        meshRenderer.material.SetColor("_Color", channel.color);

        // Get initial data of object
        this.dataChannel = channel;
        meshRenderer.sharedMaterial.color = channel.color;

        SetSize(dataChannel.size);
        SetRot(dataChannel.rotation);

        return dataMarkInstance;
    }

    public void ChangeDataMark(Channel channel)
    {
        SetPos(channel.position);
        SetSize(channel.size);
        //SetFacing(channel.facing);
        SetRot(channel.rotation);
        SetColor(channel.color);
    }

    /// <summary>
    /// Returns a prefilled channel instance
    /// </summary>
    /// <returns></returns>
    public static Channel DefaultDataChannel()
    {
        DataMark.Channel channel = new DataMark.Channel
        {
            position = new Vector3(0, 0, 0),
            rotation = new Vector3(0, 0, 0),
            facing = new Vector3(0, 0, -1),
            color = new Vector4(1, 0, 0, 1),
            //Todo: Size?
            size = new Vector3(0.03f, 0.03f, 0.03f)
        };

        return channel;
    }

    public void SetPos(Vector3 position)
    {
        dataChannel.position = position;
        dataMarkInstance.transform.localPosition = dataChannel.position;
    }

    public void SetFacing(Vector3 facingDir)
    {
        dataChannel.facing = facingDir;
        //TODO: Rotate Object
    }

    public void SetRot(Vector3 rotation)
    {
        dataChannel.rotation = rotation;
        //dataMarkInstance.transform.eulerAngles = dataChannel.rotation;
    }

    public void SetSize(Vector3 size)
    {
        dataChannel.size = size;
        dataMarkInstance.transform.localScale = dataChannel.size;

        //Pivot is in Center of Object
        float adjustedHeight = dataChannel.size.y / 2.0f;
        //dataMarkInstance.transform.Translate(0, adjustedHeight, 0); // Somehow doubles the translation value
        dataMarkInstance.transform.localPosition = new Vector3(dataChannel.position.x, dataChannel.position.y + adjustedHeight, dataChannel.position.z);
    }

    public void SetColor(Vector4 color)
    {
        dataChannel.color = color;
        meshRenderer.material.SetColor("_Color", color);
        //meshRenderer.sharedMaterial.color = color;
    }

}
