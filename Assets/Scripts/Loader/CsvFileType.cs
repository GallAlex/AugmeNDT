
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class CsvFileType : FileType
{
    // First row consists of the header
    private List<List<string>> csvValues;

    public CsvFileType(List<List<string>> csvValues)
    {
        this.csvValues = csvValues;
    }


}
