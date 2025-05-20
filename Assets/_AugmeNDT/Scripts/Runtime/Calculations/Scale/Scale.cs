// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT{
    public class Scale
    {

        // Categorical/Nominal (No ordering)
        // Ordered
        // --> Ordinal (Ordered/ Ranked)
        // --> Quantitative (supports arithmetic comparison)
        //     --> Discrete data
        //     --> Continuous data (Functions which can be interpolated)
        public enum DataScaleType
        {
            Nominal,
            Linear,
        }

        public DataScaleType dataScaleType;
        public List<double> domain;  // Data Range
        public List<double> range;   // Existing Range

        /// <summary>
        /// Given domain represents data range
        /// Given Range represents existing range to which the domain is mapped to
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="range"></param>
        public Scale(List<double> domain, List<double> range)
        {
            if ((domain == null || domain.Count > 2) || (range == null || range.Count > 2))
            {
                Debug.LogError("Format of domain/range is not correct");
                return;
            }

            this.domain = domain;
            this.range = range;

            if ((this.domain[1] - this.domain[0]) == 0.0d || (this.range[1] - this.range[0]) == 0.0d)
            {
                Debug.LogWarning("Min/Max of domain or range are equal!");
            }
        }

        /// <summary>
        /// Given domain represents data range
        /// Range is set between 0 and 1  
        /// </summary>
        /// <param name="domain"></param>
        public Scale(List<double> domain)
        {
            if (domain == null || domain.Count > 2)
            {
                Debug.LogError("Format of domain is not correct");
                return;
            }

            this.domain = domain;
            this.range = new List<double> { 0.0f, 1.0f };

            if ((this.domain[1] - this.domain[0]) == 0.0d || (this.range[1] - this.range[0]) == 0.0d)
            {
                Debug.LogWarning("Min/Max of domain or range are equal!");
            }
        }

        /// <summary>
        ///  Method accepts input between domain min/max and maps it to output between range min/max.
        /// </summary>
        /// <param name="domainValue">Value of datapoint</param>
        public virtual double GetScaledValue(double domainValue)
        {
            return 0.0f;
        }

        /// <summary>
        ///  Method accepts input between range min/max and maps it to output between domain min/max.
        /// </summary>
        /// <param name="scaledValue">Value of scaled point</param>
        public virtual double GetDomainValue(double scaledValue)
        {
            return 0.0f;
        }

        /// <summary>
        /// Method accepts input between range min/max and maps it to a domain value in a string array
        /// </summary>
        /// <param name="domainValue"></param>
        /// <returns></returns>
        public virtual string GetDomainValueName(double domainValue)
        {
            return "";
        }
    }
}
