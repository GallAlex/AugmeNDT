// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using UnityEngine;

namespace AugmeNDT{
    /// <summary>
    /// Script aligns a TextMesh to face the Camera
    /// </summary>
    public class FaceUser : MonoBehaviour
    {
        public float rotationSpeed = 2f;
        private TextMesh textMesh;
        private Transform cameraTransform;
        private Quaternion initialRotation;

        private void Start()
        {
            cameraTransform = Camera.main.transform;
            textMesh = GetComponentInChildren<TextMesh>();
            initialRotation = textMesh.transform.localRotation;
        }

        private void Update()
        {
            Vector3 targetDirection = (transform.position - cameraTransform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            textMesh.transform.localRotation = Quaternion.Slerp(textMesh.transform.localRotation, targetRotation * initialRotation, rotationSpeed * Time.deltaTime);
        }

    }
}
