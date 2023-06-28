
using System.Collections.Generic;

namespace AugmeNDT{

    /// <summary>
    /// This Class represents an attribute of a data set.
    /// It can consist of numerical or textual values. If it is textual, the number of textual values are mapped to a range (0 to n-1) of numerical values.
    /// </summary>
    public class Attribute
    {
        private string name;                    // Name of the attribute
        private bool numericalType;             // True if the attribute is numerical, false if it is textual

        private double[] numericalValues;       // If the attribute is numerical, this array contains the numerical values
        private string[] textualValues;         // If the attribute is textual, this array contains the textual values

        private double[] minMaxValue;           // Minimum/Maximum value of the attribute (numericalValues)

        private double[] numericalValuesNorm;   // Numerical values normalized between 0 and 1
        private double[] minMaxValueNorm;       // Minimum/Maximum value of the normalized attribute (numericalValuesNorm)

        #region Getter/Setter

        public string GetName()
        {
            return name;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public bool IsNumerical()
        {
            return numericalType;
        }

        public double[] GetNumericalVal()
        {
            return numericalValues;
        }

        public string[] GetTextualVal()
        {
            return textualValues;
        }

        public double[] GetMinMaxVal()
        {
            return minMaxValue;
        }

        /// <summary>
        /// Lazy loading of normalized numerical attribute values
        /// </summary>
        /// <returns></returns>
        public double[] GetNumericalValNorm()
        {
            if (numericalValuesNorm == null)
            {
                NormalizeValues();
                SetMinMaxValuesNorm();
            }
            return numericalValuesNorm;
        }

        /// <summary>
        /// Lazy loading of normalized min value
        /// </summary>
        /// <returns></returns>
        public double[] GetMinMaxValNorm()
        {
            if (numericalValuesNorm == null)
            {
                NormalizeValues();
                SetMinMaxValuesNorm();
            }
            return minMaxValueNorm;
        }

        #endregion

        public Attribute(string attributeName, double[] values)
        {
            this.name = attributeName;
            numericalType = true;

            numericalValues = values;
            SetMinMaxValues();
        }

        public Attribute(string attributeName, string[] values)
        {
            this.name = attributeName;
            numericalType = false;

            textualValues = values;
            SetNumberValuesForTextualAttr();
            SetMinMaxValues();
        }

        /// <summary>
        /// Returns the number of values of the attribute.
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfValues()
        {
            return numericalValues.Length;
        }

        /// <summary>
        /// If a textual attribute is given, this method will set the numerical values for the attribute.
        /// The numerical values go from 0 to n-1, where n is the number of textual values.
        /// </summary>
        private void SetNumberValuesForTextualAttr()
        {
             numericalValues = new double[textualValues.Length];

             for (int i = 0; i < textualValues.Length; i++)
             {
                numericalValues[i] = i;
             }

        }

        /// <summary>
        /// Sets the minimum and maximum values of the attribute based on the numerical values.
        /// </summary>
        private void SetMinMaxValues()
        {
            double minValue = numericalValues[0];
            double maxValue = numericalValues[0];

            for (int i = 1; i < numericalValues.Length; i++)
            {
                if (numericalValues[i] < minValue)
                {
                    minValue = numericalValues[i];
                }
                else if (numericalValues[i] > maxValue)
                {
                    maxValue = numericalValues[i];
                }
            }

            minMaxValue = new double[2] { minValue, maxValue };
        }

        /// <summary>
        /// Calculates the normalized numerical values ([0,1]) of the numerical attribute.
        /// </summary>
        private void NormalizeValues()
        {
            List<double> domain = new List<double>(2)
            {
                minMaxValue[0],
                minMaxValue[1]
            };

            ScaleLinear currentNormScale = new ScaleLinear(domain);
            numericalValuesNorm = currentNormScale.GetNormalizedArray(numericalValues);
        }

        /// <summary>
        /// Sets the minimum and maximum values of the attribute based on the normalized numerical values.
        /// </summary>
        private void SetMinMaxValuesNorm()
        {
            double minValueNorm = numericalValuesNorm[0];
            double maxValueNorm = numericalValuesNorm[0];

            for (int i = 1; i < numericalValuesNorm.Length; i++)
            {
                if (numericalValuesNorm[i] < minValueNorm)
                {
                    minValueNorm = numericalValuesNorm[i];
                }
                else if (numericalValuesNorm[i] > maxValueNorm)
                {
                    maxValueNorm = numericalValuesNorm[i];
                }
            }

            minMaxValueNorm = new double[2] { minValueNorm, maxValueNorm };
        }
    }
}
