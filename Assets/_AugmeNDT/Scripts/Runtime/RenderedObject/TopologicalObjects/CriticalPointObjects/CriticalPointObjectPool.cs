using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    public class CriticalPointObjectPool : MonoBehaviour
    {
        private static CriticalPointObjectPool _instance;
        public static CriticalPointObjectPool Instance => _instance;

        [SerializeField] private GameObject pointPrefab;
        [SerializeField] private int initialPoolSize = 2622;

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

            // Load the prefab from Resources (or you can assign it via Inspector)
            if (pointPrefab == null)
                pointPrefab = Resources.Load<GameObject>("Prefabs/DataVisPrefabs/TopologicalVis/InteractiveCriticalPointPrefab");

            Initialize();
        }

        private void Initialize()
        {
            // Create a container for the object pool
            poolContainer = new GameObject("CriticalPointPool").transform;
            poolContainer.SetParent(transform);

            // Fill the pool with initial objects
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePoolItem();
            }
        }

        private GameObject CreatePoolItem()
        {
            GameObject obj = Instantiate(pointPrefab, poolContainer);
            obj.SetActive(false);
            pooledObjects.Enqueue(obj);
            return obj;
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
            // Make a copy of active objects (since we are modifying the collection)
            List<GameObject> objectsToReturn = new List<GameObject>(activeObjects);

            foreach (var obj in objectsToReturn)
            {
                ReturnToPool(obj);
            }
        }
    }
}
