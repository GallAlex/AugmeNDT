using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace AugmeNDT
{
    public class OnClickBookshelf : MRTKBaseInteractable

    {
        private Transform xrCameraTransform;

     
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.green;
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.red;
        }

        // Implementing other methods if needed
    }
}
