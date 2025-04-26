using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages topological data, including critical points and gradient information.
    /// Loads data from CSV files and makes it accessible to visualization components.
    /// Handles transformations for rotation, position and scale changes.
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

        private string path;
        private Vector3 originalDimensions;
        private Vector3 lastVolumePosition;
        private Vector3 lastVolumeScale;
        private Quaternion lastVolumeRotation;

        public Vector3 min3D = new Vector3(0, 0, 5);
        public Vector3 max3D = new Vector3(24, 8, 40);

        // Store initial local positions relative to volume for proper transformation
        private List<Vector3> gradientLocalPositions = new List<Vector3>();
        private List<Vector3> gradientLocalDirections = new List<Vector3>();
        private List<Vector3> criticalPointLocalPositions = new List<Vector3>();
        private Vector3 minLocalPosition;
        private Vector3 maxLocalPosition;

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
            // Check if volume transform has changed
            if (volumeTransform.position != lastVolumePosition ||
                volumeTransform.lossyScale != lastVolumeScale ||
                volumeTransform.rotation != lastVolumeRotation)
            {
                UpdateDataTransforms();

                // Store current transform values
                lastVolumePosition = volumeTransform.position;
                lastVolumeScale = volumeTransform.lossyScale;
                lastVolumeRotation = volumeTransform.rotation;
            }
        }

        private void UpdateDataTransforms()
        {
            // Calculate transform for all data points
            for (int i = 0; i < gradientList.Count; i++)
            {
                // Apply full transformation (scale, rotation, translation) for position
                gradientList[i].Position = CalculateWorldPosition(
                    volumeTransform,
                    gradientLocalPositions[i]
                );

                // For direction vectors, we only care about scale and rotation, not translation
                Vector3 scaledDirection = Vector3.Scale(gradientLocalDirections[i], volumeTransform.lossyScale);
                Vector3 worldDirection = volumeTransform.rotation * scaledDirection;
                gradientList[i].Direction = worldDirection;

                // Update magnitude from the new direction vector
                gradientList[i].Magnitude = worldDirection.magnitude;
            }

            // Update critical point positions
            for (int i = 0; i < criticalPointList.Count; i++)
            {
                criticalPointList[i].Position = CalculateWorldPosition(
                    volumeTransform,
                    criticalPointLocalPositions[i]
                );
            }

            // Update min3D and max3D vectors
            min3D = CalculateWorldPosition(volumeTransform, minLocalPosition);
            max3D = CalculateWorldPosition(volumeTransform, maxLocalPosition);
        }

        /// <summary>
        /// Calculates world position based on parent transform and local position
        /// </summary>
        private Vector3 CalculateWorldPosition(Transform parent, Vector3 localPosition)
        {
            // 1. Scale
            Vector3 scaledPosition = Vector3.Scale(localPosition, parent.lossyScale);

            // 2. Rotation
            Vector3 rotatedPosition = parent.rotation * scaledPosition;

            // 3. Translation
            Vector3 worldPosition = parent.position + rotatedPosition;

            return worldPosition;
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
            StoreLocalTransforms(); // Store local positions for future transform updates
            TDAHandler.instance.ActivateTDAScenes(config);
        }

        /// <summary>
        /// Store initial local positions relative to volume transform for accurate transformations
        /// </summary>
        private void StoreLocalTransforms()
        {
            // Clear previous data
            gradientLocalPositions.Clear();
            gradientLocalDirections.Clear();
            criticalPointLocalPositions.Clear();

            // Store local positions for gradients
            foreach (var gradient in gradientList)
            {
                // Calculate local position relative to volume
                Vector3 localPosition = volumeTransform.InverseTransformPoint(gradient.Position);
                gradientLocalPositions.Add(localPosition);

                // Calculate local direction
                // We need to convert from world to local space for the direction vector
                Vector3 localDirection = Quaternion.Inverse(volumeTransform.rotation) * gradient.Direction;
                // Remove scale influence
                localDirection.x /= volumeTransform.lossyScale.x;
                localDirection.y /= volumeTransform.lossyScale.y;
                localDirection.z /= volumeTransform.lossyScale.z;

                gradientLocalDirections.Add(localDirection);
            }

            // Store local positions for critical points
            foreach (var criticalPoint in criticalPointList)
            {
                Vector3 localPosition = volumeTransform.InverseTransformPoint(criticalPoint.Position);
                criticalPointLocalPositions.Add(localPosition);
            }

            // Store local positions for min3D and max3D
            minLocalPosition = volumeTransform.InverseTransformPoint(min3D);
            maxLocalPosition = volumeTransform.InverseTransformPoint(max3D);
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

                // Apply initial rotation if there is any
                finalPos = volumePosition + (volumeTransform.rotation * (scaledPos - (volumeWorldScale / 2f)));

                // Also adjust direction vector with rotation
                scaledDirection = volumeTransform.rotation * scaledDirection;

                // New gradient object
                scaledGradients.Add(new GradientDataset(
                    gradient.ID,
                    finalPos,
                    scaledDirection,
                    scaledMagnitude
                ));

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

                // Add the world-space position of the Volume and apply rotation
                Vector3 finalPos = volumePosition + (volumeTransform.rotation * (scaledPos - (volumeWorldScale / 2f)));

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

            // Add world position of the Volume, adjusting for center position and applying rotation
            Vector3 adjustedMin = scaledMin - (volumeWorldScale / 2f);
            Vector3 adjustedMax = scaledMax - (volumeWorldScale / 2f);

            // Apply rotation and then translate
            Vector3 finalMin = volumePosition + (volumeTransform.rotation * adjustedMin);
            Vector3 finalMax = volumePosition + (volumeTransform.rotation * adjustedMax);

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