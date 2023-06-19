using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    public class AbstractDataset
    {

        /// <summary>
        /// Derived Attributes
        /// </summary>
        public enum DerivedAttributes
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


            NumberOfDerivedAttributes
        }


        //######### Basic Dataset Infos #########

        public int attributesCount;                                 // Number of attributes from the csv file which can be accessed
        public int derivedAttributesParamsCount;                    // Number of derived parameters from the attributes which can be accessed

        public string[] attributes;                                 // Attributes in the order found in the header of the csv file
        public string[] derivedAttributesParams;                    // Derived parameters from the attributes in the order found in the header of the csv file

        public int numericalAttributeCount;                         // Number of numerical attributes
        private List<string> numericalAttributeNames;               // Attributes found in the header of the csv file
        private List<int> numberOfValues;                           // Number of values for each attribute from the csv file

        public Dictionary<string, double[]> numericalValues;        // List of all numerical attributes with their <name,values>
        //public List<double[]> minMaxNormalizedValues;

        //public Dictionary<string, string[]> textualAttributes;    // List of all textual attributes with their <name,values>


        //######### Normalized Dataset #########

        // Stores the normalized Values of each numerical attribute
        public Dictionary<string, double[]> normalizedValues;
        //public List<double[]> minMaxNormalizedValues;


        //######### Statistical Values #########

        // Stores the calculated statistic data for each normalized attribute
        public Dictionary<string, DistributionValues> statisticValues;
        //public List<DistributionValues> minMaxStatisticValues;

        /* TODO: All Dictionary have the Attributes as String in common
         * So save strings off possible Infos which can be retrived ?
         * Like LENGTH --> Orig Value OR Normalized Value OR Statistic Value --> MEAN, MODALITY,...
        */ 

        public AbstractDataset(List<string> attributeNames, Dictionary<string, double[]> numericalData)        
        {
            numericalAttributeCount = attributeNames.Count;
            numericalAttributeNames = attributeNames;
            numericalValues = numericalData;
            
            //textualAttributes = new Dictionary<string, string[]>();
            
            CheckAttributeValueCount();
            DataPreparation();
        }

        //######### Test #########

        //public double GetValues01(int attributeId, DerivedAttributes derivedAttributes)
        //{
        //    switch (derivedAttributes)
        //    {
        //        case DerivedAttributes.LargestElement:
        //            return statisticValues[numericalAttributeNames[attributeId]].LargestElement;
        //    }
        //}



        //#########      #########


        public Dictionary<string, double[]> GetNumericDic()
        {
            return numericalValues;
        }

        public Dictionary<string, double[]> GetNormalizedDic()
        {
            return normalizedValues;
        }

        public Dictionary<string, DistributionValues> GetStatisticDic()
        {
            return statisticValues;
        }

        public List<string> GetAttributeNames()
        {
            return numericalAttributeNames;
        }

        public string GetAttributeName(int attributeId)
        {
            return numericalAttributeNames[attributeId];
        }

        public double[] GetValues(int attributeId)
        {
            return numericalValues[GetAttributeName(attributeId)];
        }

        public double[] GetNormalizedValues(int attributeId)
        {
            return normalizedValues[GetAttributeName(attributeId)];
        }

        private void CheckAttributeValueCount()
        {
            numberOfValues = new List<int>(numericalValues.Count);

            // Runs through all attributes and checks the number of values
            foreach (var attribute in numericalValues)
            {
                numberOfValues.Add(attribute.Value.Length);
            }
        }

        /// <summary>
        /// Calculates the normalized and statistical Values of the Dataset
        /// </summary>
        private void DataPreparation()
        {
            // Normalize Values between 0 to 1 and calculate statistic info
            List<Scale> normalizeScales = new List<Scale>(numericalAttributeCount);
            normalizedValues = new Dictionary<string, double[]>(numericalAttributeCount);
            statisticValues = new Dictionary<string, DistributionValues>(numericalAttributeCount);

            //Calculate Min Max for every Attribute
            foreach (var dataValue in numericalValues)
            {
                double currentMin = dataValue.Value.Min();
                double currentMax = dataValue.Value.Max();

                List<double> domain = new List<double>(2)
                {
                    currentMin,
                    currentMax
                };

                ScaleLinear currentNormScale = new ScaleLinear(domain);
                normalizeScales.Add(currentNormScale);

                // Normalize Values
                normalizedValues.Add(dataValue.Key, currentNormScale.GetNormalizedArray(dataValue.Value));

                // Calculate statistic values from dataset
                DistributionValues distVal = new DistributionValues();
                distVal.GetDescriptiveStatisticValues(currentNormScale.GetNormalizedArray(dataValue.Value));
                statisticValues.Add(dataValue.Key, distVal);
            }

        }

        public void PrintStatisticValues()
        {
            //#### PRINT STATISTIC VALUES ####
            string statisticValuesString = ">> DataSet: \n";
            // Output statisticValues for Debug Purposes
            foreach (var statisticValue in statisticValues)
            {
                statisticValuesString += statisticValue.Key + ": \n" + statisticValue.Value.PrintDistributionValues();
                statisticValuesString += "\n";
            }
            Debug.Log(statisticValuesString);
            //#### END PRINT STATISTIC VALUES ####
        }

    }
}
