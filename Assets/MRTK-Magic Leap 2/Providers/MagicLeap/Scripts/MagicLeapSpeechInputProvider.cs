// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UInput = UnityEngine.Input;

namespace MagicLeap.MRTK.DeviceManagement.Input
{

    [MixedRealityDataProvider(typeof(IMixedRealityInputSystem), SupportedPlatforms.Android, "Magic Leap Speech Input")]
    public class MagicLeapSpeechInputProvider : BaseInputDeviceManager, IMixedRealitySpeechSystem,
    IMixedRealityCapabilityCheck
    {
        private MagicLeapSpeechInputProfile profile;

        private MLVoiceIntentsConfiguration voiceConfiguration;

        private Dictionary<uint, string> voiceIds;

        private int randomRangeMin = 0;

        private int randomRangeMax = 1000;

        private bool isRunning = false;

        public static MagicLeapSpeechInputProvider Instance = null;

#pragma warning disable 414
        // Was Voice Intents permission granted by user
        private bool permissionGranted = false;
#pragma warning restore 414
        private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

        public MagicLeapSpeechInputProvider(IMixedRealityInputSystem inputSystem,
            string name = null,
            uint priority = DefaultPriority,
            BaseMixedRealityProfile baseProfile = null) : base(inputSystem, name, priority, baseProfile)
        {
        }

        public override void Initialize()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            base.Initialize();
        }

        /// <summary>
        /// The keywords to be recognized and optional keyboard shortcuts.
        /// </summary>
        private SpeechCommands[] Commands => InputSystemProfile.SpeechCommandsProfile.SpeechCommands;

        /// <summary>
        /// The Input Source for Windows Speech Input.
        /// </summary>
        public IMixedRealityInputSource InputSource => globalInputSource;

        /// <summary>
        /// The global input source used by the the speech input provider to raise events.
        /// </summary>
        private BaseGlobalInputSource globalInputSource = null;

        public bool IsRecognitionActive
        {
            get
            {
#if !UNITY_EDITOR
                return MLVoice.IsStarted;
#else
                return isRunning;
#endif

            }
        }

        #region IMixedRealityCapabilityCheck Implementation

        /// <inheritdoc />
        public bool CheckCapability(MixedRealityCapability capability)
        {
#if !UNITY_EDITOR
            return MLVoice.VoiceEnabled;
#else
            return capability == MixedRealityCapability.VoiceCommand;
#endif
        }

        #endregion IMixedRealityCapabilityCheck Implementation

        /// <inheritdoc />
        public void StartRecognition()
        {
            if (!voiceConfiguration)
            {
                voiceConfiguration = ScriptableObject.CreateInstance<MLVoiceIntentsConfiguration>();
            }

            if (voiceIds != null)
            {
                voiceIds.Clear();
            }

            voiceIds = new Dictionary<uint, string>();
            isRunning = true;
            InitializeKeywordRecognizer();
        }

        /// <inheritdoc />
        public void StopRecognition()
        {
#if !UNITY_EDITOR
            MLVoice.OnVoiceEvent -= VoiceEvent;
            MLVoice.Stop();

            permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
            permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
#endif
            isRunning = false;
        }

        /// <inheritdoc />
        public override void Enable()
        {
#if UNITY_EDITOR
            // Done in Permission Callback on Magic Leap Device
            StartRecognition();
#endif
            permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
            permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;

            MLPermissions.RequestPermission(MLPermission.VoiceInput, permissionCallbacks);

            // Call the base here to ensure any early exits do not
            // artificially declare the service as enabled.
            base.Enable();
        }

