// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using MagicLeap.MRTK.DeviceManagement.Input;
using UnityEngine.XR.MagicLeap;
using Microsoft.MixedReality.Toolkit;
using System;
using HapticsExtension = UnityEngine.XR.MagicLeap.InputSubsystem.Extensions.Haptics;

#if UNITY_EDITOR
#region CustomEditorRegion
[CustomEditor(typeof(MagicLeapHapticHandler))]
public class MagicLeapHapticHandlerEditor : Editor
{
    private Dictionary<MagicLeapHapticHandler.HapticsStyle, string[]> exclusionMap;
    SerializedProperty PatternListProperty;

    private void OnEnable()
    {
        exclusionMap = new Dictionary<MagicLeapHapticHandler.HapticsStyle, string[]>();
        // Define what to exclude from inspector in default mode
        exclusionMap[MagicLeapHapticHandler.HapticsStyle.DefaultHaptics] = new string[] { "buzzPattern", "predefinedPattern", "PatternList" };

        // Define properties to exclude in buzz mode
        exclusionMap[MagicLeapHapticHandler.HapticsStyle.Buzz] = new string[] { "predefinedPattern", "PatternList" };

        // Define properties to exclude in pre-define mode
        exclusionMap[MagicLeapHapticHandler.HapticsStyle.Predefined] = new string[] { "buzzPattern", "PatternList" };

        // Define properties to exclude in custom mode
        exclusionMap[MagicLeapHapticHandler.HapticsStyle.CustomPattern] = new string[] { "predefinedPattern", "buzzPattern"};

        PatternListProperty = serializedObject.FindProperty("PatternList");
    }


    public override void OnInspectorGUI()
    {
        MagicLeapHapticHandler handler = (MagicLeapHapticHandler)target;

        DrawPropertiesExcluding(serializedObject, exclusionMap[handler.style]);

        serializedObject.ApplyModifiedProperties();
    }
}
#endregion
#region CustomPropertyDrawerRegion
[CustomPropertyDrawer(typeof(CustomPattern))]
public class CustomPatternDrawer : PropertyDrawer
{
    SerializedProperty styleProperty;
    SerializedProperty buzzProperty;
    SerializedProperty predefinedProperty;

    //prop
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);


        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        styleProperty = property.FindPropertyRelative("style");
        buzzProperty = property.FindPropertyRelative("buzz");
        predefinedProperty = property.FindPropertyRelative("predefinedBuzz");

        // Calculate rects
        var styleRect = new Rect(position.x, position.y, 80, position.height);
        var buzzRect = new Rect(position.x + 130, position.y, 60, position.height);
        var predefinedRect = new Rect(position.x + 130, position.y, 60, position.height);


        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        EditorGUI.PrefixLabel(styleRect, new GUIContent("Style:"));
        styleRect.x += 40;
        EditorGUI.PropertyField(styleRect, styleProperty, GUIContent.none);

        if (styleProperty.enumValueFlag == (int)HapticsExtension.Type.Buzz)
        {
            // Draw buzz rect

            buzzRect.width = 35;
            EditorGUI.PrefixLabel(buzzRect, new GUIContent("StartHz:"));
            buzzRect.x += 50;
            EditorGUI.PropertyField(buzzRect, buzzProperty.FindPropertyRelative("startHz"), GUIContent.none);

            buzzRect.x += 40;
            EditorGUI.PrefixLabel(buzzRect, new GUIContent("EndHz:"));
            buzzRect.x += 50;
            EditorGUI.PropertyField(buzzRect, buzzProperty.FindPropertyRelative("endHz"), GUIContent.none);

            buzzRect.x += 40;
            EditorGUI.PrefixLabel(buzzRect, new GUIContent("Duration:"));
            buzzRect.x += 60;
            EditorGUI.PropertyField(buzzRect, buzzProperty.FindPropertyRelative("durationMs"), GUIContent.none);

            buzzRect.x += 40;
            EditorGUI.PrefixLabel(buzzRect, new GUIContent("Amplitude:"));
            buzzRect.x += 60;
            EditorGUI.PropertyField(buzzRect, buzzProperty.FindPropertyRelative("amplitude"), GUIContent.none);
        }
        else
        {
            // Draw predefinedBuzz
            EditorGUI.PrefixLabel(predefinedRect, new GUIContent("Type:"));
            predefinedRect.x += 40;
            EditorGUI.PropertyField(predefinedRect, predefinedProperty, GUIContent.none);
        }

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }
}
#endregion
#endif


[System.Serializable]
public struct CustomPattern
{
    public HapticsExtension.Type style;

    public Buzz buzz;

    public HapticsExtension.PreDefined.Type predefinedBuzz;

}

[System.Serializable]
public struct Buzz
{
    [Range(0, 1250), SerializeField]
    [Tooltip("Start frequency of the buzz command (0 - 1250)")]
    float startHz;

