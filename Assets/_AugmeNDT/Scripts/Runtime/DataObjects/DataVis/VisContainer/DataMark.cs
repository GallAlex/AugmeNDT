using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.InputSystem;

public class DataMark
{
    private readonly int markID = 0;
    
    public struct Channel
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 size; // width, height, depth;
        public Vector3 facing;
        public Vector4 color;

    }

    public bool selected = false;

    private Channel dataChannel;
    
    [SerializeField]
    private GameObject dataMarkPrefab;
    private GameObject dataMarkInstance;
    private MeshRenderer meshRenderer;


    // Interactor
    private VisInteractor visInteractor;
    private DataMarkInteractable dataMarkInteractable;
    
    public DataMark(int iD)
    {
        if (this.dataMarkPrefab == null) this.dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Sphere");

        markID = iD;
        dataChannel = DefaultDataChannel();
    }
    
    public DataMark(int iD, GameObject dataMarkPrefab)
    {
        if (dataMarkPrefab == null)
        {
            this.dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Sphere");
        }
        else this.dataMarkPrefab = dataMarkPrefab;

        markID = iD;
        dataChannel = DefaultDataChannel();
    }

    public int GetDataMarkId()
    {
        return markID;
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

    public void SetVisInteractor(VisInteractor interactor)
    {
        visInteractor = interactor;
    }
    
    public GameObject CreateDataMark(Transform visContainer, Channel channel)
    {
        dataMarkInstance = GameObject.Instantiate(dataMarkPrefab, channel.position, Quaternion.Euler(channel.rotation), visContainer);
        dataMarkInstance.name = dataMarkPrefab.name+ "_" + markID;
        meshRenderer = dataMarkInstance.GetComponent<MeshRenderer>();

        //TODO: Add Interactable Component through Code?
        // Interaction Component
        dataMarkInteractable = dataMarkInstance.GetComponent<DataMarkInteractable>();

        this.dataChannel = channel;

        if (visInteractor != null)
        {
            dataMarkInteractable.Init(this, visInteractor);
        }
        else
        {
            dataMarkInteractable.DisableInteraction(); // Disable Interaction
        }

        SetColor(dataChannel.color);
        SetSize(dataChannel.size);
        SetRot(dataChannel.rotation);

        return dataMarkInstance;
    }

    public GameObject GetDataMarkInstance()
    {
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
