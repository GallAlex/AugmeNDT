using Assets.Scripts.DataStructure;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages topological data, including critical points and gradient information.
    /// Loads data from CSV files and makes it accessible to visualization components.
    /// </summary>
    public class TopologicalDataObject : MonoBehaviour
    {
        public static TopologicalDataObject Instance;
        public List<CriticalPointDataset> criticalPointList = new List<CriticalPointDataset>();
        public List<GradientDataset> gradientList = new List<GradientDataset>();
        public float maxMag = 0; // Maximum gradient magnitude
        
        private void Awake()
        {
            Instance = this;
            gradientList = TTKCalculations.GetGradientAllVectorField();
            criticalPointList = TTKCalculations.GetCriticalPointAllVectorField();
            maxMag = gradientList.Max(x => x.Magnitude);
        }
    }
}
