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
    }

    public void readMetaInfo(string filePath)
    {
        if (!File.Exists(filePath)) return;

        string[] lines = File.ReadAllLines(filePath);

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
                Debug.Log("Ndims: " + mhdFile.NDims);
            }
            else if (name == "BinaryData")
            {
                bool temp = true;
                Boolean.TryParse(value, out temp);
                mhdFile.BinaryData = temp;
                Debug.Log("BinaryData: " + mhdFile.BinaryData);
            }
            else if (name == "BinaryDataByteOrderMSB")
            {
                bool temp = false;
                Boolean.TryParse(value, out temp);
                if(temp) mhdFile.Endianness = Endianness.BigEndian;
                else mhdFile.Endianness = Endianness.LittleEndian;

                Debug.Log("BinaryDataByteOrderMSB: " + mhdFile.Endianness);
            }
            else if (name == "CompressedData")
            {
                bool temp = false;
                Boolean.TryParse(value, out temp);
                mhdFile.CompressedData = temp;
                Debug.Log("CompressedData: " + mhdFile.CompressedData);
            }
            else if (name == "CompressedDataSize")
            {
                int temp = 0;
                Int32.TryParse(value, out temp);
                mhdFile.CompressedDataSize = temp;
                Debug.Log("CompressedDataSize: " + mhdFile.CompressedDataSize);
            }
            else if (name == "TransformMatrix")
            {
                Char[] charNumbers = value.Where(Char.IsDigit).ToArray();
                int[] numbers = charNumbers.Select(x => Convert.ToInt32(x.ToString())).ToArray();
                mhdFile.TransformMatrix = numbers;

                Debug.Log("TransformMatrix: " + mhdFile.TransformMatrix.ToString());
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
                mhdFile.ContentFormat = MhdFileType.GetFormatByName(value);
                Debug.Log("ElementType: " + mhdFile.ContentFormat);
            }
            else if (name == "ElementDataFile")
            {
                mhdFile.ElementDataFile = value;
                Debug.Log("ElementDataFile: " + mhdFile.ElementDataFile);
            }
            
        }

        // if path from mhd is missing or wrong
        if (!File.Exists(mhdFile.ElementDataFile))
        {
            mhdFile.ElementDataFile = replaceTargetPath(filePath);
        }

        //Read Info and store in Raw File with path to raw file
        rawFile = new RawFileType(mhdFile.ElementDataFile, mhdFile.DimSize[0], mhdFile.DimSize[1], mhdFile.DimSize[2], mhdFile.ContentFormat, mhdFile.Endianness, mhdFile.SkipBytes);
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
}
