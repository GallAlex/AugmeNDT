// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;


namespace AugmeNDT
{
    public class DataEnsemble
    {
        private List<AbstractDataset> abstractDatasets;

        public DataEnsemble()
        {
            abstractDatasets = new List<AbstractDataset>();
        }

        public void AddAbstractDataSet(AbstractDataset dataSet)
        {
            abstractDatasets.Add(dataSet);
        }

        public AbstractDataset GetDataSet(int dataSetId)
        {
            return abstractDatasets[dataSetId];
        }

        public int GetDataSetCount()
        {
            return abstractDatasets.Count;
        }

        public string[] GetAbstractDataSetNames()
        {
            string[] dataSetNames = new string[abstractDatasets.Count];

            for (var index = 0; index < abstractDatasets.Count; index++)
            {
                dataSetNames[index] = abstractDatasets[index].GetDatasetName();
            }

            return dataSetNames;
        }

        //#####################     ATTRIBUTE METHODS     #####################


        public Attribute GetHeaderAttribute(int dataSetId)
        {
            return abstractDatasets[dataSetId].GetHeader();
        }

        /// <summary>
        /// Returns the the selected attribute of the selected dataset.
        /// </summary>
        /// <param name="dataSetId"></param>
        /// <param name="attributeId"></param>
        /// <returns></returns>
        public Attribute GetAttribute(int dataSetId, int attributeId)
        {
            return abstractDatasets[dataSetId].GetAttribute(attributeId);
        }

        public Attribute GetAttribute(int attributeId)
        {
            // Use List as the same attribute from different datasets can have different amount of values
            List<double> aggregatedValues = new List<double>();

            for (int dataset = 0; dataset < abstractDatasets.Count; dataset++)
            {
                var currentAttribute = GetAttribute(dataset, attributeId);
                var values = currentAttribute.GetNumericalVal();

                for (int j = 0; j < values.Length; j++)
                {
                    aggregatedValues.Add(values[j]);
                }
            }

            return new Attribute(GetAttribute(0, attributeId).GetName(),aggregatedValues.ToArray());
        }

        /// <summary>
        /// Returns the minimum and maximum value of the selected attribute for the dataset.
        /// </summary>
        /// <param name="dataSetId"></param>
        /// <param name="attributeId"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public double[] GetMinMaxAttrVal(int dataSetId, int attributeId, bool normalized)
        {
            return GetMinMaxValues(abstractDatasets[dataSetId].GetMinMaxAttributeValues(attributeId, normalized));
        }


        /// <summary>
        /// Returns the minimum and maximum value for each Attributes for one datasets.
        /// </summary>
        /// <param name="dataSetId"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public double[] GetMinMaxAttrVal(int dataSetId, bool normalized)
        {
            return GetMinMaxValues(abstractDatasets[dataSetId].GetAllAttributeValues(normalized));
        }

        /// <summary>
        /// Returns the minimum and maximum value for each Attributes over all datasets.
        /// </summary>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public double[] GetMinMaxAttrVal(bool normalized)
        {
            double[] aggregatedValues = new double[abstractDatasets.Count * 2];

            for (int i = 0; i < abstractDatasets.Count; i++)
            {
                double[] minMaxOfDataset = GetMinMaxAttrVal(i, normalized);

                for (int j = 0; j < minMaxOfDataset.Length; j++)
                {
                    aggregatedValues[j + (i * minMaxOfDataset.Length)] = minMaxOfDataset[j];
                }
            }

            return GetMinMaxValues(aggregatedValues);
        }

        /// <summary>
        /// Returns the found Ids of the given value in the selected attribute for the selected dataset.
        /// </summary>
        /// <param name="dataSetId"></param>
        /// <param name="attributeId"></param>
        /// <param name="value"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public List<int> GetIndexOfAttrVal(int dataSetId, int attributeId, double value, bool normalized)
        {
            return abstractDatasets[dataSetId].GetAttribute(attributeId).GetIndexOfValue(value, normalized);
        }

        public List<int> GetIndexOfAttrValRange(int dataSetId, int attributeId, double[] minMaxValRange, bool normalized)
        {
            return abstractDatasets[dataSetId].GetAttribute(attributeId).GetIndexOfValueRange(minMaxValRange, normalized);
        }

