using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the creation and flow of 2D sphere objects along gradient streamlines
    /// </summary>
    public class FlowObject2DManager : MonoBehaviour
    {
        public static FlowObject2DManager Instance;

        private static RectangleManager rectangleManager;
        private static StreamLine2D streamLine2DInstance;
        private static Transform container;

        public int numSpheres; // Number of spheres to maintain simultaneously
        private bool enableSpheres = true;
        private bool stopFlowObjects = true;
        private GameObject spherePrefab; // Sphere prefab
        private Vector3 parentScale;

        /// <summary>
        /// Config settings
        /// </summary>
        private float sphereSpeed;
        private float lifetime;
        private Vector3 localScale = Vector3.one;
        private float baseScale = 0.03f; // Base scale for spheres
        private bool useDynamicSize = true;

        /// <summary>
        /// Initializes the singleton instance and loads the sphere prefab resource
        /// </summary>
        private void Awake()
        {
            // Initialize singleton instance
            Instance = this;
            spherePrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/TopologicalVis/2dmovingSphere");
        }

        /// <summary>
        /// Gets references to other manager instances required for operation
        /// </summary>
        private void Start()
        {
            // Get references to other managers
            rectangleManager = RectangleManager.rectangleManager;
            streamLine2DInstance = StreamLine2D.Instance;
        }

        /// <summary>
        /// Starts the flow simulation by spawning moving spheres
        /// </summary>
        public void StartFlowObject()
        {
            SetContainer();
            stopFlowObjects = false;
            if (enableSpheres)
            {
                StartCoroutine(SpawnMovingSpheres());
            }
        }

        /// <summary>
        /// Creates and configures the container that holds all the flowing sphere objects
        /// Sets default values for sphere parameters if not already initialized
        /// </summary>
        private void SetContainer()
        {
            //Initial setup
            if (container == null)
            {

                sphereSpeed = 0.01f;
                lifetime = 15f;
                numSpheres = 8;
                localScale = Vector3.one * baseScale;

                container = new GameObject("2DMovingSpheres").transform;
                container.parent = rectangleManager.volumeTransform;

                // Scale inversely to parent to maintain consistent visual size
                parentScale = rectangleManager.volumeTransform.gameObject.transform.localScale;
                container.transform.localScale = new Vector3(
                    1f / parentScale.x,
                    1f / parentScale.y,
                    1f / parentScale.z
                );
            }

            if (useDynamicSize)
            {
                parentScale = rectangleManager.volumeTransform.gameObject.transform.localScale;
                localScale = new Vector3(
                    baseScale * parentScale.x,
                    baseScale * parentScale.y,
                    baseScale * parentScale.z
                );
            }
        }

        /// <summary>
        /// Pauses the flow simulation and destroys all currently active spheres
        /// </summary>
        public void PauseFlowObject()
        {
            if (stopFlowObjects == true)
                return;

            stopFlowObjects = true;
            GameObject.FindGameObjectsWithTag("2DMovingSphere").ToList().ForEach(x => Destroy(x));
        }

        /// <summary>
        /// Coroutine that handles spawning and maintaining the specified number of moving spheres
        /// </summary>
        /// <returns>IEnumerator for coroutine processing</returns>
        private IEnumerator SpawnMovingSpheres()
        {
            while (true)
            {
                if (stopFlowObjects)
                    break;

                // Get gradient data and boundary information
                List<GradientDataset> generatedGradientPoints = rectangleManager.GetGradientPoints();
                Bounds bounds = rectangleManager.GetRectangleBounds();

                // Count current active spheres
                int currentSpheres = GameObject.FindGameObjectsWithTag("2DMovingSphere").Length;

                // Spawn additional spheres if needed to maintain the target count
                if (currentSpheres < numSpheres && streamLine2DInstance.lineObjs.Count > 0)
                {
                    int spheresToSpawn = numSpheres - currentSpheres;

                    for (int i = 0; i < spheresToSpawn; i++)
                    {
                        // Get a random streamline from the available lines
                        GameObject randomLineObj = streamLine2DInstance.lineObjs[Random.Range(0, streamLine2DInstance.lineObjs.Count)];
                        LineRenderer lineRenderer = randomLineObj.GetComponent<LineRenderer>();

                        if (lineRenderer != null && lineRenderer.positionCount > 0)
                        {
                            // Get start position from the beginning of the streamline
                            Vector3 startPosition = lineRenderer.GetPosition(0);

                            // Convert local position to world position
                            startPosition = randomLineObj.transform.TransformPoint(startPosition);

                            // Get all points from the streamline to pass to the flow object
                            Vector3[] streamlinePoints = new Vector3[lineRenderer.positionCount];
                            lineRenderer.GetPositions(streamlinePoints);

                            // Convert local positions to world positions
                            for (int j = 0; j < streamlinePoints.Length; j++)
                            {
                                streamlinePoints[j] = randomLineObj.transform.TransformPoint(streamlinePoints[j]);
                            }

                            // Instantiate sphere at selected position
                            GameObject sphere = Instantiate(spherePrefab, startPosition, Quaternion.identity);
                            sphere.transform.parent = container;
                            sphere.transform.localScale = localScale; // Uniform small scale
                            sphere.tag = "2DMovingSphere"; // Tag for tracking spheres

                            // Initialize flow behavior on the sphere, passing the streamline points
                            FlowObject2D movingSphere = sphere.GetComponent<FlowObject2D>();
                            movingSphere.StartFlowAlongStreamline(streamlinePoints, generatedGradientPoints, bounds,
                                streamLine2DInstance.streamLineStepSize, sphereSpeed, lifetime);
                        }
                    }
                }

                // Wait before checking sphere count again
                yield return new WaitForSeconds(1f);
            }
        }
    }
}