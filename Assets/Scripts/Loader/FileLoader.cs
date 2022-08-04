using System.IO;
using System;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Abstract class for loader of various files
/// </summary>
public abstract class FileLoader
{
    /*Voxel data*/
    public VoxelDataset voxelDataset;

    /*3D model data*/
    //public PolygonalDataset polygonalDataset;

    public abstract Task loadData(string filePath);

    public abstract void createDataset();

    public override string ToString()
    {
        return base.ToString() + ": \n";
    }
}
