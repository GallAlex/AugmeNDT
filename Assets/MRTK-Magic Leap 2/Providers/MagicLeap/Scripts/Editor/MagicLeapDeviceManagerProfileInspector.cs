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
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Editor;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MagicLeapDeviceManagerProfile))]

public class MagicLeapDeviceManagerProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
{
    private SerializedProperty DisableControllerWhenNotInHand;
    private SerializedProperty MinimumControllerDistance;
    private SerializedProperty MaximumControllerDistance;
    private SerializedProperty EnableControllerDelayTime;
    private SerializedProperty DisableControllerDelayTime;

    protected override void OnEnable()
    {
        base.OnEnable();

        DisableControllerWhenNotInHand = serializedObject.FindProperty("DisableControllerWhenNotInHand");
        MinimumControllerDistance = serializedObject.FindProperty("MinimumDistanceToHand");
        MaximumControllerDistance = serializedObject.FindProperty("MaximumDistanceFromHead");
        EnableControllerDelayTime = serializedObject.FindProperty("EnableControllerDelay");
        DisableControllerDelayTime = serializedObject.FindProperty("DisableControllerDelay");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Magic Leap Settings", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledGroupScope(IsProfileLock((BaseMixedRealityProfile)target)))
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(DisableControllerWhenNotInHand);

                var magicLeapHandTrackingInputProvider = CoreServices.GetInputSystemDataProvider<MagicLeapHandTrackingInputProvider>();
                bool showToolTip = DisableControllerWhenNotInHand.boolValue && magicLeapHandTrackingInputProvider == null;
                bool showDetectionValues = DisableControllerWhenNotInHand.boolValue;

                if (showToolTip)
                {
                    EditorGUILayout.HelpBox("Hand Detection will be disabled at runtime. Add the Magic Leap Hand Tracking Input Provider" +
                                            " to the MRTK Configuration Profile", MessageType.Warning);
                }

                if (showDetectionValues)
                {
                    EditorGUILayout.PropertyField(MinimumControllerDistance);
                    if (MinimumControllerDistance.floatValue <= 0.03f)
                        MinimumControllerDistance.floatValue = 0.03f;

                    EditorGUILayout.PropertyField(MaximumControllerDistance);
                    if (MaximumControllerDistance.floatValue <= 0.05f)
                        MaximumControllerDistance.floatValue = 0.05f;

                    EditorGUILayout.PropertyField(EnableControllerDelayTime);
                    EditorGUILayout.PropertyField(DisableControllerDelayTime);
                }

                serializedObject.ApplyModifiedProperties();
            }
     
    }

    protected override bool IsProfileInActiveInstance()
    {
        var profile = target as BaseMixedRealityProfile;

        return MixedRealityToolkit.IsInitialized && profile != null &&
                MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile != null &&
                MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile.DataProviderConfigurations != null &&
                MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile.DataProviderConfigurations.Any(s => profile == s.Profile);
    }
}