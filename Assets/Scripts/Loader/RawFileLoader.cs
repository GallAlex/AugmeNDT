using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class RawFileLoader : FileLoader
{
    protected RawFileType rawFile;

    #region Getter/Setter
    public RawFileType RawFile { get => rawFile; set => rawFile = value; }
    #endregion


    // Only for derived class with meta info
    protected RawFileLoader(string filePath)
    {
        //Has to happen on main thread
        voxelDataset = ScriptableObject.CreateInstance<VoxelDataset>();
    }

    public RawFileLoader(string filePath, int dimX, int dimY, int dimZ, DataContentFormat contentFormat, Endianness endianness, int skipBytes)
    {
        //Has to happen on main thread
        voxelDataset = ScriptableObject.CreateInstance<VoxelDataset>();
        rawFile = new RawFileType(filePath, dimX, dimY, dimZ, contentFormat, endianness, skipBytes);
    }


    public override async Task LoadData(string filePath)
    {
        await ReadBinaryInfo(filePath);

        Debug.Log(rawFile.ToString());

        // Check 3D texture max size
        voxelDataset.FixDimensions();

    }

    private async Task ReadBinaryInfo(string filePath)
    {
        Task<BinaryReader> binaryReaderTask = GetBinaryReader(filePath);
        using BinaryReader reader = await binaryReaderTask;//.ConfigureAwait(false);
        long fileLength = (long)reader.BaseStream.Length;
        
        // Check that the dimension does not exceed the file size
        long expectedFileSize = (long)(rawFile.DimX * rawFile.DimY * rawFile.DimZ) * GetSampleFormatSize(rawFile.ContentFormat) + rawFile.SkipBytes;
        if (fileLength < expectedFileSize)
        {
            Debug.LogError($"The dimension({rawFile.DimX}, {rawFile.DimY}, {rawFile.DimZ}) exceeds the file size. Expected file size is {expectedFileSize} bytes, while the actual file size is {fileLength} bytes");
            return;
        }

        // In versions of .NET prior to 4.5, the maximum object size is 2GB
        // https://stackoverflow.com/questions/2415434/the-limitation-on-the-size-of-net-array
        if (expectedFileSize > System.Int32.MaxValue)
        {
            Debug.LogError("File size (" + expectedFileSize + ") is greater then the maximum possible array size (" + System.Int32.MaxValue + ")");
            return;
        }

        FillVoxelDataset();

        // Skip header (if any)
        if (rawFile.SkipBytes > 0)
        {
            reader.ReadBytes(rawFile.SkipBytes);
        }
           

        int uDimension = rawFile.DimX * rawFile.DimY * rawFile.DimZ;
        voxelDataset.data = new int[uDimension];

        // Read the data/sample values
        for (int i = 0; i < uDimension; i++)
        {
            voxelDataset.data[i] = ReadDataValue(reader);
        }

    }

    private void FillVoxelDataset()
    {
        Debug.Log("Fill Voxel Dataset");
        voxelDataset.datasetName = Path.GetFileName(rawFile.FilePath);
        voxelDataset.filePath = rawFile.FilePath;
        voxelDataset.dimX = rawFile.DimX;
        voxelDataset.dimY = rawFile.DimY;
        voxelDataset.dimZ = rawFile.DimZ;
    }

    private int ReadDataValue(BinaryReader reader)
    {
        switch (rawFile.ContentFormat)
        {
            case DataContentFormat.Int8:
                {
                    sbyte dataval = reader.ReadSByte();
                    return (int)dataval;
                }
            case DataContentFormat.Int16:
                {
                    short dataval = reader.ReadInt16();
                    if (rawFile.Endianness == Endianness.BigEndian)
                    {
                        byte[] bytes = BitConverter.GetBytes(dataval);
                        Array.Reverse(bytes, 0, bytes.Length);
                        dataval = BitConverter.ToInt16(bytes, 0);
                    }
                    return (int)dataval;
                }
            case DataContentFormat.Int32:
                {
                    int dataval = reader.ReadInt32();
                    if (rawFile.Endianness == Endianness.BigEndian)
                    {
                        byte[] bytes = BitConverter.GetBytes(dataval);
                        Array.Reverse(bytes, 0, bytes.Length);
                        dataval = BitConverter.ToInt32(bytes, 0);
                    }
                    return (int)dataval;
                }
            case DataContentFormat.Int64: 
                {
                    long dataval = reader.ReadInt64();
                    if (rawFile.Endianness == Endianness.BigEndian)
                    {
                        byte[] bytes = BitConverter.GetBytes(dataval);
                        Array.Reverse(bytes, 0, bytes.Length);
                        dataval = BitConverter.ToInt64(bytes, 0);
                    }
                    return (int)dataval;
                }
            case DataContentFormat.Float32: 
                {
                    float dataval = reader.ReadSingle();
                    if (rawFile.Endianness == Endianness.BigEndian)
                    {
                        byte[] bytes = BitConverter.GetBytes(dataval);
                        Array.Reverse(bytes, 0, bytes.Length);
                        dataval = BitConverter.ToSingle(bytes, 0);
                    }
                    return (int)dataval;
                }
            case DataContentFormat.Float64: 
                {
                    double dataval = reader.ReadDouble();
                    if (rawFile.Endianness == Endianness.BigEndian)
                    {
                        byte[] bytes = BitConverter.GetBytes(dataval);
                        Array.Reverse(bytes, 0, bytes.Length);
                        dataval = BitConverter.ToDouble(bytes, 0);
                    }
                    return (int)dataval;
                }
            case DataContentFormat.Uint8:
                {
                    return (int)reader.ReadByte();
                }
            case DataContentFormat.Uint16:
                {
                    ushort dataval = reader.ReadUInt16();
                    if (rawFile.Endianness == Endianness.BigEndian)
                    {
                        byte[] bytes = BitConverter.GetBytes(dataval);
                        Array.Reverse(bytes, 0, bytes.Length);
                        dataval = BitConverter.ToUInt16(bytes, 0);
                    }
                    return (int)dataval;
                }
            case DataContentFormat.Uint32:
                {
                    uint dataval = reader.ReadUInt32();
                    if (rawFile.Endianness == Endianness.BigEndian)
                    {
                        byte[] bytes = BitConverter.GetBytes(dataval);
                        Array.Reverse(bytes, 0, bytes.Length);
                        dataval = BitConverter.ToUInt32(bytes, 0);
                    }
                    return (int)dataval;
                }
            case DataContentFormat.Uint64:
                {
                    ulong dataval = reader.ReadUInt64();
                    if (rawFile.Endianness == Endianness.BigEndian)
                    {
                        byte[] bytes = BitConverter.GetBytes(dataval);
                        Array.Reverse(bytes, 0, bytes.Length);
                        dataval = BitConverter.ToUInt64(bytes, 0);
                    }
                    return (int)dataval;
                }
            default:
                throw new NotImplementedException("Unimplemented data content format");
        }
    }

    private int GetSampleFormatSize(DataContentFormat format)
    {
        switch (format)
        {
            case DataContentFormat.Int8:
                return 1;
            case DataContentFormat.Uint8:
                return 1;
            case DataContentFormat.Int16:
                return 2;
            case DataContentFormat.Uint16:
                return 2;
            case DataContentFormat.Int32:
                return 4;
            case DataContentFormat.Uint32:
                return 4;
            case DataContentFormat.Int64:
                return 8;
            case DataContentFormat.Uint64:
                return 8;
            case DataContentFormat.Float32:
                return 4;
            case DataContentFormat.Float64:
                return 8;
        }
        throw new NotImplementedException();
    }

}