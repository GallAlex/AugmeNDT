using System.Collections.Generic;
using UnityEngine;

public class ScaleOrdinal : Scale
{
    private List<string> domainNames;

    private double domainMin = 0.0d;
    private double domainMax = 10.0d;
            
    private double rangeMin = 0.0d;
    private double rangeMax = 1.0d;


    public ScaleOrdinal(List<double> domain, List<double> range, List<string> names) : base(domain, range)
    {
        domainMin = domain[0];
        domainMax = domain[1];

        if (domainMin != 0.0d || domainMax != names.Count - 1)
        {
            Debug.LogWarning("Domain of ordinal scale is not correct! Are some attribute names missing?");
        }

        rangeMin = range[0];
        rangeMax = range[1];
    }

    public override double GetScaledValue(double domainValue)
    {
        var domainRange = domainMax - domainMin;
        var newRange = rangeMax - rangeMin;

        if (domainRange == 0.0d || newRange == 0.0d)
        {
            Debug.LogWarning("Min/Max of domain or range are equal!");
            return 0.0d;
        }

        return (((domainValue - domainMin) * newRange) / domainRange) + rangeMin;
    }

    public override double GetDomainValue(double scaledValue)
    {
        var scaledRange = rangeMax - rangeMin;
        var newDomain = domainMax - domainMin;

        if (scaledRange == 0.0d || newDomain == 0.0d)
        {
            Debug.LogWarning("Min/Max of domain or range are equal!");
            return 0.0d;
        }

        return (((scaledValue - rangeMin) * newDomain) / scaledRange) + domainMin;
    }

    /// <summary>
    /// Returns the domain name for the given scaled value.
    /// Calculates the Domain Value first.
    /// </summary>
    /// <param name="scaledValue"></param>
    /// <returns></returns>
    public string GetDomainName(double scaledValue)
    {
        double domainVal = GetDomainValue(scaledValue);

        return domainNames[(int)domainVal];
    }

}
