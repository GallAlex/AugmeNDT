// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using MagicLeap.MRTK.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;


[CustomEditor(typeof(MagicLeapSpatialMeshObserverProfile))]
public class MagicLeapSpatialMeshObserverProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
{
    private List<SerializedProperty> properties = new List<SerializedProperty>();

    protected override void OnEnable()
    {
        base.OnEnable();

        // Obtain all SerializedProperties, but only those explicitly declared in MagicLeapSpatialMeshObserverProfile.
        // Only showing MagicLeapSpatialMeshObserverProfile declared fields, which override all inherited fields,
        // in order to avoid confusion on what is relevant to the feature.
        SerializedProperty property = serializedObject.GetIterator();
        var expanded = true;
        while (property.NextVisible(expanded))
        {
            expanded = false;
            if (typeof(MagicLeapSpatialMeshObserverProfile).GetField(property.name,
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
            {
                properties.Add(property.Copy());
            }
        }
    }

    public override void OnInspectorGUI()
    {
        using (new EditorGUI.DisabledGroupScope(IsProfileLock((BaseMixedRealityProfile)target)))
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((MagicLeapSpatialMeshObserverProfile)target), GetType(), false);
            }

            foreach (var property in properties)
            {
                EditorGUILayout.PropertyField(property);
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