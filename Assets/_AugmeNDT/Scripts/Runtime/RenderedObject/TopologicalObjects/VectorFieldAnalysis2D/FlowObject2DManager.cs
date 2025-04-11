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
        public static FlowObject2DManager Instance;

        private static RectangleManager rectangleManager;
        private static StreamLine2D streamLine2DInstance;
        private static Transform container;

        public int numSpheres; // Number of spheres to maintain simultaneously
        private bool enableSpheres = true;
        private bool stopFlowObjects = true;
        private float sphereSpeed;
        private float lifetime;
        private float localScale;
        private GameObject spherePrefab; // Sphere prefab

        private void Awake()
        {
            // Initialize singleton instance
            Instance = this;
            spherePrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/TopologicalVis/2dmovingSphere");
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
            if (container != null)
                return;

            sphereSpeed = 0.01f;
            lifetime = 15f;
            numSpheres = 8;
            localScale = 0.03f;

            container = new GameObject("2DMovingSpheres").transform;
            Transform rectangleTransform = rectangleManager.GetInteractiveRectangleContainer();
            container.parent = rectangleTransform;
            
            Vector3 parentScale = rectangleTransform.gameObject.transform.localScale;
            container.transform.localScale = new Vector3(
                1f / parentScale.x,
                1f / parentScale.y,
                1f / parentScale.z
            );
            ;
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
                if (currentSpheres < numSpheres)
                {
                    int spheresToSpawn = numSpheres - currentSpheres;

                    for (int i = 0; i < spheresToSpawn; i++)
                    {
                        // Pick a random starting position from gradient points
                        Vector3 startPosition = generatedGradientPoints[UnityEngine.Random.Range(0, generatedGradientPoints.Count())].Position;

                        // Instantiate sphere at selected position
                        GameObject sphere = Instantiate(spherePrefab, startPosition, Quaternion.identity);
                        sphere.transform.parent = container;
                        sphere.transform.localScale = Vector3.one * localScale; // Uniform small scale
                        sphere.tag = "2DMovingSphere"; // Tag for tracking spheres

                        // Initialize flow behavior on the sphere
                        FlowObject2D movingSphere = sphere.GetComponent<FlowObject2D>();
                        movingSphere.StartFlow(generatedGradientPoints, bounds, streamLine2DInstance.streamLineStepSize, sphereSpeed,lifetime);
                    }
                }

                // Wait before checking sphere count again
                yield return new WaitForSeconds(1f);
            }
        }
    }
}