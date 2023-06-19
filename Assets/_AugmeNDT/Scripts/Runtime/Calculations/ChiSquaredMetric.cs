using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;

namespace AugmeNDT
{

    /// <summary>
    /// Class implements the ChiSquaredMetric to compare the similarity of distributions
    /// </summary>
    public class ChiSquaredMetric
    {

        /// <summary>
        /// The chi-square goodness-of-fit test is applied to binned data and is testing the similarity between two distributions
        /// https://www.itl.nist.gov/div898/handbook/eda/section3/eda35f.htm
        /// </summary>
        /// <param name="dist1"></param>
        /// <param name="dist2"></param>
        /// <returns></returns>
        public static double CalculateChiSquaredMetric(double[] dist1, double[] dist2)
        {
            int binCount = DistributionCalc.GetNumberOfBins(Math.Max(dist1.Length, dist2.Length));
            double chiSquare = 0.0;

            Histogram hist1 = new Histogram(dist1, binCount);
            Histogram hist2 = new Histogram(dist2, binCount);

            for (int i = 0; i < binCount; i++)
            {//for each bin

                if (hist2[i].Count != 0)
                {
                    int diff = ((int)hist1[i].Count) - ((int)hist2[i].Count);
                    chiSquare += Math.Pow(((double)diff), 2) / (hist2[i].Count);
                }
                else
                {
                    int diff = ((int)hist1[i].Count);
                    chiSquare += Math.Pow(((double)diff), 2);
                }
            }

            return chiSquare;
        }

        //public static double CalculateChiSquaredMetric(double[] dist1, double[] dist2)
        //{
        //    if (dist1.Length != dist2.Length) { throw new ArgumentException("Distributions must have the same length"); }

        //    return GoodnessOfFit.RSquared(dist1, dist2);
        //}

    }
}
