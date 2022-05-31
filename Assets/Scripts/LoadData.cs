using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class LoadData : MonoBehaviour
{
    [SerializeField]
    private string fileToImport;

    [SerializeField]
    private int dimX;

    [SerializeField]
    private int dimY;

    [SerializeField]
    private int dimZ;

    [SerializeField]
    private int bytesToSkip = 0;

    [SerializeField]
    private DataContentFormat dataFormat = DataContentFormat.Int16;

    [SerializeField]
    private Endianness endianness = Endianness.LittleEndian;

    //[SerializeField]
    private VoxelDataset dataset;

    [SerializeField]
    private VolumeRenderedObject renderedVolume;

    // Start is called before the first frame update
    void Start()
    {
        LoadDataset(fileToImport);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void LoadDataset(string filePath)
    {
        fileToImport = filePath;

        //if (Path.GetExtension(fileToImport) == ".ini")
        //    fileToImport = fileToImport.Replace(".ini", ".raw");

        //// Try parse ini file (if available)
        //DatasetIniData initData = DatasetIniReader.ParseIniFile(fileToImport + ".ini");
        //if (initData != null)
        //{
        //    dimX = initData.dimX;
        //    dimY = initData.dimY;
        //    dimZ = initData.dimZ;
        //    bytesToSkip = initData.bytesToSkip;
        //    dataFormat = initData.format;
        //    endianness = initData.endianness;
        //}

        //this.minSize = new Vector2(300.0f, 200.0f);

        //FileLoader fileLoader = new RawFileLoader(fileToImport, dimX, dimY, dimZ, dataFormat, endianness, bytesToSkip);
        fileToImport = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
        FileLoader fileLoader = new MhdFileLoader(fileToImport);
        fileLoader.loadData(fileToImport);
        dataset = fileLoader.voxelDataset;

        if (dataset != null)
        {
            //Render GameObject
            GameObject volume = new GameObject("VolumeRenderedObject_" + dataset.datasetName);
            renderedVolume = volume.AddComponent<VolumeRenderedObject>();
            renderedVolume.CreateObject(dataset);

            // Save the texture to your Unity Project
            //AssetDatabase.CreateAsset(dataset.GetDataTexture(), "Assets/Textures/Example3DTexture.asset");
            
        }
        else
        {
            Debug.LogError("Failed to import datset");
        }


    }
}

