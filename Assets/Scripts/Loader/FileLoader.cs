using System.IO;
using System;
using UnityEngine;

/// <summary>
/// Abstract class for loader of various files
/// </summary>
public abstract class FileLoader
{
    /*Voxel data*/
    public VoxelDataset voxelDataset;

    /*3D model data*/
    //public PolygonalDataset polygonalDataset;

    public abstract void loadData(string filePath);

    public abstract void createDataset();

    public override string ToString()
    {
        return base.ToString() + ": \n";
    }
}
