using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Controls a 2D flow object that moves along predefined streamlines
    /// </summary>
    public class FlowObject2D : MonoBehaviour
    {
        // Reference to the rectangle manager for boundary checks
        private static RectangleManager rectangleManager;

        /// <summary>
        /// Initiates the flow simulation along gradient streamlines using rungeKutta4
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
        /// Initiates the flow simulation along a predefined streamline path
        /// </summary>
        /// <param name="streamlinePoints">Array of points defining the streamline path</param>
        /// <param name="gradientPoints">List of gradient data points as fallback</param>
        /// <param name="cubeBounds">Boundary constraints for the flow simulation</param>
        /// <param name="streamlineStepSize">Step size for calculations</param>
        /// <param name="sphereSpeed">Movement speed of the flow object</param>
        /// <param name="lifetime">Maximum duration in seconds before the flow object is destroyed</param>
        public void StartFlowAlongStreamline(Vector3[] streamlinePoints, List<GradientDataset> gradientPoints,
            Bounds cubeBounds, float streamlineStepSize, float sphereSpeed, float lifetime)
        {
            rectangleManager = RectangleManager.rectangleManager;
            StartCoroutine(MoveAlongStreamline(streamlinePoints, gradientPoints, cubeBounds,
                streamlineStepSize, sphereSpeed, lifetime));
        }

        /// <summary>
        /// Coroutine that handles the movement of the object along gradient streamlines using RungeKutta4
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

            // Get rectangle normal for projection
            Vector3 normal = rectangleManager.GetRectangleNormal();
            Vector3[] corners = rectangleManager.GetRectangleCorners();

            while (elapsedTime < lifetime)
            {
                // Get direction with returnPosition parameter set to false
                Vector3 direction = SpatialCalculations.RungeKutta4(
                    currentPosition,
                    gradientPoints,
                    streamlineStepSize,
                    false  // Return direction, not position
                );

                // Exit if magnitude is too small (converged or stagnant flow)
                if (direction.magnitude < 0.01f) break;

                // Project direction onto the plane
                direction = direction - Vector3.Dot(direction, normal) * normal;

                // Normalize and apply speed
                direction = direction.normalized * sphereSpeed * Time.deltaTime;

                // Calculate next position
                Vector3 nextPosition = currentPosition + direction;

                // Project position onto the plane to prevent drift
                nextPosition = SpatialCalculations.ProjectPointOntoRectanglePlane(nextPosition, corners, normal);

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

        /// <summary>
        /// Coroutine that handles the movement of the object along a predefined streamline path
        /// </summary>
        /// <param name="streamlinePoints">Array of points defining the streamline path</param>
        /// <param name="gradientPoints">List of gradient data points (as fallback)</param>
        /// <param name="cubeBounds">Boundary constraints for the flow simulation</param>
        /// <param name="streamlineStepSize">Step size (for reference only)</param>
        /// <param name="sphereSpeed">Movement speed of the flow object</param>
        /// <param name="lifetime">Maximum duration in seconds before the flow object is destroyed</param>
        /// <returns>IEnumerator for coroutine processing</returns>
        private IEnumerator MoveAlongStreamline(Vector3[] streamlinePoints, List<GradientDataset> gradientPoints,
            Bounds cubeBounds, float streamlineStepSize, float sphereSpeed, float lifetime)
        {
            if (streamlinePoints == null || streamlinePoints.Length < 2)
            {
                // Fallback to gradient-based movement if streamline is invalid
                yield return StartCoroutine(StartMoveSphere(gradientPoints, cubeBounds, streamlineStepSize, sphereSpeed, lifetime));
                yield break;
            }

            // Start from the beginning of the streamline
            int currentPointIndex = 0;
            float elapsedTime = 0f;

            while (currentPointIndex < streamlinePoints.Length - 1 && elapsedTime < lifetime)
            {
                // Calculate current and next points in the streamline
                Vector3 currentPoint = streamlinePoints[currentPointIndex];
                Vector3 nextPoint = streamlinePoints[currentPointIndex + 1];

                // Calculate direction vector between current and next point
                Vector3 direction = (nextPoint - currentPoint).normalized;

                // Calculate distance to move this frame
                float distanceToMove = sphereSpeed * Time.deltaTime;
                float distanceToNextPoint = Vector3.Distance(transform.position, nextPoint);

                // Check if we would reach or pass the next point
                if (distanceToMove >= distanceToNextPoint)
                {
                    // Move directly to the next point and advance to the next segment
                    transform.position = nextPoint;
                    currentPointIndex++;

                    // If we reached the end of the streamline, break the loop
                    if (currentPointIndex >= streamlinePoints.Length - 1)
                    {
                        break;
                    }
                }
                else
                {
                    // Move along the direction by the calculated distance
                    transform.position = Vector3.MoveTowards(transform.position, nextPoint, distanceToMove);
                }

                // Exit if object goes outside the boundary mesh (safety check)
                if (!rectangleManager.IsPointInsideMesh(transform.position))
                {
                    break;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Destroy the object when flow completes or times out
            Destroy(gameObject);
        }
    }
}