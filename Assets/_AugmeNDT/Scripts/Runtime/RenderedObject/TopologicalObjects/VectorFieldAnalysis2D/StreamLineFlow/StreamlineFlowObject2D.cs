using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Controls a 2D flow object that moves along predefined streamlines
    /// </summary>
    public class StreamlineFlowObject2D : MonoBehaviour
    {
        /// <summary>
        /// Initiates the flow simulation along a predefined streamline path
        /// </summary>
        /// <param name="streamlinePoints">Array of points defining the streamline path</param>
        /// <param name="gradientPoints">List of gradient data points as fallback</param>
        /// <param name="cubeBounds">Boundary constraints for the flow simulation</param>
        /// <param name="streamlineStepSize">Step size for calculations</param>
        /// <param name="sphereSpeed">Movement speed of the flow object</param>
        /// <param name="lifetime">Maximum duration in seconds before the flow object is destroyed</param>
        public void StartFlowAlongStreamline(
            Vector3[] streamlinePoints, List<GradientDataset> gradientPoints,
            float streamlineStepSize, float sphereSpeed, 
            float lifetime)
        {
            StartCoroutine(MoveAlongStreamline(streamlinePoints, gradientPoints,
                streamlineStepSize, sphereSpeed, lifetime));
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
        private IEnumerator MoveAlongStreamline(
            Vector3[] streamlinePoints, List<GradientDataset> gradientPoints,
            float streamlineStepSize, float sphereSpeed, 
            float lifetime)
        {
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

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Destroy the object when flow completes or times out
            Destroy(gameObject);
        }
    }
}