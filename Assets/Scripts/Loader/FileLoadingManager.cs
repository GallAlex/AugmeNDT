//using UnityEditor;
using System.IO;
using System;
using UnityEngine;

public enum DatasetType
{
    Raw,
    Mhd,
    Csv,
    DICOM,
    Unknown
}

/// <summary>
/// Concrete class for loading a file based on its extension and selects the appropriate loader (factory) for it.
/// </summary>
public class FileLoadingManager
{
    private string filePath;
    private FileLoader loaderFactory;

    public void loadData()
    {
        //this.filePath = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "raw, zraw, mhd, dicom, dcm");
        DatasetType fileTyp = GetDatasetType(filePath);

        //Choose Loader here
        switch (fileTyp)
        {
            case DatasetType.Raw:
                //open RawFileLoaderWindow
                //loaderFactory = new RawFileLoader(filePath, dimX, dimY, dimZ, contentFormat, endianness, skipBytes);
                break;
            case DatasetType.Mhd:
                loaderFactory = new MhdFileLoader(filePath);
                break;
            case DatasetType.Csv:
                throw new NotImplementedException(fileTyp.ToString() + " extension is currently not supported");
            case DatasetType.DICOM:
                throw new NotImplementedException(fileTyp.ToString() + " extension is currently not supported");
            case DatasetType.Unknown:
            default:
                return;
        }

    }


    /// <summary>
    /// Returns the detected extension type of the file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
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
                throw new NotImplementedException("Data extension format [" + extension  + "] not supported");
        }

        return datasetType;
    }

}
