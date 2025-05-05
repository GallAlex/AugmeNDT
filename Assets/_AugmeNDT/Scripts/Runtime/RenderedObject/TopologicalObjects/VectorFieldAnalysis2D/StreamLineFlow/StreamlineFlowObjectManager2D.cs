using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    public class StreamlineFlowObjectManager2D : MonoBehaviour
    {
        public static StreamlineFlowObjectManager2D instance;

        /// <summary>
        /// Flow settings for particle visualization
        /// </summary>
        private Transform flowContainer = null;             // Container for flow particles
        private bool stopFlowObjects = true;                // Flag to stop flow animation
        private float sphereSpeed = 0.01f;                  // Speed of flowing particles
        private float lifetime = 15f;                       // Lifetime of each particle
        private int numSpheres = 100;                       // Number of particles per streamline
        private Vector3 localScale = Vector3.one * 0.003f;  // Scale of flow particles
        private GameObject spherePrefab;                    // Sphere prefab for particles
        
        // List of currently selected streamline GameObjects
        private List<GameObject> selectedGameObjects = new List<GameObject>();

        // Vector field data for particle flow visualization
        private List<GradientDataset> gradeints = new List<GradientDataset>();

        private void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Initializes and starts the flow object system
        /// </summary>
        public void StartFlowObject()
        {
            if (flowContainer == null)
                return;

            stopFlowObjects = false;
            StartCoroutine(SpawnMovingSpheres());
        }

        public void SetAndStartFlowObjects(List<GradientDataset> gradeintList, List<GameObject> selectedGameObjectList, Transform parentContainer)
        {
            stopFlowObjects = true;

            // Initial setup
            if (flowContainer == null)
            {
                flowContainer = new GameObject("Interactive2DMovingSpheres").transform;
                flowContainer.parent = parentContainer;
                spherePrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/TopologicalVis/Interactive2DFLOW");
            }

            gradeints.Clear();
            selectedGameObjects.Clear();

            gradeints = gradeintList;
            selectedGameObjects = selectedGameObjectList;

            stopFlowObjects = false;
            StartCoroutine(SpawnMovingSpheres());
        }

        public void PauseFlowObjects()
        {
            stopFlowObjects = true;
            // Find and destroy all flow particles
            var objs = GameObject.FindGameObjectsWithTag("Interactive2DSphere");
            foreach (var obj in objs)
            {
                DestroyImmediate(obj);
            }
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
    }
}
