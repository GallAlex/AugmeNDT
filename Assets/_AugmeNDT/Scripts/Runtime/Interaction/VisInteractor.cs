using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisInteractor
{
    public virtual void OnTouch(string ID)
    {
        Debug.Log("OnTouch");
    }

    public virtual void OnClick(string ID)
    {
        Debug.Log("OnClick");
    }


    /// <summary>
    /// Splits the id string at the underscore symbol and returns the number after the underscore
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public static int GetIDNumber(string ID)
    {
        int index = ID.IndexOf('_');
        string number = ID.Substring(index + 1); // +1 to skip the underscore

        return Convert.ToInt32(number);
    }

    /// <summary>
    /// Splits the id string at the underscore symbol and returns the name before the underscore
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public static string GetIDName(string ID)
    {
        int index = ID.IndexOf('_');
        string name = ID.Substring(0, index);

        return name;
    }
}
