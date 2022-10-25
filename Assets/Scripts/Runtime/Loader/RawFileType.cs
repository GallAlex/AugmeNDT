
public enum DataContentFormat
{
    Int8, //MET_CHAR
    Uint8, //MET_UCHAR
    Int16, //MET_SHORT
    Uint16, //MET_USHORT
    Int32, //MET_INT
    Uint32, //MET_UINT
    Int64, //MET_LONG_LONG
    Uint64, //MET_ULONG_LONG
    Float32, //MET_FLOAT
    Float64 //MET_DOUBLE
}

public enum Endianness
{
    LittleEndian,
    BigEndian
}


public class RawFileType : FileType
{

    /*Values for loading the raw files */
    private int dimX;
    private int dimY;
    private int dimZ;
    private DataContentFormat contentFormat;
    private Endianness endianness;
    private int skipBytes;

    #region Getter/Setter

    public int DimX { get => dimX; set => dimX = value; }
    public int DimY { get => dimY; set => dimY = value; }
    public int DimZ { get => dimZ; set => dimZ = value; }
    public DataContentFormat ContentFormat { get => contentFormat; set => contentFormat = value; }
    public Endianness Endianness { get => endianness; set => endianness = value; }
    public int SkipBytes { get => skipBytes; set => skipBytes = value; }

    #endregion

    // Only for derived class with meta info
    protected RawFileType(string filePath) 
    { 
    
    }

    public RawFileType(string filePath, int dimX, int dimY, int dimZ, DataContentFormat contentFormat, Endianness endianness, int skipBytes)
    {
        FilePath = filePath;
        DimX = dimX;
        DimY = dimY;
        DimZ = dimZ;
        ContentFormat = contentFormat;
        Endianness = endianness;
        SkipBytes = skipBytes;
    }

    public override string ToString()
    {

        string values = "DimX = " + DimX.ToString() + "\n";
        values += "DimY = " + DimY.ToString() + "\n";
        values += "DimZ = " + DimZ.ToString() + "\n";
        values += "ContentFormat = " + contentFormat.ToString() + "\n";
        values += "Endianness = " + Endianness.ToString() + "\n";
        values += "skipBytes = " + SkipBytes.ToString() + "\n";

        return base.ToString() + values;

    }
}
