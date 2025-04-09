using System.Collections;
using UnityEngine;

namespace AugmeNDT
{

    public class FiberChecker : MonoBehaviour
    {
        public string path;

        private void Start()
        {
            StartCoroutine(CheckForFiberRoutine());
        }

        IEnumerator CheckForFiberRoutine()
        {
            while (true)
            {
                GameObject volume = GameObject.Find("DataVisGroup_0/fibers.raw/Volume");
                GameObject fiber = GameObject.Find("DataVisGroup_0/fibers.raw");
                if (fiber != null && volume != null && volume.activeInHierarchy)
                {
                    GameObject topologicalDataObject = new GameObject("TopologicalDataObject");
                    var tdo = topologicalDataObject.AddComponent<TopologicalDataObject>();
                    tdo.path = path;
                    
                    // Reset transform to align with parent
                    topologicalDataObject.transform.parent = volume.transform;
                    topologicalDataObject.transform.localPosition = Vector3.zero;
                    topologicalDataObject.transform.localRotation = Quaternion.identity;
                    topologicalDataObject.transform.localScale = Vector3.one;

                    yield break;
                }

                
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
