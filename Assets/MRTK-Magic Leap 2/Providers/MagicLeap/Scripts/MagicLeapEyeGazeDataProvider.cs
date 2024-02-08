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
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    // Added inside of the scope to prevent conflicts with Unity's 2020.3 Version Control package.
    using Microsoft.MixedReality.Toolkit;
    
    [MixedRealityDataProvider(
    typeof(IMixedRealityInputSystem),
    SupportedPlatforms.Android,
    "Magic Leap Eye Gaze Provider",
    "Profiles/DefaultMixedRealityEyeTrackingProfile.asset", "MixedRealityToolkit.SDK",
    true)]
    public class MagicLeapEyeGazeDataProvider : BaseInputDeviceManager, IMixedRealityEyeGazeDataProvider, IMixedRealityCapabilityCheck, IMixedRealityEyeSaccadeProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inputSystem">The <see cref="Microsoft.MixedReality.Toolkit.Input.IMixedRealityInputSystem"/> instance that receives data from this provider.</param>
        /// <param name="name">Friendly name of the service.</param>
        /// <param name="priority">Service priority. Used to determine order of instantiation.</param>
        /// <param name="profile">The service's configuration profile.</param>
        public MagicLeapEyeGazeDataProvider(
            IMixedRealityInputSystem inputSystem,
            string name,
            uint priority,
            BaseMixedRealityProfile profile) : base(inputSystem, name, priority, profile)
        {

        }

        /// <inheritdoc />
        public bool SmoothEyeTracking { get; set; } = false;

        /// <inheritdoc />
        public IMixedRealityEyeSaccadeProvider SaccadeProvider => this;

        /// <inheritdoc />
        public event Action OnSaccade;

        /// <inheritdoc />
        public event Action OnSaccadeX;

        /// <inheritdoc />
        public event Action OnSaccadeY;

        private readonly float smoothFactorNormalized = 0.96f;
        private readonly float saccadeThreshInDegree = 2.5f; // in degrees (not radians)

        private Ray? oldGaze;
        private int confidenceOfSaccade = 0;
        private int confidenceOfSaccadeThreshold = 4;
        private Ray saccade_initialGazePoint;
        private readonly List<Ray> saccade_newGazeCluster = new List<Ray>();

        private InputDevice eyeTrackingDevice = default(InputDevice);

        // Used to get eyes action data.
        private MagicLeapInputs.EyesActions eyesActions;
        // Used to get ml inputs.
        private MagicLeapInputs mlInputs;
        // Was EyeTracking permission granted by user
        private bool permissionGranted = false;
        private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

        public bool OverrideCalibrationRequirement = true;

#region IMixedRealityCapabilityCheck Implementation

        /// <inheritdoc />
        public bool CheckCapability(MixedRealityCapability capability) => eyeTrackingDevice.isValid && capability == MixedRealityCapability.EyeTracking;

