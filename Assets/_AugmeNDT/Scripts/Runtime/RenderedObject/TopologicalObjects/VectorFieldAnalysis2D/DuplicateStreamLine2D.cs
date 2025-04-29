using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit;
using static UnityEngine.UI.GridLayoutGroup;
using Unity.VisualScripting;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering.VirtualTexturing;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

namespace AugmeNDT
{
    /// <summary>
    /// Duplicates streamlines from StreamLine2D to another location with interactive features
    /// </summary>
    public class DuplicateStreamLine2D : MonoBehaviour
    {
        // Singleton pattern - ensures only one instance exists in the scene
        public static DuplicateStreamLine2D instance;

        // Reference to the pool for interactive streamline objects
        private static InteractiveStreamLineObjectPool interactiveStreamLinePool;

        #region Duplicate Settings
        [Header("Duplicate Settings")]
        [Tooltip("Offset position for the duplicate container")]
        // Determines where the duplicate streamlines appear relative to the original
        private Vector3 positionOffset = new Vector3(0.5f, 0, 0);

        [Tooltip("Scale factor for the duplicate container")]
        // Scales the duplicate streamlines - Vector3.one means no scaling
        private Vector3 scaleModifier = Vector3.one;

        [Tooltip("Apply rotation to duplicate streamlines")]
        // Rotates the duplicate streamlines around each axis (x, y, z)
        private Vector3 rotationAngles = Vector3.zero;
        #endregion

        #region Interactive Features
        [Header("Interactive Features")]
        [Tooltip("Color when selected")]
        // Color to highlight streamlines when they are selected by the user
        [SerializeField] private Color selectedColor = Color.green;

        [Tooltip("Thickness multiplier when selected")]
        // How much thicker the streamline gets when selected (1.2 = 20% thicker)
        [SerializeField] private float selectedWidthMultiplier = 1.2f;

        [Tooltip("Size factor for collider capsules relative to line width")]
        // Controls the size of colliders relative to the streamline thickness
        [SerializeField] private float capsuleRadiusFactor = 0.8f;

        [Tooltip("Distance between collider points")]
        // How closely spaced the colliders are along each streamline
        [SerializeField] private float colliderSpacing = 0.05f;
        #endregion

        #region Streamline Properties
        // Thickness of streamlines
        private float strlineWidth;

        // Single color for all streamlines
        private Color strlineColor;
        #endregion

        #region Data Storage
        // Container for duplicated streamlines
        private GameObject duplicateContainer;

        // Stores the offset positions of all duplicated streamlines
        private List<List<Vector3>> newStreamLines = new List<List<Vector3>>();

        // List of all duplicated streamline GameObjects
        private List<GameObject> dupObjs = new List<GameObject>();

        // Vector field data for particle flow visualization
        private List<GradientDataset> gradeints = new List<GradientDataset>();

        // List of currently selected streamline GameObjects
        private List<GameObject> selectedGameObjects = new List<GameObject>();

        // Reference to the data visualization group
        private Transform dataVisGroup;
        #endregion

        private BoundsControl boundsControl;

        #region Unity Lifecycle Methods
        // Awake is called when the script instance is loaded
        private void Awake()
        {
            // Set this as the singleton instance
            instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Get reference to the object pool for efficient line creation
            interactiveStreamLinePool = InteractiveStreamLineObjectPool.Instance;

            // Find the data visualization group in the scene
            dataVisGroup = GameObject.Find("DataVisGroup_0").transform;

            // Create the container that will hold all duplicated streamlines
            CreateContainer();
        }
        #endregion

        #region Container Management
        /// <summary>
        /// Creates and configures the container that will hold all duplicated streamlines
        /// </summary>
        private void CreateContainer()
        {
            // Only create a new container if one doesn't already exist
            if (duplicateContainer == null)
            {
                // Create a new GameObject to act as the container
                duplicateContainer = new GameObject("DuplicateStreamLinesContainer");

                // Parent it to the rectangle manager's volume transform
                duplicateContainer.transform.SetParent(GameObject.Find("DataVisGroup_0").transform, true);
            }
        }

