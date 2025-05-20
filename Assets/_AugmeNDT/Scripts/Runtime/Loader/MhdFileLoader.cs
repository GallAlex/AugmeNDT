// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT{
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

        public override async Task LoadData(string filePath)
        {
            await ReadMetaInfo(filePath);

            //Check if compressed Format
            if (mhdFile.CompressedData == true) 
            {
                Debug.LogError("The compressed formats are currently not supported");
                return;

            }

            await base.LoadData(rawFile.FilePath);
            Debug.Log(this.ToString());
        }

        public async Task ReadMetaInfo(string filePath)
        {
            List<string> stringList = new List<string>();
            //string[] lines = File.ReadAllLines(filePath);
            string rawFilePath = "";

            Task<StreamReader> streamReaderTask = GetStreamReader(filePath);
            using StreamReader sr = await streamReaderTask;//.ConfigureAwait(false);

            string tempLine;
            // Read and display lines from the file until the end of the file is reached.
            while ((tempLine = await sr.ReadLineAsync()) != null)
            {
                stringList.Add(tempLine);
            }
            string[] lines = stringList.ToArray();

            foreach (string line in lines)

            {
                string[] parts = line.Split('=');
                if (parts.Length != 2) continue;

                string name = parts[0].Trim(' '); //Remove spaces
                string value = parts[1].Trim(' '); //Remove spaces
                value = value.ToLower(); //because of boolean value names

                if (name == "NDims")
                {
                    int temp = 0;
                    Int32.TryParse(value, out temp);
                    mhdFile.NDims = temp;
                }
                else if (name == "HeaderSize")
                {
                    int temp = 0;
                    Int32.TryParse(value, out temp);
                    mhdFile.HeaderSize = temp;
                }
                else if (name == "BinaryData")
                {
                    bool temp = true;
                    Boolean.TryParse(value, out temp);
                    mhdFile.BinaryData = temp;
                }
                else if (name == "BinaryDataByteOrderMSB")
                {
                    int temp = 0;
                    Int32.TryParse(value, out temp);
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
                else if (name == "ElementSpacing") 
                {
                    float[] numbers = value.Split(' ').Select(x => float.Parse(x, CultureInfo.InvariantCulture)).ToArray();
                    Debug.Log("Element Spacing: " + numbers[0] + " " + numbers[1] + " " + numbers[2]);
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
                }
            
            }

            //Check path to raw file
            rawFilePath = await CheckRawFilePath(filePath, mhdFile.ElementDataFile);

            CreateRawFileType(rawFilePath);
        }

        private void CreateRawFileType(string rawFilePath)
        {
            //Read Info and store in Raw File with path to raw file
            rawFile = new RawFileType(rawFilePath, mhdFile.DimSize[0], mhdFile.DimSize[1], mhdFile.DimSize[2], mhdFile.ElementSpacing[0], mhdFile.ElementSpacing[1], mhdFile.ElementSpacing[2], MhdFileType.GetFormatByName(mhdFile.ElementType), (Endianness)mhdFile.ByteOrderMSB, mhdFile.HeaderSize);

        }

        /// <summary>
        /// Method check the Path of the raw file
        /// </summary>
        /// <param name="mhdFilePath"></param>
        /// <param name="rawFileName"></param>
        /// <returns>String with path of .raw file</returns>
        private async Task<String> CheckRawFilePath(string mhdFilePath, string rawFileName)
        {
            string path = "";
            //If raw file name is in mhd the look in current folder...
            if(rawFileName != "")
            {
                path = Path.Join(Path.GetDirectoryName(mhdFilePath), rawFileName);
            }
            //... or try replacing .mhd with .raw
            else 
            { 
                path = mhdFilePath.Replace(".mhd", ".raw");
                mhdFile.ElementDataFile = Path.GetFileName(path);
            }

            //Check if file exists for UWP and .NET
            if (!await CheckIfFileExists(path))
            {
                Debug.LogError("Raw File on Path " + path + " does not exist");
                return null;
            }

            return path;
        }

        public override string ToString()
        {
            string values = mhdFile.ToString() + "\n";
            values += rawFile.ToString() + "\n";
        
            return base.ToString() + values;
        }

    }
}
