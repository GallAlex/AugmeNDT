using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{

    public static class GaussianFilterUtils
    {
        /// <summary>
        /// Applies Gaussian smoothing to the given list of gradient points using parallel processing.
        /// This method reduces noise by averaging nearby gradients with a Gaussian weight function.
        /// </summary>
        /// <param name="generatedGradientPoints">List of gradient points to be smoothed.</param>
        /// <param name="gaussianSigma">The standard deviation (sigma) of the Gaussian kernel.</param>
        /// <returns>A new list of smoothed gradient points.</returns>
        public static List<GradientDataset> ApplyGaussianSmoothingParallel(List<GradientDataset> generatedGradientPoints, float gaussianSigma)
        {
            // Thread-safe koleksiyon kullanarak çıktıyı topluyoruz
            ConcurrentBag<GradientDataset> smoothedGradientPoints = new ConcurrentBag<GradientDataset>();

            // Her bir nokta için paralel işlem yürütüyoruz
            Parallel.ForEach(generatedGradientPoints, point =>
            {
                // Her nokta için belirli mesafedeki komşuları buluyoruz
                List<GradientDataset> neighbors = generatedGradientPoints
                    .Where(p => Vector3.Distance(point.Position, p.Position) <= 0.5f)
                    .ToList();

                // Skip if no neighbors are found (avoid division by zero later).
                if (neighbors.Count == 0)
                    return; // Parallel.ForEach içinde continue yerine return kullanılır

                Vector3 weightedDirection = Vector3.zero;
                float weightedMagnitude = 0f;
                float totalWeight = 0f;

                // Apply Gaussian weight function to each neighboring point.
                foreach (var neighbor in neighbors)
                {
                    // Compute Euclidean distance between the point and its neighbor.
                    float distance = Vector3.Distance(point.Position, neighbor.Position);

                    // Gaussian weight function: exp(-d^2 / (2 * sigma^2))
                    float weight = Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * Mathf.Pow(gaussianSigma, 2)));

                    // Accumulate weighted values
                    weightedDirection += neighbor.Direction * weight;
                    weightedMagnitude += neighbor.Magnitude * weight;
                    totalWeight += weight;
                }

                // Normalize weighted sum to prevent bias
                if (totalWeight > 0)
                {
                    weightedDirection /= totalWeight;
                    weightedMagnitude /= totalWeight;
                }

                // Thread-safe koleksiyona yeni oluşturulan noktayı ekliyoruz
                smoothedGradientPoints.Add(new GradientDataset(
                    point.ID,
                    point.Position,
                    weightedDirection.normalized,
                    weightedMagnitude));
            });

            // Debug.Log çağrısını paralel döngü dışında yapıyoruz
            Debug.Log("Gaussian Kernel Smoothing completed with parallel processing.");

            // ConcurrentBag'i normal listeye dönüştürüyoruz
            // Not: ConcurrentBag sırayı korumaz, eğer sıra önemliyse ID bazlı sıralama eklenebilir
            return smoothedGradientPoints.ToList();
        }
    }
}
