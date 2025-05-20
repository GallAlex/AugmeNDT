// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

namespace AugmeNDT
{
    using MixedReality.Toolkit;
    using MixedReality.Toolkit.Input;
    using MixedReality.Toolkit.Subsystems;

    public class VisibleOnCloseInteraction : MonoBehaviour
    {
        //TODO: MRTK3 Rework set scripts
        //public float VisibilityRange = 0.2f; // The range in which hands affect object visibility
        public List<MonoBehaviour> AffectedScripts;

        private bool interactionEnabled = true;
        private Renderer objectRenderer;
        private Collider objectColl;

        private IHandsAggregatorSubsystem handsAggregator;
        

        private void Start()
        {
            handsAggregator = XRSubsystemHelpers.GetFirstRunningSubsystem<IHandsAggregatorSubsystem>();
            objectRenderer = GetComponent<Renderer>();
            objectColl = this.gameObject.GetComponent<Collider>();
        }

        private void Update()
        {

            if (interactionEnabled)
            {
                // Check if any hand is in range
                bool leftHandInRange = IsHandInRange(XRNode.LeftHand);
                bool rightHandInRange = IsHandInRange(XRNode.RightHand);

                // If no hand is in range, set the object invisible
                if (!leftHandInRange && !rightHandInRange)
                {
                    ToggleScripts(false);
                }
                // If only left hand is in range, set the object visible
                else if (leftHandInRange)
                {
                    ToggleScripts(true);
                }
                // If only right hand is in range, set the object visible
                else if (rightHandInRange)
                {
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
            ToggleScripts(show);
        }

        private bool IsHandInRange(XRNode hand)
        {
            handsAggregator.TryGetJoint(TrackedHandJoint.IndexDistal, hand, out HandJointPose jointPose);

            Vector3 handPosition = jointPose.Pose.position;
            return objectColl.bounds.Contains(handPosition);

        }

        // Methods enables/disables all scripts/components in the list
        private void ToggleScripts(bool enable)
        {
            // Sets visibility of the object
            objectRenderer.enabled = enable;

            // Run through all scripts in the list and enable/disable them
            foreach (MonoBehaviour script in AffectedScripts)
            {
                if(script != null){
                    script.enabled = enable;
                }
            }

        }

    }
}