        /// <summary>
        /// Updates the position of the container based on bounds and corners
        /// </summary>
        /// <param name="normal">Normal vector of the plane</param>
        /// <param name="bounds">Bounding box of the original streamlines</param>
        /// <param name="corners">Array of corner points</param>
        private void UpdateContainerPosition(Vector3 normal, Bounds bounds, Vector3[] corners)
        {
            // Calculate the center position from the four corners
            Vector3 position = (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;

            // Move the container to this position plus the offset
            duplicateContainer.transform.position = position + positionOffset;
            SetupBoundsControl(duplicateContainer);
        }
        #endregion

        #region Streamline Update Methods
        /// <summary>
        /// Main method to update and duplicate streamlines
        /// </summary>
        /// <param name="normal">Normal vector of the plane</param>
        /// <param name="bounds">Bounding box</param>
        /// <param name="corners">Corner points</param>
        /// <param name="streamLines">Original streamline points</param>
        /// <param name="streamlineWidth">Width of streamlines</param>
        /// <param name="streamlineColor">Color of streamlines</param>
        /// <param name="vectorfield">Vector field data for particle flow</param>
        public void UpdateDuplicateStreamlines(Vector3 normal, Bounds bounds, Vector3[] corners,
            List<List<Vector3>> streamLines, float streamlineWidth, Color streamlineColor, List<GradientDataset> vectorfield)
        {
            // Return all existing lines to the object pool
            ReturnAllLinesToPool();

            // Clear existing data structures
            newStreamLines.Clear();
            StopImmediatelyAllFlow();
            gradeints.Clear();

            // Update the container position
            UpdateContainerPosition(normal, bounds, corners);

            // Create offset copies of all streamlines
            foreach (var itemList in streamLines)
            {
                List<Vector3> newItem = new List<Vector3>();

                // Apply offset to each point in the streamline
                foreach (var item in itemList)
                {
                    newItem.Add(item + positionOffset);
                }
                newStreamLines.Add(newItem);
            }

            // Create offset copies of vector field data for particle flow
            foreach (var vector in vectorfield)
            {
                GradientDataset newVector = new GradientDataset(
                    vector.ID,
                    vector.Position + positionOffset,  // Apply offset to position
                    vector.Direction,
                    vector.Magnitude
                    );

                gradeints.Add(newVector);
            }

            // Store streamline appearance properties
            strlineColor = streamlineColor;
            strlineWidth = streamlineWidth;

            // Start creating streamlines in batches to avoid performance spikes
            StartCoroutine(CreateStreamlinesInBatches(newStreamLines, 10));
        }

        /// <summary>
        /// Creates streamlines in batches over multiple frames to prevent performance spikes
        /// </summary>
        /// <param name="streamLines">List of streamline point sequences</param>
        /// <param name="batchSize">Number of streamlines to create per frame</param>
        /// <returns>IEnumerator for coroutine processing</returns>
        private IEnumerator CreateStreamlinesInBatches(List<List<Vector3>> streamLines, int batchSize)
        {
            int totalCount = streamLines.Count;
            int processedCount = 0;

            // Process streamlines in batches over multiple frames
            while (processedCount < totalCount)
            {
                // Create batchSize streamlines each frame
                int currentBatchSize = Mathf.Min(batchSize, totalCount - processedCount);

                for (int i = 0; i < currentBatchSize; i++)
                {
                    CreateLineRenderer(streamLines[processedCount + i]);
                }

                processedCount += currentBatchSize;

                // Wait for the next frame
                yield return null;
            }

            // After all streamlines are created, start the flow visualization
            StartFlowObject();
        }

        /// <summary>
        /// Creates a LineRenderer GameObject for visualizing a streamline
        /// </summary>
        /// <param name="points">List of points defining the streamline path</param>
        private void CreateLineRenderer(List<Vector3> points)
        {
            // Skip streamlines with too few points
            if (points.Count < 2)
                return;

            // Get line object from the pool for efficient reuse
            GameObject lineObj = interactiveStreamLinePool.GetPooledObject();
            lineObj.name = "InteractiveStreamline_" + points.First().ToSafeString();
            lineObj.transform.SetParent(duplicateContainer.transform, true);

            // Configure the LineRenderer component
            LineRenderer lr = lineObj.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;

            // Set up line geometry
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.startWidth = lr.endWidth = strlineWidth;

            // Configure line appearance for 2D visualization
            lr.alignment = LineAlignment.View;      // Face the camera
            lr.numCapVertices = lr.numCornerVertices = 5;   // Smooth ends and corners

            // Set line color
            lr.startColor = lr.endColor = strlineColor;

            // Add collision detection along the line
            AddOptimizedColliders(lineObj, points);

            // Add to the list of duplicated objects
            dupObjs.Add(lineObj);
        }
        #endregion

        #region Interaction Methods
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
            boundsColl.size = new Vector3(0.15f, 0.0001f, 0.23f); // Default plane size with reduced height
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

            boundsControl.enabled = false;
            Debug.Log("BoundsControl initially disabled due to proximity settings");

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

            manipulator.OnManipulationEnded.AddListener(OnMovingEnded);
            manipulator.OnManipulationStarted.AddListener(OnMovingStarted);
        }

