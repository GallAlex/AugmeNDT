using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the creation and flow of 3D sphere objects along gradient streamlines
    /// </summary>
    public class FlowObject3DManager : MonoBehaviour
    {
        public GameObject spherePrefab; // Sphere prefab
        public static FlowObject3DManager Instance;

        public int numSpheres = 8; // Number of spheres to maintain simultaneously
        private bool enableSpheres = true;
        private bool stopFlowObjects = false;

        private static Rectangle3DManager rectangle3DManager;
        private static StreamLine3D streamLine3DInstance;

        private void Awake()
        {
            // Initialize singleton instance
            Instance = this;
        }

        private void Start()
        {
            // Get references to other managers
            rectangle3DManager = Rectangle3DManager.rectangle3DManager;
            streamLine3DInstance = StreamLine3D.Instance;
        }

        #region Flow
        /// <summary>
        /// Starts the flow simulation by spawning moving spheres
        /// </summary>
        public void StartFlowObject()
        {
            stopFlowObjects = false;
            if (enableSpheres)
            {
                StartCoroutine(SpawnMovingSpheres());
            }
        }

        /// <summary>
        /// Pauses the flow simulation and destroys all currently active spheres
        /// </summary>
        public void PauseFlowObject()
        {
            stopFlowObjects = true;
            GameObject.FindGameObjectsWithTag("Moving3DSphere").ToList().ForEach(x => Destroy(x));
        }

        /// <summary>
        /// Coroutine that handles spawning and maintaining the specified number of moving spheres
        /// </summary>
        /// <returns>IEnumerator for coroutine processing</returns>
        private IEnumerator SpawnMovingSpheres()
        {
            // Get gradient data and boundary information
            List<GradientDataset> generatedGradientPoints = rectangle3DManager.GetGradientPoints();
            Bounds bounds = rectangle3DManager.GetWireframeCubeBounds();
            float streamlineStepSize = streamLine3DInstance.streamlineStepSize;

            while (true)
            {
                if (stopFlowObjects)
                    break;

                // Count current active spheres
                int currentSpheres = GameObject.FindGameObjectsWithTag("Moving3DSphere").Length;

                // Spawn additional spheres if needed to maintain the target count
                if (currentSpheres < numSpheres)
                {
                    int spheresToSpawn = numSpheres - currentSpheres;

                    for (int i = 0; i < spheresToSpawn; i++)
                    {
                        // Pick a random starting position from gradient points
                        Vector3 startPosition = generatedGradientPoints[Random.Range(0, generatedGradientPoints.Count())].Position;

                        // Instantiate sphere at selected position
                        GameObject sphere = Instantiate(spherePrefab, startPosition, Quaternion.identity);
                        sphere.tag = "Moving3DSphere";

                        // Initialize flow behavior on the sphere
                        FlowObject3D movingSphere = sphere.GetComponent<FlowObject3D>();
                        movingSphere.StartFlow(generatedGradientPoints, bounds, streamlineStepSize);
                    }
                }

                // Wait before checking sphere count again
                yield return new WaitForSeconds(1f);
            }
        }
        #endregion Flow
    }
}