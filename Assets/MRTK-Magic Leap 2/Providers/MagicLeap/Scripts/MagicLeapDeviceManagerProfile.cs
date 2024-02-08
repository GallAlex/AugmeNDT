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
            "Mixed Reality/Toolkit/Profiles/Magic Leap Device Manager Profile",
        fileName = "MagicLeapDeviceManagerProfile", order = 4)]
    [MixedRealityServiceProfile(typeof(MagicLeapDeviceManager))]
    public class MagicLeapDeviceManagerProfile : BaseMixedRealityProfile
    {
        [Tooltip("If enabled, the controller input device will be disable when it is not being held.")]
        public bool DisableControllerWhenNotInHand = true;

        [Header("Controller Detection Settings")]
        [Tooltip(
            "The minimum distance the controller needs to be to a hand to be considered held. " +
            "Default: 0.15 meters or 6 inches. Needs to be at least 0.03 meters.")]
        public float MinimumDistanceToHand = 0.15f;

        [Tooltip(
            "The maximum distance the controller can be from the Magic Leap Headset before the controller disconnects. " +
            "Default: 1 meters or ~3.28 feet. Needs to be at least 0.5 meters.")]
        public float MaximumDistanceFromHead = 1f;

        [Tooltip(
            "The delay (in seconds) before the controller is considered being being held. " +
            "Default: 1.0f - Lower values may result in the controller connecting while it's sitting idle.")]
        [Range(0.5f, 3.0f)]
        public float EnableControllerDelay = 1.0f;

        [Tooltip(
            "The delay (in seconds) before the controller is considered idle. " +
            "Default: 2.0f - Lower values may result in the controller disconnecting while it's being held.")]
        [Range(0.5f, 3.0f)]
        public float DisableControllerDelay = 2.0f;
    }
}
