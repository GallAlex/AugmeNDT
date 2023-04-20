using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class CsvLoader : FileLoader
{
    private CsvFileType csvFile;
    private List<List<string>> csvValues;

    private Encoding encoding;
    private char splitChar = ',';
    private int skipRows = 4;
    private bool hasSpatialValues = true;

    private bool automaticDetection = true; //Detects if the file has spatial values or not and how many rows to skip

    public CsvLoader()
    {
        //Has to happen on main thread
        polyFiberDataset = ScriptableObject.CreateInstance<PolyFiberData>();
    }

    public CsvLoader(bool hasSpatialValues, int skipRows)
    {
        //Has to happen on main thread
        polyFiberDataset = ScriptableObject.CreateInstance<PolyFiberData>();

        automaticDetection = false;
        this.skipRows = skipRows;
        this.hasSpatialValues = hasSpatialValues;
    }

    public override async Task LoadData(string filePath)
    {
        await ReadCsv(filePath);

        polyFiberDataset.FillPolyFiberData(csvValues);

    }

    private async Task ReadCsv(string filePath)
    {
        Task<StreamReader> streamReaderTask = GetStreamReader(filePath);
        using var reader = await streamReaderTask;
        encoding = reader.CurrentEncoding;

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