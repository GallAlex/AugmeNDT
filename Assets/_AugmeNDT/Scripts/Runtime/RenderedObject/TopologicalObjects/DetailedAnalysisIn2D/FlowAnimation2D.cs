using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    public class FlowAnimation2D : MonoBehaviour
    {
        public static FlowAnimation2D Instance;

        public int numSpheres = 5;
        public float sphereSpeed = 0.5f;
        public float lifetime = 15f;
        public GameObject spherePrefab; // Küre prefabı
        
        private bool stopFlowObjects = false;
        private static StreamLine2D streamLine2DInstance;
        
        private void Awake()
        {
            Instance = this;        
        }
        private void Start()
        {
            streamLine2DInstance = StreamLine2D.Instance;
        }

        public void StartFlowObject()
        {
            if (streamLine2DInstance == null || streamLine2DInstance.streamLine2DCalculation == null)
                return;

            stopFlowObjects = false;
            StartCoroutine(SpawnMovingSpheres());
        }

        public void PauseFlowObject()
        {
            stopFlowObjects = true;
            GameObject.FindGameObjectsWithTag("2DMovingSphere").ToList().ForEach(x => Destroy(x));
        }

        private IEnumerator SpawnMovingSpheres()
        {
            while (true)
            {
                if (stopFlowObjects)
                    break;

                int currentSpheres = GameObject.FindGameObjectsWithTag("2DMovingSphere").Length;

                if (currentSpheres < numSpheres)
                {
                    int spheresToSpawn = numSpheres - currentSpheres;

                    for (int i = 0; i < spheresToSpawn; i++)
                    {
                        Vector3 startPosition = streamLine2DInstance.streamLine2DCalculation.GetRandomPointInTriangle();
                        GameObject sphere = Instantiate(spherePrefab, startPosition, Quaternion.identity);
                        sphere.tag = "2DMovingSphere"; // Küreleri takip etmek için
                        FlowObjects movingSphere = sphere.AddComponent<FlowObjects>();
                        movingSphere.Initialize(sphereSpeed, lifetime);
                        movingSphere.StartMoveSphere(
                            borders2D: streamLine2DInstance.streamLine2DCalculation.GetBorderPoints(),
                            streamlineStepSize: streamLine2DInstance.streamLine2DCalculation.streamlineStepSize,
                            gradientPoints: streamLine2DInstance.streamLine2DCalculation.gradientPoints,
                            is2DField: true);
                    }
                }

                yield return new WaitForSeconds(1f); 
            }
        }

    }
}