        private void InitializeKeywordRecognizer()
        {
            if (!Application.isPlaying ||
                (Commands == null) ||
                (Commands.Length == 0) ||
                InputSystemProfile == null
            )
            {
                return;
            }

            globalInputSource =
                Service?.RequestNewGlobalInputSource("Magic Leap Speech Input Source", sourceType: InputSourceType.Voice);

            if (voiceConfiguration.VoiceCommandsToAdd == null)
            {
                voiceConfiguration.VoiceCommandsToAdd = new List<MLVoiceIntentsConfiguration.CustomVoiceIntents>();
            }

            if (voiceConfiguration.AllVoiceIntents == null)
            {
                voiceConfiguration.AllVoiceIntents = new List<MLVoiceIntentsConfiguration.JSONData>();
            }

            // feed speech commands into config
            foreach (SpeechCommands command in Commands)
            {
                MLVoiceIntentsConfiguration.CustomVoiceIntents newIntent;
                newIntent.Value = command.Keyword;

                uint val = (uint)UnityEngine.Random.Range(randomRangeMin, randomRangeMax);
                while (voiceIds.ContainsKey(val))
                {
                    val = (uint)UnityEngine.Random.Range(randomRangeMin, randomRangeMax);
                }

                newIntent.Id = val;

                voiceConfiguration.VoiceCommandsToAdd.Add(newIntent);
                voiceIds.Add(val, command.Keyword);
            }

            profile = InputSystemProfile.SpeechCommandsProfile as MagicLeapSpeechInputProfile;

            voiceConfiguration.AutoAllowAllSystemIntents = profile.AutoAllowAllSystemIntents;
            voiceConfiguration.SystemCommands = profile.SystemCommands;
            voiceConfiguration.SlotsForVoiceCommands = profile.SlotsForVoiceCommands;

#if !UNITY_EDITOR
            MLResult result = MLVoice.SetupVoiceIntents(voiceConfiguration);

            if (result.IsOk)
            {
                MLVoice.OnVoiceEvent += VoiceEvent;
            }
            else
            {
                Debug.LogError("Failed to Setup Voice Intents with result: " + result);
            }
#endif

        }

        private static readonly ProfilerMarker UpdatePerfMarker =
            new ProfilerMarker("[MRTK] MagicLeapSpeechInputProvider.Update");

        /// <inheritdoc />
        public override void Update()
        {
            using (UpdatePerfMarker.Auto())
            {
                base.Update();
#if !UNITY_EDITOR
                if (!permissionGranted)
                {
                    return;
                }
#endif
#if UNITY_EDITOR
                for (int i = 0; i < Commands.Length; i++)
                {
                    if (UInput.GetKeyDown(Commands[i].KeyCode))
                    {
                        MLVoice.IntentEvent newEvent = new MLVoice.IntentEvent();
                        newEvent.EventName = Commands[i].LocalizedKeyword;
                        newEvent.EventID = voiceIds.FirstOrDefault(X => X.Value == Commands[i].LocalizedKeyword).Key;

                        VoiceEvent(true, newEvent);
                    }
                }
#endif
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
#if !UNITY_EDITOR
            StopRecognition(); 
#endif
            isRunning = false;
            base.Disable();
        }

        private static readonly ProfilerMarker OnPhraseRecognizedPerfMarker =
            new ProfilerMarker("[MRTK] MagicLeapInputProvider.OnPhraseRecognized");

        void VoiceEvent(in bool wasSuccessful, in MLVoice.IntentEvent voiceEvent)
        {
            using (OnPhraseRecognizedPerfMarker.Auto())
            {
                if (wasSuccessful)
                {
                    globalInputSource.UpdateActivePointers();

                    int index = 0;

                    for (int i = 0; i < Commands.Length; i++)
                    {
                        if (Commands[i].Keyword == voiceEvent.EventName)
                        {
                            index = i;
                        }
                    }

                    Service?.RaiseSpeechCommandRecognized(InputSource, RecognitionConfidenceLevel.High,
                        TimeSpan.Zero, DateTime.UtcNow, Commands[index]);
                }
            }
        }

        private void OnPermissionDenied(string permission)
        {
            MLPluginLog.Error($"MagicLeapSpeechInputProvider {permission} permission denied.");
        }

        private void OnPermissionGranted(string permission)
        {
            permissionGranted = true;
            StartRecognition();
        }
    }
}