#endregion IMixedRealityCapabilityCheck Implementation

        /// <inheritdoc />
        public override void Initialize()
        {
            if (Application.isPlaying)
            {
                ReadProfile();
            }

            base.Initialize();
        }

        public override void Enable()
        {
            if (Application.isPlaying)
            {
                mlInputs = new MagicLeapInputs();
                mlInputs.Enable();

                permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
                permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
                permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
                MLPermissions.RequestPermission(MLPermission.EyeTracking, permissionCallbacks);
            }

            base.Enable();
        }

        public override void Disable()
        {
            permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
            permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;

            mlInputs.Disable();
            mlInputs.Dispose();
            base.Disable();
        }

        private void ReadProfile()
        {
            if (ConfigurationProfile == null)
            {
                Debug.LogError("Magic Leap Eye Tracking Provider requires a configuration profile to run properly.");
                return;
            }

            MixedRealityEyeTrackingProfile profile = ConfigurationProfile as MixedRealityEyeTrackingProfile;
            if (profile == null)
            {
                Debug.LogError("Magic Leap Eye Tracking Provider's configuration profile must be a MixedRealityEyeTrackingProfile.");
                return;
            }

            SmoothEyeTracking = profile.SmoothEyeTracking;

        }

        private static readonly ProfilerMarker UpdatePerfMarker = new ProfilerMarker("[MRTK] MagicLeapEyeGazeDataProvider.Update");

        /// <inheritdoc />
        public override void Update()
        {

            using (UpdatePerfMarker.Auto())
            {
                if (!permissionGranted)
                {
                    return;
                }

                if (!eyeTrackingDevice.isValid)
                {
                    eyeTrackingDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice);

                    if (!eyeTrackingDevice.isValid)
                    {
                        Service?.EyeGazeProvider?.UpdateEyeTrackingStatus(this, false);
                        return;
                    }
                }

                InputSubsystem.Extensions.TryGetEyeTrackingState(eyeTrackingDevice, out var trackingState);

               
                if (trackingState.FixationConfidence > .5f || OverrideCalibrationRequirement)
                {
                    Service?.EyeGazeProvider?.UpdateEyeTrackingStatus(this, true);
                    var eyes = eyesActions.Data.ReadValue<UnityEngine.InputSystem.XR.Eyes>();

                    Vector3 worldPosition = CameraCache.Main.transform.position;
                    Vector3 fixationPoint = MixedRealityPlayspace.TransformPoint(eyes.fixationPoint);
                    Vector3 worldRotation = (fixationPoint - worldPosition).normalized;


                    Ray newGaze = new Ray(worldPosition, worldRotation);
                    if (SmoothEyeTracking)
                    {
                        newGaze = SmoothGaze(newGaze);
                    }

                    Service?.EyeGazeProvider?.UpdateEyeGaze(this, newGaze, DateTime.UtcNow);
                }
                else
                {
                    Service?.EyeGazeProvider?.UpdateEyeTrackingStatus(this, false);
                }
            }

        }


        private static readonly ProfilerMarker SmoothGazePerfMarker = new ProfilerMarker("[MRTK] MagicLeapEyeGazeDataProvider.SmoothGaze");

        /// <summary>
        /// Smooths eye gaze by detecting saccades and tracking gaze clusters.
        /// </summary>
        /// <param name="newGaze">The ray to smooth.</param>
        /// <returns>The smoothed ray.</returns>
        private Ray SmoothGaze(Ray? newGaze)
        {
            using (SmoothGazePerfMarker.Auto())
            {
                if (!oldGaze.HasValue)
                {
                    oldGaze = newGaze;
                    return newGaze.Value;
                }

                Ray smoothedGaze = new Ray();
                bool isSaccading = false;

                // Handle saccades vs. outliers: Instead of simply checking that two successive gaze points are sufficiently 
                // apart, we check for clusters of gaze points instead.
                // 1. If the user's gaze points are far enough apart, this may be a saccade, but also could be an outlier.
                //    So, let's mark it as a potential saccade.
                if ((IsSaccading(oldGaze.Value, newGaze.Value) && (confidenceOfSaccade == 0)))
                {
                    confidenceOfSaccade++;
                    saccade_initialGazePoint = oldGaze.Value;
                    saccade_newGazeCluster.Clear();
                    saccade_newGazeCluster.Add(newGaze.Value);
                }
                // 2. If we have a potential saccade marked, let's check if the new points are within the proximity of 
                //    the initial saccade point.
                else if ((confidenceOfSaccade > 0) && (confidenceOfSaccade < confidenceOfSaccadeThreshold))
                {
                    confidenceOfSaccade++;

                    // First, let's check that we don't just have a bunch of random outliers
                    // The assumption is that after a person saccades, they fixate for a certain 
                    // amount of time resulting in a cluster of gaze points.
                    for (int i = 0; i < saccade_newGazeCluster.Count; i++)
                    {
                        if (IsSaccading(saccade_newGazeCluster[i], newGaze.Value))
                        {
                            confidenceOfSaccade = 0;
                        }

                        // Meanwhile we want to make sure that we are still looking sufficiently far away from our 
                        // original gaze point before saccading.
                        if (!IsSaccading(saccade_initialGazePoint, newGaze.Value))
                        {
                            confidenceOfSaccade = 0;
                        }
                    }
                    saccade_newGazeCluster.Add(newGaze.Value);
                }
                else if (confidenceOfSaccade == confidenceOfSaccadeThreshold)
                {
                    isSaccading = true;
                }

                // Saccade-dependent local smoothing
                if (isSaccading)
                {
                    smoothedGaze.direction = newGaze.Value.direction;
                    smoothedGaze.origin = newGaze.Value.origin;
                    confidenceOfSaccade = 0;
                }
                else
                {
                    smoothedGaze.direction = oldGaze.Value.direction * smoothFactorNormalized + newGaze.Value.direction * (1 - smoothFactorNormalized);
                    smoothedGaze.origin = oldGaze.Value.origin * smoothFactorNormalized + newGaze.Value.origin * (1 - smoothFactorNormalized);
                }

                oldGaze = smoothedGaze;
                return smoothedGaze;
            }
        }

        private static readonly ProfilerMarker IsSaccadingPerfMarker = new ProfilerMarker("[MRTK] MagicLeapEyeGazeDataProvider.IsSaccading");

        private bool IsSaccading(Ray rayOld, Ray rayNew)
        {
            using (IsSaccadingPerfMarker.Auto())
            {
                Vector3 v1 = rayOld.origin + rayOld.direction;
                Vector3 v2 = rayNew.origin + rayNew.direction;

                if (Vector3.Angle(v1, v2) > saccadeThreshInDegree)
                {
                    Vector2 hv1 = new Vector2(v1.x, 0);
                    Vector2 hv2 = new Vector2(v2.x, 0);
                    if (Vector2.Angle(hv1, hv2) > saccadeThreshInDegree)
                    {
                        PostOnSaccadeHorizontally();
                    }

                    Vector2 vv1 = new Vector2(0, v1.y);
                    Vector2 vv2 = new Vector2(0, v2.y);
                    if (Vector2.Angle(vv1, vv2) > saccadeThreshInDegree)
                    {
                        PostOnSaccadeVertically();
                    }

                    PostOnSaccade();

                    return true;
                }
                return false;
            }
        }

        private void PostOnSaccade() => OnSaccade?.Invoke();
        private void PostOnSaccadeHorizontally() => OnSaccadeX?.Invoke();
        private void PostOnSaccadeVertically() => OnSaccadeY?.Invoke();

        private void OnPermissionDenied(string permission)
        {
            MLPluginLog.Error($"MagicLeapGazeDataProvider {permission} permission denied.");
        }

        private void OnPermissionGranted(string permission)
        {
            permissionGranted = true;
            InputSubsystem.Extensions.MLEyes.StartTracking();
            eyesActions = new MagicLeapInputs.EyesActions(mlInputs);
        }
    }
}

