// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    /// <summary>
    /// Configuration profile settings for Hand Tracking .
    /// </summary>
    [CreateAssetMenu(
        menuName =
            "Mixed Reality/Toolkit/Profiles/Magic Leap Speech Commands Profile",
        fileName = "MagicLeapSpeechInputProfile", order = (int)CreateProfileMenuItemIndices.Speech)]
    [MixedRealityServiceProfile(typeof(MagicLeapSpeechInputProvider))]
    public class MagicLeapSpeechInputProfile : MixedRealitySpeechCommandsProfile
    {
        [Tooltip("If True, will disregard the SystemCommands selected and allow all System Intents. If no System Inents are desired, leave this false and the SystemCommands empty.")]
        public bool AutoAllowAllSystemIntents;

        [Tooltip("Flag to indicate which System Intents should be enabled from within the application. In an experimental state as there may be issues using voice commands on any pop-up windows that appear because of the enabled system commands. If no System Inents are desired, leave this list empty.")]
        public MLVoiceIntentsConfiguration.SystemIntentFlags SystemCommands;

        [Tooltip("Slot Name should be unique as it will be used as a reference in CustomVoiceIntents values when placed between { }")]
        public List<MLVoiceIntentsConfiguration.SlotData> SlotsForVoiceCommands;
    }
}