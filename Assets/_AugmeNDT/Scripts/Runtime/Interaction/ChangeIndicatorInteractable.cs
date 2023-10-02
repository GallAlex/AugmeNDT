using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    public class ChangeIndicatorInteractable : MonoBehaviour
    {
        public Interactable interactable;
        public VisStackedHistogram refToClass;

        public string indicatorID = "Bin_Dataset1_Dataset2";

        // Start is called before the first frame update
        void Awake()
        {
            interactable = GetComponent<Interactable>();
        }
        void Start()
        {
            var onTouchReceiver = interactable.AddReceiver<InteractableOnTouchReceiver>();
            onTouchReceiver.OnTouchStart.AddListener(() => refToClass.OnTouchIndicator(indicatorID)); ;
        }

    }
}
