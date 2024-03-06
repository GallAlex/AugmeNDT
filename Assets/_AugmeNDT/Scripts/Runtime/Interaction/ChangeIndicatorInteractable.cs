using MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace AugmeNDT
{
    public class ChangeIndicatorInteractable : MonoBehaviour
    {
        public MRTKBaseInteractable interactable;
        public VisStackedHistogram refToClass;

        public string indicatorID = "Bin_Dataset1_Dataset2";

        // Start is called before the first frame update
        void Awake()
        {
            interactable = GetComponent<MRTKBaseInteractable>();
        }
        void Start()
        {
            //var onTouchReceiver = interactable.
                //AddReceiver<InteractableSelectMode>();
         //   onTouchReceiver.OnTouchStart.AddListener(() => refToClass.OnTouchIndicator(indicatorID)); ;
        }

    }
}
