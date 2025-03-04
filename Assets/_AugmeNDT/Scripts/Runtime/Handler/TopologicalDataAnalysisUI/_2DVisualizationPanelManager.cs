using UnityEngine;
using UnityEngine.UI;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the 2D visualization panel UI, handling user interactions 
    /// such as toggling vector fields, streamlines, and flow visualization.
    /// </summary>
    public class _2DVisualizationPanelManager : MonoBehaviour
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
        private static Detailed2DVectorFieldObjectVis detailed2DVectorFieldObjectInstance;
        private static InteractiveIntersectionPointVis interactiveIntersectionPointVisInstance;
        private static StreamLine2D streamLine2DInstance;
        private static FlowAnimation2D flowAnimation2DInstance;
        private void Start()
        {
            detailed2DVectorFieldObjectInstance = Detailed2DVectorFieldObjectVis.Instance;
            interactiveIntersectionPointVisInstance = InteractiveIntersectionPointVis.Instance;
            streamLine2DInstance = StreamLine2D.Instance;
            flowAnimation2DInstance = FlowAnimation2D.Instance;

            interactiveObjectDisplayed.onValueChanged.AddListener(InteractiveObjectDisplayed);
            
            showVectorForce.onClick.AddListener(ShowVectorForceOnField);
            hideVectorForce.onClick.AddListener(HideVectorForceOnField);
            
            showStreamLine.onClick.AddListener(ShowStreamLineOnField);
            hideStreamLine.onClick.AddListener(HideStreamLineOnField);

            showFlow.onClick.AddListener(ShowFlows);
            hideFlow.onClick.AddListener(HideFlows);

            back.onClick.AddListener(BackToMainMenu);

            // Display 2D intersection points by default
            interactiveIntersectionPointVisInstance.Show2DSpheres();
        }
        /// <summary>
        /// Toggles the visibility of interactive intersection objects in the visualization.
        /// </summary>
        /// <param name="isOn">True to display, False to hide.</param>
        private void InteractiveObjectDisplayed(bool isOn)
        {
            if (isOn)
                interactiveIntersectionPointVisInstance.Show2DSpheres();
            else
                interactiveIntersectionPointVisInstance.Hide2DSpheres();
        }

        /// <summary>
        /// Displays vector forces on the field by visualizing gradient points.
        /// </summary>
        private void ShowVectorForceOnField()
        {
            detailed2DVectorFieldObjectInstance.VisualizePoints();
        }

        /// <summary>
        /// Hides vector force arrows from the field.
        /// </summary>
        private void HideVectorForceOnField()
        {
            detailed2DVectorFieldObjectInstance.ShowHideArrows(showArrows: false);
        }

        /// <summary>
        /// Draws streamlines in the 2D field.
        /// </summary>
        private void ShowStreamLineOnField()
        {
            streamLine2DInstance.ShowStreamLines();
        }

        /// <summary>
        /// Hides the drawn streamlines.
        /// </summary>
        private void HideStreamLineOnField()
        {
            streamLine2DInstance.HideStreamLines();
        }

        /// <summary>
        /// Starts flow visualization in the 2D field.
        /// </summary>
        private void ShowFlows()
        {
            flowAnimation2DInstance.StartFlowObject();
        }

        /// <summary>
        /// Pauses the flow visualization.
        /// </summary>
        private void HideFlows()
        {
            flowAnimation2DInstance.PauseFlowObject();
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
