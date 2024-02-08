
# MRTK Magic Leap 2

This setup guide assumes you have already configured your Unity Project for ML2. (https://developer.magicleap.cloud/learn/docs/guides/unity/getting-started/configure-unity-settings)

## Current Status

| Feature | Status |
|--|--|
| Controller | Release |
| Eye Tracking (Not calibrated) | Release |
| Hand Tracking | Pre-Release |
| Voice Commands | Pre-Release |

## Prerequisites

- Magic Leap SDK v1.4.0-dev2
- Magic Leap Unity SDK v1.12.0
- MRTK Foundations v2.8
- MRTK Examples v2.8
- A configured Magic Leap 2 Unity project

## Getting Started

### Configure Your Project

1. In Player Settings set the **Active Input Handling** to **Both**. Restarting the editor may be required.
2. Import the  **TMP Essential Resources**  by selecting  **Window > TextMeshPro > Import TMP Essential Resources**.
3. Open the Unity Preferences window and set the **Script Changes While Playing** setting to **Stop Playing and Recompile** or **Recompile and Conintue Playing** .
4. Install Universal RP from the Package Manager and set a **UniversalRenderPipelineAsset** in the **Graphics Settings**
5. Make sure **Custom Main Manifest** is selected under **Publishing Settings** so MagicLeap Manifest Settings can be set.

### Import MRTK

1. Download version 2.8 (Latest at this time is 2.8.2) of  **MRTK Foundation**  and  **MRTK Examples**  from the MRTK [GitHub](https://github.com/Microsoft/MixedRealityToolkit-Unity/releases).
2. Import the  **MRTK Foundation 2.8**  package into your Unity project. Apply the recommended settings from the popup window that appears after doing so.
3. It will ask for Script Updating Consent, select **Yes, for these and other files that might be found later**.
4. Next, import the  **MRTK Examples 2.8**  package into your project. Note: Some Occulus prefabs may log an error, these can be cleared.
5. Note that the MRTK Tools package is now required for future upgrading to a newer version of MRTK according to the releases page linked to in #1.  
6. From the top menu Select **Mixed Reality > Toolkit > Utilities> Upgrade MRTK Standard Shader for Universal Render Pipeline**. This will be greyed out unless a URP pipeline asset is set in the Graphics Settings

### Edit MRTK Standard Shader

The standard shader in MRTK may give an error in 2022.2 when building and may block Build and Run. Make these edits in the shader to fix this if this happens.

1. `fixed facing : VFACE` to `bool facing : SV_IsFrontFace` at line 775
2. `* facing` to `* (facing ? 1.0 : -1.0)` at lines 956 & 959

### Import MRTK Magic Leap 2

1. Download the MRTK Magic Leap 2 Unity Asset.
2. Import the asset into your project.

### (Potentially needed) Tracked Pose Driver

It may be needed for Users to add the tracked pose driver to the camera themselves.

### MRTK Magic Leap 2 Examples

You can test the MRTK Magic Leap 2 implementation using the Example scenes located under `MRTK-Magic Leap 2/Samples/`

Make sure the `MixedRealityToolkit's` configuration profile is set to `MagicLeap2 ...`

## Troubleshooting

### Configure Hand Tracking Settings

#### Track a Single Hand

This can be done by setting the following setting on `Start()`

```csharp
        MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings = MagicLeapHandTrackingInputProvider.HandSettings.Left;
```

#### Disable Hand Tracking

Hand tracking can be disabled by setting the MagicLeapHandTrackingInputProvider's `CurrentHandSettings` to `HandSettings.None`

```csharp
        MagicLeapHandTrackingInputProvider.Instance.CurrentHandSettings = MagicLeapDeviceManager.HandSettings.None;
```

The Magic Leap HandTracking Input Data Provider has Magic leap Settings to easily control these and set them in editor.
