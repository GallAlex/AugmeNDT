// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using MagicLeap.MRTK.DeviceManagement.Input;
using UnityEngine;
namespace MagicLeap.MRTK.Samples
{
    /// <summary>
    /// Demo script to show how to toggle hand tracking settings.
    /// The settings can be changed at runtime
    /// </summary>
    [System.Obsolete("Hand Tracking Settings are now located in the MagicLeapHandTrackingInputProfile attached to the Magic Leap Hand Tracking Input Data Provider. Settings can still be changed at runtime the same way.")]
    public class SetHandTrackingSettings : MonoBehaviour
    {
        public MagicLeapHandTrackingInputProvider.HandSettings _settings;

        // Start is called before the first frame update
        void Start()
        {
            MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings = _settings;
        }
    }
}
