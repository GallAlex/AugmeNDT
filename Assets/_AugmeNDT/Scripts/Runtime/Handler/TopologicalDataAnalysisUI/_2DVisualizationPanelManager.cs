using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the 2D visualization panel UI, handling user interactions 
    /// such as toggling vector fields, streamlines, and flow visualization.
    /// </summary>
    public class _2DVisualizationPanelManager : MonoBehaviour
    {
        // References to singleton instances of visualization objects
        private static Glyph2DVectorField detailed2DVectorFieldObjectInstance;
        private static StreamLine2D streamLine2DInstance;
        private static FlowObject2DManager flowAnimation2DInstance;
        private static RectangleManager rectangleManager;

        // Toggle states for each visualization type
        private bool showArrows = false;
        private bool showStreamline = false;
        private bool showFlows = false;
        private bool showRectangle = false;

        private void Start()
        {
            // Initialize references to all manager instances
            detailed2DVectorFieldObjectInstance = Glyph2DVectorField.Instance;
            streamLine2DInstance = StreamLine2D.Instance;
            flowAnimation2DInstance = FlowObject2DManager.Instance;
            rectangleManager = RectangleManager.rectangleManager;

            // Automatically toggle the rectangle once on startup
            InteractiveRectangleDisplayed();
        }

        /// <summary>
        /// Toggles the visibility of the interactive rectangle in the 2D view.
        /// </summary>
        public void InteractiveRectangleDisplayed()
        {
            if (!showRectangle)
            {
                rectangleManager.ShowRectangle();
                showRectangle = true;
            }
            else
            {
                rectangleManager.HideRectangle();
                showRectangle = false;
            }
        }

        /// <summary>
        /// Toggles the display of vector force arrows using gradient data.
        /// </summary>
        public void ShowHideVectorForceOnField()
        {
            if (!showArrows)
            {
                detailed2DVectorFieldObjectInstance.VisualizePoints(); // First time draw
                showArrows = true;
            }
            else
            {
                showArrows = false;
                detailed2DVectorFieldObjectInstance.ShowHideArrows(showArrows); // Toggle visibility
            }
        }

        /// <summary>
        /// Toggles the rendering of streamlines in the 2D field.
        /// </summary>
        public void ShowHideStreamLineOnField()
        {
            if (!showStreamline)
            {
                streamLine2DInstance.ShowStreamLines(); // Draw streamlines
                showStreamline = true;
            }
            else
            {
                streamLine2DInstance.HideStreamLines(); // Remove streamlines
                showStreamline = false;
            }
        }

        /// <summary>
        /// Toggles animated flow object visualization in the 2D field.
        /// </summary>
        public void ShowHideFlows()
        {
            if (!showFlows)
            {
                flowAnimation2DInstance.StartFlowObject(); // Start moving particles/objects
                showFlows = true;
            }
            else
            {
                flowAnimation2DInstance.PauseFlowObject(); // Pause the animation
                showFlows = false;
            }
        }

        /// <summary>
        /// Returns to the main menu by disabling this panel and enabling the main menu panel.
        /// </summary>
        public void BackToMainMenu()
        {
            gameObject.SetActive(false); // Hide the current UI panel
            MainManager.Instance.ShowMainMenu(); // Show the main menu UI
        }
    }
}
