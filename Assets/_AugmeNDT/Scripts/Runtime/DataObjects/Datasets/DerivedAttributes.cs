using System;
using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// This Class represents the derived attributes of a numerical attribute of a data set.
    /// </summary>
    public class DerivedAttributes
    {
        /// <summary>
        /// Possible Calcuation on numerical Attributes (double[])
        /// </summary>
        public enum DerivedAttributeCalc
        {
            // DistributionValues
            LargestElement,
            SmallestElement,
            Median,
            Mean,
            Variance,
            StdDev,
            Iqr,
            UpperQuartile,
            LowerQuartile,
            Kurtosis,
            Skewness,
            Modality,
            //Other
            NumberOfDerivedAttributes
        }

        public List<string> derivedAttributeNames;                  // Name of derived parameters 
        public int derivedAttributesCount;                          // Number of derived parameters

        // External classes for calculations
        private List<DistributionValues> distributionValues;    // Stores the distribution values of the numerical attribute [0] and the normalized numerical attribute values [1]

        //TODO: Datastructure which stores min/max?

        public DerivedAttributes(double[] numericalValues, double[] numericalValuesNorm)
        {
            InitializeDerivedAttributes();
            CalculateDerivedValues(numericalValues, numericalValuesNorm);
        }

        /// <summary>
        /// Returns all derived attribute values for that attribute in an array
        /// </summary>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public double[] GetDerivedAttributeValues(bool normalized)
        {
            double[] aggregatedValues = new double[derivedAttributesCount];

            for (int i = 0; i < derivedAttributesCount; i++)
            {
                aggregatedValues[i] = GetDerivedValue((DerivedAttributeCalc)i, normalized);
            }

            return aggregatedValues;
        }

        /// <summary>
        /// Return the value of a specific derived attribute
        /// Value can be calculated on the normalized or unnormalized numerical attribute values
        /// </summary>
        /// <param name="derivedAttr"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public double GetDerivedValue(DerivedAttributeCalc derivedAttr, bool normalized)
        {
            //If normalized use index 1
            int norm = normalized ? 1 : 0;

            switch ((DerivedAttributeCalc)derivedAttr)
            {
                // DistributionValues
                case DerivedAttributeCalc.LargestElement:
                    return distributionValues[norm].LargestElement;
                case DerivedAttributeCalc.SmallestElement:
                    return distributionValues[norm].SmallestElement;
                case DerivedAttributeCalc.Median:
                    return distributionValues[norm].Median;
                case DerivedAttributeCalc.Mean:
                    return distributionValues[norm].Mean;
                case DerivedAttributeCalc.Variance:
                    return distributionValues[norm].Variance;
                case DerivedAttributeCalc.StdDev:
                    return distributionValues[norm].StdDev;
                case DerivedAttributeCalc.Iqr:
                    return distributionValues[norm].Iqr;
                case DerivedAttributeCalc.UpperQuartile:
                    return distributionValues[norm].UpperQuartile;
                case DerivedAttributeCalc.LowerQuartile:
                    return distributionValues[norm].LowerQuartile;
                case DerivedAttributeCalc.Kurtosis:
                    return distributionValues[norm].Kurtosis;
                case DerivedAttributeCalc.Skewness:
                    return distributionValues[norm].Skewness;
                case DerivedAttributeCalc.Modality:
                    return distributionValues[norm].Modality;
                default:
                    throw new NotImplementedException();
            }
        }

        public static string[] GetDerivedAttributeNames()
        {
            string[] names = new string[(int)DerivedAttributeCalc.NumberOfDerivedAttributes];

            // Save the strings from the Enum in the derivedAttributesParams List
            for (int i = 0; i < (int)DerivedAttributeCalc.NumberOfDerivedAttributes; i++)
            {
                names[i] = ((DerivedAttributeCalc)i).ToString();
            }

            return names;
        }

        private void InitializeDerivedAttributes()
        {
            // How many derived attributes are there?
            derivedAttributesCount = (int)DerivedAttributeCalc.NumberOfDerivedAttributes;
            derivedAttributeNames = new List<string>(derivedAttributesCount);

            derivedAttributeNames.AddRange(GetDerivedAttributeNames());
        }

        private void CalculateDerivedValues(double[] numericalValues, double[] numericalValuesNorm)
        {
            distributionValues = new List<DistributionValues>(derivedAttributesCount);

            // Add Distribution class
            distributionValues.Add(new DistributionValues());
            distributionValues.Add(new DistributionValues());

            // Calculate Distribution Values
            distributionValues[0].GetDescriptiveStatisticValues(numericalValues);
            distributionValues[1].GetDescriptiveStatisticValues(numericalValuesNorm);
        }
    }
}
