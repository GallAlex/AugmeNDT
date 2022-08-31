using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;


public class CsvLoader : FileLoader
{
    private CsvFileType csvFile;
    private List<List<string>> csvValues;

    private char splitChar = ',';
    private int skipRows = 4;

    public override async Task LoadData(string filePath)
    {
        Task<StreamReader> streamReaderTask = GetStreamReader(filePath);
        using var reader = await streamReaderTask;

        csvValues = new List<List<string>>();

        for (int skip = 0; skip < skipRows; skip++)
        {
            await reader.ReadLineAsync();
        }

        string headerLine = await reader.ReadLineAsync();
        string[] headerNames = headerLine.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);


        foreach (var name in headerNames)
        {
            csvValues.Add(new List<string> { name });
        }



        while (!reader.EndOfStream)
        {
            string line = await reader.ReadLineAsync();
            string[] values = line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);

            for (int feature = 0; feature < csvValues.Count; feature++)
            {
                csvValues[feature].Add(values[feature]);
            }

        }
    }

    public override void CreateDataset()
    {
        csvFile = new CsvFileType(csvValues);
        PrintCsv();
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

        Debug.Log("CSV Output: \n" + csvOutput);
    }

}