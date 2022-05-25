using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class for various file types and their parameters
/// </summary>
public abstract class FileType
{

    private string filePath;

    #region Getter/Setter
    public string FilePath { get => filePath; set => filePath = value; }
    #endregion

    //public string fileName;
    //public string fileExtension;



}
