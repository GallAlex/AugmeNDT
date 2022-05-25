using System.IO;
using System;
using UnityEngine;

public enum DatasetType
{
    Unknown,
    Raw,
    Mhd,
    Csv,
    DICOM
}

/// <summary>
/// Concrete class for loading a file based on its extension and selects the appropriate loader (factory) for it.
/// </summary>
public class FileLoadingManager
{


    public void loadData(string filePath)
    {
        //Choose Loader here
    }

    public static DatasetType GetDatasetType(string filePath)
    {
        DatasetType datasetType;

        // Get .* extension
        string extension = Path.GetExtension(filePath);

        switch (extension)
        {
            case ".raw":
            case ".zraw":
                datasetType = DatasetType.Raw;
                break;
            case ".mhd":
                datasetType = DatasetType.Mhd;
                break;
            case ".csv":
                datasetType = DatasetType.Csv;
                break;
            case ".dicom":
            case ".dcm":
                datasetType = DatasetType.DICOM;
                break;
            default:
                datasetType = DatasetType.Unknown;
                break;
        }

        return datasetType;
    }

}