        //#####################     DERIVED ATTRIBUTE METHODS     #####################

        public DerivedAttributes GetDerivedAttribute(int dataSetId, int attributeId)
        {
            return abstractDatasets[dataSetId].GetAttribute(attributeId).GetDerivedAttributes();
        }

        public string[] GetDerivedAttributeNames()
        {
            return DerivedAttributes.GetDerivedAttributeNames();
        }

        /// <summary>
        /// Returns the values of the selected derived attribute for each Attribut for all datasets.
        /// </summary>
        /// <param name="derivedId"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public double[] GetDerivedAttributeValues(DerivedAttributes.DerivedAttributeCalc derivedId, bool normalized)
        {
            double[] aggregatedValues = new double[abstractDatasets.Count * abstractDatasets[0].attributesCount];

            for (int i = 0; i < abstractDatasets.Count; i++)
            {
                double[] valOfDataset = abstractDatasets[i].GetDerivedAttribute(derivedId, normalized);

                for (int j = 0; j < valOfDataset.Length; j++)
                {
                    aggregatedValues[j + (i * valOfDataset.Length)] = valOfDataset[j];
                }
            }

            return aggregatedValues;
        }


        /// <summary>
        /// Returns the values of the selected derived attribute for each Attribut.
        /// </summary>
        /// <param name="dataSetId"></param>
        /// <param name="derivedId"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public double[] GetDerivedAttributeValues(int dataSetId, DerivedAttributes.DerivedAttributeCalc derivedId, bool normalized)
        {
            return abstractDatasets[dataSetId].GetDerivedAttribute(derivedId, normalized);
        }

        /// <summary>
        /// Returns the minimum and maximum value of the selected derived attribute for each Attributes over all datasets.
        /// </summary>
        /// <param name="derivedId"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public double[] GetMinMaxDerivedAttrVal(DerivedAttributes.DerivedAttributeCalc derivedId, bool normalized)
        {
            double[] aggregatedValues = new double[abstractDatasets.Count * 2]; // 2 vals (min and max) per dataset

            for (int i = 0; i < abstractDatasets.Count; i++)
            {
                double[] minMaxOfDataset = GetMinMaxValues(GetDerivedAttributeValues(i, derivedId, normalized));

                for (int j = 0; j < minMaxOfDataset.Length; j++)
                {
                    aggregatedValues[j + (i * minMaxOfDataset.Length)] = minMaxOfDataset[j];
                }
            }

            return GetMinMaxValues(aggregatedValues);
        }

        /// <summary>
        /// Returns the minimum and maximum value of the selected derived attribute over all Attributes.
        /// </summary>
        /// <param name="dataSetId"></param>
        /// <param name="derivedId"></param>
        /// <param name="normalized"></param>
        /// <returns></returns>
        public double[] GetMinMaxDerivedAttrVal(int dataSetId, DerivedAttributes.DerivedAttributeCalc derivedId, bool normalized)
        {
            return GetMinMaxValues(GetDerivedAttributeValues(dataSetId, derivedId, normalized));
        }





        // TODO: MOVE TO SPECIAL DATASET UTILITY CLASS
        //#####################     UTILITY METHODS     #####################

        /// <summary>
        /// Gets the minimum and maximum values of the attribute based on the numerical values.
        /// </summary>
        public static double[] GetMinMaxValues(double[] array)
        {
            double minValue = array[0];
            double maxValue = array[0];

            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] < minValue)
                {
                    minValue = array[i];
                }
                else if (array[i] > maxValue)
                {
                    maxValue = array[i];
                }
            }

            return new double[2] { minValue, maxValue };
        }

        /// <summary>
        /// Method represents a string array as a numerical array with numbers from 0 to n.
        /// Useful for Channel definitions.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static double[] GetNumericalRepresentation(string[] array)
        {
            double[] numericalArray = new double[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                numericalArray[i] = i;
            }

            return numericalArray;
        }

        public static string[] ConvertToStringArray(double[] array)
        {
            string[] stringArray = new string[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                stringArray[i] = array[i].ToString();
            }

            return stringArray;
        }

    }
}
