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


#if UNITY_EDITOR 
    protected static async Task<StreamReader> GetStreamReader(string filePath)
    {
        StreamReader reader = new StreamReader(filePath);
        return reader;
    }

    protected static async Task<BinaryReader> GetBinaryReader(string filePath)
    {
        BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open));
        return reader;
    }

    protected static async Task<bool> CheckIfFileExists(string filePath)
    {
        return File.Exists(filePath);
    }
#endif


#if !UNITY_EDITOR && UNITY_WSA_10_0

    protected static async Task<StreamReader> GetStreamReader(string filePath)
    {
        StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
        if (file == null) Debug.LogError("StorageFile is null");

        var randomAccessStream = await file.OpenReadAsync();
        Stream stream = randomAccessStream.AsStreamForRead();
        StreamReader str = new StreamReader(stream);

        return str;
    }

    protected static async Task<BinaryReader> GetBinaryReader(string filePath)
    {
        StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
        if(file == null) Debug.LogError("StorageFile is null");

        var randomAccessStream = await file.OpenReadAsync();
        Stream stream = randomAccessStream.AsStreamForRead();
        if (stream == null) Debug.LogError("stream is null");
        BinaryReader binr = new BinaryReader(stream);

        return binr;
    }

    protected static async Task<bool> CheckIfFileExists(string filePath)
    {
        try
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
        }
        catch (Exception)
        {

            return false;
        }

        return true;        
    }
#endif
}
