using UnityEngine;
using UnityEngine.UI;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the 3D visualization panel UI, handling user interactions 
    /// such as toggling vector fields, streamlines, and flow visualization in 3D space.
    /// </summary>
    public class _3DVisualizationPanelManager :MonoBehaviour
    {
        public Toggle interactiveObjectDisplayed;

        public Button showVectorForce;
        public Button hideVectorForce;

        public Button showStreamLine;
        public Button hideStreamLine;

        public Button showFlow;
        public Button hideFlow;

        public Button back;

        // Static instances of visualization components
        private static Detailed3DVectorFieldObjectVis detailed3DVectorFieldObjectInstance;
        private static InteractiveIntersectionPointVis interactiveIntersectionPointVisInstance;
        private static StreamLine3D streamLine3DInstance;

        private void Start()
        {
            detailed3DVectorFieldObjectInstance = Detailed3DVectorFieldObjectVis.Instance;
            interactiveIntersectionPointVisInstance = InteractiveIntersectionPointVis.Instance;
            streamLine3DInstance = StreamLine3D.Instance;

            interactiveObjectDisplayed.onValueChanged.AddListener(InteractiveObjectDisplayed);

            showVectorForce.onClick.AddListener(ShowVectorForceOnField);
            hideVectorForce.onClick.AddListener(HideVectorForceOnField);

            showStreamLine.onClick.AddListener(ShowStreamLineOnField);
            hideStreamLine.onClick.AddListener(HideStreamLineOnField);

            showFlow.onClick.AddListener(ShowFlows);
            hideFlow.onClick.AddListener(HideFlows);

            back.onClick.AddListener(BackToMainMenu);

            // Display 3D intersection points by default
            interactiveIntersectionPointVisInstance.Show3DSpheres();
        }

        /// <summary>
        /// Toggles the visibility of interactive intersection objects in the 3D visualization.
        /// </summary>
        /// <param name="isOn">True to display, False to hide.</param>
        private void InteractiveObjectDisplayed(bool isOn)
        {
            if (isOn)
                interactiveIntersectionPointVisInstance.Show3DSpheres();
            else
                interactiveIntersectionPointVisInstance.Hide3DSpheres();
        }

        /// <summary>
        /// Displays vector forces on the 3D field by visualizing gradient points.
        /// </summary>
        private void ShowVectorForceOnField()
        {
            detailed3DVectorFieldObjectInstance.VisualizePoints();
        }

        /// <summary>
        /// Hides vector force arrows from the 3D field.
        /// </summary>
        private void HideVectorForceOnField()
        {
            detailed3DVectorFieldObjectInstance.ShowHideArrows(showArrows: false);
        }

        /// <summary>
        /// Draws streamlines in the 3D field.
        /// </summary>
        private void ShowStreamLineOnField()
        {
            streamLine3DInstance.DrawStreamlines();
        }

        /// <summary>
        /// Hides the drawn streamlines in the 3D field.
        /// </summary>
        private void HideStreamLineOnField()
        {
            streamLine3DInstance.HideStreamLines();
        }

        /// <summary>
        /// Starts flow visualization in the 3D field.
        /// </summary>
        private void ShowFlows()
        {
            streamLine3DInstance.StartFlowObject();
        }

        /// <summary>
        /// Pauses the flow visualization.
        /// </summary>
        private void HideFlows()
        {
            streamLine3DInstance.PauseFlowObject();
        }

        /// <summary>
        /// Closes the current panel and returns to the main menu.
        /// </summary>
        private void BackToMainMenu()
        {
            gameObject.SetActive(false); // Deactivate the current UI panel
            TDAMainMenu.Instance.ShowMainMenu(); // Activate the main menu
        }
    }
}