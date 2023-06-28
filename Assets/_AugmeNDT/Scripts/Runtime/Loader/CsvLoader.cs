using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT{
    public class CsvLoader : FileLoader
    {
        private CsvFileType csvFile;
        private List<List<string>> csvValues;

        private Encoding encoding;
        private char splitChar = ',';
        private int skipRows = IAbstractData.SkipRows;
        private bool hasSpatialValues = false;

        private bool automaticDetectionSuccesful = false; // File type could be detected automatically

        public CsvLoader()
        {
            //Has to happen on main thread
            polyFiberDataset = ScriptableObject.CreateInstance<PolyFiberData>();
        }

        /// <summary>
        /// Constructor with rows to skip at the beginning of the file
        /// </summary>
        /// <param name="hasSpatialValues"></param>
        /// <param name="skipRows"></param>
        public CsvLoader(bool hasSpatialValues, int skipRows)
        {
            //Has to happen on main thread
            polyFiberDataset = ScriptableObject.CreateInstance<PolyFiberData>();

            this.skipRows = skipRows;
            this.hasSpatialValues = hasSpatialValues;
        }

        public override async Task LoadData(string filePath)
        {
            await ReadCsv(filePath);

            this.filePath = filePath;
            fileName = Path.GetFileNameWithoutExtension(filePath);

            datasetType = FileLoadingManager.DatasetType.Secondary;

            if (hasSpatialValues)
            {
                secondaryDataType = ISecondaryData.SecondaryDataType.Spatial;
                polyFiberDataset.FillPolyFiberData(csvValues);
                polyFiberDataset.SetDatasetName(fileName);
            }
            else
            {
                secondaryDataType = ISecondaryData.SecondaryDataType.Abstract;
                abstractDataset = csvFile.GetDataSet();
                abstractDataset.SetDatasetName(fileName);
            }
        

        }

        private async Task ReadCsv(string filePath)
        {
            Task<StreamReader> streamReaderTask = GetStreamReader(filePath);
            using var reader = await streamReaderTask;
            encoding = reader.CurrentEncoding;

            //Get Meta Infos in first line
            string metaLine = await reader.ReadLineAsync();
            string[] metaInfo = metaLine.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
            ProcessMetaInfos(metaInfo);
            //TODO: If no meta info is provided then header might be alredy read!! No additonal ReadLineAsync needed and skipRow could be -1!

            csvValues = new List<List<string>>();

            for (int skip = 0; skip < skipRows; skip++)
            {
                await reader.ReadLineAsync();
            }

            string headerLine = await reader.ReadLineAsync();
            string[] headerNames = headerLine.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);


            if (headerNames == null || headerNames.Length < 1)
            {
                Debug.LogError("CSV File Header row is empty");
            }

            // Get header names from first row
            foreach (var name in headerNames)
            {
                var trimmedName = name.Trim(' '); //Remove leading and trailing spaces
                csvValues.Add(new List<string> { trimmedName });
            }


            // Get next rows and assign value to specific column (header)
            while (!reader.EndOfStream)
            {
                string line = await reader.ReadLineAsync();
                string[] values = line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);

                if (values == null || values.Length < 1)
                {
                    Debug.LogError("CSV File has no value row");
                }

                for (int feature = 0; feature < csvValues.Count; feature++)
                {
                    csvValues[feature].Add(values[feature]);
                }

            }

            Debug.Log("CSV File loaded");
            
            csvFile = new CsvFileType(csvValues);
            //PrintCsv();
        }

        /// <summary>
        /// Checks if the file has abstract or spatial values and sets how many rows to skip
        /// </summary>
        /// <param name="metaInfo"></param>
        private void ProcessMetaInfos(string[] metaInfo)
        {
            if (metaInfo.Length < 1)
            {
                Debug.LogError("First line is empty");
                return;
            }
            switch (metaInfo[0])
            {
                case ISecondaryData.AbstractDataIdentifier:
                    automaticDetectionSuccesful = true;
                    hasSpatialValues = false;
                    skipRows = IAbstractData.SkipRows;
                    Debug.Log("Abstract Dataset detected");
                    break;
                case ISecondaryData.SpatialDataIdentifier:
                    automaticDetectionSuccesful = true;
                    hasSpatialValues = true;
                    skipRows = ISpatialData.SkipRows;
                    //TODO: Check Type of spatial data
                    Debug.Log("Spatial Dataset ["+ metaInfo[1] + "] detected");
                    break;
                default:
                    automaticDetectionSuccesful = false;
                    Debug.LogError("CSV File has no valid meta information. Use abstract dataset as default");
                    break;
            }
        }

        /// <summary>
        /// Prints the csv with the header as as first row
        /// </summary>
        public void PrintCsv()
        {
            string csvOutput = "";

            for (int rowIndex = 0; rowIndex < csvValues[0].Count; rowIndex++)
            {
                csvOutput += "| ";

                for (int columnsIndex = 0; columnsIndex < csvValues.Count; columnsIndex++)
                {
                    csvOutput += csvValues[columnsIndex][rowIndex] + "\t | ";
                }

                csvOutput += " \n";
            }

            Debug.Log("CSV Output [" + encoding + "]: \n" + csvOutput);
        }

    }
}