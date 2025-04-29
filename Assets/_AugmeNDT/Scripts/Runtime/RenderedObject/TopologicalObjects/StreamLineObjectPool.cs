using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// StreamLineObjectPool implements an object pooling pattern for streamline objects in Unity.
    /// This pool manages the creation, allocation, and reuse of GameObjects containing LineRenderer components
    /// used for rendering streamlines, improving performance by reducing instantiation overhead.
    /// This follows the Singleton pattern to ensure only one instance exists throughout the application.
    /// </summary>
    public class StreamLineObjectPool : MonoBehaviour
    {
        #region Singleton Pattern

        // Static instance to implement Singleton pattern - there should only be one pool manager
        private static StreamLineObjectPool _instance;

        // Public property to access the singleton instance
        public static StreamLineObjectPool Instance => _instance;

        #endregion

        #region Serialized Fields

        // Number of streamline objects to create when initializing the pool
        // Having a larger initial pool size prevents runtime allocations during peak usage
        [SerializeField] private int initialPoolSize = 5000;

        // Material applied to all streamline objects for visual rendering
        // Can be assigned in the Inspector or will be loaded from Resources if null
        [SerializeField] private Material streamlineMaterial;

        #endregion

        #region Private Fields

        // Queue for managing available (inactive) objects in the pool
        // Using a Queue ensures first-in-first-out (FIFO) behavior
        private Queue<GameObject> pooledObjects = new Queue<GameObject>();

        // List to track currently active (in-use) objects
        // Useful for batch operations and tracking pool state
        private List<GameObject> activeObjects = new List<GameObject>();

        // Transform that acts as the parent container for all pooled objects
        // Helps keep the hierarchy organized and improves performance
        private Transform poolContainer;

        #endregion

        #region Unity Lifecycle Methods

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Implements Singleton pattern initialization and sets up the object pool.
        /// </summary>
        private void Awake()
        {
            // Enforce singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            // Load the material if it hasn't been assigned in the Inspector
            if (streamlineMaterial == null)
                streamlineMaterial = (Material)Resources.Load("Materials/StreamLine");

            Initialize();
        }

        #endregion

        #region Pool Initialization

        /// <summary>
        /// Initializes the object pool by creating a container transform and pre-filling it
        /// with the specified number of streamline objects.
        /// </summary>
        private void Initialize()
        {
            // Create a container GameObject to hold all pooled objects
            // This improves hierarchy organization and performance by reducing transform searches
            poolContainer = new GameObject("StreamlinePoolContainer").transform;
            poolContainer.SetParent(transform);

            // Pre-fill the pool with initialized objects
            // This prevents expensive runtime allocation during gameplay
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePoolItem();
            }
        }

        #endregion

        #region Object Creation and Management

        /// <summary>
        /// Creates a new streamline object and adds it to the pool.
        /// This method creates a GameObject with a LineRenderer component and appropriate settings.
        /// </summary>
        /// <returns>The newly created GameObject</returns>
        private GameObject CreatePoolItem()
        {
            // Create new GameObject with descriptive name
            GameObject lineObj = new GameObject("Streamline");
            lineObj.transform.SetParent(poolContainer);

            // Add and configure LineRenderer component
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = streamlineMaterial;
            lr.useWorldSpace = false; // Line positions will be relative to the object's transform

            // Disable the object initially since it's in the pool
            lineObj.SetActive(false);

            // Add to the pool queue
            pooledObjects.Enqueue(lineObj);

            return lineObj;
        }

        /// <summary>
        /// Retrieves an inactive object from the pool, or creates a new one if the pool is empty.
        /// Marks the object as active and adds it to the tracking list.
        /// </summary>
        /// <returns>A GameObject ready for use as a streamline</returns>
        public GameObject GetPooledObject()
        {
            // Check if we need to expand the pool
            // If the pool is exhausted, create a new object on-demand
            if (pooledObjects.Count == 0)
            {
                CreatePoolItem();
            }

            // Get an object from the pool and prepare it for use
            GameObject obj = pooledObjects.Dequeue();
            activeObjects.Add(obj);
            obj.SetActive(true);

            return obj;
        }

        /// <summary>
        /// Returns an active object back to the pool, resetting its state for future use.
        /// This method deactivates the object and resets all relevant components.
        /// </summary>
        /// <param name="obj">The GameObject to return to the pool</param>
        public void ReturnToPool(GameObject obj)
        {
            // Deactivate the object to save performance
            obj.SetActive(false);

            // Reset transform to default values
            obj.transform.SetParent(poolContainer);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            obj.name = "Streamline";

            // Reset the LineRenderer component
            // Clear all points from the line
            LineRenderer lr = obj.GetComponent<LineRenderer>();
            lr.positionCount = 0;

            // Update tracking collections
            activeObjects.Remove(obj);
            pooledObjects.Enqueue(obj);
        }

        /// <summary>
        /// Returns all currently active objects back to the pool.
        /// Useful for cleanup operations or scene transitions.
        /// </summary>
        public void ReturnAllToPool()
        {
            // Create a copy of the active objects list because we'll be modifying it
            // during iteration (the collection is modified by ReturnToPool)
            List<GameObject> objectsToReturn = new List<GameObject>(activeObjects);

            // Return each active object back to the pool
            foreach (var obj in objectsToReturn)
            {
                ReturnToPool(obj);
            }
        }

        #endregion
    }
}