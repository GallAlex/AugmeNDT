using MathNet.Numerics.Statistics;
using System;
using UnityEngine;

public class HistogramValues
{
    private Histogram hist;


    public HistogramValues(double[] data)
    {
        hist = new Histogram(data, DistributionCalc.GetNumberOfBins(data.Length));
    }

    public int GetBinCount()
    {
        return hist.BucketCount;
    }

    public double GetBinWidth()
    {
        return hist[0].Width;
    }

    public double[] GetMinMaxFrequency()
    {
        double min = Double.MaxValue;
        double max = Double.MinValue;
        for (int i = 0; i < hist.BucketCount; i++)
        {
            double frequency = GetBinFrequency(i);
            if (frequency < min)
            {
                min = frequency;
            }
            if (frequency > max)
            {
                max = frequency;
            }
        }
        return new[] { min, max };
    }

    public int[] GetBinFrequencies()
    {
        int[] binCounts = new int[hist.BucketCount];

        for (int i = 0; i < hist.BucketCount; i++)
        {
            binCounts[i] = (int)GetBinFrequency(i);
        }

        return binCounts;
    }

    public double GetBinFrequency(int bin)
    {
        return hist[bin].Count;
    }

    public double[] GetLowerUpperBoundValue()
    {
        return new[] { GetLowerBound(), GetUpperBound() };
    }
    public double[] GetAllLowerBoundValues()
    {
        double[] lowerBound = new double[hist.BucketCount];

        for (int i = 0; i < hist.BucketCount; i++)
        {
            lowerBound[i] = GetLowerBinBound(i);
        }

        return lowerBound;
    }

    public double[] GetAllUpperBoundValues()
    {
        double[] lowerBound = new double[hist.BucketCount];

        for (int i = 0; i < hist.BucketCount; i++)
        {
            lowerBound[i] = GetUpperBinBound(i);
        }

        return lowerBound;
    }

    public double GetLowerBound()
    {
        return hist.LowerBound;
    }

    public double GetLowerBinBound(int bin)
    {
        return hist[bin].LowerBound;
    }

    public double GetUpperBound()
    {
        return hist.UpperBound;
    }

    public double GetUpperBinBound(int bin)
    {
        return hist[bin].UpperBound;
    }
}
