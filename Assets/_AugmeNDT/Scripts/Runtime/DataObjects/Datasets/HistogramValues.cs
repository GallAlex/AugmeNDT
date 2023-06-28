using System;
using MathNet.Numerics.Statistics;

namespace AugmeNDT{
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

        public double[] GetBinFrequencies()
        {
            double[] binCounts = new double[hist.BucketCount];

            for (int i = 0; i < hist.BucketCount; i++)
            {
                binCounts[i] = GetBinFrequency(i);
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

        /// <summary>
        /// Run through all bins and create a string array with the lower (exclusive) and upper bound (inclusive) of each bin
        /// </summary>
        /// <returns></returns>
        public string[] GetBinIntervals()
        {
            int binCount = GetBinCount();
            string[] binBoundIntervals = new string[binCount];
            int[] binNumbers = new int[binCount];                 // Array numbering all bins from 0 to n

            for (int i = 0; i < binCount; i++)
            {
                if (i == 0)
                {
                    binBoundIntervals[i] = GetLowerBinBound(i).ToString("[ 0.00") + " - " + GetUpperBinBound(i).ToString("0.00 ]");
                }
                else
                {
                    binBoundIntervals[i] = GetLowerBinBound(i).ToString("] 0.00") + " - " + GetUpperBinBound(i).ToString("0.00 ]");
                }
                binNumbers[i] = i;
            }

            return binBoundIntervals;
        }
    }
}
