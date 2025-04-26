using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The class stores critical points calculated by TTK
/// </summary>
public class CriticalPointDataset
{
    public int ID;
    public int Type; // type of the critical point (source,sink,saddle-1,saddle-2 etc.)
    public Vector3 Position;
    public string TypeName;

    public CriticalPointDataset(int id, int type, Vector3 position)
    {
        ID = id;
        Type = type;
        Position = position;
        TypeName = typeName[type];
    }

    private List<string> typeName = new List<string>()
        {
            {"Sink" },      //0
            {"1-Saddle"},   //1
            {"2-Saddle"},   //2
            {"Source" },    //3
        };
}
