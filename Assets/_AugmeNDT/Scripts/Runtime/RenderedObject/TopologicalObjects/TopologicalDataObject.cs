using Assets.Scripts.DataStructure;
using Microsoft.MixedReality.Toolkit.Experimental.Physics;
using System.Collections;
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
        public TopologyConfigData config;
        public TTKCalculations ttkCalculation;
        public Transform volumeTransform;

        // Transformed versions of the datasets, scaled and centered for visualization
        public List<GradientDataset> gradientList = new List<GradientDataset>();
        public List<CriticalPointDataset> criticalPointList = new List<CriticalPointDataset>();
        public float maxMag = 0; // Maximum gradient magnitude used for normalization or color mapping

        private string path;
        private Vector3 originalDimensions;
        private Vector3 lastVolumePosition;
        private Vector3 lastVolumeScale;
        public Quaternion lastVolumeRotation;

        public Vector3 min3D = new Vector3(0, 0, 5);
        public Vector3 max3D = new Vector3(24, 8, 40);

        private void Awake()
        {
            instance = this; // Singleton assignment
            volumeTransform = GameObject.Find("Volume").transform;
            lastVolumePosition = volumeTransform.position;
            lastVolumeScale = volumeTransform.lossyScale;
            lastVolumeRotation = volumeTransform.rotation;

            LoadTopologyConfiguration();

            ttkCalculation = new TTKCalculations();
            ttkCalculation.mhdPath = path;

            Initialize();
        }

        public void UpdateData()
        {
            // DON'T CHANGE THE ORDER
            UpdateDataPositions();
            UpdateDataRotation();
            UpdateDataScale();
        }

        private void UpdateDataPositions()
        {
            Vector3 positionDelta = volumeTransform.position - lastVolumePosition;
            if (volumeTransform.position == lastVolumePosition)
                return;

            // Update gradient list using parallel processing
            System.Threading.Tasks.Parallel.ForEach(gradientList, gradient => {
                gradient.Position += positionDelta;
            });

            // Update critical point list using parallel processing
            System.Threading.Tasks.Parallel.ForEach(criticalPointList, criticalPoint => {
                criticalPoint.Position += positionDelta;
            });

            // Update min3D and max3D vectors in the same way
            min3D += positionDelta;
            max3D += positionDelta;

            lastVolumePosition = volumeTransform.position;
        }

        private void UpdateDataScale()
        {
            if (volumeTransform.lossyScale == lastVolumeScale)
                return;

            Vector3 scaleRatio = new Vector3(
                volumeTransform.lossyScale.x / lastVolumeScale.x,
                volumeTransform.lossyScale.y / lastVolumeScale.y,
                volumeTransform.lossyScale.z / lastVolumeScale.z
            );

            Vector3 volumeCenter = volumeTransform.position;

            System.Threading.Tasks.Parallel.ForEach(gradientList, gradient => {
                // Calculate position relative to center
                Vector3 relativePos = gradient.Position - volumeCenter;

                // Scale the relative position
                relativePos.x *= scaleRatio.x;
                relativePos.y *= scaleRatio.y;
                relativePos.z *= scaleRatio.z;

                // Set the new position
                gradient.Position = volumeCenter + relativePos;

                // Scale the direction vector - FIXED
                Vector3 newDirection = new Vector3(
                    gradient.Direction.x * scaleRatio.x,
                    gradient.Direction.y * scaleRatio.y,
                    gradient.Direction.z * scaleRatio.z
                );
                gradient.Direction = newDirection;

                // Calculate new magnitude
                gradient.Magnitude = gradient.Direction.magnitude;
            });

            // Critical point update remains the same
            System.Threading.Tasks.Parallel.ForEach(criticalPointList, criticalPoint => {
                Vector3 relativePos = criticalPoint.Position - volumeCenter;
                relativePos.x *= scaleRatio.x;
                relativePos.y *= scaleRatio.y;
                relativePos.z *= scaleRatio.z;
                criticalPoint.Position = volumeCenter + relativePos;
            });

            // Update min3D and max3D vectors
            // Calculate position of min3D relative to center
            Vector3 minRelativePos = min3D - volumeCenter;
            // Scale the relative position
            minRelativePos.x *= scaleRatio.x;
            minRelativePos.y *= scaleRatio.y;
            minRelativePos.z *= scaleRatio.z;
            // Set the new min3D position
            min3D = volumeCenter + minRelativePos;

            // Calculate position of max3D relative to center
            Vector3 maxRelativePos = max3D - volumeCenter;
            // Scale the relative position
            maxRelativePos.x *= scaleRatio.x;
            maxRelativePos.y *= scaleRatio.y;
            maxRelativePos.z *= scaleRatio.z;
            // Set the new max3D position
            max3D = volumeCenter + maxRelativePos;

            // Update maximum magnitude value
            maxMag = 0;
            foreach (var gradient in gradientList)
            {
                maxMag = maxMag >= gradient.Magnitude ? maxMag : gradient.Magnitude;
            }

            // Update scale last
            lastVolumeScale = volumeTransform.lossyScale;
        }

        private void UpdateDataRotation()
        {
            // Check if rotation has changed
            if (Quaternion.Angle(volumeTransform.rotation, lastVolumeRotation) < 0.1f)
                return;

            // Calculate rotation delta
            Quaternion rotationDelta = volumeTransform.rotation * Quaternion.Inverse(lastVolumeRotation);
            Vector3 volumeCenter = volumeTransform.position;

            // Update gradient positions and directions with rotation
            System.Threading.Tasks.Parallel.ForEach(gradientList, gradient => {
                // Calculate relative position from center
                Vector3 relativePos = gradient.Position - volumeCenter;

                // Apply rotation to relative position
                relativePos = rotationDelta * relativePos;

                // Update position
                gradient.Position = volumeCenter + relativePos;

                // Apply same rotation to direction vector
                gradient.Direction = rotationDelta * gradient.Direction;
            });

            // Update critical points with rotation
            System.Threading.Tasks.Parallel.ForEach(criticalPointList, criticalPoint => {
                Vector3 relativePos = criticalPoint.Position - volumeCenter;
                relativePos = rotationDelta * relativePos;
                criticalPoint.Position = volumeCenter + relativePos;
            });

            // Update min3D and max3D vectors
            Vector3 minRelativePos = min3D - volumeCenter;
            minRelativePos = rotationDelta * minRelativePos;
            min3D = volumeCenter + minRelativePos;

            Vector3 maxRelativePos = max3D - volumeCenter;
            maxRelativePos = rotationDelta * maxRelativePos;
            max3D = volumeCenter + maxRelativePos;

            // Update last rotation
            lastVolumeRotation = volumeTransform.rotation;
        }

        private void LoadTopologyConfiguration()
        {
            string configPath = System.IO.Path.Combine(Application.streamingAssetsPath, "topologyConfig.json");
            if (System.IO.File.Exists(configPath))
            {
                string jsonData = System.IO.File.ReadAllText(configPath);
                config = JsonUtility.FromJson<TopologyConfigData>(jsonData);
            }
            else
            {
                // Create a new configuration with default values
                config = new TopologyConfigData();
                string jsonData = JsonUtility.ToJson(config, true); // true = pretty formatting
                try
                {
                    System.IO.File.WriteAllText(configPath, jsonData);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }

            path = config.mhdPath;
            originalDimensions = config.mhdDimension;
        }

        private void Initialize()
        {
            ConvertData(ttkCalculation.GetGradientAllVectorField(), ttkCalculation.GetCriticalPointAllVectorField());
            ConvertMinMax3DVectors();
            TDAHandler.instance.ActivateTDAScenes(config);
        }

        private void ConvertData(List<GradientDataset> orjGradientList, List<CriticalPointDataset> orjCriticalPointDataset)
        {
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

                // Adjust direction vector according to world scale
                Vector3 scaledDirection = new Vector3(
                    gradient.Direction.x * volumeWorldScale.x / originalDimensions.x,
                    gradient.Direction.y * volumeWorldScale.y / originalDimensions.y,
                    gradient.Direction.z * volumeWorldScale.z / originalDimensions.z
                );

                // Calculate new magnitude from scaled direction vector
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
        /// <summary>
        /// Converts Min3D and Max3D vectors from raw data coordinates to world coordinates
        /// </summary>
        private void ConvertMinMax3DVectors()
        {
            // Volume's world scale and position
            Vector3 volumeWorldScale = volumeTransform.lossyScale;
            Vector3 volumePosition = volumeTransform.position;

            // Normalize Min3D (convert to 0-1 range)
            Vector3 normalizedMin = new Vector3(
                min3D.x / originalDimensions.x,
                min3D.y / originalDimensions.y,
                min3D.z / originalDimensions.z
            );

            // Normalize Max3D (convert to 0-1 range)
            Vector3 normalizedMax = new Vector3(
                max3D.x / originalDimensions.x,
                max3D.y / originalDimensions.y,
                max3D.z / originalDimensions.z
            );

            // Convert normalized coordinates to world scale
            Vector3 scaledMin = new Vector3(
                normalizedMin.x * volumeWorldScale.x,
                normalizedMin.y * volumeWorldScale.y,
                normalizedMin.z * volumeWorldScale.z
            );

            Vector3 scaledMax = new Vector3(
                normalizedMax.x * volumeWorldScale.x,
                normalizedMax.y * volumeWorldScale.y,
                normalizedMax.z * volumeWorldScale.z
            );

            // Add world position of the Volume and adjust according to center position
            Vector3 finalMin = volumePosition + scaledMin - (volumeWorldScale / 2f);
            Vector3 finalMax = volumePosition + scaledMax - (volumeWorldScale / 2f);

            // Assign converted values
            min3D = finalMin;
            max3D = finalMax;
        }
        /// <summary>
        /// Calculates the optimal scale rate for gradient calculations based on the volume dimensions.
        /// This scale rate is used to determine the appropriate grid density for visualizations.
        /// </summary>
        /// <returns>A scale rate value optimized for the current volume dimensions</returns>
        public float GetOptimalScaleRateToCalculation()
        {
            // Calculate the ratio between world scale and original dimensions
            Vector3 volumeWorldScale = volumeTransform.lossyScale;

            // Get the average scale across all three dimensions
            // This provides a balanced approach rather than using only min or max values
            float averageScale = (volumeWorldScale.x / originalDimensions.x +
                                 volumeWorldScale.y / originalDimensions.y +
                                 volumeWorldScale.z / originalDimensions.z) / 3f;

            // Calculate a scale rate inversely proportional to the average scale
            // As the volume size increases, we want a smaller scale rate (denser grid)
            // This ensures appropriate visualization detail at any volume scale
            float scaleRate = 0.05f / averageScale;

            // Clamp the value to a reasonable range to prevent extremely sparse or dense grids
            // This ensures calculation performance while maintaining visualization quality
            return Mathf.Clamp(scaleRate, 0.01f, 0.1f);
        }
    }
}