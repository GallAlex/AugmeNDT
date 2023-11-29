using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using UnityEngine;

namespace AugmeNDT{
    
    public class SelectionBoxInteractable : MonoBehaviour
    {
        public VisibleOnCloseInteraction visibleOnCloseInteractionScript;
        public Interactable interactable;

        public VisMDDGlyphs refToMDDGlyph;
        public Bounds chartArea;

        public int selectionBoxID;

        private Vector3 initalSelectionBoxPos;
        private Quaternion initalSelectionBoxRot;

        private Vis refToVis; 

        void Start()
        {
            var onGrabReceiver = interactable.AddReceiver<InteractableOnGrabReceiver>();
            initalSelectionBoxPos = this.transform.localPosition;
            initalSelectionBoxRot = this.transform.localRotation;

            onGrabReceiver.OnGrab.AddListener(() => OnGrab());
            onGrabReceiver.OnRelease.AddListener(() => OnGrabRelease());
        }

        void Update()
        {
            if (refToVis != null)
            {
                if (!CheckDraggedDistanceReached(refToVis.visContainerObject.transform.localPosition))
                {
                    RemoveDraggedOutVis();
                }
            }
        }

        public void OnGrab()
        {
            //Debug.Log("Grabbed Box: " + this.selectionBoxID);
            visibleOnCloseInteractionScript.EnableInteraction(false);
            visibleOnCloseInteractionScript.ShowObject(true);
        }

        public void OnGrabRelease()
        {
            //Debug.Log("Released Grab: " + this.selectionBoxID);
            var check = CheckDraggedDistanceReached(this.transform.localPosition);
            //Debug.Log("CheckDraggedDistanceReached: " + check);

            if (!check)
            {
                //Reset Selection Box
                this.transform.localPosition = initalSelectionBoxPos;
                this.transform.localRotation = initalSelectionBoxRot;
                visibleOnCloseInteractionScript.EnableInteraction(true);
                visibleOnCloseInteractionScript.ShowObject(false);
            }
            else
            {
                // Move Pos to the left front corner of the selection box
                Vector3 newPos = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y - 0.5f, this.transform.localPosition.z - 0.5f);

                // Hide selection box
                visibleOnCloseInteractionScript.EnableInteraction(false);
                visibleOnCloseInteractionScript.ShowObject(false);

                refToVis = refToMDDGlyph.CreateNewVis(selectionBoxID, newPos);
            }

            
        }

        /// <summary>
        /// Methods checks if the currentPos is outside the chart Area.
        /// </summary>
        /// <returns></returns>
        private bool CheckDraggedDistanceReached(Vector3 currentPos)
        {
            bool reached = !chartArea.Contains(currentPos);
            //Debug.Log("Reached?: " + reached + "\nchartArea: " + chartArea + "\n currentPos: " + currentPos);

            return reached;
        }


        // If the selection box is not dragged out of the area it gets resetted to its initial position
        private void RemoveDraggedOutVis()
        {
            if (refToVis != null)
            {
                refToVis.DeleteVis();
                refToVis = null;
            }

            // Show selection box
            visibleOnCloseInteractionScript.EnableInteraction(true);
            visibleOnCloseInteractionScript.ShowObject(true);

            this.transform.localPosition = initalSelectionBoxPos;
            this.transform.localRotation = initalSelectionBoxRot;
        }


    }
}
