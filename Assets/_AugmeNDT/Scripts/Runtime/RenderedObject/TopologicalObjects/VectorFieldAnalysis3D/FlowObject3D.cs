using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Controls a 3D flow object (sphere) that moves along gradient streamlines
    /// </summary>
    public class FlowObject3D : MonoBehaviour
    {
        // Movement speed of the sphere
        private float sphereSpeed = 1.0f;

        // Maximum duration in seconds before the flow object is destroyed
        private float lifetime = 15;

        /// <summary>
        /// Initiates the flow simulation for a sphere along gradient streamlines
        /// </summary>
        /// <param name="gradientPoints">List of gradient data points defining the flow field</param>
        /// <param name="cubeBounds">Boundary constraints for the flow simulation</param>
        /// <param name="streamlineStepSize">Step size for the Runge-Kutta calculation</param>
        public void StartFlow(List<GradientDataset> gradientPoints, Bounds cubeBounds, float streamlineStepSize)
        {
            StartCoroutine(StartMoveSphere(gradientPoints, cubeBounds, streamlineStepSize));
        }

        /// <summary>
        /// Coroutine that handles the movement of the sphere along gradient streamlines
        /// </summary>
        /// <param name="gradientPoints">List of gradient data points defining the flow field</param>
        /// <param name="cubeBounds">Boundary constraints for the flow simulation</param>
        /// <param name="streamlineStepSize">Step size for the Runge-Kutta calculation</param>
        /// <returns>IEnumerator for coroutine processing</returns>
        private IEnumerator StartMoveSphere(List<GradientDataset> gradientPoints, Bounds cubeBounds, float streamlineStepSize)
        {
            // Exit if no gradient points are provided
            if (gradientPoints == null)
                yield return null;

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

                // Exit if sphere goes outside the boundary
                if (!cubeBounds.Contains(currentPosition)) break;

                // Move the sphere to next position
                transform.position = nextPosition;
                currentPosition = nextPosition;
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            // Destroy the sphere object when flow completes or times out
            Destroy(gameObject);
        }
    }
}