        private void OnMovingStarted(ManipulationEventData eventData)
        {
            foreach (var item in selectedGameObjects)
            {
                LineRenderer lr = item.GetComponent<LineRenderer>();
                lr.startColor = lr.endColor = strlineColor;
                lr.startWidth = lr.endWidth = strlineWidth;
            }

            StopImmediatelyAllFlow();
        }

        private void OnMovingEnded(ManipulationEventData eventData)
        {
            StartFlowObject();
        }

        /// <summary>
        /// Adds optimized capsule colliders along the line for better interaction
        /// </summary>
        private void AddOptimizedColliders(GameObject lineObj, List<Vector3> points)
        {
            // Create container for interaction points
            GameObject collidersContainer = new GameObject("LineColliders");
            collidersContainer.transform.SetParent(lineObj.transform, false);

            // Calculate collider radius based on line width
            float radius = strlineWidth * capsuleRadiusFactor;

            // Create colliders along the line segments
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 start = points[i];
                Vector3 end = points[i + 1];
                Vector3 direction = end - start;
                float segmentLength = direction.magnitude;

                // Skip very short segments
                if (segmentLength < 0.001f)
                    continue;

                // Calculate how many colliders to place on this segment
                int colliderCount = Mathf.Max(1, Mathf.FloorToInt(segmentLength / colliderSpacing));
                float step = segmentLength / colliderCount;

                // Create colliders evenly spaced along the segment
                for (int j = 0; j < colliderCount; j++)
                {
                    float t = (j + 0.5f) / colliderCount; // Position at middle of each sub-segment
                    Vector3 position = Vector3.Lerp(start, end, t);

                    // Create collider GameObject
                    GameObject colliderObj = new GameObject($"Collider_{i}_{j}");
                    colliderObj.transform.SetParent(collidersContainer.transform, false);
                    colliderObj.transform.localPosition = position;

                    // Orient collider along the line segment
                    colliderObj.transform.LookAt(colliderObj.transform.position + direction);

                    // Add box collider
                    BoxCollider boxCollider = colliderObj.AddComponent<BoxCollider>();
                    boxCollider.size = new Vector3(radius * 2, radius * 2, step * 1.1f);

                    // Add near interaction grabbable for hand input
                    colliderObj.AddComponent<NearInteractionGrabbable>();

                    // Add touchable volume for hand interactions
                    colliderObj.AddComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionTouchableVolume>();

                    // Make the collider object trigger the selection of its parent line
                    Microsoft.MixedReality.Toolkit.UI.Interactable interactable = colliderObj.AddComponent<Microsoft.MixedReality.Toolkit.UI.Interactable>();
                    interactable.OnClick.AddListener(() => ToggleSelection(lineObj));
                }
            }
        }

        /// <summary>
        /// Toggles the selection state of a streamline and updates its appearance
        /// </summary>
        private void ToggleSelection(GameObject lineObj)
        {
            bool newSelectionState = true;

            // Toggle the selection state
            if (selectedGameObjects.Contains(lineObj))
            {
                selectedGameObjects.Remove(lineObj);
                newSelectionState = false;
            }
            else
            {
                selectedGameObjects.Add(lineObj);
            }

            // Update the line's appearance based on selection state
            LineRenderer lr = lineObj.GetComponent<LineRenderer>();
            if (newSelectionState)
            {
                // Change to selected color and increase width
                lr.startColor = lr.endColor = selectedColor;
                lr.startWidth = lr.endWidth = strlineWidth * selectedWidthMultiplier;
            }
            else
            {
                // Revert to original color and width
                lr.startColor = lr.endColor = strlineColor;
                lr.startWidth = lr.endWidth = strlineWidth;
            }
        }

        /// <summary>
        /// Returns all line objects to the pool for reuse
        /// </summary>
        private void ReturnAllLinesToPool()
        {
            if (interactiveStreamLinePool != null)
            {
                foreach (var obj in dupObjs)
                {
                    interactiveStreamLinePool.ReturnToPool(obj);
                }
                dupObjs.Clear();
            }
        }
        #endregion

        #region Flow Visualization
        /// <summary>
        /// Flow settings for particle visualization
        /// </summary>
        private Transform flowContainer = null;     // Container for flow particles
        private bool stopFlowObjects = true;        // Flag to stop flow animation
        private float sphereSpeed = 0.01f;          // Speed of flowing particles
        private float lifetime = 15f;               // Lifetime of each particle
        private int numSpheres = 100;               // Number of particles per streamline
        private Vector3 localScale = Vector3.one * 0.003f;  // Scale of flow particles
        private GameObject spherePrefab;            // Sphere prefab for particles

