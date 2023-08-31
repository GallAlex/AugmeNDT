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
        private GameObject selectionBox;

        private Vis refToVis; 

        void Start()
        {
            var onGrabReceiver = interactable.AddReceiver<InteractableOnGrabReceiver>();
            onGrabReceiver.OnGrab.AddListener(() => OnGrab());
            onGrabReceiver.OnRelease.AddListener(() => OnGrabRelease());
            selectionBox = this.gameObject;
            initalSelectionBoxPos = selectionBox.transform.localPosition;
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
            Debug.Log("Grab!");
            visibleOnCloseInteractionScript.interactionEnabled = false;
            visibleOnCloseInteractionScript.ShowObject(true);
        }

        public void OnGrabRelease()
        {
            Debug.Log("Released Grab!");
            var check = CheckDraggedDistanceReached(selectionBox.transform.localPosition);
            Debug.Log("CheckDraggedDistanceReached: " + check);

            if (!check)
            {
                //Reset Selection Box
                selectionBox.transform.localPosition = initalSelectionBoxPos;
                visibleOnCloseInteractionScript.interactionEnabled = true;
                visibleOnCloseInteractionScript.ShowObject(false);
            }
            else
            {
                // Move Pos to the left front corner of the selection box
                Vector3 newPos = new Vector3(selectionBox.transform.localPosition.x, selectionBox.transform.localPosition.y - 0.5f, selectionBox.transform.localPosition.z - 0.5f);
                
                // Hide selection box
                visibleOnCloseInteractionScript.interactionEnabled = false;
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
            visibleOnCloseInteractionScript.interactionEnabled = true;
            visibleOnCloseInteractionScript.ShowObject(true);

            selectionBox.transform.localPosition = initalSelectionBoxPos;
        }


    }
}
