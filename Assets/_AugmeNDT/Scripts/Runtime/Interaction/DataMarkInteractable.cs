using MixedReality.Toolkit.Input;
using MixedReality.Toolkit;
using UnityEngine;

namespace AugmeNDT{
    public class DataMarkInteractable : MonoBehaviour
    {
        public DataMark dataMark;
        public VisInteractor visInteractor;
        public MRTKBaseInteractable interactable;

        public string dataMarkID = "DataMark_";

        public void Init(DataMark mark, VisInteractor interactor)
        {
            dataMark = mark;
            visInteractor = interactor;
            dataMarkID += dataMark.GetDataMarkId();
        }

        void Start()
        {
          //TODO
         //   var onTouchReceiver = interactable.AddReceiver<InteractableOnTouchReceiver>();
          //  onTouchReceiver.OnTouchStart.AddListener(() => visInteractor.OnTouch(dataMarkID)); ;
        }

        public void DisableInteraction()
        {
            //Disable Collider, interactable,...
            GetComponent<Collider>().enabled = false;
            //TODO//GetComponent<NearInteractionTouchable>().enabled = false;
            interactable.enabled = false;

            //Disable Script
            this.enabled = false;
        }
    }
}
