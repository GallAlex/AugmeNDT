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
        public int[] pivotPointCenter;   // Is the x,y,z center of the object the pivot point?
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 size;            // width, height, depth;
        public Vector3 facing;
        public Vector4 color;

    }

    public bool selected = false;
    public Channel dataChannel;

    private const float minSize = 0.0000001f; // Minimum size of the data mark for drawing

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
            pivotPointCenter = new int[]{1, 1, 1},
            position = new Vector3(0, 0, 0),
            rotation = new Vector3(0, 0, 0),
            size = new Vector3(0.03f, 0.03f, 0.03f),
            facing = new Vector3(0, 0, -1),
            color = new Vector4(1, 0, 0, 1)
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

        SetPivotPointCenter(dataChannel.pivotPointCenter);
        SetPos(dataChannel.position);
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

    public void SetPivotPointCenter(int[] pivotPointCenter)
    {
        dataChannel.pivotPointCenter = pivotPointCenter;
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

        // We change the size of the object to a minimum size if it is smaller than the minimum drawable size but keep the dataChannel.size to it original value
        Vector3 newSize = size;
        // Based on the Pivot Point we have to change the position of the object (based on its size)
        Vector3 newPos = dataChannel.position;

        //For each size check if bigger or equal to min size
        if (newSize.x < minSize) newSize.x = minSize;
        if (newSize.y < minSize) newSize.y = minSize;
        if (newSize.z < minSize) newSize.z = minSize;

        // Draw with min size scaling
        dataMarkInstance.transform.localScale = newSize;


        //If Pivot is not in Center of Object but at left front bottom corner
        if (dataChannel.pivotPointCenter[0] == 0)
            newPos = new Vector3(newPos.x + newSize.x / 2.0f, newPos.y, newPos.z);
        if (dataChannel.pivotPointCenter[1] == 0)
        {
            newPos = new Vector3(newPos.x, newPos.y + newSize.y / 2.0f, newPos.z);
        }
        if (dataChannel.pivotPointCenter[2] == 0)
            newPos = new Vector3(newPos.x, newPos.y, newPos.z + newSize.z / 2.0f);

        dataMarkInstance.transform.localPosition = newPos;
    }

    public void SetColor(Vector4 color)
    {
        dataChannel.color = color;
        meshRenderer.material.SetColor("_Color", color);
        //meshRenderer.sharedMaterial.color = color;
    }
}
