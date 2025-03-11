namespace AugmeNDT
{
    using UnityEngine;

    /// <summary>
    /// Allows wireframe objects to be dragged using mouse input
    /// </summary>
    public class DraggableResizableWireframe : MonoBehaviour
    {
        private Camera mainCamera;
        private bool isDragging = false;
        private Vector3 dragStartPos;

        /// <summary>
        /// Initializes the component by finding the main camera
        /// </summary>
        void Start()
        {
            mainCamera = Camera.main;
        }

        /// <summary>
        /// Called when mouse button is pressed on the object
        /// Initiates dragging operation and stores initial mouse position
        /// </summary>
        void OnMouseDown()
        {
            isDragging = true;
            dragStartPos = GetMouseWorldPosition();
        }

        /// <summary>
        /// Called when mouse is moved while button is held down
        /// Updates the object position based on mouse movement
        /// </summary>
        void OnMouseDrag()
        {
            if (isDragging)
            {
                Vector3 dragOffset = GetMouseWorldPosition() - dragStartPos;
                transform.position += dragOffset;
                dragStartPos = GetMouseWorldPosition();
            }
        }

        /// <summary>
        /// Called when mouse button is released
        /// Ends the dragging operation
        /// </summary>
        void OnMouseUp()
        {
            isDragging = false;
        }

        /// <summary>
        /// Converts screen mouse position to world coordinates
        /// </summary>
        /// <returns>World space position of the mouse cursor</returns>
        Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = mainCamera.WorldToScreenPoint(transform.position).z;
            return mainCamera.ScreenToWorldPoint(mousePoint);
        }
    }
}