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
        public GameObject selectionBox;
        public Bounds chartArea;

        public int selectionBoxID;

        private Vector3 initalSelectionBoxPos;
        private Vis refToVis; 

        void Start()
        {
            var onGrabReceiver = interactable.AddReceiver<InteractableOnGrabReceiver>();
            onGrabReceiver.OnGrab.AddListener(() => OnGrab());
            onGrabReceiver.OnRelease.AddListener(() => OnGrabRelease());

            initalSelectionBoxPos = selectionBox.transform.localPosition;
        }

        void Update()
        {
            if (refToVis != null)
            {
                if (!CheckDraggedDistanceReached(refToVis.visContainerObject.transform.localPosition))
                {
                    ShowSelectionBox();
                }
            }
        }

        public void OnGrab()
        {
            Debug.Log("Grab!");
            visibleOnCloseInteractionScript.enabled = false;
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
            }
            else
            {
                Vector3 newPos = new Vector3(selectionBox.transform.localPosition.x, selectionBox.transform.localPosition.y - 0.5f, selectionBox.transform.localPosition.z - 0.5f);
                refToVis = refToMDDGlyph.CreateNewVis(selectionBoxID, newPos);
            }

            visibleOnCloseInteractionScript.enabled = true;
        }

        /// <summary>
        /// Methods checks if the new Pos of the selection box is outside the chart Area.
        /// If the selection box is not outside the chart area the selection box is reset to its initial position.
        /// </summary>
        /// <returns></returns>
        private bool CheckDraggedDistanceReached(Vector3 currentPos)
        {
            bool reached = !chartArea.Contains(currentPos);
            //Debug.Log("Reached?: " + reached + "\nchartArea: " + chartArea + "\n currentPos: " + currentPos);


            return reached;
        }

        // If the selection box is dragged out of the chart area, the selection box is hidden and a new Vis is created
        private void HideSelectionBox()
        {
            // Move Pos to the left front corner of the selection box
            Vector3 newPos = new Vector3(selectionBox.transform.localPosition.x, selectionBox.transform.localPosition.y - 0.5f, selectionBox.transform.localPosition.z - 0.5f);
            // Hide selection box
            //selectionBox.SetActive(false);

            refToVis = refToMDDGlyph.CreateNewVis(selectionBoxID, newPos);
        }

        // If the selection box is not dragged out of the area it gets resetted to its initial position
        private void ShowSelectionBox()
        {
            if (refToVis != null)
            {
                refToVis.DeleteVis();
                refToVis = null;
                //selectionBox.SetActive(true);
            }
            visibleOnCloseInteractionScript.enabled = true;
            selectionBox.transform.localPosition = initalSelectionBoxPos;
        }


    }
}
