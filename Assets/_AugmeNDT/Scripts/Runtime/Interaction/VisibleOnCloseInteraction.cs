using UnityEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using Microsoft.MixedReality.OpenXR;
//using Handedness = MixedReality.Toolkit.Utilities.Handedness;
using System.Collections.Generic;
using UnityEngine.XR;

namespace AugmeNDT
{
    public class VisibleOnCloseInteraction : MonoBehaviour
    {
        /*   //public float visibilityRange = 0.2f; // The range in which hands affect object visibility
           public List<MonoBehaviour> affectedScripts;

           private bool interactionEnabled = true;
           private Renderer objectRenderer;
           private Collider objectColl;

           private void Start()
           {
               objectRenderer = GetComponent<Renderer>();
               objectColl = this.gameObject.GetComponent<Collider>();
           }

           private void Update()
           {
               //if (interactionEnabled)
               //{
               //    // Check if any hand is in range
               //    //bool leftHandInRange = IsHandInRange(Handedness.Left);
               //    //bool rightHandInRange = IsHandInRange(Handedness.Right);

               //    // If no hand is in range, set the object invisible
               //    if (!leftHandInRange && !rightHandInRange)
               //    {
               //        ToggleScripts(false);
               //    }
               //    // If only left hand is in range, set the object visible
               //    else if (leftHandInRange)
               //    {
               //        ToggleScripts(true);
               //    }
               //    // If only right hand is in range, set the object visible
               //    else if (rightHandInRange)
               //    {
               //        ToggleScripts(true);
               //    } 
               }
           }

           public void EnableInteraction(bool enable)
           {
             //  interactionEnabled = enable;
           }

           public void ShowObject(bool show)
           {
               ToggleScripts(show);
           }

           private bool IsHandInRange(Handedness hand)
           {
               // Get the hand joint position
               MixedRealityPose pose;
               HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexDistalJoint, hand, out pose);

               Vector3 handPosition = pose.Position;

               return objectColl.bounds.Contains(handPosition);
           }

           // Methods enables/disables all scripts/components in the list
           private void ToggleScripts(bool enable)
           {
               objectRenderer.enabled = enable;

               // Run through all scripts in the list and enable/disable them
               foreach (MonoBehaviour script in affectedScripts)
               {
                   script.enabled = enable;
               }

           }

       */
    }
}
