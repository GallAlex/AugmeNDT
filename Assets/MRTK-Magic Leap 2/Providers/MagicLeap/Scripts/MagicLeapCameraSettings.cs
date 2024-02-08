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
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.MagicLeap;


namespace MagicLeap.MRTK.DeviceManagement.Input
{
    /// <summary>
    /// Camera settings provider for use with XR SDK.
    /// </summary>
    [MixedRealityDataProvider(
        typeof(IMixedRealityCameraSystem),
        SupportedPlatforms.Android,
        "MagicLeap Camera Settings")]
    public class MagicLeapCameraSettings : BaseCameraSettingsProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cameraSystem">The instance of the camera system which is managing this provider.</param>
        /// <param name="name">Friendly name of the provider.</param>
        /// <param name="priority">Provider priority. Used to determine order of instantiation.</param>
        /// <param name="profile">The provider's configuration profile.</param>
        public MagicLeapCameraSettings(
            IMixedRealityCameraSystem cameraSystem,
            string name = null,
            uint priority = DefaultPriority,
            BaseCameraSettingsProfile profile = null) : base(cameraSystem, name, priority, profile)
        {
            ReadProfile();
        }

        private UnityEngine.SpatialTracking.TrackedPoseDriver trackedPoseDriver = null;

        private MLAudioOutputPluginBehavior mlAudioOutputPluginBehavior = null;
        private UnityEngine.XR.MagicLeap.MagicLeapCamera magicLeapCamera = null;



        #region IMixedRealityCameraSettings

        private Transform _stereoConvergencePoint;
        private bool _protectedSurface;

        /// <inheritdoc/>
        public override bool IsOpaque => XRSubsystemHelpers.DisplaySubsystem?.displayOpaque ?? true;
        public MagicLeapCameraSettingsProfile SettingsProfile => ConfigurationProfile as MagicLeapCameraSettingsProfile;

        private void ReadProfile()
        {
            if (SettingsProfile == null)
            {
                Debug.LogWarning("A profile was not specified for the Unity AR Camera Settings provider.\nUsing Microsoft Mixed Reality Toolkit default options.");
                return;
            }

            _stereoConvergencePoint = SettingsProfile.StereoConvergencePoint;
            _protectedSurface = SettingsProfile.ProtectedSurface;
        }

        /// <inheritdoc/>
        public override void Enable()
        {
            // Only track the TrackedPoseDriver if we added it ourselves.
            // There may be a pre-configured TrackedPoseDriver on the camera.
            if (!CameraCache.Main.GetComponent<TrackedPoseDriver>())
            {
                trackedPoseDriver = CameraCache.Main.gameObject.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
            }
            if (!CameraCache.Main.GetComponent<MLAudioOutputPluginBehavior>())
            {
                mlAudioOutputPluginBehavior = CameraCache.Main.gameObject.AddComponent<MLAudioOutputPluginBehavior>();
            }
            if (!CameraCache.Main.GetComponent<UnityEngine.XR.MagicLeap.MagicLeapCamera>())
            {
                magicLeapCamera = CameraCache.Main.gameObject.AddComponent<UnityEngine.XR.MagicLeap.MagicLeapCamera>();
                magicLeapCamera.StereoConvergencePoint = _stereoConvergencePoint;
                magicLeapCamera.ProtectedSurface = _protectedSurface;
                // MagicLeapCamera will override some camera settings on Awake(), so re-apply them after adding it.
                ApplyConfiguration();
            }

            base.Enable();

        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (trackedPoseDriver != null)
            {
                UnityObjectExtensions.DestroyObject(trackedPoseDriver);
                trackedPoseDriver = null;
            }

            if (magicLeapCamera != null)
            {
                UnityObjectExtensions.DestroyObject(magicLeapCamera);
                magicLeapCamera = null;
            }

            if (mlAudioOutputPluginBehavior != null)
            {
                UnityObjectExtensions.DestroyObject(mlAudioOutputPluginBehavior);
                mlAudioOutputPluginBehavior = null;
            }

            base.Disable();
        }

        #endregion IMixedRealityCameraSettings
    }
}
