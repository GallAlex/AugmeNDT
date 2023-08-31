using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.OpenXR;
using Handedness = Microsoft.MixedReality.Toolkit.Utilities.Handedness;
using System.Collections.Generic;
using UnityEngine.XR;

namespace AugmeNDT
{
    public class VisibleOnCloseInteraction : MonoBehaviour
    {
        //public float visibilityRange = 0.2f; // The range in which hands affect object visibility
        public List<MonoBehaviour> affectedScripts;
        public bool interactionEnabled = true;

        private Renderer objectRenderer;
        private Collider objectColl;

        private void Start()
        {
            objectRenderer = GetComponent<Renderer>();
            objectColl = this.gameObject.GetComponent<Collider>();
        }

        private void Update()
        {
            if (interactionEnabled)
            {
                // Check if any hand is in range
                bool leftHandInRange = IsHandInRange(Handedness.Left);
                bool rightHandInRange = IsHandInRange(Handedness.Right);

                // If no hand is in range, set the object invisible
                if (!leftHandInRange && !rightHandInRange)
                {
                    objectRenderer.enabled = false;
                    ToggleScripts(false);
                }
                // If only left hand is in range, set the object visible
                else if (leftHandInRange)
                {
                    objectRenderer.enabled = true;
                    ToggleScripts(true);
                }
                // If only right hand is in range, set the object visible
                else if (rightHandInRange)
                {
                    objectRenderer.enabled = true;
                    ToggleScripts(true);
                } 
            }
        }

        public void EnableInteraction(bool enable)
        {
            interactionEnabled = enable;
        }

        public void ShowObject(bool show)
        {
            objectRenderer.enabled = show;
            ToggleScripts(show);
        }

        private bool IsHandInRange(Handedness hand)
        {
            // Get the hand joint position
            MixedRealityPose pose;
            HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexMiddleJoint, hand, out pose);

            Vector3 handPosition = pose.Position;

            /*
            // Calculate the distance between the object's bounds and the hand
            float distanceToHand = objectBounds.SqrDistance(handPosition);

            // Calculate the scaled visibility range based on the object's bounds size
            float scaledVisibilityRange = visibilityRange * Mathf.Max(objectBounds.size.x, Mathf.Max(objectBounds.size.y, objectBounds.size.z));

            Debug.Log("Distance to hand: " + distanceToHand + " | Scaled visibility range: " + scaledVisibilityRange);
            Debug.Log("Distance to hand has to be <= " + (scaledVisibilityRange * scaledVisibilityRange));

            // Check if the hand is within the scaled visibility range of the object's bounds
            return distanceToHand <= (scaledVisibilityRange * scaledVisibilityRange);
            */

            return objectColl.bounds.Contains(handPosition);
        }

        // Methods enables/disables all scripts/components in the list
        private void ToggleScripts(bool enable)
        {
            // Run through all scripts in the list and enable/disable them
            foreach (MonoBehaviour script in affectedScripts)
            {
                script.enabled = enable;
            }

        }

    }
}
