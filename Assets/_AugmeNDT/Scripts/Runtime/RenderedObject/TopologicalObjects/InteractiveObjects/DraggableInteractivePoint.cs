using UnityEngine;

namespace AugmeNDT
{
    public class DraggableInteractivePoint : MonoBehaviour
    {
        private Vector3 offset;
        private Camera cam;

        private void Start()
        {
            cam = Camera.main;
        }

        private void OnMouseDown()
        {
            offset = transform.position - GetMouseWorldPos();
        }

        private void OnMouseDrag()
        {
            transform.position = GetMouseWorldPos() + offset;
        }

        private Vector3 GetMouseWorldPos()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = cam.WorldToScreenPoint(transform.position).z;
            return cam.ScreenToWorldPoint(mousePoint);
        }
    }
}