    [Range(0, 1250), SerializeField]
    [Tooltip("End frequency of the buzz command (0 - 1250)")]
    float endHz;

    [SerializeField]
    [Tooltip("Duration of the buzz command in milliseconds (ms)")]
    float durationMs;

    [SerializeField]
    [Range(0, 100)]
    [Tooltip("Amplitude of the buzz command, as a percentage (0 - 100)")]
    float amplitude;

    public void Deconstruct(out ushort start, out ushort end, out ushort duration, out byte amp)
    {
        start = (ushort)startHz;
        end = (ushort)endHz;
        duration = (ushort)durationMs;
        amp = (byte)amplitude;
    }
}


public class MagicLeapHapticHandler : BaseInputHandler, IMixedRealityInputHandler, IMixedRealityInputHandler<float>, IMixedRealityInputHandler<Vector2>
{

    [SerializeField]
    [Tooltip("Input Action to handle")]
    private MixedRealityInputAction InputAction = MixedRealityInputAction.None;

    public bool InvokedOnInputUp = false;
    public bool InvokedOnInputDown = true;

    [SerializeField]
    public enum HapticsStyle
    {
        DefaultHaptics,
        Buzz,
        Predefined,
        CustomPattern,
    }

    

    [SerializeField]
    public HapticsStyle style;


    [SerializeField]
    [Tooltip("Define a pattern by defining a collection of buzzes or pre-defined types")]
    public List<CustomPattern> PatternList;

    [SerializeField]
    [Tooltip("Pattern of buzz in terms of Start and End frequency, Duration, and Amplitude")]
    Buzz buzzPattern;





    [SerializeField]
    HapticsExtension.PreDefined.Type predefinedPattern;


    private IMixedRealityController[] controls;

    private MagicLeapMRTKController MLController;

    private MagicLeapMRTKController GetActiveController()
    {
        controls = MagicLeapDeviceManager.Instance.GetActiveControllers();
        if (controls.Length == 0)
        {
            Debug.Log("Failed to detect ML controller");
            return null;
        }
        return (MagicLeapMRTKController)controls[0];
    }

    public void InvokeCustomHaptics(List<CustomPattern> pattern)
    {
        if (MLController == null)
        {
            // In case called via external script
            MLController = GetActiveController();
        }
        // iterate over list
        foreach (CustomPattern element in pattern)
        {
            if (element.style is HapticsExtension.Type.Buzz)
            {
                var (start, end, duration, amp) = element.buzz; 
                MLController?.StartHapticImpulse(start, end, duration, amp);
            }
            else
            {
                MLController?.StartHapticImpulse(predefinedPattern);
            }
        }
    }

    public void InvokeHaptics()
    {
        MLController = GetActiveController();
        if (style == HapticsStyle.DefaultHaptics)
        {
            MLController?.StartHapticImpulse(0, 0);
        }
        else if (style == HapticsStyle.Buzz)
        {
            var (startHz, endHz, duration, amp) = buzzPattern;
            MLController?.StartHapticImpulse(startHz, endHz, duration, amp);
        }
        else if (style == HapticsStyle.Predefined)
        {
            MLController?.StartHapticImpulse(predefinedPattern);
        }
        else
        {
            InvokeCustomHaptics(PatternList);
        }
    }

    public void StopHaptics()
    {
        MLController = GetActiveController();
        MLController?.StopHapticFeedback();
    }

    protected override void RegisterHandlers()
    {
        // Register in IMixedRealityHapticsFeedback when it inherets from
        //CoreServices.InputSystem?.RegisterHandler<IMixedRealityHapticFeedback>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<Vector2>>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<float>>(this);
    }

    protected override void UnregisterHandlers()
    {
        // Unregister in IMixedRealityHapticsFeedback when it inherets from
        //CoreServices.InputSystem?.UnregisterHandler<IMixedRealityHapticFeedback>(this);
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<Vector2>>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<float>>(this);

    }


    public void OnInputChanged(InputEventData<float> eventData)
    {
        if (eventData.MixedRealityInputAction.Id == InputAction.Id)
        {
            InvokeHaptics();
        }
    }

    public void OnInputChanged(InputEventData<Vector2> eventData)
    {
        if (eventData.MixedRealityInputAction.Id == InputAction.Id)
        {
            InvokeHaptics();
        }
    }

    public void OnInputUp(InputEventData eventData)
    {
        if (eventData.MixedRealityInputAction.Id == InputAction.Id && InvokedOnInputUp)
        {
            InvokeHaptics();
        }
    }

    public void OnInputDown(InputEventData eventData)
    {
        if (eventData.MixedRealityInputAction.Id == InputAction.Id && InvokedOnInputDown)
        {
            InvokeHaptics();
        }
    }
}
