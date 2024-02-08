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
    [MixedRealityDataProvider(typeof(IMixedRealityInputSystem), SupportedPlatforms.Android, "Magic Leap Device Manager")]
    public class MagicLeapDeviceManager : XRSDKDeviceManager
    {
        private bool? IsActiveLoader =>
            LoaderHelpers.IsLoaderActive<MagicLeapLoader>();

        public static MagicLeapDeviceManager Instance = null;

        //If enabled, the controller input device will be disable when it is not being held.
        public bool DisableControllerWhenNotInHand;

        private List<IMixedRealityController> trackedControls = new List<IMixedRealityController>();

        private MagicLeapMRTKController currentController;
        private bool mlControllerCallbacksActive = false;

        private InputDevice controllerDevice;
        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;

        private bool testHandedness
        {
            get
            {
                return handTrackingInputProviderPresent && DisableControllerWhenNotInHand;
            }
        }

        //If enabled, the controller input device will be disable when it is not being held.
        private bool handTrackingInputProviderPresent;
        //Toggled using the controller's connect/disconnect callbacks.
        private bool isConnected;
        //Toggled in the update loop based on the value of isConnected.
        private bool didConnect;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="registrar">The <see cref="IMixedRealityServiceRegistrar"/> instance that loaded the data provider.</param>
        /// <param name="inputSystem">The <see cref="Microsoft.MixedReality.Toolkit.Input.IMixedRealityInputSystem"/> instance that receives data from this provider.</param>
        /// <param name="name">Friendly name of the service.</param>
        /// <param name="priority">Service priority. Used to determine order of instantiation.</param>
        /// <param name="profile">The service's configuration profile.</param>
        public MagicLeapDeviceManager(
            IMixedRealityInputSystem inputSystem,
            string name = null,
            uint priority = DefaultPriority,
            BaseMixedRealityProfile profile = null) : base(inputSystem, name, priority, profile)
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

            IsEnabled = true;
            SetupInput();
        }

        private void SetupInput()
        {
            MagicLeapDeviceManagerProfile profile = ConfigurationProfile as MagicLeapDeviceManagerProfile;
            if (profile != null)
            {
                MLControllerHandedness.MinimumDistanceToHand = profile.MinimumDistanceToHand;
                MLControllerHandedness.MaximumDistanceFromHead = profile.MaximumDistanceFromHead;
                MLControllerHandedness.DisableControllerDelay = profile.DisableControllerDelay;
                MLControllerHandedness.EnableControllerDelay = profile.EnableControllerDelay;
                DisableControllerWhenNotInHand = profile.DisableControllerWhenNotInHand;
            }

            handTrackingInputProviderPresent = MagicLeapHandTrackingInputProvider.Instance != null;

            if (!mlControllerCallbacksActive)
            {
                if (mlInputs != null)
                {
                    mlInputs.Enable();
                    controllerActions.Enable();
                }
                else
                {
                    mlInputs = new MagicLeapInputs();
                    mlInputs.Enable();
                    controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);
                    controllerActions.IsTracked.performed += MLControllerConnected;
                    controllerActions.IsTracked.canceled += MLControllerDisconnected;
                }
                mlControllerCallbacksActive = true;
            }
        }

        public override void Update()
        {
            if (IsEnabled && MLDevice.IsReady())
            {
                if (currentController == null && controllerActions.IsTracked.IsPressed())
                {
                    ConnectMLController();
                }
                else
                {
                    if (currentController != null)
                    {
                        Handedness currentHandedness = MLControllerHandedness.GetControllerHandedness();
                        if ((testHandedness == false && currentHandedness == Handedness.None)
                            || currentController.ControllerHandedness == MLControllerHandedness.GetControllerHandedness())
                        {
                            currentController.UpdatePoses();
                        }
                        else
                        {
                            DisableController(currentController);
                        }
                    }
                }

                if (isConnected != didConnect)
                {
                    if (isConnected)
                    {
                        ConnectMLController();
                    }
                    else
                    {
                        TryDisconnect();
                    }

                    isConnected = didConnect;
                }
            }
        }

        public override void Disable()
        {
            if (mlControllerCallbacksActive)
            {
                mlInputs.Disable();
                controllerActions.Disable();

                DisableController(currentController);
                mlControllerCallbacksActive = false;
            }

            if (Instance == this)
            {
                Instance = null;
            }

        }

        public override IMixedRealityController[] GetActiveControllers()
        {
            return trackedControls.ToArray<IMixedRealityController>();
        }

        /// <inheritdoc />
        public override bool CheckCapability(MixedRealityCapability capability)
        {
            return (capability == MixedRealityCapability.MotionController);
        }


        #region Controller Management

        void MLControllerConnected(UnityEngine.InputSystem.InputAction.CallbackContext callbackContext)
        {
                ConnectMLController();
                didConnect = true;
        }

        private void ConnectMLController()
        {
            if (!controllerDevice.isValid)
                controllerDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand);

            if (controllerDevice.isValid)
            {
                Handedness handedness = MLControllerHandedness.GetControllerHandedness();

                handedness = testHandedness == false && handedness == Handedness.None ? Handedness.Left: handedness;

                if (currentController == null)
                {
                    if (handedness != Handedness.None)
                    {
                        var pointers = RequestPointers(SupportedControllerType.GenericUnity, handedness);
                        var inputSourceType = InputSourceType.Controller;

                        var inputSource = Service?.RequestNewGenericInputSource($"Magic Leap {handedness} Controller", pointers, inputSourceType);

                        MagicLeapMRTKController controller = new MagicLeapMRTKController(controllerDevice, TrackingState.NotTracked, handedness, inputSource);
                        for (int i = 0; i < controller.InputSource?.Pointers?.Length; i++)
                        {
                            controller.InputSource.Pointers[i].Controller = controller;
                        }

                        Debug.Log("Controller Connected and found and valid and registered. Handedness : " + handedness);
                        currentController = controller;
                        Service?.RaiseSourceDetected(controller.InputSource, controller);
                        trackedControls.Add(controller);
                        controller.UpdatePoses();
                    }
                }
            }
        }

        private void MLControllerDisconnected(UnityEngine.InputSystem.InputAction.CallbackContext callbackContext)
        {
            didConnect = false;
        }

        private void TryDisconnect()
        {
            if (!controllerDevice.isValid)
                controllerDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand);

            if (currentController != null && !controllerDevice.isValid)
            {
                DisableController(currentController);
                Debug.Log("Controller Disconnected");
            }
        }

        private void DisableController(MagicLeapMRTKController mrtkController)
        {
            IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;
            inputSystem?.RaiseSourceLost(mrtkController.InputSource, mrtkController);
            RecyclePointers(mrtkController.InputSource);
            trackedControls.Remove(mrtkController);
            mrtkController.CleanupController();
            currentController = null;
        }

        #endregion

    }
}

