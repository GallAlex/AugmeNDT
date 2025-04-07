using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the creation and flow of 2D sphere objects along gradient streamlines
    /// </summary>
    public class FlowObject2DManager : MonoBehaviour
    {
        public GameObject spherePrefab; // Sphere prefab
        public static FlowObject2DManager Instance;

        private static RectangleManager rectangleManager;
        private static StreamLine2D streamLine2DInstance;
        private static Transform parentContainer;

        public int numSpheres = 1; // Number of spheres to maintain simultaneously
        private bool enableSpheres = true;
        private bool stopFlowObjects = false;
        private float streamLineStepSize;
        private float sphereSpeed;
        private float lifetime;

        private void Awake()
        {
            // Initialize singleton instance
            Instance = this;
        }

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

        private void SetContainer()
        {
            if (parentContainer != null)
                return;

            parentContainer = GameObject.Find("DataVisGroup_0/fibers.raw").transform;
            streamLineStepSize = streamLine2DInstance.streamLineStepSize * rectangleManager.scaleRateToCalculation;
            sphereSpeed = 0.05f;
            lifetime = 15f;
        }

        /// <summary>
        /// Pauses the flow simulation and destroys all currently active spheres
        /// </summary>
        public void PauseFlowObject()
        {
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
                if (currentSpheres < numSpheres)
                {
                    int spheresToSpawn = numSpheres - currentSpheres;

                    for (int i = 0; i < spheresToSpawn; i++)
                    {
                        // Pick a random starting position from gradient points
                        Vector3 startPosition = generatedGradientPoints[UnityEngine.Random.Range(0, generatedGradientPoints.Count())].Position;

                        // Instantiate sphere at selected position
                        GameObject sphere = Instantiate(spherePrefab, startPosition, Quaternion.identity);
                        sphere.transform.parent = parentContainer;
                        sphere.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f); // Uniform small scale
                        sphere.tag = "2DMovingSphere"; // Tag for tracking spheres

                        // Initialize flow behavior on the sphere
                        FlowObject2D movingSphere = sphere.GetComponent<FlowObject2D>();
                        movingSphere.StartFlow(generatedGradientPoints, bounds, streamLineStepSize,sphereSpeed,lifetime);
                    }
                }

                // Wait before checking sphere count again
                yield return new WaitForSeconds(1f);
            }
        }
    }
}