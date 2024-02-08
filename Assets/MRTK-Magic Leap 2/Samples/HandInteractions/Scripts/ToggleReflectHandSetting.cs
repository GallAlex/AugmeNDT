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

using Microsoft.MixedReality.Toolkit.UI;

namespace MagicLeap.MRTK.Samples
{
    // Added inside of the scope to prevent conflicts with Unity's 2020.3 Version Control package.
    using Microsoft.MixedReality.Toolkit.Input;
    
    [RequireComponent(typeof(Interactable))]
    public class ToggleReflectHandSetting : MonoBehaviour
    {
       public MagicLeapHandTrackingInputProvider.HandSettings SettingToReflect;
        Interactable interactable;

        private void Awake()
        {
            interactable = GetComponent<Interactable>();
        }

        private void Start()
        {
            UpdateToggle();
        }

        private void UpdateToggle()
        {
            if (MagicLeapHandTrackingInputProvider.Instance == null)
            {
                Debug.Log("Device Manager Not here");
                return;
            }
            
            switch(SettingToReflect)
            {
                case MagicLeapHandTrackingInputProvider.HandSettings.Both:
                    interactable.IsToggled =
                        MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings == MagicLeapHandTrackingInputProvider.HandSettings.Both;
                    break;
            
                case MagicLeapHandTrackingInputProvider.HandSettings.Left:
                    interactable.IsToggled =
                        MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings == MagicLeapHandTrackingInputProvider.HandSettings.Both ||
                        MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings == MagicLeapHandTrackingInputProvider.HandSettings.Left;
                    break;
            
                case MagicLeapHandTrackingInputProvider.HandSettings.Right:
                    interactable.IsToggled =
                        MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings == MagicLeapHandTrackingInputProvider.HandSettings.Both ||
                        MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings == MagicLeapHandTrackingInputProvider.HandSettings.Right;
                    break;
            }
        }

        public void ReflectToggleButton(Interactable interactable)
        {
            MagicLeapHandTrackingInputProvider.HandSettings CurrentHandSettings = MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings;
            MagicLeapHandTrackingInputProvider.HandSettings NewHandSettings = MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings;
            
            switch (SettingToReflect)
            {
                case MagicLeapHandTrackingInputProvider.HandSettings.Left:
                    switch (CurrentHandSettings)
                    {
                        case MagicLeapHandTrackingInputProvider.HandSettings.Left:
                            if (!interactable.IsToggled) // Turn off Left
                            {
                                NewHandSettings = MagicLeapHandTrackingInputProvider.HandSettings.None;
                            }
                            break;
            
                        case MagicLeapHandTrackingInputProvider.HandSettings.Right:
                            if (interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapHandTrackingInputProvider.HandSettings.Both;
                            }
                            break;
            
                        case MagicLeapHandTrackingInputProvider.HandSettings.None:
                            if (interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapHandTrackingInputProvider.HandSettings.Left;
                            }
                            break;
            
                        case MagicLeapHandTrackingInputProvider.HandSettings.Both:
                            if (!interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapHandTrackingInputProvider.HandSettings.Right;
                            }
                            break;
                    }
                    break;
            
                case MagicLeapHandTrackingInputProvider.HandSettings.Right:
                    switch (MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings)
                    {
                        case MagicLeapHandTrackingInputProvider.HandSettings.Right:
                            if (!interactable.IsToggled) // Turn off Right
                            {
                                NewHandSettings = MagicLeapHandTrackingInputProvider.HandSettings.None;
                            }
                            break;
            
                        case MagicLeapHandTrackingInputProvider.HandSettings.Left:
                            if (interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapHandTrackingInputProvider.HandSettings.Both;
                            }
                            break;
            
                        case MagicLeapHandTrackingInputProvider.HandSettings.None:
                            if (interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapHandTrackingInputProvider.HandSettings.Right;
                            }
                            break;
            
                        case MagicLeapHandTrackingInputProvider.HandSettings.Both:
                            if (!interactable.IsToggled)
                            {
                                NewHandSettings = MagicLeapHandTrackingInputProvider.HandSettings.Left;
                            }
                            break;
                    }
                    break;
            
                case MagicLeapHandTrackingInputProvider.HandSettings.Both:
                    NewHandSettings = interactable.IsToggled ?
                        MagicLeapHandTrackingInputProvider.HandSettings.Both : MagicLeapHandTrackingInputProvider.HandSettings.None;
                    break;
            }
            MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings = NewHandSettings;
            //Debug.Log("New Hand Settings: " + MagicLeapDeviceManager.Instance.CurrentHandSettings);
        }
    }
}
