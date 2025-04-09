using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace AugmeNDT
{
    /// <summary>
    /// Controls a 3D flow object (sphere) that moves along gradient streamlines
    /// </summary>
    public class FlowObject3D : MonoBehaviour
    {
        // Movement speed of the sphere
        private float sphereSpeed = 0.005f; // Düşürüldü çünkü vektör alanı artık daha hassas

        // Maximum duration in seconds before the flow object is destroyed
        private float lifetime = 15;

        // Reference to spatial grid for faster interpolation
        private Dictionary<Vector3Int, List<GradientDataset>> spatialGrid;
        private float cellSize;

        /// <summary>
        /// Initiates the flow simulation for a sphere along gradient streamlines
        /// </summary>
        /// <param name="gradientPoints">List of gradient data points defining the flow field</param>
        /// <param name="spatialGrid">Spatial grid for faster interpolation</param>
        /// <param name="cellSize">Cell size for spatial grid</param>
        /// <param name="cubeBounds">Boundary constraints for the flow simulation</param>
        /// <param name="streamlineStepSize">Step size for the Runge-Kutta calculation</param>
        public void StartFlow(List<GradientDataset> gradientPoints, Dictionary<Vector3Int, List<GradientDataset>> spatialGrid,
                              float cellSize, Bounds cubeBounds, float streamlineStepSize)
        {
            this.spatialGrid = spatialGrid;
            this.cellSize = cellSize;
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
            if (gradientPoints == null || spatialGrid == null)
                yield return null;

            Vector3 currentPosition = transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < lifetime)
            {
                // Calculate direction using the new interpolation method
                Vector3 direction = SpatialCalculations.InterpolateVectorField(currentPosition, cellSize, spatialGrid);

                // Exit if magnitude is too small (converged or stagnant flow)
                if (direction.magnitude < 0.001f) break;

                // Calculate next position based on direction and speed
                Vector3 nextPosition = currentPosition + direction.normalized * sphereSpeed * Time.deltaTime;

                // Exit if sphere goes outside the boundary
                if (!cubeBounds.Contains(nextPosition)) break;

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