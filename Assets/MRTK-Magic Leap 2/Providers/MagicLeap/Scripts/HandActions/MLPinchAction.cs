// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2023 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using MagicLeap.MRTK.Utilities;
using UnityEngine.XR.MagicLeap.Native;
using static UnityEngine.XR.MagicLeap.MLHandActions.MLHandActions;
using static UnityEngine.XR.MagicLeap.MLHandActions.MLHandActions.NativeBindings;

namespace UnityEngine.XR.MagicLeap.MLHandActions
{
    /// <summary>
    /// Experimental Magic Leap API used to help pinch performance, especially during manipulations.
    /// </summary>
    internal static class MLPinchAction
    {
        /// <summary>
        /// Whether or not the system recognizes an active left-handed pinch
        /// </summary>
        public static bool LeftPinchDown { get; private set; } = false;

        /// <summary>
        /// Whether or not the system recognizes an active right-handed pinch
        /// </summary>
        public static bool RightPinchDown { get; private set; } = false;

        /// <summary>
        /// Whether or not the helper class is currently active
        /// </summary>
        public static bool Active => _active && _wrappersBound;
        private static bool _active = false;

        /// <summary>
        /// Whether the MLHandActions API has been successfully loaded
        /// </summary>
        private static bool _wrappersBound = false;

        /// <summary>
        /// Native callback function pointer storage
        /// </summary>
        private static MLHandActionCallbacks _callback;

        /// <summary>
        /// Managed callback used for adding new hand action data on Unity's main thread
        /// </summary>
        private static HandActionDelegate _handActionEventDelegate;

        /// <summary>
        /// Used to bind to Unity runtime methods
        /// </summary>
        private static MonoBehaviourProxy _proxy;

        /// <summary>
        /// A handle to the MLHandActions tracker
        /// </summary>
        private static ulong _handle = MagicLeapXrProviderNativeBindings.InvalidHandle;

        static MLPinchAction()
        {
            _proxy = new GameObject("MLPinchActions").AddComponent<MonoBehaviourProxy>();
            _proxy.OnPause += (paused) =>
            {
                if (paused) { Stop(); }
                else { Start(); }
            };
            GameObject.DontDestroyOnLoad(_proxy);
            Start();
        }

        /// <summary>
        /// Manually sets the system pinch helper class as active or inactive
        /// </summary>
        public static void SetActive(bool active)
        {
            _active = active;
        }

        /// <summary>
        /// Handles the hand action callback and sets a pinch state
        /// </summary>
        /// <param name="action">The HandAction data object for the received hand action</param>
        private static void HandleHandAction(MLHandActions.HandAction action)
        {
            if (action.Type == HandActionType.PinchTouch)
            {
                if (action.HandIndex == 0) // Left
                {
                    LeftPinchDown = action.State != HandActionState.End;
                }
                else // Right
                {
                    RightPinchDown = action.State != HandActionState.End;
                }
            }
        }

        private static void Start()
        {
            if (Active) { return; }
            // Wrap initialization calls in try catches. Dll Entry exceptions will be caught and will disable status
            try
            {
                MLInputCreate(ref _handle);
                _callback = MLHandActionCallbacks.Create(OnHandActionCallback);
                var code = MLInputSetHandActionCallbacks(_handle, ref _callback, IntPtr.Zero);
                _wrappersBound = MLResult.IsOK(code);
                _handActionEventDelegate += HandleHandAction;
                _active = true;
            }
            catch (Exception)
            {
                // Failed to loadAPI
                _wrappersBound = false;
                _active = false;
            }
        }

        private static void Stop()
        {
            LeftPinchDown = false;
            RightPinchDown = false;
            _active = false;
            _wrappersBound = false;
            MLInputDestroy(_handle);
            _handActionEventDelegate -= HandleHandAction;
            _handle = MagicLeapXrProviderNativeBindings.InvalidHandle;
        }

        [AOT.MonoPInvokeCallback(typeof(MLHandActionDelegate))]
        private static void OnHandActionCallback(ref MLHandAction action, IntPtr data)
        {
            MLTime.ConvertSystemTimeToMLTime(action.Timestamp, out MLTime mlTimestamp);
            var managed = new HandAction
            {
                HandIndex = action.HandIndex,
                Type = action.Type,
                State = action.State,
                Position = action.Pose.Position.ToVector3(),
                Rotation = new Quaternion(action.Pose.Rotation.X, action.Pose.Rotation.Y,
                                                action.Pose.Rotation.Z, action.Pose.Rotation.W),
                RawPosition = action.RawHandPose.Position.ToVector3(),
                RawRotation = new Quaternion(action.RawHandPose.Rotation.X, action.RawHandPose.Rotation.Y,
                                                    action.RawHandPose.Rotation.Z, action.RawHandPose.Rotation.W),
                Distance = action.Distance,
                Timestamp = mlTimestamp
            };
            MLThreadDispatch.Call(managed, _handActionEventDelegate);
        }
    }

}