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
        public static FlowObject3DManager Instance;

        // Number of spheres to maintain simultaneously
        [Range(1, 100)]
        public int numSpheres = 8;

        public float localScaleRate = 0.003f;

        private bool stopFlowObjects = false;

        public float lifeTime = 15f;
        public float sphereSpeed = 0.01f;

        private static StreamLine3D streamLine3DInstance;
        private static Rectangle3DManager rectangle3DManager;
        private GameObject spherePrefab; // Sphere prefab

        private void Awake()
        {
            // Initialize singleton instance
            Instance = this;
            spherePrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/TopologicalVis/MovingSphere");
        }

        private void Start()
        {
            // Get references to other managers
            streamLine3DInstance = StreamLine3D.Instance;
        }

        #region Flow
        /// <summary>
        /// Starts the flow simulation by spawning moving spheres
        /// </summary>
        public void StartFlowObject()
        {
            stopFlowObjects = false;
            StartCoroutine(SpawnMovingSpheres());
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
            Bounds bounds = rectangle3DManager.GetRectangleBounds();
            float streamlineStepSize = streamLine3DInstance.streamlineStepSize;

            // Get spatial grid properties from StreamLine3D
            float cellSize = streamLine3DInstance.cellSize;

            // Build spatial grid if needed
            Dictionary<Vector3Int, List<GradientDataset>> spatialGrid = streamLine3DInstance.spatialGrid;

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
                        sphere.transform.parent = streamLine3DInstance.container;
                        sphere.tag = "Moving3DSphere";
                        sphere.transform.localScale = Vector3.one * localScaleRate;
                        TrailRenderer trailRenderer = sphere.GetComponent<TrailRenderer>();
                        if (trailRenderer != null)
                        {
                            trailRenderer.startWidth = localScaleRate;
                            trailRenderer.endWidth = localScaleRate;
                        }

                        // Initialize flow behavior on the sphere
                        FlowObject3D movingSphere = sphere.GetComponent<FlowObject3D>();
                        movingSphere.StartFlow(generatedGradientPoints, spatialGrid, cellSize, bounds, streamlineStepSize, lifeTime, sphereSpeed);
                    }
                }

                // Wait before checking sphere count again
                yield return new WaitForSeconds(1);
            }
        }
        #endregion Flow
    }
}