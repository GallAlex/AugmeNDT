using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;


namespace AugmeNDT
{
    public class KernelDensityEstimation
    {
        private List<double[]> kdeResult;   // Stores the x and y result of the KDE


        /// <summary>
        /// Calculates the Kernel Density Estimation of the given data. Returns a list with the x and y result of the KDE. 
        /// The numberOfResults defines the number of points for which the KDE is calculated. The points selected are sampled on a linear spaces between min and max of the data. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bandwidth"></param>
        /// <param name="numberOfResults"></param>
        /// <returns></returns>
        public List<double[]> CalculateKernelDensity(double[] data, double bandwidth, int numberOfResults)
        {
            double[] kdeXResult = new double[numberOfResults];
            double[] kdeYResult = new double[numberOfResults];

            double[] spacedPoints = Generate.LinearSpaced(numberOfResults, data.Min(), data.Max());

            for (int i = 0; i < numberOfResults; i++)
            {
                kdeYResult[i] = KernelDensity.EstimateGaussian(spacedPoints[i], bandwidth, data);
                kdeXResult[i] = spacedPoints[i];
            }


            kdeResult = new List<double[]>
            {
                kdeXResult,
                kdeYResult
            };


            return kdeResult;
        }


}

}