        /// <summary>
        /// Initializes and starts the flow object system
        /// </summary>
        private void StartFlowObject()
        {
            // Initial setup
            if (flowContainer == null)
            {
                flowContainer = new GameObject("Interactive2DMovingSpheres").transform;
                flowContainer.parent = dataVisGroup;
                spherePrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/TopologicalVis/Interactive2DFLOW");
            }

            stopFlowObjects = false;
            StartCoroutine(SpawnMovingSpheres());
        }

        /// <summary>
        /// Manages spawning and lifecycle of moving spheres along selected streamlines
        /// </summary>
        private IEnumerator SpawnMovingSpheres()
        {
            // Dictionary to track spheres for each selected streamline
            Dictionary<GameObject, List<GameObject>> lineToSpheres = new Dictionary<GameObject, List<GameObject>>();

            while (true)
            {
                if (stopFlowObjects)
                    break;

                // Initialize dictionary to track spheres for each line
                foreach (var obj in selectedGameObjects)
                {
                    if (!lineToSpheres.ContainsKey(obj))
                    {
                        lineToSpheres[obj] = new List<GameObject>();
                    }
                }

                // Clean up any references to destroyed spheres
                foreach (var kvp in lineToSpheres.ToList())
                {
                    kvp.Value.RemoveAll(sphere => sphere == null);
                }

                // Process each selected object
                foreach (var lineObj in selectedGameObjects)
                {
                    LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();

                    if (lineRenderer != null && lineRenderer.positionCount > 0)
                    {
                        // Check if we need to spawn spheres for this line
                        List<GameObject> currentSpheres = lineToSpheres[lineObj];

                        // Calculate how many spheres to spawn
                        int spheresToSpawn = numSpheres - currentSpheres.Count;

                        if (spheresToSpawn > 0)
                        {
                            // Get start position from the beginning of the streamline
                            Vector3 startPosition = lineRenderer.GetPosition(0);

                            // Convert local position to world position
                            startPosition = lineObj.transform.TransformPoint(startPosition);

                            // Get all points from the streamline to pass to the flow object
                            Vector3[] streamlinePoints = new Vector3[lineRenderer.positionCount];
                            lineRenderer.GetPositions(streamlinePoints);

                            // Convert local positions to world positions
                            for (int j = 0; j < streamlinePoints.Length; j++)
                            {
                                streamlinePoints[j] = lineObj.transform.TransformPoint(streamlinePoints[j]);
                            }

                            // Spawn the needed number of spheres
                            for (int i = 0; i < spheresToSpawn; i++)
                            {
                                // Instantiate sphere at selected position
                                GameObject sphere = Instantiate(spherePrefab, startPosition, Quaternion.identity);
                                sphere.transform.parent = flowContainer;
                                sphere.transform.localScale = localScale; // Uniform small scale
                                sphere.tag = "Interactive2DSphere"; // Tag for tracking spheres

                                // Add reference to the tracking dictionary
                                lineToSpheres[lineObj].Add(sphere);

                                // Initialize flow behavior on the sphere, passing the streamline points
                                StreamlineFlowObject2D movingSphere = sphere.GetComponent<StreamlineFlowObject2D>();
                                movingSphere.StartFlowAlongStreamline(streamlinePoints, gradeints,
                                    StreamLine2D.Instance.streamLineStepSize, sphereSpeed, lifetime);
                            }
                        }
                    }
                }

                // Remove entries for lines that are no longer selected
                foreach (var key in lineToSpheres.Keys.ToList())
                {
                    if (!selectedGameObjects.Contains(key))
                    {
                        // Destroy any remaining spheres for this line
                        foreach (var sphere in lineToSpheres[key])
                        {
                            if (sphere != null)
                            {
                                Destroy(sphere);
                            }
                        }
                        lineToSpheres.Remove(key);
                    }
                }

                // Wait before checking sphere count again
                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Immediately stops all flow visualization and cleans up
        /// </summary>
        private void StopImmediatelyAllFlow()
        {
            stopFlowObjects = true;
            selectedGameObjects.Clear();

            // Find and destroy all flow particles
            var objs = GameObject.FindGameObjectsWithTag("Interactive2DSphere");
            foreach (var obj in objs)
            {
                DestroyImmediate(obj);
            }
        }
        #endregion
    }
}