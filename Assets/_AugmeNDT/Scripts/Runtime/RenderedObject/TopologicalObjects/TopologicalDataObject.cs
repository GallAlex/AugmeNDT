using Assets.Scripts.DataStructure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace AugmeNDT
{
    /// <summary>
    /// Manages topological data, including critical points and gradient information.
    /// Loads data from CSV files and makes it accessible to visualization components.
    /// </summary>
    public class TopologicalDataObject : MonoBehaviour
    {
        public static TopologicalDataObject instance;
        public float scaleRate = 0.02f;

        // Original gradient and critical point data, untouched after loading
        private List<GradientDataset> orjGradientList = new List<GradientDataset>();
        private List<CriticalPointDataset> orjCriticalPointList = new List<CriticalPointDataset>();

        // Transformed versions of the datasets, scaled and centered for visualization
        private List<GradientDataset> gradientList = new List<GradientDataset>();
        private List<CriticalPointDataset> criticalPointList = new List<CriticalPointDataset>();
        private Vector3? centroid = null;

        public float maxMag = 0; // Maximum gradient magnitude used for normalization or color mapping

        private void Awake()
        {
            instance = this; // Singleton assignment

            // Load original gradient data and calculate maximum magnitude
            orjGradientList = TTKCalculations.GetGradientAllVectorField();
            maxMag = orjGradientList.Max(x => x.Magnitude);

            // Load original critical point data
            orjCriticalPointList = TTKCalculations.GetCriticalPointAllVectorField();
        }

        /// <summary>
        /// Returns the transformed gradient list, computing it if not cached.
        /// Applies centroid centering and scaling to fit visualization space.
        /// </summary>
        public List<GradientDataset> GetGradientList()
        {
            if (gradientList.Any())
                return gradientList;

            Vector3 centroid = ComputeCentroid();
            foreach (var gradient in orjGradientList)
            {
                Vector3 newPosition = (gradient.Position - centroid) * scaleRate; // Normalize to local space
                GradientDataset localGradient = new GradientDataset(
                    gradient.ID,
                    newPosition,
                    gradient.Direction,
                    gradient.Magnitude
                );

                gradientList.Add(localGradient);
            }
            return gradientList;
        }

        /// <summary>
        /// Returns the transformed critical point list, computing it if not cached.
        /// Applies centroid centering and scaling.
        /// </summary>
        public List<CriticalPointDataset> GetCriticalPointList()
        {
            if (criticalPointList.Any())
                return criticalPointList;

            Vector3 centroid = ComputeCentroid();

            foreach (var criticalPoint in orjCriticalPointList)
            {
                Vector3 newPosition = (criticalPoint.Position - centroid) * scaleRate;

                CriticalPointDataset localCriticalPoint = new CriticalPointDataset(
                    criticalPoint.ID,
                    criticalPoint.Type,
                    newPosition
                );

                criticalPointList.Add(localCriticalPoint);
            }

            return criticalPointList;
        }

        /// <summary>
        /// Computes the geometric center (centroid) of all gradient positions.
        /// Used for centering the dataset in the scene.
        /// </summary>
        private Vector3 ComputeCentroid()
        {
            if (centroid != null)
                return (Vector3)centroid;

            Vector3 sum = Vector3.zero;
            foreach (var gradient in orjGradientList)
            {
                sum += gradient.Position;
            }

            centroid = sum / orjGradientList.Count;
            return (Vector3)centroid;
        }
    }
}
