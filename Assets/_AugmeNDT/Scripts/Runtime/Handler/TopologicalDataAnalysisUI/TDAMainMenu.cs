using AugmeNDT;
using UnityEngine;
using UnityEngine.UI;

namespace AugmeNDT
{
    /// <summary>
    /// It is managing the Topological Data Analysis main menu.
    /// This class provides an interface to navigate between different visualization panels.
    /// </summary>
    public class TDAMainMenu : MonoBehaviour
    {
        public static TDAMainMenu Instance;

        // UI buttons for navigating different visualization options
        public Button criticalPointsButton;
        public Button vectorFieldButton;
        public Button _2DAnalysisButton;
        public Button _3DAnalysisButton;

        // UI Panels representing different visualization categories
        public GameObject InfoPanel;          // Main menu panel
        public GameObject VectorFieldPanel;   // Panel for Vector Field visualization
        public GameObject CriticalPointPanel; // Panel for Critical Point visualization
        public GameObject _2DPanel;           // Panel for 2D analysis
        public GameObject _3DPanel;           // Panel for 3D analysis

        // Reference to the main data object managing topological data
        private static TopologicalDataObject Instances;

        /// <summary>
        /// Ensures that there is only one instance of TDAMainMenu in the scene
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        /// <summary>
        /// Initializes button listeners and retrieves the TopologicalDataObject instance.
        /// </summary>
        private void Start()
        {
            Instances = TopologicalDataObject.Instance;
            criticalPointsButton.onClick.AddListener(CriticalPointVisualize);
            vectorFieldButton.onClick.AddListener(VectorFieldVisualize);
            _2DAnalysisButton.onClick.AddListener(_2DPanelVisualization);
            _3DAnalysisButton.onClick.AddListener(_3DPanelVisualization);
        }

        /// <summary>
        /// Displays the main menu panel.
        /// </summary>
        public void ShowMainMenu()
        {
            InfoPanel.SetActive(true);
        }

        #region private
        /// <summary>
        /// Activates the Critical Points visualization panel and hides the main menu.
        /// </summary>
        private void CriticalPointVisualize()
        {
            InfoPanel.SetActive(false);
            CriticalPointPanel.SetActive(true);
        }

        /// <summary>
        /// Activates the Vector Field visualization panel and hides the main menu.
        /// </summary>
        private void VectorFieldVisualize()
        {
            InfoPanel.SetActive(false);
            VectorFieldPanel.SetActive(true);
        }

        /// <summary>
        /// Activates the 2D Analysis visualization panel and hides the main menu.
        /// </summary>
        private void _2DPanelVisualization()
        {
            InfoPanel.SetActive(false);
            _2DPanel.SetActive(true);
        }

        /// <summary>
        /// Activates the 3D Analysis visualization panel and hides the main menu.
        /// </summary>
        private void _3DPanelVisualization()
        {
            InfoPanel.SetActive(false);
            _3DPanel.SetActive(true);
        }
        #endregion private
    }
}