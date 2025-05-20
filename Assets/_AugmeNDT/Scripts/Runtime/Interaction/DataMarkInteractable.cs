// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using UnityEngine;

namespace AugmeNDT{
    public class DataMarkInteractable : MonoBehaviour
    {
        public DataMark dataMark;
        public VisInteractor visInteractor;
        //TODO: MRTK3 Rework
        //public Interactable interactable;

        public string dataMarkID = "DataMark_";

        public void Init(DataMark mark, VisInteractor interactor)
        {
            dataMark = mark;
            visInteractor = interactor;
            dataMarkID += dataMark.GetDataMarkId();
        }

        void Start()
        {
            //TODO: MRTK3 Rework
            /*
            var onTouchReceiver = interactable.AddReceiver<InteractableOnTouchReceiver>();
            onTouchReceiver.OnTouchStart.AddListener(() => visInteractor.OnTouch(dataMarkID));
            */
        }

        public void DisableInteraction()
        {
            //Disable Collider, interactable,...
            GetComponent<Collider>().enabled = false;
            //TODO: MRTK3 Rework
            //GetComponent<NearInteractionTouchable>().enabled = false;
            //interactable.enabled = false;

            //Disable Script
            this.enabled = false;
        }
    }
}
