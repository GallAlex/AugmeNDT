using AugmeNDT;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace AugmeNDT
{
    /// <summary>
    /// It is managing the Topological Data Analysis main menu.
    /// This class provides an interface to navigate between different visualization panels.
    /// </summary>
    public class MainManager : MonoBehaviour
    {
        public static MainManager Instance;

        // UI Panels representing different visualization categories
        public GameObject infoPanel;          // Main menu panel
        public GameObject vectorFieldPanel;   // Panel for Vector Field visualization
        public GameObject criticalPointPanel; // Panel for Critical Point visualization
        public GameObject _2DPanel;           // Panel for 2D analysis
        public GameObject _3DPanel;           // Panel for 3D analysis

        /// <summary>
        /// Ensures that there is only one instance of TDAMainMenu in the scene
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        /// <summary>
        /// Displays the main menu panel.
        /// </summary>
        public void ShowMainMenu()
        {
            infoPanel.SetActive(true);
        }

        public void HideMainMenu()
        {
            infoPanel.SetActive(false);
        }

        #region private
        /// <summary>
        /// Activates the Critical Points visualization panel and hides the main menu.
        /// </summary>
        public void CriticalPointVisualize()
        {
            infoPanel.SetActive(false);
            criticalPointPanel.SetActive(true);
        }

        /// <summary>
        /// Activates the Vector Field visualization panel and hides the main menu.
        /// </summary>
        public void VectorFieldVisualize()
        {
            infoPanel.SetActive(false);
            vectorFieldPanel.SetActive(true);
        }

        /// <summary>
        /// Activates the 2D Analysis visualization panel and hides the main menu.
        /// </summary>
        public void _2DPanelVisualization()
        {
            infoPanel.SetActive(false);
            _2DPanel.SetActive(true);
        }

        /// <summary>
        /// Activates the 3D Analysis visualization panel and hides the main menu.
        /// </summary>
        public void _3DPanelVisualization()
        {
            infoPanel.SetActive(false);
            _3DPanel.SetActive(true);
        }
        #endregion private
    }
}