using System;
using System.Collections.Generic;
using UnityEngine;

public class ScaleNominal:Scale
{
    private List<string> scaledValueNames;

    private double domainMin = 0.0d;
    private double domainMax = 100.0d;

    private double rangeMin = 0.0d;
    private double rangeMax = 1.0d;

    public ScaleNominal(List<double> domain, List<double> range, List<string> names) : base(domain, range)
    {
        dataScaleType = DataScale.Nominal;

        if (domain[1] == names.Count || domain[0] != 0)
        {
            Debug.LogError("Not enough Names for the domain entered");
            return;
        }

        domainMin = domain[0];
        domainMax = domain[1];

        rangeMin = range[0];
        rangeMax = range[1];

        scaledValueNames = names;
    }

    public ScaleNominal(List<double> domain, List<string> names) : base(domain)
    {
        dataScaleType = DataScale.Nominal;

        if (domain[1] == names.Count || domain[0] != 0)
        {
            Debug.LogError("Not enough Names for the domain entered");
            return;
        }

        domainMin = domain[0];
        domainMax = domain[1];

        rangeMin = range[0];
        rangeMax = range[1];

        scaledValueNames = names;
    }


    public override double GetScaledValue(double domainValue)
    {
        return domainValue * ((rangeMax-rangeMin) / domainMax);
    }

    public override double GetDomainValue(double scaledValue)
    {
        //TODO: Rework to not use double (with int rounding problems obsolete)
        return Math.Round((domainMax * scaledValue) / (rangeMax - rangeMin));
    }


    public override string GetScaledValueName(double domainValue)
    {
        //Covert domainValue to int
        int domainValueToInt = Convert.ToInt32(domainValue);
        return scaledValueNames[domainValueToInt];
    }
    
}
