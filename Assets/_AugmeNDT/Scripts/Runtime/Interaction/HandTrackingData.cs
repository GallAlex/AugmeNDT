// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

namespace AugmeNDT
{

    /**
     * Class to query data about the position/actions of the hands
     */
    public class HandTrackingData : MonoBehaviour
    {
        [SerializeField]
        private GameObject mrtkLeftHand;

        [SerializeField]
        private GameObject mrtkRightHand;

        [SerializeField]
        public bool CheckPos;


        private IHandsAggregatorSubsystem handsAggregator;

        private void Start()
        {
            // Get the first running hands subsystem.
            handsAggregator = XRSubsystemHelpers.GetFirstRunningSubsystem<IHandsAggregatorSubsystem>();
            //HandsUtils = handsAggregator.TryGetEntireHand()
            
        }

        private void Update()
        {
            if (CheckPos)
            {
                Debug.Log("Right Hand pos -- " + GetJointPosition(XRNode.RightHand, TrackedHandJoint.IndexDistal));

                Debug.Log("Left Hand pos -- " + GetJointPosition(XRNode.LeftHand, TrackedHandJoint.IndexDistal));
            }

        }

        public bool LeftHandAvailable()
        {
            return handsAggregator.TryGetJoint(TrackedHandJoint.IndexTip, XRNode.LeftHand, out HandJointPose jointPose);
        }

        public bool RightHandAvailable()
        {
            return handsAggregator.TryGetJoint(TrackedHandJoint.IndexTip, XRNode.RightHand, out HandJointPose jointPose);
        }

        private Vector3 GetJointPosition(XRNode hand, TrackedHandJoint joint)
        {

            handsAggregator.TryGetJoint(joint, hand, out HandJointPose jointPose);

            return jointPose.Pose.position;
        }
    }

}
