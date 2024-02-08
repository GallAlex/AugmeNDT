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

[CustomEditor(typeof(MagicLeapHandTrackingInputProfile))]
public class MagicLeapHandTrackingInputProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
{
    private SerializedProperty HandednessSettings;
    private SerializedProperty HandRayType;
    private SerializedProperty GestureInteractionType;
    private SerializedProperty PinchMaintainValue;
    private SerializedProperty PinchTriggerValue;
    private SerializedProperty DisableHandHoldingController;

    protected override void OnEnable()
    {
        base.OnEnable();

        HandednessSettings = serializedObject.FindProperty("HandednessSettings");
        HandRayType = serializedObject.FindProperty("HandRayType");
        GestureInteractionType = serializedObject.FindProperty("GestureInteractionType");
        PinchMaintainValue = serializedObject.FindProperty("PinchMaintainValue");
        PinchTriggerValue = serializedObject.FindProperty("PinchTriggerValue");
        DisableHandHoldingController = serializedObject.FindProperty("DisableHandHoldingController");

    }

    public override void OnInspectorGUI()
    {
        using (new EditorGUI.DisabledGroupScope(IsProfileLock((BaseMixedRealityProfile)target)))
        {
            serializedObject.Update();

            

            EditorGUILayout.PropertyField(HandednessSettings);
            EditorGUILayout.PropertyField(DisableHandHoldingController);

            var magicLeapDeviceManager = CoreServices.GetInputSystemDataProvider<MagicLeapDeviceManager>();
            bool showToolTip = DisableHandHoldingController.boolValue && magicLeapDeviceManager == null;

            if (showToolTip)
            {
                EditorGUILayout.HelpBox("Controller Detection will be disabled at runtime. Add the Magic Leap Device Manager Input Provider" +
                                        " to the MRTK Configuration Profile", MessageType.Warning);
            }
            EditorGUILayout.PropertyField(HandRayType);
            EditorGUILayout.PropertyField(GestureInteractionType);

            if (GestureInteractionType.enumValueIndex == (int)MagicLeapHandTrackingInputProfile.MLGestureType.KeyPoints || GestureInteractionType.enumValueIndex == (int)MagicLeapHandTrackingInputProfile.MLGestureType.Both)
            {
                EditorGUILayout.PropertyField(PinchMaintainValue);
                EditorGUILayout.PropertyField(PinchTriggerValue);
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