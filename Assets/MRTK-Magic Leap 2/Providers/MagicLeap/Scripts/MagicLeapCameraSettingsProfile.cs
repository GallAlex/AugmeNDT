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
using Microsoft.MixedReality.Toolkit.CameraSystem;
using UnityEngine;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    /// <summary>
    /// Configuration profile for the Windows Mixed Reality camera settings provider.
    /// </summary>
    [CreateAssetMenu(
        menuName =
            "Mixed Reality/Toolkit/Providers/Magic Leap/Magic Leap Camera Settings Profile",
        fileName = "MagicLeapCameraSettingsProfile", order = 100)]
    [MixedRealityServiceProfile(typeof(MagicLeapCameraSettings))]
    public class MagicLeapCameraSettingsProfile : BaseCameraSettingsProfile
    {
        [SerializeField]
        [Tooltip("Transform you want to be the focus point of the camera. Can help improve alignment when performing Mixed Reality Capture. Leave empty to use default value")]
        private Transform _stereoConvergencePoint;
        public Transform StereoConvergencePoint => _stereoConvergencePoint;

        [SerializeField]
        [Tooltip("Content for this app is protected and should not be recorded or captured")]
        private bool _protectedSurface;
        public bool ProtectedSurface => _protectedSurface;
    }
}