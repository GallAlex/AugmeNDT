using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace AugmeNDT{
    public class DataMarkInteractable : MonoBehaviour
    {
        public DataMark dataMark;
        public VisInteractor visInteractor;
        public Interactable interactable;

        public string dataMarkID = "DataMark_";

        public void Init(DataMark mark, VisInteractor interactor)
        {
            dataMark = mark;
            visInteractor = interactor;
            dataMarkID += dataMark.GetDataMarkId();
        }

        void Start()
        {
            var onTouchReceiver = interactable.AddReceiver<InteractableOnTouchReceiver>();
            onTouchReceiver.OnTouchStart.AddListener(() => visInteractor.OnTouch(dataMarkID)); ;
        }

        public void DisableInteraction()
        {
            //Disable Collider, interactable,...
            GetComponent<Collider>().enabled = false;
            GetComponent<NearInteractionTouchable>().enabled = false;
            interactable.enabled = false;

            //Disable Script
            this.enabled = false;
        }
    }
}
