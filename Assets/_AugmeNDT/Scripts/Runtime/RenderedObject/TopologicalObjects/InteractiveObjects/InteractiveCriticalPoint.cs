using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Enables interaction with critical points in the scene.
    /// Clicking on a critical point displays its details in the UI.
    /// </summary>
    public class InteractiveCriticalPoint : MonoBehaviour
    {
        public int pointID;
        public int pointType;
        public Vector3 pointPosition;

        /// <summary>
        /// When a user clicks on the critical point, it sends the information
        /// to the `CriticalPointPanelManager` for display.
        /// </summary>
        private void OnMouseDown()
        {
            if (CriticalPointPanelManager.Instance == null)
            {
                Debug.LogError("CriticalPointPanelManager is not found.");
                return;
            }

            CriticalPointPanelManager.Instance.ShowPointInfo(pointID, pointType, pointPosition);
        }
    }
}