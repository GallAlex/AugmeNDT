// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using Unity.Profiling;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR.MagicLeap.MLHandActions;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    // Added inside of the scope to prevent conflicts with Unity's 2020.3 Version Control package.
    using Microsoft.MixedReality.Toolkit;
    using GestureClassification = InputSubsystem.Extensions.MLGestureClassification;

    [MixedRealityController(SupportedControllerType.ArticulatedHand,
        new[] { Handedness.Left, Handedness.Right },
        flags: MixedRealityControllerConfigurationFlags.UseCustomInteractionMappings)]
    public class MagicLeapHand : BaseHand
    {

        public MagicLeapHand(
            TrackingState trackingState,
            Handedness controllerHandedness,
            IMixedRealityInputSource inputSource = null,
            MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerHandedness, inputSource, interactions,
                new ArticulatedHandDefinition(inputSource, controllerHandedness))
        {
            handDefinition = Definition as ArticulatedHandDefinition;
        }

        #region IMixedRealityHand Implementation

        /// <inheritdoc/>
        public override bool TryGetJoint(TrackedHandJoint joint, out MixedRealityPose pose) => _magicLeapHandJointProvider.JointPoses.TryGetValue(joint, out pose);

        #endregion IMixedRealityHand Implementation
        private ArticulatedHandDefinition handDefinition;


        /// <summary>
        /// If true, the current joint pose supports far interaction via the default controller ray.  
        /// </summary>
        public override bool IsInPointingPose
        {
            get
            {
                if (!IsPositionAvailable)
                    return false;
                // We check if the palm forward is roughly in line with the camera lookAt
                if (!TryGetJoint(TrackedHandJoint.Palm, out var palmPose) || CameraCache.Main == null) return false;
                
                Transform cameraTransform = CameraCache.Main.transform;
                Vector3 projectedPalmUp = Vector3.ProjectOnPlane(-palmPose.Up, cameraTransform.up);
                
                // We check if the palm forward is roughly in line with the camera lookAt
                return Vector3.Dot(cameraTransform.forward, projectedPalmUp) > .3f;
            }
        }

        /// <summary>
        /// If true, the hand is in air tap gesture, also called the pinch gesture.
        /// </summary>
        public bool IsPinching { set; get; }

        public bool IsGrabbing { set; get; }

        private InputDevice  _hand;
        private InputDevice _gestureHand;
        private static readonly ProfilerMarker UpdateStatePerfMarker = new ProfilerMarker("[MRTK] MagicLeapArticulatedHand.UpdateState");

        private MixedRealityPose currentPointerPose = MixedRealityPose.ZeroIdentity;
        private MixedRealityPose gripPose = MixedRealityPose.ZeroIdentity;

        //For hand ray
        private MagicLeapHandTrackingInputProfile.MLHandRayType handRayType = MagicLeapHandTrackingInputProfile.MLHandRayType.MRTKHandRay;
        private HandRay handRay;

        private MagicLeapHandJointProvider _magicLeapHandJointProvider;

        private bool useMLGestureClassification = true;
        private MagicLeapHandTrackingInputProfile.MLGestureType gestureType;
        private bool gesturesStarted = false;
        private bool gesturesIgnoreFirstUpdate = true;

        private float stickyPinch = 0.1f;
        private float triggerPinch = 0.5f;
        private float gesturePinchValue = 0.15f;
        private float gesturePinchReleaseValue = 0.18f; 

        public void Initalize(InputDevice hand)
        {
            _hand = hand;
            handRay = new HandRay();
            _magicLeapHandJointProvider = new MagicLeapHandJointProvider(ControllerHandedness);
        }

        /// <summary>
        /// Set whether to use Magic Leap's Gesture Classification API for HandTracking Gestures.
        /// </summary>
        public void SetUseMLGestures(MagicLeapHandTrackingInputProfile.MLGestureType useMLGestures)
        {
            gestureType = useMLGestures;
            useMLGestureClassification = useMLGestures == MagicLeapHandTrackingInputProfile.MLGestureType.Both 
                                         || useMLGestures == MagicLeapHandTrackingInputProfile.MLGestureType.MLGestureClassification;
            if (!gesturesStarted)
            {
                gesturesStarted = SetupMagicLeapGestures();
            }
        }

        /// <summary>
        /// Set the which logic to use to calculate the Hand Ray
        /// </summary>
        public void SetUseMLHandRay(MagicLeapHandTrackingInputProfile.MLHandRayType handRayType)
        {
           this.handRayType = handRayType;
        }

        public void setPinchValues(float pinchMaintain, float pinchTrigger)
        {
            stickyPinch = pinchMaintain;
            triggerPinch = pinchTrigger;
        }

        /// <summary>
        /// Updates the joint poses and interactions for the articulated hand.
        /// </summary>
        public void DoUpdate(bool isTracked)
        {
            using (UpdateStatePerfMarker.Auto())
            {

                if (isTracked)
                {
                    // We are not using the gesture device for hand center due to rotations
                    // being reported incorrectly
                    _magicLeapHandJointProvider.UpdateHandJoints(_hand, _gestureHand);

                    IsPositionAvailable = _magicLeapHandJointProvider.IsPositionAvailable;
                    IsRotationAvailable = _magicLeapHandJointProvider.IsRotationAvailable;
                    if (IsPositionAvailable)
                    {

                        TrackingState = TrackingState.Tracked;

                        // Update hand joints and raise event via handDefinition
                        handDefinition?.UpdateHandJoints(_magicLeapHandJointProvider.JointPoses);

                        if (useMLGestureClassification && !gesturesStarted)
                        {
                            gesturesStarted = SetupMagicLeapGestures();
                            if (!gesturesStarted && !gesturesIgnoreFirstUpdate)
                            {
                                Debug.Log("MagicLeapHand failed to find Gesture InputDevice. Falling back to MRTK for Pinch and Grip.");
                            }
                            gesturesIgnoreFirstUpdate = false;
                        }

                        CalculatePinch();
                        CalculateGrab();
                        UpdateHandRay();
                        UpdateVelocity();
                    }
                }
                else
                {
                    TrackingState = TrackingState.NotTracked;
                    IsPositionAvailable = IsRotationAvailable = false;
                    IsGrabbing = false;
                    IsPinching = false;
                    _magicLeapHandJointProvider.Reset();
                }

                UpdateInteractions();
            }
        }

        /// <summary>
        /// Determines if the user is grabbing using gesture recognition and key points
        /// </summary>
        private void CalculateGrab()
        {
            var isGrabbingGesture = IsGrabbing;
            var isGrabbingInterpreted = IsGrabbing;

            // If the gestures are not was not detected, we try to check if the user is grabbing using the key points
            gripPose = _magicLeapHandJointProvider.JointPoses[TrackedHandJoint.Palm];

            //Oculus and Leap motion providers do this "out of sync" call as well
            CoreServices.InputSystem?.RaiseSourcePoseChanged(InputSource, this, _magicLeapHandJointProvider.JointPoses[TrackedHandJoint.Palm]);

            bool isIndexGrabbing = IsIndexGrabbing(isGrabbingInterpreted);
            bool isMiddleGrabbing = IsMiddleGrabbing(isGrabbingInterpreted);
            isGrabbingInterpreted = isIndexGrabbing && isMiddleGrabbing;

            //Set the gesture grab in-case gestures have not started.
            isGrabbingGesture = isGrabbingInterpreted;
            //If the gesture device is valid, use the posture type.
            if (useMLGestureClassification && gesturesStarted && _gestureHand.TryGetFeatureValue(InputSubsystem.Extensions.DeviceFeatureUsages.HandGesture.GesturePosture,
                    out uint postureInt))
            {
                GestureClassification.PostureType postureType = (GestureClassification.PostureType)postureInt;

                if (postureType == GestureClassification.PostureType.Grasp)
                {
                    isGrabbingGesture = true;
                }
                else
                {
                    isGrabbingGesture = false;
                }
            }

            switch (gestureType)
            {
                case MagicLeapHandTrackingInputProfile.MLGestureType.KeyPoints:
                    IsGrabbing = isGrabbingInterpreted;
                    break;
                case MagicLeapHandTrackingInputProfile.MLGestureType.MLGestureClassification:
                    IsGrabbing = isGrabbingGesture;
                    break;
                case MagicLeapHandTrackingInputProfile.MLGestureType.Both:
                    IsGrabbing = isGrabbingInterpreted || isGrabbingGesture;
                    break;
            }
        }

        /// <summary>
        /// Determines if the user is pinching using gesture recognition and key points
        /// </summary>
        private void CalculatePinch()
        {
            // ML Gesture Classification pinch detection
            bool isPinchingGesture = IsPinching;
            if (useMLGestureClassification && gesturesStarted && GestureClassification.TryGetFingerState(_gestureHand, GestureClassification.FingerType.Index, out GestureClassification.FingerState indexFingerState))
            {
                bool isGesturePinch = indexFingerState.PostureData.PinchNormalAngle <= gesturePinchValue;
                bool isGesturePinchRelease = indexFingerState.PostureData.PinchNormalAngle > gesturePinchReleaseValue;

                // the action pinch release enables us to persist occluded pinches
                bool isActionPinchRelease = true;
                if (MLPinchAction.Active)
                {
                    isActionPinchRelease = ControllerHandedness == Handedness.Left ? !MLPinchAction.LeftPinchDown : !MLPinchAction.RightPinchDown;
                }

                if (isPinchingGesture)
                {
                    isPinchingGesture = !(isGesturePinchRelease && isActionPinchRelease);
                }
                else
                {
                    isPinchingGesture = isGesturePinch;
                }
            }

            // For interpreted pinch, compare the normalized pinch value with the profile settings for normalized trigger values.
            float pinchStrength = HandPoseUtils.CalculateIndexPinch(ControllerHandedness);
            // If we are already pinching, we make the pinch a bit sticky.  Otherwise, only consider pinching if finger confidence is high.
            bool isPinchingInterpreted = IsPinching ? pinchStrength > stickyPinch : pinchStrength > triggerPinch;

            switch (gestureType)
            {
                case MagicLeapHandTrackingInputProfile.MLGestureType.KeyPoints:
                    IsPinching = isPinchingInterpreted;
                    break;
                case MagicLeapHandTrackingInputProfile.MLGestureType.MLGestureClassification:
                    IsPinching = isPinchingGesture;
                    break;
                case MagicLeapHandTrackingInputProfile.MLGestureType.Both:
                    IsPinching = isPinchingGesture || isPinchingInterpreted;
                    break;
            }
        }

        private bool IsIndexGrabbing(bool activelyGrabbing)
        {
            if (TryGetJoint(TrackedHandJoint.Wrist, out var wristPose) &&
                TryGetJoint(TrackedHandJoint.IndexTip, out var indexTipPose) &&
                TryGetJoint(TrackedHandJoint.IndexMiddleJoint, out var indexMiddlePose))
            {
                // compare wrist-middle to wrist-tip
                Vector3 wristToIndexTip = indexTipPose.Position - wristPose.Position;
                Vector3 wristToIndexMiddle = indexMiddlePose.Position - wristPose.Position;
                // Make grabbing a little sticky if activelyGrabbing
                return wristToIndexMiddle.sqrMagnitude >= wristToIndexTip.sqrMagnitude * (activelyGrabbing ? .8f : 1.0f);
            }
            return false;
        }

        private bool IsMiddleGrabbing(bool activelyGrabbing)
        {
            if (TryGetJoint(TrackedHandJoint.Wrist, out var wristPose) &&
                TryGetJoint(TrackedHandJoint.MiddleTip, out var middleTipPose) &&
                TryGetJoint(TrackedHandJoint.MiddleMiddleJoint, out var middleMiddlePose))
            {
                // compare wrist-middle to wrist-tip
                Vector3 wristToMiddleTip = middleTipPose.Position - wristPose.Position;
                Vector3 wristToMiddleMiddle = middleMiddlePose.Position - wristPose.Position;
                // Make grabbing a little sticky if activelyGrabbing
                return wristToMiddleMiddle.sqrMagnitude >= wristToMiddleTip.sqrMagnitude * (activelyGrabbing ? .8f : 1.0f);
            }
            return false;
        }

        /// <summary>
        /// Raises MRTK input system events based on joint pose data.
        /// </summary>
        protected void UpdateInteractions()
        {
            for (int i = 0; i < Interactions?.Length; i++)
            {
                switch (Interactions[i].InputType)
                {
                    case DeviceInputType.SpatialPointer:
                        Interactions[i].PoseData = currentPointerPose;
                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, currentPointerPose);
                        }
                        break;
                    case DeviceInputType.SpatialGrip:
                        Interactions[i].PoseData = gripPose;
                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, gripPose);
                        }
                        break;
                    case DeviceInputType.Select:
                    case DeviceInputType.TriggerPress:
                    case DeviceInputType.GripPress:
                        Interactions[i].BoolData = IsGrabbing || IsPinching;
                        if (Interactions[i].Changed)
                        {
                            if (Interactions[i].BoolData)
                            {
                                CoreServices.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                            else
                            {
                                CoreServices.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                        }
                        break;
                    case DeviceInputType.IndexFinger:
                        handDefinition?.UpdateCurrentIndexPose(Interactions[i]);
                        break;
                    case DeviceInputType.ThumbStick:
                    //  Not supported
                        break;
                }
            }
        }

        /// <summary>
        /// Calculate the hand ray using the key points. The hand ray's origin is located
        /// between the thumb and index knuckles. The direction of the ray is based on an
        /// interpreted shoulder and the position of the pointer origin.
        /// </summary>
        protected void UpdateHandRay()
        {
            // Pointer Origin Position
            TryGetJoint(TrackedHandJoint.IndexKnuckle, out MixedRealityPose indexKnucklePose);
            currentPointerPose.Position = indexKnucklePose.Position;
            switch (handRayType)
            {
                case MagicLeapHandTrackingInputProfile.MLHandRayType.MLHandRay:
                    //Pointer Rotation
                    Camera mainCam = Camera.main;
                    float extraRayRotationX = -20.0f;
                    float extraRayRotationY = 25.0f * ((ControllerHandedness == Handedness.Left) ? 1.0f : -1.0f);
                    Quaternion targetRotation = Quaternion.LookRotation(currentPointerPose.Position - mainCam.transform.position, Vector3.up);
                    Vector3 euler = targetRotation.eulerAngles + new Vector3(extraRayRotationX, extraRayRotationY, 0.0f);
                    currentPointerPose.Rotation = Quaternion.Euler(euler);
                    break;
                case MagicLeapHandTrackingInputProfile.MLHandRayType.MRTKHandRay:
                    handRay.Update(indexKnucklePose.Position, GetPalmNormal(), CameraCache.Main.transform, ControllerHandedness);
                    currentPointerPose.Position = handRay.Ray.origin;
                    currentPointerPose.Rotation = Quaternion.LookRotation(handRay.Ray.direction);
                    break;
            }
        }

        private bool SetupMagicLeapGestures()
        {
            GestureClassification.StartTracking();
            string deviceName = (ControllerHandedness.IsLeft() ? GestureClassification.LeftGestureInputDeviceName : GestureClassification.RightGestureInputDeviceName);

            if (!_gestureHand.isValid)
            {
                List<InputDevice> foundDevices = new List<InputDevice>();
                InputDevices.GetDevices(foundDevices);

                foreach (InputDevice device in foundDevices)
                {
                    if (device.name == deviceName)
                    {
                        _gestureHand = device;
                        break;
                    }
                }

                if (!_gestureHand.isValid)
                {
                    // Potentially will be invalid when StartTracking is called the first time. Setup will check again in the update.
                    return false;
                }
            }

            return true;
        }

    }
}

