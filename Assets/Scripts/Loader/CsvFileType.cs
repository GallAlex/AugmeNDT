
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class CsvFileType : FileType
{
    // First row consists of the header
    private List<List<string>> csvValues;

    #region Getter/Setter
    public List<List<string>> CsvValues
    {
        get => csvValues;
        set => csvValues = value;
    }
    #endregion


    public CsvFileType(List<List<string>> csvValues)
    {
        this.csvValues = csvValues;
    }


}
