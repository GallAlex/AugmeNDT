**Unity MRTK Package 1.12.1**
===================================

* Magic Leap SDK Version 1.4.0-dev2
* Magic Leap Unity SDK Version 1.12.0
* Unity Version: 2022.2
* MRTK Foundations v2.8.2
* MRTK Examples v2.8.2

**Release Focus**
-----------------

Incremental Update to 1.12.0

**Features**
------------

* Voice Intents - Refer to SpeechCommandsDemoMagicLeap scene for usage.
* Control - Refer to ControlMagicLeapDemo scene for usage.
* HandTracking - Refer to HandInterationExamplesMagicleap for usage. Only Partially implemented on platform currently.
* EyeTracking - Refer to EyeTrackingDemoMagicLeap for usage.
* Meshing - Refer to MeshingDemoMagicLeap for usage.
* AllInteractions - Refer to InteractionsDemoMagicLeap for Handtracking, Control, EyeTracking, and Voice usage together.

**1.12.1**
----------------------------

* Added support for a new minimum near clip distance of 25cm when adjusted in Settings.
* Fixed an issue with the pinch logic not correctly observing the Pinch Trigger and Maintain values. 

**Known Issues**
----------------

* Handtracking Performance issues when interacting with other objects. Continuous Improvements made each Sprint.
* To use the simulator when running MRTK, you must set the **Script Changes While Playing** setting to **Stop Playing and Recompile** or **Recompile and Conintue Playing** in the Unity **Preferences**.
* Occasionally the controller may not connect properly when launching a new application. Pausing and resuming the application resolves this.

**Important Notes**
-------------------

* Instead of copying a configuration file, clone the DefaultMixedReality version and make adjustments. We have found copying an MRTK configuration file can cause issues such as Input Data Providers not loading or visualizers not attaching properly.
* Controller Visualizer sometimes stops positioning and the logs say: Left_ControllerModel(Clone) is missing a IMixedRealityControllerVisualizer component! This happens sporatically, we have found adding the MixedRealityControllerVisualizer component to the model itself resolves this.
* If your application builds and results in a blank/empty scene, you must adjust your projects quality settings. (Known issue in editor. This will be resolved in future 2022.2 editor releases) To resolve this, remove all but one of the quality presets in your projects quality settings (**Edit>Player Settings>Quality**)
* Users may need to add the tracked pose driver to the camera themselves.
