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
        Material pinkMaterial;

        private void Awake()
        {
            // Initialize singleton instance
            Instance = this;
            spherePrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/TopologicalVis/MovingSphere");


            // Yeni pembe materyal oluştur
            Material pinkMaterial = new Material(Shader.Find("Standard"));
            pinkMaterial.color = new Color(1f, 0.4f, 0.7f); // Pembe renk
            pinkMaterial.EnableKeyword("_EMISSION");
            pinkMaterial.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.7f) * 0.5f); // Hafif parlaklık

        }

        private void Start()
        {
            // Get references to other managers
            streamLine3DInstance = StreamLine3D.Instance;
            rectangle3DManager = Rectangle3DManager.rectangle3DManager;
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
                if (currentSpheres < numSpheres && streamLine3DInstance.LineObjs.Count > 0)
                {
                    int spheresToSpawn = numSpheres - currentSpheres;

                    for (int i = 0; i < spheresToSpawn; i++)
                    {
                        // Pick a random streamline from the available lines
                        GameObject randomLineObj = streamLine3DInstance.LineObjs[Random.Range(0, streamLine3DInstance.LineObjs.Count)];
                        LineRenderer lineRenderer = randomLineObj.GetComponent<LineRenderer>();

                        if (lineRenderer != null && lineRenderer.positionCount > 0)
                        {
                            // Get all points from the streamline
                            Vector3[] streamlinePoints = new Vector3[lineRenderer.positionCount];
                            lineRenderer.GetPositions(streamlinePoints);

                            // Convert local positions to world positions
                            for (int j = 0; j < streamlinePoints.Length; j++)
                            {
                                streamlinePoints[j] = randomLineObj.transform.TransformPoint(streamlinePoints[j]);
                            }

                            // Get start position from the beginning of the streamline
                            Vector3 startPosition = streamlinePoints[0];

                            // Instantiate sphere at selected position
                            GameObject sphere = Instantiate(spherePrefab, startPosition, Quaternion.identity);
                            sphere.transform.parent = streamLine3DInstance.container;
                            sphere.tag = "Moving3DSphere";
                            sphere.transform.localScale = Vector3.one * localScaleRate;

                            // Küreye yeni materyali ata
                            Renderer renderer = sphere.GetComponent<Renderer>();
                            if (renderer != null)
                            {
                                renderer.material = pinkMaterial;
                            }

                            // Initialize flow behavior on the sphere, passing the streamline points
                            FlowObject3D movingSphere = sphere.GetComponent<FlowObject3D>();
                            movingSphere.StartFlowAlongStreamline(streamlinePoints, generatedGradientPoints, bounds,
                                                                 streamlineStepSize, lifeTime, sphereSpeed);
                        }
                        else
                        {
                            // Fallback: pick a random starting position from gradient points if we couldn't get a streamline
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

                            // Initialize flow behavior on the sphere using the original gradient method
                            FlowObject3D movingSphere = sphere.GetComponent<FlowObject3D>();
                            movingSphere.StartFlow(generatedGradientPoints, spatialGrid, cellSize, bounds, streamlineStepSize, lifeTime, sphereSpeed);
                        }
                    }
                }

                // Wait before checking sphere count again
                yield return new WaitForSeconds(1);
            }
        }
        #endregion Flow
    }
}