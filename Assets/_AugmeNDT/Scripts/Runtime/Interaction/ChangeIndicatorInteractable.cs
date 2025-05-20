using MixedReality.Toolkit.UX;
using UnityEngine;

namespace AugmeNDT
{
    public class ChangeIndicatorInteractable : MonoBehaviour
    {
        public VisChronoBins RefToClass;

        public string indicatorID = "Bin_Dataset1_Dataset2";

        void Start()
        {
            var onTouchReceiver = GetComponent<PressableButton>();
            onTouchReceiver.selectEntered.AddListener((onTouch) => RefToClass.OnTouchIndicator(indicatorID));
        }

    }
}
