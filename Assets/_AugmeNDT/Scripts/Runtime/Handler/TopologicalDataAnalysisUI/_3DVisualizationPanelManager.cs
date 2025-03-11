using UnityEngine;
using UnityEngine.UI;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the 3D visualization panel UI, handling user interactions 
    /// such as toggling vector fields, streamlines, flow objects, and critical points.
    /// </summary>
    public class _3DVisualizationPanelManager : MonoBehaviour
    {
        // Instances of the 3D visualization managers
        private static Glyph3DVectorField detailed3DVectorFieldObjectInstance;
        private static FlowObject3DManager flowObject3DManagerInstance;
        private static StreamLine3D streamLine3DInstance;
        private static Rectangle3DManager rectangle3DManager;

        // Toggle states for each visualization feature
        private bool showHideVectorForce = false;
        private bool showHideCriticalPoints = false;
        private bool showHideStreamLine = false;
        private bool showHideFlows = false;
        private bool interactiveObjectDisplayed = false;

        private void Start()
        {
            // Get singleton instances
            detailed3DVectorFieldObjectInstance = Glyph3DVectorField.instance;
            flowObject3DManagerInstance = FlowObject3DManager.Instance;
            streamLine3DInstance = StreamLine3D.Instance;
            rectangle3DManager = Rectangle3DManager.rectangle3DManager;

            // Show the interactive rectangle on startup
            InteractiveObjectDisplayed();
        }

        /// <summary>
        /// Toggles the visibility of the interactive rectangle in 3D space.
        /// </summary>
        public void InteractiveObjectDisplayed()
        {
            if (!interactiveObjectDisplayed)
            {
                rectangle3DManager.ShowRectangle();
                interactiveObjectDisplayed = true;
            }
            else
            {
                rectangle3DManager.HideRectangle();
                interactiveObjectDisplayed = false;
            }
        }

        /// <summary>
        /// Toggles the visibility of 3D vector field arrows (glyphs).
        /// </summary>
        public void ShowHideVectorForceOnField()
        {
            if (!showHideVectorForce)
            {
                detailed3DVectorFieldObjectInstance.ShowVectorField();
                showHideVectorForce = true;
            }
            else
            {
                detailed3DVectorFieldObjectInstance.HideVectorField();
                showHideVectorForce = false;
            }
        }

        /// <summary>
        /// Toggles visibility of critical points (e.g. sinks, sources, saddles).
        /// </summary>
        public void ShowHideCriticalPoints()
        {
            if (!showHideCriticalPoints)
            {
                detailed3DVectorFieldObjectInstance.ShowCriticalPoints();
                showHideCriticalPoints = true;
            }
            else
            {
                detailed3DVectorFieldObjectInstance.HideCriticalPoints();
                showHideCriticalPoints = false;
            }
        }

        /// <summary>
        /// Toggles the display of streamlines in 3D space.
        /// </summary>
        public void ShowHideStreamLineOnField()
        {
            if (!showHideStreamLine)
            {
                streamLine3DInstance.ShowStreamLines();
                showHideStreamLine = true;
            }
            else
            {
                streamLine3DInstance.HideStreamLines();
                showHideStreamLine = false;
            }
        }

        /// <summary>
        /// Starts or pauses flow animation in the 3D field.
        /// </summary>
        public void ShowHideFlows()
        {
            if (!showHideFlows)
            {
                flowObject3DManagerInstance.StartFlowObject();
                showHideFlows = true;
            }
            else
            {
                flowObject3DManagerInstance.PauseFlowObject();
                showHideFlows = false;
            }
        }

        /// <summary>
        /// Closes the current panel and navigates back to the main menu UI.
        /// </summary>
        public void BackToMainMenu()
        {
            gameObject.SetActive(false); // Hide this panel
            MainManager.Instance.ShowMainMenu(); // Show main menu
        }
    }
}
