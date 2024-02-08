// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    /// <summary>
    /// Configuration profile settings for Hand Tracking .
    /// </summary>
    [CreateAssetMenu(
        menuName =
            "Mixed Reality/Toolkit/Profiles/Magic Leap Hand Tracking Profile",
        fileName = "MagicLeapHandTrackingInputProfile", order = (int)CreateProfileMenuItemIndices.HandTracking)]
    [MixedRealityServiceProfile(typeof(MagicLeapHandTrackingInputProvider))]
    public class MagicLeapHandTrackingInputProfile : BaseMixedRealityProfile
    {
        [Header("Magic Leap Settings")]
        [Tooltip("Choose which hands to track.")]
        public MagicLeapHandTrackingInputProvider.HandSettings HandednessSettings = MagicLeapHandTrackingInputProvider.HandSettings.Both;

        public enum MLHandRayType { MLHandRay, MRTKHandRay }

        [Tooltip("Which algorithm should be used when calculating the angle and stability of the hand ray. " +
                 "Choose between the preferred MLHandRay or the generic MRTKHandRay calculation.")]
        public MLHandRayType HandRayType = MLHandRayType.MLHandRay;

        [Tooltip("When enabled, the Device Manager will disable hand tracking on the hand that is holding the controller.")]
        public bool DisableHandHoldingController = true;

        public enum MLGestureType { KeyPoints, MLGestureClassification, Both }
        [Tooltip("Use either the Tracked Key Points, Magic Leap Gesture Classification , or Both to determine gestures.")]
        public MLGestureType GestureInteractionType = MLGestureType.Both;

        [Tooltip("The lowest value returned by HandPoseUtils.CalculateIndexPinch from MRTK to maintain a Pinch Gesture. Default: 0.1f")]
        [Range(0.0f, 1.0f)]
        public float PinchMaintainValue = 0.1f;
        [Tooltip("The lowest value returned by HandPoseUtils.CalculateIndexPinch from MRTK to trigger a Pinch Gesture. Default 0.5f")]
        [Range(0.0f, 1.0f)]
        public float PinchTriggerValue = 0.5f;
    }
}
