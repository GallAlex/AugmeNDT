// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using MagicLeap.MRTK.DeviceManagement.Input;
using Microsoft.MixedReality.Toolkit;

namespace MagicLeap.MRTK.Samples
{
    /// <summary>
    /// Demo script to show how to change the behavior of MRTK pointers.
    /// The settings can be changed at runtime
    /// </summary>
    public class MRTKPointerSettings : MonoBehaviour, IMixedRealitySourceStateHandler
    {
        [SerializeField]
        public PointerBehavior MotionControllerPointerBehavior = PointerBehavior.AlwaysOn;


        bool leftHandDetected = false;
        bool rightHandDetected = false;
        bool controllerDetected = false;

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            if (eventData.Controller != null && eventData.Controller.GetType() == typeof(MagicLeapHand))
            {
                if (eventData.Controller.ControllerHandedness.IsLeft())
                {
                    leftHandDetected = true;
                }
                if (eventData.Controller.ControllerHandedness.IsRight())
                {
                    rightHandDetected = true;
                }
            }
            else if (eventData.Controller != null && eventData.Controller.GetType() == typeof(MagicLeapMRTKController))
            {
                controllerDetected = true;
            }
            adjustFallbackController();

         }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            if (eventData.Controller != null && eventData.Controller.GetType() == typeof(MagicLeapHand))
            {
                if (eventData.Controller.ControllerHandedness.IsLeft())
                {
                    leftHandDetected = false;
                }
                if (eventData.Controller.ControllerHandedness.IsRight())
                {
                    rightHandDetected = false;
                }
            }
            else if (eventData.Controller != null && eventData.Controller.GetType() == typeof(MagicLeapMRTKController))
            {
                controllerDetected = false;
            }
            adjustFallbackController();
        }

        private void adjustFallbackController()
        {

            if (!controllerDetected && !leftHandDetected && !rightHandDetected)
            {
                PointerUtils.SetGazePointerBehavior(PointerBehavior.AlwaysOn);
            }
            else
            {
                PointerUtils.SetGazePointerBehavior(PointerBehavior.AlwaysOff);
            }
        }
        private void OnEnable()
        {
            CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
        }


        private void OnDisable()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealitySourceStateHandler>(this);
        }

        // Keeps the pointer active on the Motion Controller
        void Start()
        {
            PointerUtils.SetMotionControllerRayPointerBehavior(MotionControllerPointerBehavior);
        }
    }
}
