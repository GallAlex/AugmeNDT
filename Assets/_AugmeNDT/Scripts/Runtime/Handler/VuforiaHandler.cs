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
        public bool vuforiaState = true;

        public void SetVuforiaActive(bool vuforiaState)
        {
            VuforiaBehaviour.Instance.enabled = vuforiaState;
            vuforiaState = vuforiaState;
        }

    }
}
