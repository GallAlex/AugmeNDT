using System.Collections.Generic;
using UnityEngine;

public class Scale
{
    public List<float> domain; // Data Range
    public List<float> range; // Existing Range


    public Scale(List<float> domain, List<float> range)
    {
        this.domain = domain;
        this.domain = range;
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
