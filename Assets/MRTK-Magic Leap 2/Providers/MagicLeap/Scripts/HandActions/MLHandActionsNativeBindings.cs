// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END

using System;
using System.Runtime.InteropServices;
using UnityEngine.XR.MagicLeap.Native;

namespace UnityEngine.XR.MagicLeap.MLHandActions
{
    /// <summary>
    /// Bindings for experimental Magic Leap API.
    /// </summary>
    internal class MLHandActions
    {
        /// <summary>
        /// Delegate to handle incoming MLHandActions
        /// </summary>
        public delegate void HandActionDelegate(HandAction action);
        /// <summary>
        /// Hand Action states
        /// </summary>
        public enum HandActionState
        {
            /// <summary>
            /// Action Start (Default)
            /// </summary>
            Start = 0,

            /// <summary>
            /// Action Continue
            /// </summary>
            Continue = 1,

            /// <summary>
            /// Action End
            /// </summary>
            End = 2
        }

        /// <summary>
        /// Hand Action types
        /// </summary>
        public enum HandActionType
        {
            /// <summary>
            /// No action. (Default)
            /// </summary>
            None = 0,

            /// <summary>
            /// Cursor<br/>
            /// A raycast pose that is drawn from user's shoulder to the hand index MCP joint.
            /// This is triggered by a discrete event, so it has only a Start state.
            /// </summary>
            Cursor = 1,

            /// <summary>
            /// Relative Cursor<br/>
            /// This is triggered by a discrete event, so it has only a Start state.
            /// </summary>
            RelativeCursor = 2,

            /// <summary>
            /// Pinch Tap<br/>
            /// This action is triggered when the user conduct an open Pinch, a closed Pinch (index and thumb tips touch)
            /// and back to open Pinch in sequence within 400 ms. <br/>
            /// This is triggered by a discrete event, so it has only a Start state.
            /// </summary>
            PinchTap = 3,

            /// <summary>
            /// Pinch Hold<br/>
            /// This action is triggered when the user conduct an open Pinch followed by a closed Pinch (index and thumb tips touch)
            /// and then hold the touch for 1 second.  <br/>
            /// This is triggered by a discrete event, so it has only a Start state.
            /// </summary>
            PinchHold = 4,

            /// <summary>
            /// Pinch Release<br/>
            /// This action is triggered when the user releases a Pinch Hold (so index and thumb tips stops touching). <br/>
            /// This is triggered by a discrete event, so it has only a Start state.
            /// </summary>
            PinchHoldRelease = 5,

            /// <summary>
            /// PinchTouch<br/>
            /// This action is triggered with multiple states based on user's gesture.</br>
            /// -MLHandActionState_Start: when the user conducts an open Pinch followed by a closed Pinch.</br>
            /// -MLHandActionState_Continue: when the user keeps the closed Pinch.</br>
            /// -MLHandActionState_End: following a closed Pinch, the user conducts an open Pinch.
            /// </summary>
            PinchTouch = 6
        }

        /// <summary>
        /// A structure containing information about identified hand action.
        /// </summary>
        public struct HandAction
        {
            /// <summary>
            /// The index of the Hand to which this action was identified against<br/>
            /// 0 denotes Left<br/>
            /// 1 denotes Right
            /// </summary>
            public uint HandIndex;

            /// <summary>
            /// The type of hand action performed.
            /// </summary>
            public HandActionType Type;

            /// <summary>
            /// The state of the hand action.
            /// </summary>
            public HandActionState State;

            /// <summary>
            /// Position of the hand action's transform
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Rotation of the hand action's transform
            /// </summary>
            public Quaternion Rotation;

            /// <summary>
            /// Raw hand action pose data only used for debug purposes.
            /// </summary>
            public Vector3 RawPosition;

            /// <summary>
            /// Raw hand action pose data only used for debug purposes.
            /// </summary>
            public Quaternion RawRotation;

            /// <summary>
            /// Distance between relevant fingers (dependent of the type) <br/>For example, for <c>Pinch</c> pose
            /// it is the distance between the thumb tip and the index tip.
            /// </summary>
            public float Distance;

            /// <summary>
            /// Timestamp of the action.
            /// </summary>
            public MLTime Timestamp;

            public override string ToString()
            {
                return $"[ HandIndex={HandIndex}, Type={Type}, State={State},\n" +
                    $"\tPosition={Position},\n" +
                    $"\tRotation={Rotation},\n" +
                    $"\tRawPosition=({RawPosition.x}, {RawPosition.y}, {RawPosition.z}),\n" +
                    $"\tRawRotation=({RawRotation.w}, {RawRotation.y}, {RawRotation.z}, {RawRotation.w}),\n" +
                    $"\tDistance={Distance},\n" +
                    $"\tTimestamp={Timestamp} ]";
            }
        }

        /// <summary>
        /// A callback type to receive MLHandActions data
        /// </summary>
        public delegate void MLHandActionDelegate(ref NativeBindings.MLHandAction action, IntPtr data);

        /// <summary>
        /// Wrapper for native data types associated with MLHandActions
        /// </summary>
        internal class NativeBindings
        {
            /// <summary>
            /// A structure that encapsulates recognized interaction events associated with hand tracking
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MLHandAction
            {
                public uint Version;

                public uint HandIndex;

                public HandActionType Type;

                public HandActionState State;

                [MarshalAs(UnmanagedType.LPStruct)]
                public MagicLeapNativeBindings.MLTransform Pose;

                [MarshalAs(UnmanagedType.LPStruct)]
                public MagicLeapNativeBindings.MLTransform RawHandPose;

                public float Distance;

                public long Timestamp;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MLHandActionCallbacks
            {
                public uint Version;
                public MLHandActionDelegate HandActionCallback;

                public static MLHandActionCallbacks Create(MLHandActionDelegate callback)
                {
                    return new MLHandActionCallbacks()
                    {
                        Version = 1,
                        HandActionCallback = callback
                    };
                }
            }

            [DllImport("input.magicleap", CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLInputCreate(ref ulong handle);
            [DllImport("input.magicleap", CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLInputDestroy(ulong handle);
            [DllImport("input.magicleap", CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLInputSetHandActionCallbacks(ulong handle, ref MLHandActionCallbacks callbacks, IntPtr userData);
        }
    }
}