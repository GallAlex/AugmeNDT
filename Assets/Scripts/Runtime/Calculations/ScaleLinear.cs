using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScaleLinear: Scale
{

    private float domainMin = 0.0f;
    private float domainMax = 100.0f;

    private float rangeMin = 0.0f;
    private float rangeMax = 1.0f;


    public ScaleLinear(List<float> domain, List<float> range) : base(domain, range)
    {
        domainMin = domain[0];
        domainMax = domain[1];

        rangeMin = range[0];
        rangeMax = range[1];
    }

    public override float GetScaledValue(float domainValue)
    {
        var domainRange = domainMax - domainMin;
        var newRange = rangeMax - rangeMin;

        if (domainRange == 0.0f || newRange == 0.0f)
        {
            Debug.LogError("Min/Max of domain or range are equal!");
            return 0.0f;
        }

        return (((domainValue - domainMin) * newRange) / domainRange) + rangeMin;
    }

    public override float GetDomainValue(float scaledValue)
    {
        var scaledRange = rangeMax - rangeMin;
        var newDomain = domainMax - domainMin;

        if (scaledRange == 0.0f || newDomain == 0.0f)
        {
            Debug.LogError("Min/Max of domain or range are equal!");
            return 0.0f;
        }

        return (((scaledValue - rangeMin) * newDomain) / scaledRange) + domainMin;
    }

}

