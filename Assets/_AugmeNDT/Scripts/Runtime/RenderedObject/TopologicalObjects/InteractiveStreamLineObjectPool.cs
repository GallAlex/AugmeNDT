using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// This class implements an object pool pattern for Unity's InteractiveStreamline objects.
    /// Object pooling is used to optimize performance by reusing objects instead of 
    /// continuously creating and destroying them, which helps reduce garbage collection
    /// and improves frame rates, especially important for streamline visualizations.
    /// </summary>
    public class InteractiveStreamLineObjectPool : MonoBehaviour
    {
        // Singleton pattern implementation
        private static InteractiveStreamLineObjectPool _instance;
        public static InteractiveStreamLineObjectPool Instance => _instance;

        // Configuration fields for the object pool
        [SerializeField] private int initialPoolSize = 5000; // Default number of streamlines
        [SerializeField] private Material streamlineMaterial;

        // Pool management collections
        private Queue<GameObject> pooledObjects = new Queue<GameObject>();  // Available objects for reuse
        private List<GameObject> activeObjects = new List<GameObject>();   // Objects currently in use
        private Transform poolContainer;                                   // Parent container for organization

        /// <summary>
        /// Unity's Awake method - called when the script instance is being loaded.
        /// Ensures singleton pattern and initializes the pool.
        /// </summary>
        private void Awake()
        {
            // Implement singleton pattern to prevent multiple instances
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            // Load the material for streamlines if not set in the Inspector
            // Resources.Load is used to load materials from the Resources folder
            if (streamlineMaterial == null)
                streamlineMaterial = (Material)Resources.Load("Materials/StreamLine");

            Initialize();
        }

        /// <summary>
        /// Initializes the object pool by creating the container and pre-filling it
        /// with a specified number of inactive objects to avoid runtime creation
        /// during visualization.
        /// </summary>
        private void Initialize()
        {
            // Create a container for the object pool to keep the hierarchy organized
            poolContainer = new GameObject("InteractiveStreamlinePoolContainer").transform;
            poolContainer.SetParent(transform);

            // Pre-fill the pool with initial objects to avoid runtime allocation
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePoolItem();
            }
        }

        /// <summary>
        /// Creates a new pooled object with LineRenderer component and proper configuration.
        /// This method is called during initialization and when the pool runs out of objects.
        /// </summary>
        /// <returns>The created GameObject configured as a streamline</returns>
        private GameObject CreatePoolItem()
        {
            // Create a new GameObject for the streamline
            GameObject lineObj = new GameObject("InteractiveStreamline");
            lineObj.transform.SetParent(poolContainer);

            // Add a LineRenderer component for rendering the streamline path
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = streamlineMaterial;  // Set the material for rendering
            lr.useWorldSpace = false;          // Use local space for better control

            // Start the object as inactive and add it to the pool
            lineObj.SetActive(false);
            pooledObjects.Enqueue(lineObj);
            return lineObj;
        }

        /// <summary>
        /// Retrieves an object from the pool for use. If the pool is empty,
        /// it creates a new object on-demand.
        /// </summary>
        /// <returns>An activated GameObject configured as a streamline</returns>
        public GameObject GetPooledObject()
        {
            // If the pool is empty, create a new object to expand capacity
            if (pooledObjects.Count == 0)
            {
                CreatePoolItem();
            }

            // Get an object from the pool and activate it for use
            GameObject obj = pooledObjects.Dequeue();
            activeObjects.Add(obj);
            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Returns an object back to the pool for reuse. Resets all properties
        /// to ensure clean state for the next use cycle.
        /// </summary>
        /// <param name="obj">The GameObject to return to the pool</param>
        public void ReturnToPool(GameObject obj)
        {
            // Deactivate the object and reset its transform
            obj.SetActive(false);
            obj.transform.SetParent(poolContainer);        // Re-parent to the pool container
            obj.transform.localPosition = Vector3.zero;    // Reset position
            obj.transform.localRotation = Quaternion.identity; // Reset rotation
            obj.transform.localScale = Vector3.one;        // Reset scale
            obj.name = "InteractiveStreamline";           // Reset name

            // Reset the LineRenderer component to a clean state
            LineRenderer lr = obj.GetComponent<LineRenderer>();
            lr.positionCount = 0;  // Clear all vertices from the line

            // Update the pool collections
            activeObjects.Remove(obj);
            pooledObjects.Enqueue(obj);
        }

        /// <summary>
        /// Returns all currently active objects back to the pool.
        /// Useful for clearing the visualization or resetting the scene.
        /// </summary>
        public void ReturnAllToPool()
        {
            // Create a copy of the active objects list because we'll modify the original collection
            // during the iteration
            List<GameObject> objectsToReturn = new List<GameObject>(activeObjects);

            // Return each active object to the pool
            foreach (var obj in objectsToReturn)
            {
                ReturnToPool(obj);
            }
        }
    }
}