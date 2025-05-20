// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.XR.Interaction.Toolkit;

namespace AugmeNDT{

    public class SelectionBoxInteractable : MonoBehaviour
    {
        public VisibleOnCloseInteraction VisibleOnCloseInteractionScript;
        public VisMDDGlyphs RefToMddGlyph;
        public Bounds ChartArea;
        public int SelectionBoxId;

        private ObjectManipulator objectManipulator;

        private Vector3 initalSelectionBoxPos;
        private Quaternion initalSelectionBoxRot;

        private Vis refToVis;

        void Start()
        {
            // Create a new interactable
            objectManipulator = GetComponent<ObjectManipulator>();

            if(objectManipulator != null)
            {
                objectManipulator.selectEntered.AddListener(OnGrab);
                objectManipulator.selectExited.AddListener(OnGrabRelease);
            }

            initalSelectionBoxPos = this.transform.localPosition;
            initalSelectionBoxRot = this.transform.localRotation;

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

        private void OnGrab(SelectEnterEventArgs args)
        {
            VisibleOnCloseInteractionScript.EnableInteraction(false);
            VisibleOnCloseInteractionScript.ShowObject(true);
        }

        private void OnGrabRelease(SelectExitEventArgs args)
        {
            var check = CheckDraggedDistanceReached(this.transform.localPosition);
            //Debug.Log("CheckDraggedDistanceReached: " + check);

            if (!check)
            {
                //Reset Selection Box
                this.transform.localPosition = initalSelectionBoxPos;
                this.transform.localRotation = initalSelectionBoxRot;
                VisibleOnCloseInteractionScript.EnableInteraction(true);
                VisibleOnCloseInteractionScript.ShowObject(false);
            }
            else
            {
                // Move Pos to the left front corner of the selection box
                Vector3 newPos = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y - 0.5f,
                    this.transform.localPosition.z - 0.5f);

                // Hide selection box
                VisibleOnCloseInteractionScript.EnableInteraction(false);
                VisibleOnCloseInteractionScript.ShowObject(false);

                refToVis = RefToMddGlyph.CreateNewVis(SelectionBoxId, newPos);
            }


        }

        /// <summary>
        /// Methods checks if the currentPos is outside the chart Area.
        /// </summary>
        /// <returns></returns>
        private bool CheckDraggedDistanceReached(Vector3 currentPos)
        {
            bool reached = !ChartArea.Contains(currentPos);
            //Debug.Log("Reached?: " + reached + "\nchartArea: " + chartArea + "\n currentPos: " + currentPos);

            return reached;
        }


        // If the selection box is not dragged out of the area it gets reset to its initial position
        private void RemoveDraggedOutVis()
        {
            if (refToVis != null)
            {
                refToVis.DeleteVis();
                refToVis = null;
            }

            // Show selection box
            VisibleOnCloseInteractionScript.EnableInteraction(true);
            VisibleOnCloseInteractionScript.ShowObject(true);

            this.transform.localPosition = initalSelectionBoxPos;
            this.transform.localRotation = initalSelectionBoxRot;
        }

        private void OnDestroy()
        {
            // Clean up listeners to avoid memory leaks
            if (objectManipulator != null)
            {
                objectManipulator.selectEntered.RemoveListener(OnGrab);
                objectManipulator.selectExited.RemoveListener(OnGrabRelease);
            }

        }
    }
}
