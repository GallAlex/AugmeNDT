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
            string typeName = criticalPointObjectVisInstance.GetCriticalTypeName(type);
            //infoText.text = $"ID: {id}\nType: {typeName}\nPosition: {position}";
        }

        public void ShowHideAllPoints()
        {
            showHideAllPoints = !showHideAllPoints ? true : false;
            
            ShowSink(showHideAllPoints);
            ShowSaddle1(showHideAllPoints);
            ShowSaddle2(showHideAllPoints);
            ShowSource(showHideAllPoints);

            criticalPointObjectVisInstance.ShowLegend(showHideAllPoints);
        }

        public void ShowSink(bool isOn)
        {
            criticalPointObjectVisInstance.FilterCriticalPointsByType(0,isOn);
        }

        public void ShowSaddle1(bool isOn)
        {
            criticalPointObjectVisInstance.FilterCriticalPointsByType(1, isOn);
        }
        public void ShowSaddle2(bool isOn)
        {
            criticalPointObjectVisInstance.FilterCriticalPointsByType(2, isOn);
        }
        public void ShowSource(bool isOn)
        {
            criticalPointObjectVisInstance.FilterCriticalPointsByType(3, isOn);
        }
    }
}
