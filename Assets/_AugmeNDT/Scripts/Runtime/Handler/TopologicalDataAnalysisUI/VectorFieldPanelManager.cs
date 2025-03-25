using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the Vector Field visualization panel.
    /// Handles toggling vector field visibility and returning to the main menu.
    /// </summary>
    public class VectorFieldPanelManager : MonoBehaviour
    {
        private VectorFieldObjectVis vectorFieldObjectVis; // Reference to the vector field visualization manager
        public bool showHideVectorField = false; // Current visibility state

        /// <summary>
        /// Initializes the vector field visualization and toggles its default state on start.
        /// </summary>
        private void Start()
        {
            vectorFieldObjectVis = VectorFieldObjectVis.instance;
            ShowHideVectorField(); // Start by showing it once (default behavior)
        }

        /// <summary>
        /// Toggles the visibility of the vector field.
        /// </summary>
        public void ShowHideVectorField()
        {
            if (!showHideVectorField)
            {
                vectorFieldObjectVis.Visualize(); // Enable the vector field
                showHideVectorField = true;
            }
            else
            {
                vectorFieldObjectVis.HideVectorField(); // Disable the vector field
                showHideVectorField = false;
            }
        }

        /// <summary>
        /// Closes the vector field panel and returns to the main menu UI.
        /// </summary>
        public void BackToMainMenu()
        {
            gameObject.SetActive(false); // Hide this panel
            MainManager.Instance.ShowMainMenu(); // Show main menu panel
        }
    }
}
