using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    public class StreamLineObjectPool : MonoBehaviour
    {
        private static StreamLineObjectPool _instance;
        public static StreamLineObjectPool Instance => _instance;

        [SerializeField] private int initialPoolSize = 800; // Default number of streamlines
        [SerializeField] private Material streamlineMaterial;

        private Queue<GameObject> pooledObjects = new Queue<GameObject>();
        private List<GameObject> activeObjects = new List<GameObject>();
        private Transform poolContainer;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            // Load the material
            if (streamlineMaterial == null)
                streamlineMaterial = (Material)Resources.Load("Materials/StreamLine");

            Initialize();
        }

        private void Initialize()
        {
            // Create a container for the object pool 
            poolContainer = new GameObject("StreamlinePoolContainer").transform;
            poolContainer.SetParent(transform);

            // Fill the pool with initial objects 
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePoolItem();
            }
        }

        private GameObject CreatePoolItem()
        {
            GameObject lineObj = new GameObject("Streamline");
            lineObj.transform.SetParent(poolContainer);

            // Add a LineRenderer component 
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = streamlineMaterial;
            lr.useWorldSpace = false;

            lineObj.SetActive(false);
            pooledObjects.Enqueue(lineObj);
            return lineObj;
        }

        public GameObject GetPooledObject()
        {
            // If the pool is empty, create a new object
            if (pooledObjects.Count == 0)
            {
                CreatePoolItem();
            }

            GameObject obj = pooledObjects.Dequeue();
            activeObjects.Add(obj);
            obj.SetActive(true);
            return obj;
        }

        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(poolContainer);
            activeObjects.Remove(obj);
            pooledObjects.Enqueue(obj);
        }

        public void ReturnAllToPool()
        {
            // Create a copy of the active objects (because we modify the collection)
            List<GameObject> objectsToReturn = new List<GameObject>(activeObjects);

            foreach (var obj in objectsToReturn)
            {
                ReturnToPool(obj);
            }
        }
    }
}