using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

namespace AugmeNDT
{
    /// <summary>
    /// Class handles the Vuforia settings and actions
    /// </summary>
    public class VuforiaHandler : MonoBehaviour
    {
        //TODO: Check if Vuforia delayed initialization is active!
        public VuforiaBehaviour vuforiaBehaviour; // VuforiaBehaviour at Main Camera

        public bool vuforiaActive = false;

        void Start()
        {
            SetVuforiaActive(vuforiaActive);
        }

        void Update()
        {
            SetVuforiaActive(vuforiaActive);
        }

        public void SetVuforiaActive(bool state)
        {
            
            if (state)
            {
                StartVuforia();
            }
            else
            {
                StopVuforia();
            }

        }

        private void StartVuforia()
        {
            // If Vuforia is not initialized, initialize it
            if (!VuforiaApplication.Instance.IsInitialized)
            {
                VuforiaApplication.Instance.Initialize();
            }

            // Start Vuforia
            if (!vuforiaBehaviour != null)
            {
                vuforiaBehaviour.enabled = true;
            }
        }

        private void StopVuforia()
        {
            // Stop Vuforia
            if (!vuforiaBehaviour != null)
            {
                vuforiaBehaviour.enabled = false;
            }

            // If Vuforia is initialized, deinitialize it
            if (VuforiaApplication.Instance.IsInitialized)
            {
                VuforiaApplication.Instance.Deinit();
            }
        }

    }
}
