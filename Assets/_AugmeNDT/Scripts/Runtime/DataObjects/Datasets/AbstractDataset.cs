using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT{
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
            //Other

            NumberOfDerivedAttributes
        }


        //######### Basic Dataset Infos #########

        public string datasetName;                                  // Name of the dataset

        public int attributesCount;                                 // Number of attributes from the csv file which can be accessed
        public int derivedAttributesCount;                          // Number of derived parameters from the attributes which can be accessed

        public List<int> numberOfValues;                            // Number of values for each attribute in the csv file

        public List<string> attributeNames;                         // Attribute names in the order found in the header of the csv file
        public List<string> derivedAttributeNames;                  // Derived parameters from the attributes in the order found in the header of the csv file

        public Attribute headerValues;                              // Attribute for the header (Attribute names in the order found in the header of the csv file)
        public List<Attribute> attributeValues;                     // List of all attributes (name, values, normValues,...), index zero is the textual Header attribut (all attribute names)
        public List<List<Attribute>> derivedAttributeValues;        // List of all derived attributes (name, values, normValues,...) for the initial attributess like statistics, metrics,... 


        public Dictionary<string, DistributionValues> statisticValues;          


        public AbstractDataset(string datasetName, List<string> attributeNames, Dictionary<string, double[]> numericalData)        
        {
            this.datasetName = datasetName;
            this.attributesCount = attributeNames.Count;
            this.attributeNames = attributeNames;

            statisticValues = new Dictionary<string, DistributionValues>();

            InitializeDatasets(numericalData);
            CheckAttributeValueCount();
            CalculateDerivedValues();
        }

        /// <summary>
        /// Creates a List of attributes and derived attributes to form the dataset.
        /// Number of attributes is defined by the csv file and the number of derived attributes is defined by the enum DerivedAttributes.
        /// </summary>
        private void InitializeDatasets(Dictionary<string, double[]> numericalData)
        {
            // Add header values (Attribute names)
            headerValues = CreateAttribute("Attributes", attributeNames.ToArray());

            //Set up Lists
            attributeValues = new List<Attribute>(attributesCount);
            derivedAttributeValues = new List<List<Attribute>>(attributesCount);

            // How many derived attributes are there?
            derivedAttributesCount = (int)DerivedAttributes.NumberOfDerivedAttributes;
            derivedAttributeNames = new List<string>(derivedAttributesCount);

            // Save the strings from the Enum in the derivedAttributesParams List
            for (int i = 0; i < derivedAttributesCount; i++)
            {
                derivedAttributeNames.Add(((DerivedAttributes)i).ToString());
            }

            // Add the remaining attributes
            for (int i = 0; i < attributesCount; i++)
            {
                attributeValues.Add(CreateAttribute(attributeNames[i], numericalData.ElementAt(i).Value));
            }

        }

        /// <summary>
        /// Sets the name of the dataset
        /// </summary>
        /// <param name="name"></param>
        public void SetDatasetName(string name)
        {
            datasetName = name;
        }

        /// <summary>
        /// Returns an Attribute with all Attribute names in the order found in the header of the csv file
        /// </summary>
        /// <returns></returns>
        public Attribute GetHeader()
        {
            return headerValues;
        }

        /// <summary>
        /// Returns the Attribute with the given Id
        /// </summary>
        /// <param name="attrId"></param>
        /// <returns></returns>
        public Attribute GetAttribute(int attrId)
        {
            return attributeValues[attrId];

        }

        /// <summary>
        /// Returns the Derived Attribute with the given Id for a specific Attribute
        /// </summary>
        /// <param name="attrId"></param>
        /// <param name="derivedAttrId"></param>
        /// <returns></returns>
        public Attribute GetDerivedAttribute(int attrId, DerivedAttributes derivedAttrId)
        {
            return derivedAttributeValues[attrId][(int)derivedAttrId];

        }

        /// <summary>
        /// Returns a new Attribute which consists of the choosen Derived Attribute values for all attributes
        /// </summary>
        /// <param name="derivedAttrId"></param>
        /// <returns></returns>
        public Attribute GetDerivedAttribute(DerivedAttributes derivedAttrId)
        {
            // Run through all attributes and add the derived attribute values to the new attribute
            double[] aggregatedValues = new double[attributesCount];
            for (var attrIndex = 0; attrIndex < derivedAttributeValues.Count; attrIndex++)
            {
                // Derived Attribute values are always one double value [0]
                aggregatedValues[attrIndex] = derivedAttributeValues[attrIndex][(int)derivedAttrId].GetNumericalVal()[0];
            }

            return CreateAttribute(derivedAttributeNames[(int)derivedAttrId], aggregatedValues);
        }

        /// <summary>
        /// Returns a new Attribute which consists of all Values of all attributes
        /// </summary>
        /// <returns></returns>
        public Attribute GetAllAttributes()
        {
            // Run through all attributes and add all values to the new attribute
            double[] aggregatedValues = new double[attributesCount * numberOfValues[0]];
            int currentIndex = 0;

            for (var attrIndex = 0; attrIndex < attributesCount; attrIndex++)
            {
                for (var valIndex = 0; valIndex < numberOfValues[0]; valIndex++)
                {
                    aggregatedValues[currentIndex] = attributeValues[attrIndex].GetNumericalVal()[valIndex];
                    currentIndex++;
                }
            }

            return CreateAttribute("Attribute Values", aggregatedValues);
        }


        /// <summary>
        /// Method runs through all attributes and stores/checks the number of values for each attribute
        /// </summary>
        private void CheckAttributeValueCount()
        {
            numberOfValues = new List<int>(attributesCount);
            int lastNumberOfValues = attributeValues[0].GetNumberOfValues();

            // Runs through all attributes and checks the number of values
            foreach (var attribute in attributeValues)
            {
                int currentNumberOfValues = attribute.GetNumberOfValues();
                if(currentNumberOfValues != lastNumberOfValues) Debug.LogWarning("Number Of Values for the Attributes do not match");
                
                numberOfValues.Add(currentNumberOfValues);
            }
        }

        /// <summary>
        /// Calculates the derived Values of the Attributes.
        /// </summary>
        private void CalculateDerivedValues()
        {
            // For each attribute...
            for (var attr = 0; attr < attributeValues.Count; attr++)
            {
                //Preprocess here everything
                derivedAttributeValues.Add(new List<Attribute>(derivedAttributesCount));

                // Calculate statistic values from dataset
                DistributionValues statVal = new DistributionValues();
                    
                // Check if textual?
                statVal.GetDescriptiveStatisticValues(attributeValues[attr].GetNumericalVal());

                statisticValues.Add(attributeNames[attr], statVal); //TODO: TEST variable

                // For each derived attribute fill values in...
                for (var derivedAttr = 0; derivedAttr < derivedAttributesCount; derivedAttr++)
                {
                    string currentDerivedAttrName = derivedAttributeNames[derivedAttr];

                    switch ((DerivedAttributes)derivedAttr)
                    {
                        // DistributionValues
                        case DerivedAttributes.LargestElement:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new []{statVal.LargestElement}));
                            break;
                        case DerivedAttributes.SmallestElement:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new[] { statVal.SmallestElement }));
                            break;
                        case DerivedAttributes.Median:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new[] { statVal.Median }));
                            break;
                        case DerivedAttributes.Mean:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new[] { statVal.Mean }));
                            break;
                        case DerivedAttributes.Variance:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new[] { statVal.Variance }));
                            break;
                        case DerivedAttributes.StdDev:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new[] { statVal.StdDev }));
                            break;
                        case DerivedAttributes.Iqr:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new[] { statVal.Iqr }));
                            break;
                        case DerivedAttributes.UpperQuartile:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new[] { statVal.UpperQuartile }));
                            break;
                        case DerivedAttributes.LowerQuartile:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new[] { statVal.LowerQuartile }));
                            break;
                        case DerivedAttributes.Kurtosis:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new[] { statVal.Kurtosis }));
                            break;
                        case DerivedAttributes.Skewness:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new[] { statVal.Skewness }));
                            break;
                        case DerivedAttributes.Modality:
                            derivedAttributeValues[attr].Add(CreateAttribute(currentDerivedAttrName, new[] { statVal.Modality }));
                            break;
                        //Other
                        //case DerivedAttributes.ChiSquared:
                        //    break;
                        default:
                            throw new NotImplementedException();
                    }

                }
            }

        }

        /// <summary>
        /// Create a numerical Attribute
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Attribute CreateAttribute(string name, double[] value)
        {
            return new Attribute(name, value);
        }

        /// <summary>
        /// Create a textual/categorical Attribute
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Attribute CreateAttribute(string name, string[] value)
        {
            return new Attribute(name, value);
        }

        public void PrintDatasetValues(bool normalizedVals)
        {
            //#### PRINT ATTRIBUTE VALUES ####
            if (normalizedVals) Debug.Log("OUTPUT: Dataset - Attributes NORMALIZED \n" + "Number of Attributes: " + (attributesCount));
            else Debug.Log("OUTPUT: Dataset - Attributes \n" + "Number of Attributes: " + (attributesCount));

            List<double[]> values = new List<double[]>();

            string[] names = new string[attributeNames.Count];

            for (var index = 0; index < attributeNames.Count; index++)
            {
                if(normalizedVals) values.Add(attributeValues[index].GetNumericalValNorm());
                else values.Add(attributeValues[index].GetNumericalVal());

                names[index] = attributeValues[index].GetName();
            }

            Debug.Log(TablePrint.ToStringTable(names, values));


            //#### PRINT DERIVED VALUES ####
            if (normalizedVals) Debug.Log("OUTPUT: Dataset - Derived Attributes NORMALIZED \n" + "Number of Derived Attributes: " + derivedAttributesCount);
            else Debug.Log("OUTPUT: Dataset - Derived Attributes \n" + "Number of Derived Attributes: " + derivedAttributesCount);

            string[] derivedNames = derivedAttributeNames.ToArray();
            string combinedDerivedVal = "";

            // Output statisticValues for Debug Purposes
            for (var derivedValList = 0; derivedValList < attributesCount; derivedValList++)
            {
                List<double[]> derivedValues = new List<double[]>();

                combinedDerivedVal += attributeNames[derivedValList] + ":\n";
                
                for (int derivedId = 0; derivedId < derivedAttributeValues[derivedValList].Count; derivedId++)
                {
                    //derivedNames[derivedId] = derivedAttributeNames[derivedId];
                    if (normalizedVals) derivedValues.Add(derivedAttributeValues[derivedValList][derivedId].GetNumericalValNorm());
                    else derivedValues.Add(derivedAttributeValues[derivedValList][derivedId].GetNumericalVal());
                }

                combinedDerivedVal += TablePrint.ToStringTable(derivedNames, derivedValues) + "\n";
            }

            Debug.Log(combinedDerivedVal);
        }

    }
}
