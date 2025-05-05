using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the visualization of 2D vector fields using arrow glyphs
    /// </summary>
    public class Glyph2DVectorField : MonoBehaviour
    {
        public static Glyph2DVectorField Instance;

        // References to other manager instances
        private static VectorObjectVis arrowObjectVisInstance;
        private static RectangleManager rectangleManager;

        // List of arrow GameObjects representing vectors
        private List<GameObject> arrows = new List<GameObject>();

        // Cached gradient data for vector field visualization
        private List<GradientDataset> generatedGradientPoints = new List<GradientDataset>();

        // Container for organizing arrow objects in hierarchy
        private Transform container;

        private static int arrowsPerFrame = 50;

        [Tooltip("Offset position for the duplicate container")]
        public Vector3 positionOffset = new Vector3(-0.5f, 0, 0);

        [Tooltip("Scale factor for the duplicate container")]
        public Vector3 scaleModifier = Vector3.one;
        private BoundsControl boundsControl;

        [Header("Proximity Settings")]
        [Tooltip("Distance at which bounds control becomes visible (meters)")]
        public float proximityThreshold = 0.5f;

        [Tooltip("How frequently to check proximity (in seconds)")]
        public float proximityCheckInterval = 0.2f;

        [Tooltip("Whether to enable proximity-based visibility")]
        public bool useProximityBasedVisibility = true;

        private Transform dataVisGroup;
        private Camera mainCamera;
        private float proximityCheckTimer;

        /// <summary>
        /// Initializes the singleton instance
        /// </summary>
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Gets references to required managers and initializes configuration values
        /// </summary>
        private void Start()
        {
            arrowObjectVisInstance = VectorObjectVis.instance;
            rectangleManager = RectangleManager.rectangleManager;
            dataVisGroup = GameObject.Find("DataVisGroup_0").transform;
            mainCamera = Camera.main;

            // Initialize proximity timer
            proximityCheckTimer = 0f;
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            // Check proximity based on timer
            //if (useProximityBasedVisibility && boundsControl != null && container != null)
            //{
            //    proximityCheckTimer -= Time.deltaTime;
            //    if (proximityCheckTimer <= 0f)
            //    {
            //        CheckProximity();
            //        proximityCheckTimer = proximityCheckInterval;
            //    }
            //}
        }

        /// <summary>
        /// Checks if the camera is close enough to show the bounds control
        /// </summary>
        private void CheckProximity()
        {
            if (boundsControl != null && container != null && mainCamera != null)
            {
                // Calculate distance between camera and object
                float distance = Vector3.Distance(mainCamera.transform.position, container.position);

                // Debug log the distance
                Debug.Log($"Distance to glyph container: {distance}, Threshold: {proximityThreshold}");

                // Show bounds control only when close enough
                bool shouldBeVisible = distance < proximityThreshold;

                // Only update if the state has changed to avoid unnecessary updates
                if (boundsControl.enabled != shouldBeVisible)
                {
                    Debug.Log($"Setting bounds control visibility to: {shouldBeVisible}");
                    boundsControl.enabled = shouldBeVisible;
                }
            }
            else
            {
                Debug.LogWarning("Missing references for proximity check");
            }
        }

        /// <summary>
        /// Creates and displays arrow glyphs to visualize the vector field
        /// </summary>
        public void Visualize(Vector3 normal, Bounds bounds, Vector3[] corners, List<GradientDataset> gradients)
        {
            CreateGlyphContainer(normal, bounds, corners);

            DestroyArrows();

            foreach (var item in gradients)
            {
                var temp = item;
                temp.Position += positionOffset;
                generatedGradientPoints.Add(temp);
            }

            StartCoroutine(CreateArrowsCoroutine(rectangleManager.config.ColorOfVectorObject));

            // Force an initial visibility check
            proximityCheckTimer = 0f;
        }

        /// <summary>
        /// Creates arrows over multiple frames to prevent performance spikes
        /// </summary>
        private IEnumerator CreateArrowsCoroutine(Color color)
        {
            // Total number of arrows
            int totalArrows = generatedGradientPoints.Count;
            arrows = new List<GameObject>(totalArrows);

            for (int i = 0; i < totalArrows; i += arrowsPerFrame)
            {
                // Get the subset of gradientPoints to process in this frame
                List<GradientDataset> batchPoints = generatedGradientPoints
                    .Skip(i)
                    .Take(Mathf.Min(arrowsPerFrame, totalArrows - i))
                    .ToList();

                // Create arrows for this batch
                List<GameObject> batchArrows = arrowObjectVisInstance.CreateArrows(batchPoints, container, 0.4f, color);

                // Add to the main list
                arrows.AddRange(batchArrows);

                // Move to the next frame
                yield return null;
            }

            Debug.Log($"Created {arrows.Count} arrow glyphs");

            // Initialize proximity check after creating arrows
            if (useProximityBasedVisibility && boundsControl != null)
            {
                // Start with bounds control disabled
                boundsControl.enabled = false;
                Debug.Log("Initially hiding bounds control until proximity condition is met");
            }
        }

        /// <summary>
        /// Destroys all arrow objects and clears the list
        /// </summary>
        private void DestroyArrows()
        {
            arrows.ForEach(x => Destroy(x));
            arrows.Clear(); // Clear the list
            generatedGradientPoints.Clear();
        }

        /// <summary>
        /// Creates a container for the streamlines with bounds control
        /// </summary>
        private void CreateGlyphContainer(Vector3 normal, Bounds bounds, Vector3[] corners)
        {
            if (container != null)
            {
                DestroyImmediate(container.gameObject);
                container = null;
            }

            container = new GameObject("Duplicate_2DGlyphs").transform;
            container.transform.SetParent(dataVisGroup);

            // Calculate position based on bounds or corners
            Vector3 position;
            Vector2 planeSize;
            if (corners != null && corners.Length >= 4)
            {
                // Use average of corners for position
                position = (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;

                // Calculate size from corners
                float width = Vector3.Distance(corners[0], corners[1]);
                float height = Vector3.Distance(corners[0], corners[3]);
                planeSize = new Vector2(width / 10f, height / 10f); // Plane is 10 units in Unity scale
            }
            else
            {
                // Use bounds center plus offset
                position = bounds.center;

                // Calculate size from bounds
                planeSize = new Vector2(bounds.size.x / 10f, bounds.size.z / 10f);
            }

            // Position the plane
            container.transform.position = position + positionOffset;

            // Scale the plane
            container.transform.localScale = new Vector3(
                planeSize.x * scaleModifier.x,
                1.0f,
                planeSize.y * scaleModifier.z
            );

            // Setup for manipulation using MRTK's BoundsControl
            SetupBoundsControl(container.gameObject);

            Debug.Log($"Created glyph container at position: {container.position}");
        }

        /// <summary>
        /// Sets up the MRTK BoundsControl for manipulating the container
        /// </summary>
        private void SetupBoundsControl(GameObject targetObject)
        {
            // Replace default collider with a box collider
            Collider existingCollider = targetObject.GetComponent<Collider>();
            if (existingCollider != null)
            {
                DestroyImmediate(existingCollider);
            }

            // Add box collider
            BoxCollider boundsColl = targetObject.AddComponent<BoxCollider>();
            boundsColl.size = new Vector3(12f, 0.0001f, 12f); // Default plane size with reduced height
            boundsColl.center = Vector3.zero;

            // BoundsControl component
            boundsControl = targetObject.AddComponent<BoundsControl>();

            // Configure BoundsControl
            boundsControl.BoundsOverride = boundsColl;

            // Disable rotation handles (spheres)
            boundsControl.RotationHandlesConfig.ShowHandleForX = false;
            boundsControl.RotationHandlesConfig.ShowHandleForY = false;
            boundsControl.RotationHandlesConfig.ShowHandleForZ = false;

            // Disable translation handles (if any)
            boundsControl.TranslationHandlesConfig.ShowHandleForX = false;
            boundsControl.TranslationHandlesConfig.ShowHandleForY = false;
            boundsControl.TranslationHandlesConfig.ShowHandleForZ = false;

            // If using proximity-based visibility, start with bounds control disabled
            if (useProximityBasedVisibility)
            {
                boundsControl.enabled = false;
                Debug.Log("BoundsControl initially disabled due to proximity settings");
            }
            else
            {
                boundsControl.enabled = true;
                Debug.Log("BoundsControl enabled (proximity checking disabled)");
            }

            // Add NearInteractionGrabbable for direct manipulation
            targetObject.AddComponent<NearInteractionGrabbable>();

            // Optional: Add ObjectManipulator for additional manipulation options
            ObjectManipulator manipulator = targetObject.AddComponent<ObjectManipulator>();
            manipulator.AllowFarManipulation = true;
            manipulator.OneHandRotationModeNear = ObjectManipulator.RotateInOneHandType.RotateAboutObjectCenter;
            manipulator.OneHandRotationModeFar = ObjectManipulator.RotateInOneHandType.RotateAboutObjectCenter;

            // Enable two-handed manipulation
            manipulator.TwoHandedManipulationType =
                Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Move |
                Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Rotate |
                Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Scale;
        }
    }
}