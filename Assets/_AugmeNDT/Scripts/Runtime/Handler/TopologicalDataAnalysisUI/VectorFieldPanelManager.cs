using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the Vector Field visualization panel.
    /// Handles toggling vector field visibility and returning to the main menu.
    /// </summary>
    public class VectorFieldPanelManager : MonoBehaviour
    {
        // UI elements
        public Button backButton;   // Button to go back to the main menu
        public Toggle showToggle;   // Toggle to show/hide the vector field (default: true)

        private VectorFieldObjectVis vectorFieldObjectVis; // Instance for managing vector field visualization

        /// <summary>
        /// Initializes UI button listeners and start visualizing the vector field
        /// </summary>
        private void Start()
        {
            backButton.onClick.AddListener(BackToMainMenu);
            showToggle.onValueChanged.AddListener(ShowVectorField);

            vectorFieldObjectVis = VectorFieldObjectVis.Instance;
            vectorFieldObjectVis.Visualize();
        }

        /// <summary>
        /// Closes the Vector Field panel and returns to the main menu.
        /// </summary>
        private void BackToMainMenu()
        {
            gameObject.SetActive(false); // Hide the panel
            TDAMainMenu.Instance.ShowMainMenu(); // Show main menu
        }

        /// <summary>
        /// Toggles the visibility of the vector field without recalculating it.
        /// </summary>
        /// <param name="isOn">True to show, False to hide</param>
        private void ShowVectorField(bool isOn)
        {
            if (isOn)
                vectorFieldObjectVis.ShowVectorField();
            else
                vectorFieldObjectVis.HideVectorField();
        }
    }
}
