using System.IO;
using System;
using UnityEngine;
using System.Threading.Tasks;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using Windows.Storage;
#endif

/// <summary>
/// Abstract class for loader of various files
/// </summary>
public abstract class FileLoader
{
    /*Voxel data*/
    public VoxelDataset voxelDataset;

    /*3D model data*/
    //public PolygonalDataset polygonalDataset;

    public abstract Task LoadData(string filePath);

    public abstract void CreateDataset();

    public override string ToString()
    {
        return base.ToString() + ": \n";
    }

#if !UNITY_EDITOR && UNITY_WSA_10_0
    protected async Task<StreamReader> getStreamReader(string path)
    {
        StorageFile file = await StorageFile.GetFileFromPathAsync(path);
        if (file == null) Debug.LogError("StorageFile is null");

        var randomAccessStream = await file.OpenReadAsync();
        Stream stream = randomAccessStream.AsStreamForRead();
        StreamReader str = new StreamReader(stream);

        return str;
    }

    protected async Task<BinaryReader> getBinaryReader(string path)
    {
        StorageFile file = await StorageFile.GetFileFromPathAsync(path);
        if(file == null) Debug.LogError("StorageFile is null");

        var randomAccessStream = await file.OpenReadAsync();
        Stream stream = randomAccessStream.AsStreamForRead();
        if (stream == null) Debug.LogError("stream is null");
        BinaryReader binr = new BinaryReader(stream);

        return binr;
    }

    protected async Task<bool> checkIfFileExists(string path)
    {
        try
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
        }
        catch (Exception)
        {

            return false;
        }

        return true;        
    }
#endif
}
