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
    }
}