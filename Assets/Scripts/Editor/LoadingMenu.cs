using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

public class LoadingMenu
{

    [MenuItem("AugmeNDT/Load dataset/Load raw dataset")]
    static void LoadDataset()
    {
        string file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
    }
}
