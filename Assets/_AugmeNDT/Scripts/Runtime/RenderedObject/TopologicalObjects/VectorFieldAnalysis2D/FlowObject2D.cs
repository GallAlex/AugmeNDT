using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Controls a 2D flow object that moves along gradient streamlines
    /// </summary>
    public class FlowObject2D : MonoBehaviour
    {
        // Reference to the rectangle manager for boundary checks
        private static RectangleManager rectangleManager;

        /// <summary>
        /// Initiates the flow simulation along gradient streamlines
        /// </summary>
        /// <param name="gradientPoints">List of gradient data points defining the flow field</param>
        /// <param name="cubeBounds">Boundary constraints for the flow simulation</param>
        /// <param name="streamlineStepSize">Step size for the Runge-Kutta calculation</param>
        /// <param name="sphereSpeed">Movement speed of the flow object</param>
        /// <param name="lifetime">Maximum duration in seconds before the flow object is destroyed</param>
        public void StartFlow(List<GradientDataset> gradientPoints, Bounds cubeBounds, 
            float streamlineStepSize, float sphereSpeed, float lifetime)

        {
            rectangleManager = RectangleManager.rectangleManager;
            StartCoroutine(StartMoveSphere(gradientPoints, cubeBounds, streamlineStepSize, sphereSpeed, lifetime));
        }

        /// <summary>
        /// Coroutine that handles the movement of the object along gradient streamlines
        /// </summary>
        /// <param name="gradientPoints">List of gradient data points defining the flow field</param>
        /// <param name="cubeBounds">Boundary constraints for the flow simulation</param>
        /// <param name="streamlineStepSize">Step size for the Runge-Kutta calculation</param>
        /// <param name="sphereSpeed">Movement speed of the flow object</param>
        /// <param name="lifetime">Maximum duration in seconds before the flow object is destroyed</param>
        /// <returns>IEnumerator for coroutine processing</returns>
        private IEnumerator StartMoveSphere(List<GradientDataset> gradientPoints, Bounds cubeBounds,
            float streamlineStepSize, float sphereSpeed, float lifetime)
        {
            // Start from current object position
            Vector3 currentPosition = transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < lifetime)
            {
                // Calculate direction using Runge-Kutta 4th order method
                Vector3 direction = SpatialCalculations.RungeKutta4(currentPosition, gradientPoints, streamlineStepSize);

                // Exit if magnitude is too small (converged or stagnant flow)
                if (direction.magnitude < 0.01f) break;

                // Calculate next position based on direction and speed
                Vector3 nextPosition = currentPosition + direction.normalized * sphereSpeed * Time.deltaTime;

                // Exit if object goes outside the boundary mesh
                if (!rectangleManager.IsPointInsideMesh(nextPosition)) break;

                // Move the object to next position
                transform.position = nextPosition;
                currentPosition = nextPosition;
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            // Destroy the object when flow completes or times out
            Destroy(gameObject);
        }
    }
}