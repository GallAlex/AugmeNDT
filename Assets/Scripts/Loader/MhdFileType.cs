using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MhdFileType : FileType
{
    private int headerSize = 0;
    private int nDims = 0;
    private bool binaryData = true;
    private bool byteOrderMSB = false;
    private bool compressedData = false;
    private int compressedDataSize = 0;
    private int[] transformMatrix;
    private int[] offset;
    private int[] centerOfRotation;
    private int[] elementSpacing;
    private int[] dimSize;
    private string elementDataFile = ""; // raw file name
    private string elementType = ""; // Format name

    #region Getter/Setter
    public int NDims { get => nDims; set => nDims = value; }
    public bool BinaryData { get => binaryData; set => binaryData = value; }
    public bool CompressedData { get => compressedData; set => compressedData = value; }
    public int CompressedDataSize { get => compressedDataSize; set => compressedDataSize = value; }
    public int[] TransformMatrix { get => transformMatrix; set => transformMatrix = value; }
    public int[] Offset { get => offset; set => offset = value; }
    public int[] CenterOfRotation { get => centerOfRotation; set => centerOfRotation = value; }
    public int[] ElementSpacing { get => elementSpacing; set => elementSpacing = value; }
    public int[] DimSize { get => dimSize; set => dimSize = value; }
    public string ElementDataFile { get => elementDataFile; set => elementDataFile = value; }
    public bool ByteOrderMSB { get => byteOrderMSB; set => byteOrderMSB = value; }
    public string ElementType { get => elementType; set => elementType = value; }
    public int HeaderSize { get => headerSize; set => headerSize = value; }
    #endregion

    public static DataContentFormat GetFormatByName(string format)
    {
        switch (format)       
        {
            case "MET_CHAR":
                return DataContentFormat.Int8;
            case "MET_UCHAR":
                return DataContentFormat.Uint8;
            case "MET_SHORT":
                return DataContentFormat.Int16;
            case "MET_USHORT":
                return DataContentFormat.Uint16;
            case "MET_INT":
                return DataContentFormat.Int32;
            case "MET_UINT":
                return DataContentFormat.Uint32;
            case "MET_LONG":
                return DataContentFormat.Int32;
            case "MET_LONG_LONG":
                return DataContentFormat.Int64;
            case "MET_ULONG":
                return DataContentFormat.Uint32;
            case "MET_ULONG_LONG":
                return DataContentFormat.Uint64;
            case "MET_FLOAT":
                return DataContentFormat.Float32;
            case "MET_DOUBLE":
                return DataContentFormat.Float64;
            default:
                return DataContentFormat.Uint32;
        }
    }

    public MhdFileType(string filePath, int headerSize = 0)
    {
        FilePath = filePath;
        this.headerSize = headerSize;
    }

    public override string ToString()
    {
        string values = "headerSize = " + HeaderSize.ToString() + "\n";
        values += "nDims = " + NDims.ToString() + "\n";
        values += "binaryData = " + BinaryData.ToString() + "\n";
        values += "compressedData = " + CompressedData.ToString() + "\n";
        values += "compressedDataSize = " + CompressedDataSize.ToString() + "\n";
        values += "transformMatrix = " + TransformMatrix.ToString() + "\n";
        values += "offset = " + Offset.ToString() + "\n";
        values += "centerOfRotation = " + CenterOfRotation.ToString() + "\n";
        values += "elementSpacing = " + ElementSpacing.ToString() + "\n";
        values += "dimSize = " + DimSize.ToString() + "\n";
        values += "elementDataFile = " + ElementDataFile.ToString() + "\n";
        values += "elementType = " + ElementType.ToString() + "\n";

        return base.ToString() + values;
    }
}
