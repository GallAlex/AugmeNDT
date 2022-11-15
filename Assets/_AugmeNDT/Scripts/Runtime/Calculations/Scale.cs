using System.Collections.Generic;
using UnityEngine;

public class Scale
{
    public enum DataScale
    {
        Linear,
        Ordinal,
    }

    public List<float> domain;  // Data Range
    public List<float> range;   // Existing Range


    public Scale(List<float> domain, List<float> range)
    {
        if ((domain == null || domain.Count > 2) || (range == null || range.Count > 2))
        {
            Debug.LogError("Format of domain/range is not correct");
            return;
        }

        this.domain = domain;
        this.range = range;
    }

    /// <summary>
    ///  Method accepts input between domain min/max and maps it to output between range min/max.
    /// </summary>
    /// <param name="domainValue">Value of datapoint</param>
    public virtual float GetScaledValue(float domainValue)
    {
        return 0.0f;
    }

    /// <summary>
    ///  Method accepts input between range min/max and maps it to output between domain min/max.
    /// </summary>
    /// <param name="scaledValue">Value of scaled point</param>
    public virtual float GetDomainValue(float scaledValue)
    {
        return 0.0f;
    }
}
