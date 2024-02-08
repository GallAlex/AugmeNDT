// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;
using UnityEngine.Serialization;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

namespace MagicLeap.MRTK.DeviceManagement
{
    public class MagicLeapTouchInputHandler : BaseInputHandler, IMixedRealityInputHandler<Vector2>, IMixedRealityInputHandler<float>, IMixedRealityInputHandler
    {
        [SerializeField]
        [Tooltip("Input Action to handle")]
        private MixedRealityInputAction InputAction = MixedRealityInputAction.None;

        [FormerlySerializedAs("EvenActions")]
        public InputActionUnityEvent ActionEvents;

        #region InputSystemGlobalHandlerListener Implementation

        protected override void RegisterHandlers()
        {
            // Register for Input Events (Listen for MR w/ Vector2, float & bool).
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<Vector2>>(this);
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<float>>(this);
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler>(this);
        }

        protected override void UnregisterHandlers()
        {
            // Unregister for Input Events.
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler<Vector2>>(this);
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler<float>>(this); 
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler>(this);
        }

        #endregion InputSystemGlobalHandlerListener Implementation

        #region IMixedRealityInputHandler Implementation
        void IMixedRealityInputHandler<Vector2>.OnInputChanged(InputEventData<Vector2> eventData)
        {
            // Handle Touchpad Touch events & eventData
            if (eventData.MixedRealityInputAction.Id == InputAction.Id)
            {
                ActionEvents.Invoke(eventData);
            }
        }

        void IMixedRealityInputHandler<float>.OnInputChanged(InputEventData<float> eventData)
        {
            // Handle Touchpad Press events & eventData
            if (eventData.MixedRealityInputAction.Id == InputAction.Id)
            {
                ActionEvents.Invoke(eventData);
            }
        }
        void IMixedRealityInputHandler.OnInputDown(InputEventData eventData)
        {
            // Handle Touchpad Touch events & eventData (true)
            if (eventData.MixedRealityInputAction.Id == InputAction.Id)
            {
                ActionEvents.Invoke(eventData);
            }
        }
        void IMixedRealityInputHandler.OnInputUp(InputEventData eventData)
        {
            // Handle Touchpad touch events & eventData (on ended) (false)
            if (eventData.MixedRealityInputAction.Id == InputAction.Id)
            {
                ActionEvents.Invoke(eventData);
            }
        }
        #endregion IMixedRealityInputHandler Implementation
    }
}