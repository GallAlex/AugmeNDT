using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AugmeNDT
{
    /// <summary>
    /// Manages the Critical Point visualization panel.
    /// Allows users to filter different types of critical points and view detailed information.
    /// </summary>
    public class CriticalPointPanelManager : MonoBehaviour
    {
        public Button backButton; // Button to return to the main menu
        public TMP_Text infoText; // UI Text to display selected critical point details

        public static CriticalPointPanelManager Instance;
        private static CriticalPointObjectVis criticalPointObjectVisInstance;
        
        // UI toggles to filter different types of critical points
        public Toggle minimumToggle;
        public Toggle saddle1Toggle;
        public Toggle saddle2Toggle;
        public Toggle maximumToggle;

        /// <summary>
        /// Implements Singleton pattern to ensure only one instance exists.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        /// <summary>
        /// Initializes UI interactions and start the critical point visualization.
        /// </summary>
        private void Start()
        {
            backButton.onClick.AddListener(BackToMainMenu);
            minimumToggle.onValueChanged.AddListener(delegate { FilterCriticalPointsByType(0, minimumToggle.isOn); });
            saddle1Toggle.onValueChanged.AddListener(delegate { FilterCriticalPointsByType(1, saddle1Toggle.isOn); });
            saddle2Toggle.onValueChanged.AddListener(delegate { FilterCriticalPointsByType(2, saddle2Toggle.isOn); });
            maximumToggle.onValueChanged.AddListener(delegate { FilterCriticalPointsByType(3, maximumToggle.isOn); });

            criticalPointObjectVisInstance = CriticalPointObjectVis.Instance;
            criticalPointObjectVisInstance.Visualize();

        }

        /// <summary>
        /// Closes the Critical Point panel and returns to the main menu.
        /// </summary>
        private void BackToMainMenu()
        {
            gameObject.SetActive(false); // Hide panel
            TDAMainMenu.Instance.ShowMainMenu(); // Show main menu
        }

        /// <summary>
        /// Displays detailed information about the selected critical point.
        /// </summary>
        /// <param name="id">Unique ID of the critical point</param>
        /// <param name="type">Type of the critical point (minimum, saddle, etc.)</param>
        /// <param name="position">Position in the 3D space</param>
        public void ShowPointInfo(int id, int type, Vector3 position)
        {
            string typeName = GetCriticalTypeName(type);
            infoText.text = $"ID: {id}\nType: {typeName}\nPosition: {position}";
        }

        /// <summary>
        /// Returns the name of the critical point type based on its ID.
        /// </summary>
        /// <param name="type">Integer ID of the critical point type</param>
        /// <returns>String representation of the critical point type</returns>
        private string GetCriticalTypeName(int type)
        {
            switch (type)
            {
                case 0: return "Minimum";
                case 1: return "1-Saddle";
                case 2: return "2-Saddle";
                case 3: return "Maximum";
                case 4: return "Degenerate";
                case 5: return "Regular";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Filters critical points based on type and user selection.
        /// </summary>
        /// <param name="type">Type ID of the critical point</param>
        /// <param name="state">True to show, False to hide</param>
        public void FilterCriticalPointsByType(int type, bool state)
        {
            if (criticalPointObjectVisInstance.criticalPointDictionary.ContainsKey(type))
            {
                foreach (GameObject point in criticalPointObjectVisInstance.criticalPointDictionary[type])
                {
                    point.SetActive(state);
                }
            }
        }
    }
}
