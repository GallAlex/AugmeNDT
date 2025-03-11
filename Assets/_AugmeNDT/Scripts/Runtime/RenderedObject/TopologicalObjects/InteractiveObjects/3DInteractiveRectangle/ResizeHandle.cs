namespace AugmeNDT
{
    using UnityEngine;

    /// <summary>
    /// Handles the resizing behavior for individual cube faces
    /// </summary>
    public class ResizeHandle : MonoBehaviour
    {
        // Reference to the parent wireframe cube
        private WireframeCube parentCube;

        // Index of the face this handle controls (0-5 for six faces of a cube)
        private int faceIndex;

        /// <summary>
        /// Initializes the resize handle with references to parent cube and face index
        /// </summary>
        /// <param name="cube">Parent wireframe cube this handle belongs to</param>
        /// <param name="index">Index of the face this handle will resize</param>
        public void Init(WireframeCube cube, int index)
        {
            parentCube = cube;
            faceIndex = index;
        }

        /// <summary>
        /// Called when mouse is dragged on this handle
        /// Resizes the corresponding face of the wireframe cube
        /// </summary>
        private void OnMouseDrag()
        {
            // Multiplier for resize sensitivity
            float resizeSpeed = 0.1f;

            // Calculate resize amount based on horizontal mouse movement
            float amount = Input.GetAxis("Mouse X") * resizeSpeed;

            // Apply resize to the specific face through the parent cube
            parentCube.ResizeFace(faceIndex, amount);
        }
    }
}