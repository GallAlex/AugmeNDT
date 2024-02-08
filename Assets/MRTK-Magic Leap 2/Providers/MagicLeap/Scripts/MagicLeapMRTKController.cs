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
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    // Added inside of the scope to prevent conflicts with Unity's 2020.3 Version Control package.
    using Microsoft.MixedReality.Toolkit;

    
    [MixedRealityController(SupportedControllerType.GenericUnity,
        new[] { Handedness.Left, Handedness.Right })]
    public class MagicLeapMRTKController : BaseController, IMixedRealityHapticFeedback
    {
        InputDevice mlController;

        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;
        private Vector3 lastTouchVector;
        private float lastPressure;

        public MagicLeapMRTKController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null) : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

        public MagicLeapMRTKController(InputDevice controller, TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null) : base(trackingState, controllerHandedness, inputSource, interactions)
        {
            mlController = controller;
            mlInputs = new MagicLeapInputs();
            mlInputs.Enable();
            controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);

            controllerActions.TouchpadPosition.performed += HandleOnTouchpadDownPerformed;
            controllerActions.TouchpadPosition.canceled += HandleOnTouchpadDownCanceled;

            controllerActions.Bumper.started += MLControllerBumperDown;
            controllerActions.Bumper.canceled += MLControllerBumperUp;

            controllerActions.Trigger.started += MLControllerTriggerDown;
            controllerActions.Trigger.canceled += MLControllerTriggerUp;

            controllerActions.Menu.started += MLControllerMenuDown;
            controllerActions.Menu.canceled += MLControllerMenuUp;
        }

        public void CleanupController()
        {
            TrackingState = TrackingState.NotTracked;

            controllerActions.TouchpadPosition.performed -= HandleOnTouchpadDownPerformed;
            controllerActions.TouchpadPosition.canceled -= HandleOnTouchpadDownCanceled;

            controllerActions.Bumper.started -= MLControllerBumperDown;
            controllerActions.Bumper.canceled -= MLControllerBumperUp;

            controllerActions.Trigger.started -= MLControllerTriggerDown;
            controllerActions.Trigger.canceled -= MLControllerTriggerUp;

            controllerActions.Menu.started -= MLControllerMenuDown;
            controllerActions.Menu.canceled -= MLControllerMenuUp;

            mlInputs.Dispose();
        }

        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
            new MixedRealityInteractionMapping(1, "Select", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(2, "Bumper Press", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(3, "Menu", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(4, "Touchpad Touch", AxisType.Digital, DeviceInputType.TouchpadTouch),
            new MixedRealityInteractionMapping(5, "Touchpad Position", AxisType.DualAxis, DeviceInputType.Touchpad),
            new MixedRealityInteractionMapping(6, "Touchpad Press", AxisType.SingleAxis, DeviceInputType.TouchpadPress),
        };

        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
            new MixedRealityInteractionMapping(1, "Select", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(2, "Bumper Press", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(3, "Menu", AxisType.Digital, DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(4, "Touchpad Touch", AxisType.Digital, DeviceInputType.TouchpadTouch),
            new MixedRealityInteractionMapping(5, "Touchpad Position", AxisType.DualAxis, DeviceInputType.Touchpad),
            new MixedRealityInteractionMapping(6, "Touchpad Press", AxisType.SingleAxis, DeviceInputType.TouchpadPress),
        };

        public override bool IsInPointingPose
        {
            get
            {
                return true;
            }
        }

        public void UpdatePoses()
        {
            bool isPositionAvailable = mlController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 position);
            bool isRotationAvailable = mlController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

            MixedRealityPose pointerPose = new MixedRealityPose(MixedRealityPlayspace.TransformPoint(position),
                MixedRealityPlayspace.Rotation * rotation);

            Interactions[0].PoseData = pointerPose;
            CoreServices.InputSystem?.RaiseSourcePoseChanged(InputSource, this, pointerPose);
            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[0].MixedRealityInputAction, pointerPose);

            TrackingState = isPositionAvailable && isRotationAvailable ? TrackingState.Tracked: TrackingState.NotTracked;

            IsPositionAvailable = isPositionAvailable;
            IsRotationAvailable = isRotationAvailable;
        }

        private void HandleOnTouchpadDownPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            HandleOnTouchpadDown(obj, true);
        }

        private void HandleOnTouchpadDownCanceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            HandleOnTouchpadDown(obj, false);
        }

        private void HandleOnTouchpadDown(UnityEngine.InputSystem.InputAction.CallbackContext obj, bool touchActive)
        {

            //This is also a good time to implement the Touchpad if you want to update that source type
            if (Interactions.Length > 4)
            {
                IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;

                // Test out touch
                Interactions[4].BoolData = touchActive;

                if (Interactions[4].Changed)
                {
                    if (touchActive)
                    {
                        inputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[4].MixedRealityInputAction);
                    }
                    else
                    {
                        inputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[4].MixedRealityInputAction);
                    }
                }

                if (touchActive)
                {
                    lastTouchVector = controllerActions.TouchpadPosition.ReadValue<Vector2>();
                    lastPressure = controllerActions.TouchpadForce.ReadValue<float>();

                    Interactions[5].Vector2Data = lastTouchVector;
                    Interactions[6].FloatData = lastPressure;

                    if (Interactions[5].Changed)
                    {
                        inputSystem?.RaisePositionInputChanged(InputSource, ControllerHandedness, Interactions[5].MixedRealityInputAction, Interactions[5].Vector2Data);
                        // There is no press without a position, therefore, they're nested. Opposite not true (press without a position)
                        if (Interactions[6].Changed) // Pressure was last down
                        {
                            inputSystem?.RaiseFloatInputChanged(InputSource, ControllerHandedness, Interactions[6].MixedRealityInputAction, Interactions[6].FloatData);
                        }
                    }

                }
                else if (Interactions[6].FloatData > 0)
                {
                    lastPressure = 0;
                    Interactions[6].FloatData = lastPressure;
                }
            }
        }

        void MLControllerBumperDown(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;
            Interactions[2].BoolData = true;
            inputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[2].MixedRealityInputAction);
        }
        void MLControllerBumperUp(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;
            Interactions[2].BoolData = false;
            inputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[2].MixedRealityInputAction);
        }

        void MLControllerMenuDown(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;
            Interactions[3].BoolData = true;
            inputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[3].MixedRealityInputAction);
        }

        void MLControllerMenuUp(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;
            Interactions[3].BoolData = false;
            inputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[3].MixedRealityInputAction);
        }

        void MLControllerTriggerDown(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;
            Interactions[1].BoolData = true;
            inputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[1].MixedRealityInputAction);
        }

        void MLControllerTriggerUp(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            IMixedRealityInputSystem inputSystem = CoreServices.InputSystem;
            Interactions[1].BoolData = false;
            inputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[1].MixedRealityInputAction);
        }

        /// <summary>
        /// Default Haptics. No customization, intensity and duration are not taken into consideration.
        /// </summary>
        public bool StartHapticImpulse(float intensity, float durationInSeconds = Single.MaxValue)
        {
            Handheld.Vibrate();
            return true;
        }

        /// <summary>
        /// Starts a buzz haptics pattern.
        /// </summary>
        /// <param name="startHz">Start frequency of the buzz command (0 - 1250).</param>
        /// <param name="endHz">End frequency of the buzz command (0 - 1250).</param>
        /// <param name="durationMs">Duration of the buzz command in milliseconds (ms).</param>
        /// <param name="amplitude">Amplitude of the buzz command, as a percentage (0 - 100).</param>
        public bool StartHapticImpulse(ushort startHz, ushort endHz, ushort durationMs, byte amplitude)
        {
            MLResult result = InputSubsystem.Extensions.Haptics.StartBuzz(startHz, endHz, durationMs, amplitude);
            return result.IsOk;
        }

        /// <summary>
        /// Starts a pre-defined haptics pattern.
        /// </summary>
        /// <param name="preDefinedType">Pre-defined pattern to be played.</param>
        public bool StartHapticImpulse(InputSubsystem.Extensions.Haptics.PreDefined.Type preDefinedType)
        {
            MLResult result = InputSubsystem.Extensions.Haptics.StartPreDefined(preDefinedType);
            return result.IsOk;
        }

        /// <summary>
        /// Starts a custom haptic pattern.
        /// </summary>
        /// <param name="customPattern">A custom haptics pattern can be played by combining Buzz haptic commands and/or pre-defined patterns. PreDefined.Create and Buzz.Create can be used and then added to the customPattern.</param>
        public bool StartHapticImpulse(InputSubsystem.Extensions.Haptics.CustomPattern customPattern)
        {
            MLResult result = customPattern.StartHaptics();
            return result.IsOk;
        }

        public void StopHapticFeedback()
        {
            MLResult result = InputSubsystem.Extensions.Haptics.Stop();
            if(!result.IsOk)
            {
                Debug.LogWarning("MagicLeapMRTKController failed to Stop Haptics with result: " + result.ToString());
            }
        }
    }
}

