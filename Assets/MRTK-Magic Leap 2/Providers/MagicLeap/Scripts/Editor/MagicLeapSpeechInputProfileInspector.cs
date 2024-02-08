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
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.MRTK.DeviceManagement.Input
{
    [CustomEditor(typeof(MagicLeapSpeechInputProfile))]
    public class MagicLeapSpeechCommandsProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
    {
        private static readonly GUIContent MinusButtonContent = new GUIContent("-", "Remove Speech Command");
        private static readonly GUIContent AddButtonContent = new GUIContent("+ Add a New Speech Command", "Add Speech Command");
        private static readonly GUIContent LocalizationContent = new GUIContent("LocalizationKey", "An optional key to lookup a localized value for keyword");
        private static readonly GUIContent KeywordContent = new GUIContent("Keyword", "Spoken word that will trigger the action.  Overridden by a localized version if LocalizationKey is specified and found");
        private static readonly GUIContent KeyCodeContent = new GUIContent("KeyCode", "The keyboard key that will trigger the action.");
        private static readonly GUIContent ActionContent = new GUIContent("Action", "The action to trigger when a keyboard key is pressed or keyword is recognized.");
        private static readonly GUIContent SlotNameContent = new GUIContent("SlotName", "A Unique name for the Slot Data");
        private static readonly GUIContent SlotValueContent = new GUIContent("SlotValue", "All Values within the slot that can be used in Speech Commands.");

        private const string ProfileTitle = "Speech Settings";
        private const string ProfileDescription = "Speech Commands are any/all spoken keywords your users will be able say to raise an Input Action in your application.";

        private SerializedProperty recognizerStartBehaviour;
        private SerializedProperty recognitionConfidenceLevel;
        private SerializedProperty autoAllowAllSystemIntents;
        private SerializedProperty systemCommands;
        private SerializedProperty slotsForVoiceCommands;

        private static bool showSpeechCommands = true;
        private static bool showSlotData = true;
        private SerializedProperty speechCommands;
        private static GUIContent[] actionLabels = System.Array.Empty<GUIContent>();
        private static int[] actionIds = System.Array.Empty<int>();
        private bool isInitialized = false;

        protected override void OnEnable()
        {
            base.OnEnable();

            isInitialized = false;

            recognizerStartBehaviour = serializedObject.FindProperty("startBehavior");
            recognitionConfidenceLevel = serializedObject.FindProperty("recognitionConfidenceLevel");
            speechCommands = serializedObject.FindProperty("speechCommands");
            autoAllowAllSystemIntents = serializedObject.FindProperty("AutoAllowAllSystemIntents");
            systemCommands = serializedObject.FindProperty("SystemCommands");
            slotsForVoiceCommands = serializedObject.FindProperty("SlotsForVoiceCommands");

            var thisProfile = target as BaseMixedRealityProfile;

            if (!MixedRealityToolkit.IsInitialized ||
                !MixedRealityToolkit.Instance.ActiveProfile.IsInputSystemEnabled ||
                MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile == null ||
                thisProfile != MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile.SpeechCommandsProfile)
            {
                return;
            }

            var inputActions = MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions;
            actionLabels = inputActions.Select(action => new GUIContent(action.Description)).Prepend(new GUIContent("None")).ToArray();
            actionIds = inputActions.Select(action => (int)action.Id).Prepend(0).ToArray();

            isInitialized = true;
        }

        public override void OnInspectorGUI()
        {
            if (!RenderProfileHeader(ProfileTitle, ProfileDescription, target, isInitialized, BackProfileType.Input))
            {
                return;
            }

            CheckMixedRealityInputActions();

            using (new EditorGUI.DisabledGroupScope(IsProfileLock((BaseMixedRealityProfile)target)))
            {
                serializedObject.Update();

                EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
                {
                    EditorGUILayout.PropertyField(recognizerStartBehaviour);
                    EditorGUILayout.PropertyField(recognitionConfidenceLevel);
                }

                EditorGUILayout.Space();
                showSpeechCommands = EditorGUILayout.Foldout(showSpeechCommands, "Speech Commands", true, MixedRealityStylesUtility.BoldFoldoutStyle);
                if (showSpeechCommands)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.HelpBox("Any word followed by a ? will be optional as will any phrase within [ ]. Values are allowed to use | as an OR. " +
                        "Anything after the pipe will be considered a seperate expression. To just use an OR between two words in an expression surround them with ( ). " +
                        "This can also be used for Slots. To indicate use of a slot, put the slot name within { }.", MessageType.Info);
                        RenderList(speechCommands);
                    }
                }

                EditorGUILayout.LabelField("Magic Leap System Commands", EditorStyles.boldLabel);
                {
                    EditorGUILayout.PropertyField(autoAllowAllSystemIntents);
                    EditorGUILayout.PropertyField(systemCommands);
                }

                EditorGUILayout.Space();

                showSlotData = EditorGUILayout.Foldout(showSlotData, "Slot Data", true, MixedRealityStylesUtility.BoldFoldoutStyle);
                if (showSlotData)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        SlotListRender(slotsForVoiceCommands);
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        protected override bool IsProfileInActiveInstance()
        {
            var profile = target as BaseMixedRealityProfile;
            return MixedRealityToolkit.IsInitialized && profile != null &&
                   MixedRealityToolkit.Instance.HasActiveProfile &&
                   MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile != null &&
                   profile == MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile.SpeechCommandsProfile;
        }

        private void RenderList(SerializedProperty list)
        {
            // Disable speech commands if we could not initialize successfully
            using (new EditorGUI.DisabledGroupScope(!isInitialized))
            {
                EditorGUILayout.Space();
                using (new EditorGUILayout.VerticalScope())
                {
                    if (InspectorUIUtility.RenderIndentedButton(AddButtonContent, EditorStyles.miniButton))
                    {
                        list.arraySize += 1;
                        var speechCommand = list.GetArrayElementAtIndex(list.arraySize - 1);
                        var localizationKey = speechCommand.FindPropertyRelative("localizationKey");
                        localizationKey.stringValue = string.Empty;
                        var keyword = speechCommand.FindPropertyRelative("keyword");
                        keyword.stringValue = string.Empty;
                        var keyCode = speechCommand.FindPropertyRelative("keyCode");
                        keyCode.intValue = (int)KeyCode.None;
                        var action = speechCommand.FindPropertyRelative("action");
                        var actionId = action.FindPropertyRelative("id");
                        actionId.intValue = 0;
                    }

                    EditorGUILayout.Space();

                    if (list == null || list.arraySize == 0)
                    {
                        EditorGUILayout.HelpBox("Create a new Speech Command.", MessageType.Warning);
                        return;
                    }

                    for (int i = 0; i < list.arraySize; i++)
                    {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            SerializedProperty speechCommand = list.GetArrayElementAtIndex(i);

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                var keyword = speechCommand.FindPropertyRelative("keyword");
                                EditorGUILayout.PropertyField(keyword, KeywordContent);
                                if (GUILayout.Button(MinusButtonContent, EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                                {
                                    list.DeleteArrayElementAtIndex(i);
                                    break;
                                }
                            }

                            var localizationKey = speechCommand.FindPropertyRelative("localizationKey");
                            EditorGUILayout.PropertyField(localizationKey, LocalizationContent);

                            var keyCode = speechCommand.FindPropertyRelative("keyCode");
                            EditorGUILayout.PropertyField(keyCode, KeyCodeContent);

                            var action = speechCommand.FindPropertyRelative("action");
                            var actionId = action.FindPropertyRelative("id");
                            var actionDescription = action.FindPropertyRelative("description");
                            var actionConstraint = action.FindPropertyRelative("axisConstraint");

                            EditorGUI.BeginChangeCheck();
                            actionId.intValue = EditorGUILayout.IntPopup(ActionContent, actionId.intValue, actionLabels, actionIds);

                            if (EditorGUI.EndChangeCheck())
                            {
                                MixedRealityInputAction inputAction = actionId.intValue == 0 ? MixedRealityInputAction.None : MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile.InputActionsProfile.InputActions[actionId.intValue - 1];
                                actionDescription.stringValue = inputAction.Description;
                                actionConstraint.intValue = (int)inputAction.AxisConstraint;
                            }
                        }
                        EditorGUILayout.Space();
                    }
                }
            }
        }

        private void SlotListRender(SerializedProperty list)
        {
            EditorGUILayout.Space();
            using (new EditorGUILayout.VerticalScope())
            {
                if (InspectorUIUtility.RenderIndentedButton(new GUIContent("+ Add New Slot Data", "Add Slot Data"), EditorStyles.miniButton))
                {
                    list.arraySize += 1;
                    var slotList = list.GetArrayElementAtIndex(list.arraySize - 1);
                    var slotListName = slotList.FindPropertyRelative("name");
                    slotListName.stringValue = string.Empty;
                    var slotListValues = slotList.FindPropertyRelative("values");
                }

                EditorGUILayout.Space();

                if (list == null || list.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("Create new Slot Data.", MessageType.Warning);
                    return;
                }

                for (int i = 0; i < list.arraySize; i++)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        SerializedProperty SlotAtIndex = list.GetArrayElementAtIndex(i);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var sName = SlotAtIndex.FindPropertyRelative("name");
                            EditorGUILayout.PropertyField(sName, SlotNameContent);
                            if (GUILayout.Button(MinusButtonContent, EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                            {
                                list.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }

                        var sValues = SlotAtIndex.FindPropertyRelative("values");
                        EditorGUILayout.PropertyField(sValues, SlotValueContent, true);
                    }
                }
            }
        }
    }
}
