// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.XRSDK.Input;
using Microsoft.MixedReality.Toolkit.XRSDK;
using UnityEngine.XR.MagicLeap;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using InputDevice = UnityEngine.XR.InputDevice;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    // Added inside of the scope to prevent conflicts with Unity's 2020.3 Version Control package.
    using Microsoft.MixedReality.Toolkit;

    /// <summary>
    /// Manages Magic Leap Device
    /// </summary>
    [MixedRealityDataProvider(typeof(IMixedRealityInputSystem), SupportedPlatforms.Android, "Magic Leap HandTracking Input")]
    public class MagicLeapHandTrackingInputProvider : XRSDKDeviceManager
    {
        Dictionary<Handedness, MagicLeapHand> trackedHands = new Dictionary<Handedness, MagicLeapHand>();
        Dictionary<Handedness, MLHandContainer> allHands = new Dictionary<Handedness, MLHandContainer>();

        private bool? IsActiveLoader =>
            LoaderHelpers.IsLoaderActive<MagicLeapLoader>();


        public static MagicLeapHandTrackingInputProvider Instance = null;

        public bool DisableHandHoldingController;

        public enum HandSettings
        {
            None,
            Left,
            Right,
            Both
        }

        public HandSettings CurrentHandSettings
        {
            get
            {
                return _CurrentHandSettings;
            }

            set
            {
                _CurrentHandSettings = value;

                // TODO: Update real-time hand settings
                switch (value)
                {
                    case HandSettings.None:
                        RemoveAllHandDevices();
                        return;

                    case HandSettings.Left:
                        if (trackedHands.ContainsKey(Handedness.Right))
                        {
                            MagicLeapHand hand = trackedHands[Handedness.Right];
                            if (hand != null)
                            {
                                RemoveHandDevice(hand);
                            }
                        }
                        break;

                    case HandSettings.Right:
                        if (trackedHands.ContainsKey(Handedness.Left))
                        {
                            MagicLeapHand hand = trackedHands[Handedness.Left];
                            if (hand != null)
                            {
                                RemoveHandDevice(hand);
                            }
                        }
                        break;
                }

            }
        }

        public MagicLeapHandTrackingInputProfile.MLGestureType GestureInteractionType
        {
            get
            {
                return gestureInteractionType;
            }

            set
            {
                gestureInteractionType = value;
            }
        }
        public MagicLeapHandTrackingInputProfile.MLHandRayType HandRayType
        {
            get
            {
                return handRayType;
            }

            set
            {
                handRayType = value;
            }
        }

        public float PinchMaintainValue
        {
            get
            {
                return pinchMaintainValue;
            }

            set
            {
                pinchMaintainValue = value;
            }
        }

        public float PinchTriggerValue
        {
            get
            {
                return pinchTriggerValue;
            }

            set
            {
                pinchTriggerValue = value;
            }
        }

        private MagicLeapHandTrackingInputProfile profile;

        private HandSettings _CurrentHandSettings = HandSettings.Both;
        private MagicLeapHandTrackingInputProfile.MLGestureType gestureInteractionType = MagicLeapHandTrackingInputProfile.MLGestureType.Both;
        private MagicLeapHandTrackingInputProfile.MLHandRayType handRayType = MagicLeapHandTrackingInputProfile.MLHandRayType.MLHandRay;

        private float pinchMaintainValue = 0.1f;
        private float pinchTriggerValue = 0.5f;

        private bool mlHandTrackingActive = false;

        private InputDevice leftHandDevice;
        private InputDevice rightHandDevice;

        // Controller Handedness
        //True if the MagicLeapDeviceManager is present in the scene
        private bool magicLeapDeviceManagerPresent;
        private bool disableHandHoldingController
        {
            get { return DisableHandHoldingController && magicLeapDeviceManagerPresent; }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="registrar">The <see cref="IMixedRealityServiceRegistrar"/> instance that loaded the data provider.</param>
        /// <param name="inputSystem">The <see cref="Microsoft.MixedReality.Toolkit.Input.IMixedRealityInputSystem"/> instance that receives data from this provider.</param>
        /// <param name="name">Friendly name of the service.</param>
        /// <param name="priority">Service priority. Used to determine order of instantiation.</param>
        /// <param name="profile">The service's configuration profile.</param>
        public MagicLeapHandTrackingInputProvider(
            IMixedRealityInputSystem inputSystem,
            string name = null,
            uint priority = DefaultPriority,
            BaseMixedRealityProfile baseProfile = null) : base(inputSystem, name, priority, baseProfile)
        {
        }
        public override void Initialize()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            base.Initialize();
        }

        private async void EnableIfLoaderBecomesActive()
        {
            await new WaitUntil(() => IsActiveLoader.HasValue);
            if (IsActiveLoader.Value)
            {
                Enable();
            }
        }

        public override void Enable()
        {
            profile = ConfigurationProfile as MagicLeapHandTrackingInputProfile;

            CurrentHandSettings = profile.HandednessSettings;
            GestureInteractionType = profile.GestureInteractionType;
            HandRayType = profile.HandRayType;
            PinchMaintainValue = profile.PinchMaintainValue;
            PinchTriggerValue = profile.PinchTriggerValue;
            DisableHandHoldingController = profile.DisableHandHoldingController;
            magicLeapDeviceManagerPresent = MagicLeapDeviceManager.Instance != null;

            if (!IsActiveLoader.HasValue)
            {
                IsEnabled = false;
                EnableIfLoaderBecomesActive();
                return;
            }
            else if (!IsActiveLoader.Value)
            {
                IsEnabled = false;
                return;
            }

            SetupInput();

            base.Enable();
        }

        private void SetupInput()
        {
            if (!mlHandTrackingActive)
            {
                if (!MLPermissions.CheckPermission(MLPermission.HandTracking).IsOk)
                {
                    Debug.LogError($"You must include the {MLPermission.HandTracking} permission in the AndroidManifest.xml to use Hand Tracking in this scene.");
                    return;
                }
                else
                {
                    InputSubsystem.Extensions.MLHandTracking.StartTracking();
                    mlHandTrackingActive = true;
                }
            }
        }

        private void FindDevices()
        {
            List<InputDevice> devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Left, devices);
            foreach (var device in devices)
            {
                if (device.isValid && device.name.Contains("MagicLeap"))
                {
                    leftHandDevice = device;
                    break;
                }
            }

            devices.Clear();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Right, devices);
            foreach (var device in devices)
            {
                if (device.isValid && device.name.Contains("MagicLeap"))
                {
                    rightHandDevice = device;
                    break;
                }
            }
        }

        public override void Update()
        {
            if (!IsEnabled)
            {
                return;
            }

            // Ensure input is active
            if (MLDevice.IsReady())
            {
                UpdateHands();
            }

        }

        protected void UpdateHands()
        {

            if (!leftHandDevice.isValid || !rightHandDevice.isValid)
                FindDevices();

            UpdateHand(rightHandDevice, Handedness.Right);
            UpdateHand(leftHandDevice, Handedness.Left);

        }

        public override void Disable()
        {

            if (mlHandTrackingActive)
            {
                RemoveAllHandDevices();

                mlHandTrackingActive = false;
            }

            if (Instance == this)
            {
                Instance = null;
            }

        }
        public override bool CheckCapability(MixedRealityCapability capability)
        {
            return (capability == MixedRealityCapability.ArticulatedHand);
        }

        #region Hand Management

     
        protected void UpdateHand(InputDevice mlHand, Handedness handedness)
        {
            if (!IsHandednessValid(handedness, CurrentHandSettings))
                return;

            bool handednessIsValid =
                disableHandHoldingController == false || MLControllerHandedness.GetControllerHandedness() != handedness;

            if (handednessIsValid && mlHand.isValid && TryGetOrAddHand(mlHand, handedness, out MagicLeapHand hand))
            {
                hand.SetUseMLHandRay(HandRayType);
                hand.SetUseMLGestures(GestureInteractionType);
                if (gestureInteractionType == MagicLeapHandTrackingInputProfile.MLGestureType.KeyPoints || gestureInteractionType == MagicLeapHandTrackingInputProfile.MLGestureType.Both)
                {
                    hand.setPinchValues(PinchMaintainValue, PinchTriggerValue);
                }
                hand.DoUpdate(allHands[handedness].IsPoseValid());
            }
            else
            {
                RemoveHandDevice(handedness);
            }
        }

        private void RemoveHandDevice(Handedness handedness)
        {
            if (trackedHands.TryGetValue(handedness, out MagicLeapHand hand))
            {
                hand.DoUpdate(allHands[handedness].IsPoseValid());
                RemoveHandDevice(hand);
            }
        }

        private bool IsHandednessValid(Handedness handedness, HandSettings settings)
        {
            switch (settings)
            {
                case HandSettings.None:
                    return false;

                case HandSettings.Left:
                    if (handedness != Handedness.Left)
                    {
                        return false;
                    }
                    break;

                case HandSettings.Right:
                    if (handedness != Handedness.Right)
                    {
                        return false;
                    }
                    break;

                case HandSettings.Both:
                    if (handedness != Handedness.Left && handedness != Handedness.Right)
                    {
                        return false;
                    }
                    break;
            }

            return true;
        }

        /// <summary>
        /// Used to determine if a hand is tracked. Stores the amount of time the hand has been considered tracked.
        /// </summary>
        public class MLHandContainer
        {
            //The hand device
            public InputDevice Device;
            //The time the hand was last tracking
            public float TrackTime;
            // The amount of time the hand has been detected
            public float LifeTime;
            //The MRTK Magic Leap Hand
            public MagicLeapHand MagicLeapHand;

            public MLHandContainer(InputDevice device)
            {
                Device = device;
            }
            public bool IsTrackingValid()
            {
                Device.TryGetFeatureValue(InputSubsystem.Extensions.DeviceFeatureUsages.Hand.Confidence, out float confidence);
                if (confidence > 0)
                {
                    TrackTime = Time.time;
                }

                bool isDoingAction = MagicLeapHand !=null && 
                                     (MagicLeapHand.IsPinching || MagicLeapHand.IsGrabbing);

                float latency = isDoingAction ? 1.25f : .75f;
                bool isTracking = Time.time - TrackTime < latency;

                return isTracking;
            }

            public bool IsPoseValid()
            {
                if (IsTrackingValid())
                {
                    LifeTime += Time.deltaTime;
                    if (LifeTime > .5f)
                    {
                        return true;
                    }
                }
                else
                {
                    LifeTime = 0;
                }
                return false;
            }

        }

        private bool TryGetOrAddHand(InputDevice mlHand, Handedness handedness, out MagicLeapHand magicLeapHand)
        {
            magicLeapHand = null;

            //Checks if the hand was previously tracked
            if (allHands.ContainsKey(handedness))
            {
                // If the hand is tracked but not considered valid, do not track it.
                // Used to filter when Left and Right hand are incorrectly identified
                if (!allHands[handedness].IsPoseValid() 
                    && (allHands[handedness].MagicLeapHand ==null || ( !allHands[handedness].MagicLeapHand.IsPositionAvailable 
                    && !allHands[handedness].MagicLeapHand.IsRotationAvailable)))
                {
                    return false;
                }
            }
            else
            {
                // If the hand has not been tracked before add it to the list
                // and start calculating the how long the hand has been considered tracked
                allHands.Add(handedness, new MLHandContainer(mlHand));
                return false;
            }

            // If the hand is valid and has been considered tracked, return it
            if (trackedHands.ContainsKey(handedness))
            {
                allHands[handedness].Device = mlHand;
                allHands[handedness].MagicLeapHand = trackedHands[handedness];
                magicLeapHand = trackedHands[handedness];
                return true;
            }

            // If the hand is valid and considered tracked, but has not been reported as track before.
            // Create a new MRTK input source provider.

            var pointers = RequestPointers(SupportedControllerType.ArticulatedHand, handedness);
            var inputSourceType = InputSourceType.Hand;

            var inputSource = Service?.RequestNewGenericInputSource($"Magic Leap {handedness} Hand", pointers, inputSourceType);

            var controller = new MagicLeapHand(TrackingState.NotTracked, handedness, inputSource);
            controller.Initalize(mlHand);

            for (int i = 0; i < controller.InputSource?.Pointers?.Length; i++)
            {
                controller.InputSource.Pointers[i].Controller = controller;
            }

            Service?.RaiseSourceDetected(controller.InputSource, controller);
            trackedHands.Add(handedness, controller);

            magicLeapHand = controller;
            allHands[handedness].MagicLeapHand = controller;
            allHands[handedness].Device = mlHand;

            return true;
        }

        private void RemoveAllHandDevices()
        {
            if (trackedHands.Count == 0) return;

            // Create a new list to avoid causing an error removing items from a list currently being iterated on.
            foreach (MagicLeapHand hand in new List<MagicLeapHand>(trackedHands.Values))
            {
                RemoveHandDevice(hand);
            }
            trackedHands.Clear();
        }

        private void RemoveHandDevice(MagicLeapHand hand)
        {
            //if (hand == null) return;

            IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;
            inputSystem?.RaiseSourceLost(hand.InputSource, hand);
            trackedHands.Remove(hand.ControllerHandedness);
            allHands.Remove(hand.ControllerHandedness);
            RecyclePointers(hand.InputSource);
        }

        #endregion

    }
}
