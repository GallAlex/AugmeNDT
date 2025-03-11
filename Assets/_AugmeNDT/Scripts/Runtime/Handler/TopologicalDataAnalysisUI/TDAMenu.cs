using UnityEngine;

namespace AugmeNDT
{
    public class TDAMenu : MonoBehaviour
    {
        public static TDAMenu instance;
        public GameObject infoPanel;  // Main menu panel

        private void Awake()
        {
            instance = this;
        }

        public void ActivateTDAInfoPanel()
        {
            infoPanel.SetActive(true);
        }
    }
}
