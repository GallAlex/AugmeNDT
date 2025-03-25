using System.Collections.Generic;
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
        public static CriticalPointPanelManager Instance;
        private static CriticalPointObjectVis criticalPointObjectVisInstance;
        private bool showHideAllPoints = false;

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
            criticalPointObjectVisInstance = CriticalPointObjectVis.instance;
            criticalPointObjectVisInstance.Visualize();
            CreateLegendColorBar();
        }

        /// <summary>
        /// Closes the Critical Point panel and returns to the main menu.
        /// </summary>
        public void BackToMainMenu()
        {
            gameObject.SetActive(false); // Hide panel
            MainManager.Instance.ShowMainMenu(); // Show main menu
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
            //infoText.text = $"ID: {id}\nType: {typeName}\nPosition: {position}";
        }

        /// <summary>
        /// Returns the name of the critical point type based on its ID.
        /// </summary>
        /// <param name="type">Integer ID of the critical point type</param>
        /// <returns>String representation of the critical point type</returns>
        public string GetCriticalTypeName(int type)
        {
            switch (type)
            {
                case 0: return "Sink";
                case 1: return "1-Saddle";
                case 2: return "2-Saddle";
                case 3: return "Source";
                default: return "Unknown";
            }
        }

        public void ShowHideAllPoints()
        {
            if (!showHideAllPoints)
            {
                showHideAllPoints = true;
                ShowSink(showHideAllPoints);
                ShowSaddle1(showHideAllPoints); 
                ShowSaddle2(showHideAllPoints); 
                ShowSource(showHideAllPoints);
            }
            else
            {
                showHideAllPoints = false;
                ShowSink(showHideAllPoints);
                ShowSaddle1(showHideAllPoints);
                ShowSaddle2(showHideAllPoints);
                ShowSource(showHideAllPoints);
            }
        }

        public void ShowSink(bool isOn)
        {
            FilterCriticalPointsByType(0,isOn);
        }
        public void ShowSaddle1(bool isOn)
        {
            FilterCriticalPointsByType(1, isOn);
        }
        public void ShowSaddle2(bool isOn)
        {
            FilterCriticalPointsByType(2, isOn);
        }
        public void ShowSource(bool isOn)
        {
            FilterCriticalPointsByType(3, isOn);
        }

        /// <summary>
        /// Creates and places the color legend bar that explains critical point types with corresponding colors and labels.
        /// </summary>
        private void CreateLegendColorBar()
        {
            // Get the color scheme used for different types of critical points
            Dictionary<int, Color> typeColors = criticalPointObjectVisInstance.typeColors;

            // Define text labels for each critical point type
            string[] labels = { "Minimum", "1-Saddle", "2-Saddle", "Maximum" };

            // Extract colors from the dictionary into a color array (ordered by key)
            Color[] colors = new Color[typeColors.Count];
            for (int i = 0; i < typeColors.Count; i++)
            {
                colors[i] = typeColors[i];
            }

            // Create an instance of the LegendColorBar class to generate the visual legend
            LegendColorBar legend = new LegendColorBar();

            // Generate the color bar using the new method that places labels on all sides
            GameObject legendObject = legend.CreateColorScalarBar(
                new Vector3(0f, 0f, 0f),   // World position where the legend will appear
                "Critical Points",         // Title of the legend
                labels,                    // Labels to display for each color segment
                colors,                    // Corresponding colors for each label
                2                          // Percentage spacing between blocks
            );

            // Attach the legend under the main container so it moves/scales with the data visualization
            legendObject.transform.parent = criticalPointObjectVisInstance.container;
            legendObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Make it interactable in immersive systems
            legendObject.AddComponent<BoxCollider>();
            legendObject.AddComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();
            legendObject.AddComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionGrabbable>();
        }


        /// <summary>
        /// Filters critical points based on type and user selection.
        /// </summary>
        /// <param name="type">Type ID of the critical point</param>
        /// <param name="state">True to show, False to hide</param>
        private void FilterCriticalPointsByType(int type, bool state)
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
