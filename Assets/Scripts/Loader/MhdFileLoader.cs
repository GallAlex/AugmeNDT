using System;
using System.IO;
using System.Linq;
using UnityEngine;



public class MhdFileLoader : RawFileLoader
{
    private MhdFileType mhdFile;

    #region Getter/Setter
    public MhdFileType MhdFile { get => mhdFile; set => mhdFile = value; }
    #endregion

    public MhdFileLoader(string filePath) : base(filePath)
    {
        mhdFile = new MhdFileType(filePath);
    }

    public override void loadData(string filePath)
    {
        readMetaInfo(filePath);
        
        //Check if compressed Format
        if(mhdFile.CompressedData == true) 
        {
            Debug.LogError("The compressed formats are currently not supported");
            return;

        }

        base.loadData(rawFile.FilePath);
        Debug.Log(this.ToString());
    }

    public void readMetaInfo(string filePath)
    {
        if (!File.Exists(filePath)) return;

        string[] lines = File.ReadAllLines(filePath);
        string rawFilePath = "";

        foreach (string line in lines)
        {
            string[] parts = line.Split('=');
            if (parts.Length != 2) continue;

            string name = parts[0].Trim(' '); //Remove spaces
            string value = parts[1].Trim(' '); //Remove spaces
            value.ToLower(); //because of boolean value names

            if (name == "NDims")
            {
                int temp = 0;
                Int32.TryParse(value, out temp);
                mhdFile.NDims = temp;
            }
            else if (name == "BinaryData")
            {
                bool temp = true;
                Boolean.TryParse(value, out temp);
                mhdFile.BinaryData = temp;
            }
            else if (name == "BinaryDataByteOrderMSB")
            {
                bool temp = false;
                Boolean.TryParse(value, out temp);
                mhdFile.ByteOrderMSB = temp;
            }
            else if (name == "CompressedData")
            {
                bool temp = false;
                Boolean.TryParse(value, out temp);
                mhdFile.CompressedData = temp;
            }
            else if (name == "CompressedDataSize")
            {
                int temp = 0;
                Int32.TryParse(value, out temp);
                mhdFile.CompressedDataSize = temp;
            }
            else if (name == "TransformMatrix")
            {
                Char[] charNumbers = value.Where(Char.IsDigit).ToArray();
                int[] numbers = charNumbers.Select(x => Convert.ToInt32(x.ToString())).ToArray();
                mhdFile.TransformMatrix = numbers;
            }
            else if (name == "Offset")
            {
                Char[] charNumbers = value.Where(Char.IsDigit).ToArray();
                int[] numbers = charNumbers.Select(x => Convert.ToInt32(x.ToString())).ToArray();

                mhdFile.Offset = numbers;
            }
            else if (name == "CenterOfRotation")
            {
                Char[] charNumbers = value.Where(Char.IsDigit).ToArray();
                int[] numbers = charNumbers.Select(x => Convert.ToInt32(x.ToString())).ToArray();
                mhdFile.CenterOfRotation = numbers;

            }
            else if (name == "ElementSpacing") // for now skips AnatomicalOrientation
            {
                Char[] charNumbers = value.Where(Char.IsDigit).ToArray();
                int[] numbers = charNumbers.Select(x => Convert.ToInt32(x.ToString())).ToArray();
                mhdFile.ElementSpacing = numbers;

            }
            else if (name == "DimSize")
            {
                int[] numbers = value.Split(' ').Select(x => Convert.ToInt32(x.ToString())).ToArray();
                mhdFile.DimSize = numbers;

            }
            else if (name == "ElementType")
            {
                mhdFile.ElementType = value;
            }
            else if (name == "ElementDataFile")
            {
                mhdFile.ElementDataFile = value;
                rawFilePath = Path.GetDirectoryName(filePath) + "/" + value; //Same Path with raw file name 
            }
            
        }

        // if raw name from mhd is missing or wrong
        if (!File.Exists(rawFilePath))
        {
            rawFilePath = replaceTargetPath(filePath); //get whole Path
            mhdFile.ElementDataFile = Path.GetFileName(rawFilePath); // Store raw file name
        }

        createRawFileType(rawFilePath);
}

    private void createRawFileType(string rawFilePath)
    {
        Endianness endianness;
        if (mhdFile.ByteOrderMSB) endianness = Endianness.BigEndian;
        else endianness = Endianness.LittleEndian;

        //Read Info and store in Raw File with path to raw file
        rawFile = new RawFileType(rawFilePath, mhdFile.DimSize[0], mhdFile.DimSize[1], mhdFile.DimSize[2], MhdFileType.GetFormatByName(mhdFile.ElementType), endianness, mhdFile.HeaderSize);

    }

    /// <summary>
    /// Method replaces the mhd Path with the raw extension (zraw, raw)
    /// </summary>
    /// <param name="mhdFilePath"></param>
    /// <returns>String with path of .zraw or .raw</returns>
    private string replaceTargetPath(string mhdFilePath)
    {
        string targetPath;
        //Get path to raw data
        targetPath = mhdFilePath.Replace(".mhd", ".raw");
        
        if (!File.Exists(targetPath))
        {
            targetPath = mhdFilePath.Replace(".mhd", ".zraw");
        }

        return targetPath;
    }

    public override string ToString()
    {
        string values = mhdFile.ToString() + "\n";
        values += rawFile.ToString() + "\n";
        
        return base.ToString() + values;
    }

}
