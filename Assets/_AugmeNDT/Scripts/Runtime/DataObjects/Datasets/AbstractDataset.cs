using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT{
    public class AbstractDataset
    {

        //######### Basic Dataset Infos #########

        public string datasetName;                                  // Name of the dataset

        public List<string> attributeNames;                         // Attribute names in the order found in the header of the csv file
        public int attributesCount;                                 // Number of attributes from the csv file which can be accessed
        public int numericAttributesCount;                          // Number of numerical attributes (from all attributes -> attributesCount)

        public Attribute headerValues;                              // Attribute for the header (Attribute names in the order found in the header of the csv file)
        public List<Attribute> attributeValues;                     // List of all attributes (name, values, normValues,...), index zero is the textual Header attribut (all attribute names)
        public List<int> numberOfValues;                            // Number of values for each attribute in the csv file


        public Dictionary<string, DistributionValues> statisticValues;          


        public AbstractDataset(string datasetName, List<string> attributeNames, Dictionary<string, double[]> numericalData)        
        {
            this.datasetName = datasetName;
            this.attributesCount = attributeNames.Count;
            this.attributeNames = attributeNames;

            statisticValues = new Dictionary<string, DistributionValues>();

            InitializeDatasets(numericalData);
            CheckAttributeValueCount();
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

            // Add attributes
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
        /// Returns the name of the dataset
        /// </summary>
        /// <returns></returns>
        public string GetDatasetName()
        {
            return datasetName;
        }

        /// <summary>
        /// Method runs through all attributes and counts the number of numerical attributes
        /// </summary>
        /// <returns></returns>
        public int GetNumericalValuesCount()
        {
            numericAttributesCount = 0;
            foreach (var attr in attributeValues)
            {
                if (attr.IsNumerical())
                {
                    numericAttributesCount++; 
                }
            }
            return numericAttributesCount;
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
        /// Returns the id of an Attribute with the given name. Looks in the attributeNames List
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetAttributeId(string name)
        {
            int attrId = attributeNames.IndexOf(name);
            if (attrId == -1)
            {
                throw new IndexOutOfRangeException("Attribute [" + name + "] does not exit in the dataset");
            }
            return attrId;
        }

        /// <summary>
        /// Returns the Derived Attributes for a specific Attribute
        /// </summary>
        /// <param name="attrId"></param>
        /// <returns></returns>
        public DerivedAttributes GetDerivedAttribute(int attrId)
        {
            return attributeValues[attrId].GetDerivedAttributes();

        }

        /// <summary>
        /// Returns all Values of all numerical attributes
        /// </summary>
        /// <returns></returns>
        public double[] GetAllAttributeValues(bool normalized)
        {
            // Run through all attributes and add all values 
            List<double> aggregatedValues = new List<double>(numericAttributesCount * (int)DerivedAttributes.DerivedAttributeCalc.NumberOfDerivedAttributes);

            for (var attrIndex = 0; attrIndex < attributesCount; attrIndex++)
            {
                if (!attributeValues[attrIndex].IsNumerical()) continue;

                double[] values;
                if(normalized) values = attributeValues[attrIndex].GetNumericalValNorm();
                else values = attributeValues[attrIndex].GetNumericalVal();
                
                aggregatedValues.AddRange(values);
            }

            return aggregatedValues.ToArray();
        }

        /// <summary>
        /// Returns the min/max values of a specific attribute
        /// </summary>
        /// <param name="attrId"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public double[] GetMinMaxAttributeValues(int attrId, bool normalized)
        {
            double[] minMax;

            if(normalized) minMax = attributeValues[attrId].GetMinMaxValNorm();
            else minMax = attributeValues[attrId].GetMinMaxVal();

            return minMax;
        }

        /// <summary>
        /// Return the min/max values over all numerical attributes
        /// </summary>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public double[] GetMinMaxAttributeValues(bool normalized)
        {
            // Run through all attributes and add all min/max values 
            double[] aggregatedValues = new double[numericAttributesCount * 2];

            for (var attrIndex = 0; attrIndex < attributesCount; attrIndex++)
            {
                if (!attributeValues[attrIndex].IsNumerical()) continue;

                double[] minMax;
                if(normalized) minMax = attributeValues[attrIndex].GetMinMaxValNorm();
                else minMax = attributeValues[attrIndex].GetMinMaxVal();

                aggregatedValues[attrIndex] = minMax[0];
                aggregatedValues[attrIndex + 1] = minMax[1];
            }

            return DataEnsemble.GetMinMaxValues(aggregatedValues);
        }

        /// <summary>
        /// Returns the value of the choosen Derived Attribute for all attributes
        /// </summary>
        /// <param name="derivedAttrId"></param>
        /// <returns></returns>
        public double[] GetDerivedAttribute(DerivedAttributes.DerivedAttributeCalc derivedAttrId, bool normalized)
        {
            // Run through all attributes and add the derived attribute values to the new attribute
            double[] aggregatedValues = new double[attributesCount];

            for (var attrIndex = 0; attrIndex < attributesCount; attrIndex++)
            {
                // Derived Attribute values are always one double value [0]
                aggregatedValues[attrIndex] = attributeValues[attrIndex].GetDerivedValue(derivedAttrId, normalized);
            } 
            
            return aggregatedValues;
        }

        /// <summary>
        /// Returns the found Ids of a given Value in a specific Attribute
        /// </summary>
        /// <param name="attrId"></param>
        /// <param name="value"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public List<int> GetIdOfAttributeValue(int attrId, double value, bool normalized)
        {
            return attributeValues[attrId].GetIndexOfValue(value, normalized);
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
        /// Create a numerical Attribute
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Attribute CreateAttribute(string name, double[] value)
        {
            numericAttributesCount++;
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

        public void RemoveAttribute(int attrId)
        {
            // Check if Id is valid
            if (attrId < 0 || attrId >= attributesCount)
            {
                throw new IndexOutOfRangeException("Attribute Id [" + attrId + "] does not exit in the dataset");
            }

            Debug.Log("Removing Attribute [" + attributeNames[attrId] + "] from Dataset [" + datasetName + "]");
            attributeValues.RemoveAt(attrId);
            attributeNames.RemoveAt(attrId);
            attributesCount--;
        }


        public void PrintDatasetValues(bool normalizedVals)
        {
            string normalizedText = "Attributes";
            if (normalizedVals) normalizedText = "Attributes NORMALIZED";

            //#### PRINT ATTRIBUTE VALUES ####
            Debug.Log("OUTPUT: Dataset [" + datasetName + "] - "+ normalizedText + " \n" + "Number of Attributes: " + (attributesCount) + ", " + "Number of data points: " + (numberOfValues[0]));

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
            string[] derivedNames = DerivedAttributes.GetDerivedAttributeNames();

            Debug.Log("OUTPUT: Dataset [" + datasetName + "] - Derived " + normalizedText + " \n" + "Number of Derived Attributes: " + derivedNames.Length + " \n");

            string combinedDerivedVal = "";
            
            for (var index = 0; index < attributeValues.Count; index++)
            {
                List<double[]> derivedValues = new List<double[]>();
                combinedDerivedVal += attributeNames[index] + ":\n";

                for (int derivedIdIndex = 0; derivedIdIndex < derivedNames.Length; derivedIdIndex++)
                {
                    derivedValues.Add(new []{attributeValues[index].GetDerivedAttributes().GetDerivedValue((DerivedAttributes.DerivedAttributeCalc)derivedIdIndex, normalizedVals)});
                }
                combinedDerivedVal += TablePrint.ToStringTable(derivedNames, derivedValues) + "\n";
            }
            
            Debug.Log(combinedDerivedVal);
        }

    }
}
