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
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;


namespace MagicLeap.MRTK.Samples
{
    public class ControllerHapticsTest: MonoBehaviour
    {
        public void StartHapticBuzz()
        {
            IMixedRealityController[] trackedControls = MagicLeapDeviceManager.Instance.GetActiveControllers();

            if(trackedControls.Length == 0)
            {
                Debug.Log("ControllerHapticsTest failed to locate an IMixedRealityController");
                return;
            }

            ((MagicLeapMRTKController)trackedControls[0]).StartHapticImpulse(700, 500, 3000, 50);
        }
    }
}
