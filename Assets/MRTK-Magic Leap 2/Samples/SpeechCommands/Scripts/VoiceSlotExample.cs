using MagicLeap.MRTK.DeviceManagement.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class VoiceSlotExample : MonoBehaviour
{
    public MeshRenderer rend;
    public Material RedMaterial;
    public Material BlueMaterial;
    public Material GreenMaterial;
    public string SlotNameColorController;

    private MagicLeapSpeechInputProvider speechProvider;
    private bool setCallback = false;

    // Start is called before the first frame update
    void Start()
    {
        if (rend == null)
        {
            rend = GetComponent<MeshRenderer>();
        }

        speechProvider = MagicLeapSpeechInputProvider.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(!setCallback)
        {
            if(MagicLeapSpeechInputProvider.Instance.IsRecognitionActive)
            {
                MLVoice.OnVoiceEvent += VoiceEvent;
                setCallback = true;
            }
        }
    }

    private void OnDestroy()
    {
        MLVoice.OnVoiceEvent -= VoiceEvent;
    }

    void VoiceEvent(in bool wasSuccessful, in MLVoice.IntentEvent voiceEvent)
    {
        if(voiceEvent.EventSlotsUsed.Count > 0)
        {
            MLVoice.EventSlot SlotData = voiceEvent.EventSlotsUsed.FirstOrDefault(s => s.SlotName == SlotNameColorController);
          
            if(SlotData.SlotName == SlotNameColorController)
            {
                switch (SlotData.SlotValue)
                {
                    case "Red":
                        {
                            rend.material = RedMaterial;
                            break;
                        }
                    case "Green":
                        {
                            rend.material = GreenMaterial;
                            break;
                        }
                    case "Blue":
                        {
                            rend.material = BlueMaterial;
                            break;
                        }
                }
            }
        }
    }
}
