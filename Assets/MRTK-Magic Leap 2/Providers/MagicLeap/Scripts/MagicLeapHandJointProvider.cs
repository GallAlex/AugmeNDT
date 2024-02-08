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
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    public class MagicLeapHandJointProvider 
    {
        public Dictionary<TrackedHandJoint, MixedRealityPose> JointPoses;

        //An array of bones that are used to map between Magic Leap and MRTK
        private readonly TrackedHandJoint[] _handJoints = new TrackedHandJoint[]
        {
            TrackedHandJoint.ThumbTip, TrackedHandJoint.ThumbDistalJoint, TrackedHandJoint.ThumbProximalJoint,TrackedHandJoint.ThumbMetacarpalJoint,
            TrackedHandJoint.None, //Thumb returns 5
            TrackedHandJoint.IndexTip, TrackedHandJoint.IndexDistalJoint, TrackedHandJoint.IndexMiddleJoint,TrackedHandJoint.IndexKnuckle,TrackedHandJoint.IndexMetacarpal,
            TrackedHandJoint.MiddleTip, TrackedHandJoint.MiddleDistalJoint, TrackedHandJoint.MiddleMiddleJoint,TrackedHandJoint.MiddleKnuckle, TrackedHandJoint.MiddleMetacarpal,
            TrackedHandJoint.RingTip, TrackedHandJoint.RingDistalJoint, TrackedHandJoint.RingMiddleJoint,TrackedHandJoint.RingKnuckle, TrackedHandJoint.RingMetacarpal,
            TrackedHandJoint.PinkyTip, TrackedHandJoint.PinkyDistalJoint, TrackedHandJoint.PinkyMiddleJoint,TrackedHandJoint.PinkyKnuckle, TrackedHandJoint.PinkyMetacarpal
        };

        //Used to see if the hand is currently being tracked.
        private List<Bone> _pinkyFingerBones = new List<Bone>();
        private List<Bone> _ringFingerBones = new List<Bone>();
        private List<Bone> _middleFingerBones = new List<Bone>();
        private List<Bone> _indexFingerBones = new List<Bone>();
        private List<Bone> _thumbBones = new List<Bone>();

        private Handedness _controllerHandedness;

        // Allow this joint provider to tolerate dropped frames for a brief time.
        // This helps to avoid dropping active interactions like pinching or grabbing.
        private const float VALID_HAND_TRACKED_TIMEOUT = .5f;
        private float _validHandTimer = 0.0f;
        private float _validHandLastTime = 0.0f;

        // Hand joint rotation data used in joint hierarchy
        private class JointRotationData
        {
            public TrackedHandJoint joint = TrackedHandJoint.None;
            public float relYaw = 0;
            public float relRoll = 0;
            public JointRotationData child = null;
            public List<JointRotationData> children = null;
        }

        // Data structure representing a full hand joint hierarchy, from wrist to finder tips, used for calculating joint rotations.
        private readonly JointRotationData _handHierarchyRoot = new JointRotationData
        {
            joint = TrackedHandJoint.Wrist,
            children = new List<JointRotationData>()
            {
                // Palm
                new JointRotationData{                   joint = TrackedHandJoint.Palm },
                // Thumb
                new JointRotationData{                   joint = TrackedHandJoint.ThumbMetacarpalJoint, relYaw = -20f, relRoll = 50f,
                         child = new JointRotationData { joint = TrackedHandJoint.ThumbProximalJoint,
                         child = new JointRotationData { joint = TrackedHandJoint.ThumbDistalJoint,
                         child = new JointRotationData { joint = TrackedHandJoint.ThumbTip } } } },
                // Index
                new JointRotationData{                   joint = TrackedHandJoint.IndexKnuckle, relYaw = -5f,
                         child = new JointRotationData { joint = TrackedHandJoint.IndexMiddleJoint,
                         child = new JointRotationData { joint = TrackedHandJoint.IndexDistalJoint,
                         child = new JointRotationData { joint = TrackedHandJoint.IndexTip } } } },
                // Middle
                new JointRotationData{                   joint = TrackedHandJoint.MiddleKnuckle,
                         child = new JointRotationData { joint = TrackedHandJoint.MiddleMiddleJoint,
                         child = new JointRotationData { joint = TrackedHandJoint.MiddleDistalJoint,
                         child = new JointRotationData { joint = TrackedHandJoint.MiddleTip } } } },
                // Ring
                new JointRotationData{                   joint = TrackedHandJoint.RingKnuckle, relYaw = 10f, relRoll = -20f,
                         child = new JointRotationData { joint = TrackedHandJoint.RingMiddleJoint,
                         child = new JointRotationData { joint = TrackedHandJoint.RingDistalJoint,
                         child = new JointRotationData { joint = TrackedHandJoint.RingTip } } } },
                // Pinky
                new JointRotationData{                   joint = TrackedHandJoint.PinkyKnuckle, relYaw = 20f, relRoll = -30f,
                         child = new JointRotationData { joint = TrackedHandJoint.PinkyMiddleJoint,
                         child = new JointRotationData { joint = TrackedHandJoint.PinkyDistalJoint,
                         child = new JointRotationData { joint = TrackedHandJoint.PinkyTip } } } }
            }
        };

        public bool IsPositionAvailable
        {
            get;
            private set;
        }
        public bool IsRotationAvailable
        {
            get;
            private set;
        }
        public MagicLeapHandJointProvider(Handedness controllerHandedness)
        {
            _controllerHandedness = controllerHandedness;
            JointPoses = new Dictionary<TrackedHandJoint, MixedRealityPose>();
        }

        public void Reset()
        {
            IsRotationAvailable = false;
            IsPositionAvailable = false;
            JointPoses.Clear();
        }

        public void UpdateHandJoints(InputDevice device, InputDevice gestureDevice)
        {
            if (JointPoses == null)
            {
                JointPoses = new Dictionary<TrackedHandJoint, MixedRealityPose>();
            }

            // Update valid hand timer
            _validHandTimer -= (Time.timeSinceLevelLoad - _validHandLastTime);
            _validHandLastTime = Time.timeSinceLevelLoad;

            device.TryGetFeatureValue(InputSubsystem.Extensions.DeviceFeatureUsages.Hand.Confidence, out float confidence);
            bool isTracking = confidence > 0;

            InputSubsystem.Extensions.MLHandTracking.TryGetKeyPointsMask(device, out bool[] keyPointsMask);
            if (!isTracking || !device.TryGetFeatureValue(CommonUsages.handData, out UnityEngine.XR.Hand hand))
            {
                // As the hand is untracked, the position and rotation availability will retain their previous values until the
                // valid hand timer has timed out, then position and rotation availability are set to false.
                if (_validHandTimer <= 0.0f)
                {
                    IsPositionAvailable = IsRotationAvailable = false;
                }
                return;
            }

            _validHandTimer = VALID_HAND_TRACKED_TIMEOUT;

            IsPositionAvailable = IsRotationAvailable = true;
            UpdateFingerBones(hand, HandFinger.Thumb, keyPointsMask, ref this._thumbBones);

            UpdateFingerBones(hand, HandFinger.Index, keyPointsMask, ref this._indexFingerBones);

            UpdateFingerBones(hand, HandFinger.Middle, keyPointsMask, ref this._middleFingerBones);

            UpdateFingerBones(hand, HandFinger.Ring, keyPointsMask, ref this._ringFingerBones);

            UpdateFingerBones(hand, HandFinger.Pinky, keyPointsMask, ref this._pinkyFingerBones);
          
            UpdateWristPosition(device);
            UpdatePalmPose(device,gestureDevice);
        }

        private void UpdateWristPosition(InputDevice hand)
        {
            if (hand.TryGetFeatureValue(InputSubsystem.Extensions.DeviceFeatureUsages.Hand.WristCenter,
                    out Bone wristBone))
            {
                if (wristBone.TryGetPosition(out Vector3 position) && wristBone.TryGetRotation(out Quaternion rotation))
                {
                    var wristPose = new MixedRealityPose(MixedRealityPlayspace.TransformPoint(position), MixedRealityPlayspace.Rotation * rotation);

                    if (!JointPoses.ContainsKey(TrackedHandJoint.Wrist))
                    {
                        JointPoses.Add(TrackedHandJoint.Wrist, wristPose);
                    }
                    else
                    {
                        JointPoses[TrackedHandJoint.Wrist] = wristPose;
                    }
                }
            }
        }

        private void UpdatePalmPose(InputDevice hand, InputDevice gestureDevice)
        {
            var palmPose = new MixedRealityPose();

            if (hand.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePosition))
            {
                palmPose.Position = MixedRealityPlayspace.TransformPoint(devicePosition);
            }

            if (hand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation))
            {
                palmPose.Rotation = MixedRealityPlayspace.Rotation * deviceRotation;
            }

            if (!JointPoses.ContainsKey(TrackedHandJoint.Palm))
            {
                JointPoses.Add(TrackedHandJoint.Palm, palmPose);
            }
            else
            {
                JointPoses[TrackedHandJoint.Palm] = palmPose;
            }
        }

        private void UpdateFingerBones(Hand hand, HandFinger finger, bool[] keyPointsMask, ref List<Bone> bones)
        {
            hand.TryGetFingerBones(finger, bones);

            // The index of the MRTK bones is different than the index of the Magic Leap bones.
            int mapIndex = 0;
            int mrtkIndex = 0;
            int fifthbone = 0;
            switch (finger)
            {
                case HandFinger.Thumb:
                    mapIndex = 0;
                    mrtkIndex = 0;
                    break;
                case HandFinger.Index:
                    mapIndex = 4;
                    mrtkIndex = 5;
                    fifthbone = 0;
                    break;
                case HandFinger.Middle:
                    mapIndex = 8;
                    mrtkIndex = 10;
                    fifthbone = 1;
                    break;
                case HandFinger.Ring:
                    mapIndex = 12;
                    mrtkIndex = 15;
                    fifthbone = 2;
                    break;
                case HandFinger.Pinky:
                    mapIndex = 16;
                    mrtkIndex = 20;
                    fifthbone = 3;
                    break;
            }

            for (int i = 0; i < bones.Count; i++)
            {
                int enumLocation = mapIndex + i;
                if (i == 4)
                {
                    enumLocation = (int)InputSubsystem.Extensions.MLHandTracking.KeyPointLocation.FifthBone + fifthbone;
                }
                if (keyPointsMask[enumLocation])
                {
                    bones[i].TryGetPosition(out Vector3 position);
                    bones[i].TryGetRotation(out Quaternion rotation);

                    var fingerPose = new MixedRealityPose(MixedRealityPlayspace.TransformPoint(position), MixedRealityPlayspace.Rotation * rotation);

                    if (!JointPoses.ContainsKey(_handJoints[mrtkIndex + i]))
                    {
                        JointPoses.Add(_handJoints[mrtkIndex + i], fingerPose);
                    }
                    else
                    {
                        JointPoses[_handJoints[mrtkIndex + i]] = fingerPose;
                    }
                }
            }

        }

    }
}
