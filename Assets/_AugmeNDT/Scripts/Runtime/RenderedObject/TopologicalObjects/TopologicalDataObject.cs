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

        // Transformed versions of the datasets, scaled and centered for visualization
        public List<GradientDataset> gradientList = new List<GradientDataset>();
        public List<CriticalPointDataset> criticalPointList = new List<CriticalPointDataset>();
        public float maxMag = 0; // Maximum gradient magnitude used for normalization or color mapping

        public string path;
        public Vector3 originalDimensions = new Vector3(24, 16, 45);

        private void Awake()
        {
            instance = this; // Singleton assignment
        }

        private void Start()
        {
            ConvertData(TTKCalculations.GetGradientAllVectorField(), TTKCalculations.GetCriticalPointAllVectorField());
            //TTKCalculations.SaveGradientListToCSV(@"C:\Users\ozdag\OneDrive\Desktop\smallDATA\result.csv", gradientList);
            TDAMenu.instance.ActivateTDAInfoPanel();
        }

        private void ConvertData(List<GradientDataset> orjGradientList, List<CriticalPointDataset> orjCriticalPointDataset)
        {
            Transform volumeTransform = GameObject.Find("DataVisGroup_0/fibers.raw/Volume").transform;

            // Get the world scale of the volume
            Vector3 volumeWorldScale = volumeTransform.lossyScale; // Actual scale in world space
            Vector3 volumePosition = volumeTransform.position;

            // Transform gradient data
            List<GradientDataset> scaledGradients = new List<GradientDataset>();
            foreach (var gradient in orjGradientList)
            {
                // Normalize position (convert to 0-1 range)
                Vector3 normalizedPos = new Vector3(
                    gradient.Position.x / originalDimensions.x,
                    gradient.Position.y / originalDimensions.y,
                    gradient.Position.z / originalDimensions.z
                );

                // Convert normalized position to world scale
                // Take into account the size of the Volume and fibers.raw
                Vector3 scaledPos = new Vector3(
                    normalizedPos.x * volumeWorldScale.x,
                    normalizedPos.y * volumeWorldScale.y,
                    normalizedPos.z * volumeWorldScale.z
                );

                // Direction vektörünü dünya ölçeğine göre ayarla
                Vector3 scaledDirection = new Vector3(
                    gradient.Direction.x * volumeWorldScale.x / originalDimensions.x,
                    gradient.Direction.y * volumeWorldScale.y / originalDimensions.y,
                    gradient.Direction.z * volumeWorldScale.z / originalDimensions.z
                );

                // Ölçeklendirilen yön vektöründen yeni magnitude hesapla
                float scaledMagnitude = scaledDirection.magnitude;

                // Add the world-space position of the Volume
                Vector3 finalPos = volumePosition + scaledPos - (volumeWorldScale / 2f); // Adjust from center position

                // New gradient object
                scaledGradients.Add(new GradientDataset(
                    gradient.ID,
                    finalPos,
                    scaledDirection,
                    scaledMagnitude
                ));

                maxMag = maxMag >= gradient.Magnitude ? maxMag : gradient.Magnitude;
            }
            
            // Transform critical points
            List<CriticalPointDataset> scaledCriticalPoints = new List<CriticalPointDataset>();
            foreach (var criticalPoint in orjCriticalPointDataset)
            {
                // Normalize position (convert to 0-1 range)
                Vector3 normalizedPos = new Vector3(
                    criticalPoint.Position.x / originalDimensions.x,
                    criticalPoint.Position.y / originalDimensions.y,
                    criticalPoint.Position.z / originalDimensions.z
                );

                // Convert normalized position to world scale
                Vector3 scaledPos = new Vector3(
                    normalizedPos.x * volumeWorldScale.x,
                    normalizedPos.y * volumeWorldScale.y,
                    normalizedPos.z * volumeWorldScale.z
                );

                // Add the world-space position of the Volume
                Vector3 finalPos = volumePosition + scaledPos - (volumeWorldScale / 2f); // Adjust from center position

                // Create new critical point object
                scaledCriticalPoints.Add(new CriticalPointDataset(
                    criticalPoint.ID,
                    criticalPoint.Type,
                    finalPos
                ));
            }

            gradientList = scaledGradients;
            criticalPointList = scaledCriticalPoints;
        }
    }
}
