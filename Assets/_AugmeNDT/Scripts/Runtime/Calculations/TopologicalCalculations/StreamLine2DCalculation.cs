using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    public class StreamLine2DCalculation
    {
        public Vector3 point1, point2, point3;
        public int numStreamlines = 400; // Kaç tane streamline çizileceği
        public float streamlineStepSize = 0.01f; // Adım büyüklüğü
        public int maxStreamlineSteps = 700; // Maksimum adım sayısı
        public List<GradientDataset> gradientPoints = new List<GradientDataset>();

        public StreamLine2DCalculation(List<Vector3> intersectionPositions,int numStreamlines,
            float streamlineStepSize, int maxStreamlineSteps, List<GradientDataset> gradientPoints)
        {
            this.point1 = intersectionPositions[0];
            this.point2 = intersectionPositions[1];
            this.point3 = intersectionPositions[2];
            this.numStreamlines = numStreamlines;
            this.streamlineStepSize = streamlineStepSize;
            this.maxStreamlineSteps = maxStreamlineSteps;
            this.gradientPoints = gradientPoints;
        }
        
        public List<List<Vector3>> CalculateStreamlines()
        {
            List<Vector3> seedPoints = GenerateOptimizedSeeds(numStreamlines, 0.1f);

            List<List<Vector3>> streamlinePoints = new List<List<Vector3>>();

            foreach (var seed in seedPoints)
            {
                streamlinePoints.Add(GenerateStreamline(seed));
            }

            return streamlinePoints;
        }
        private List<Vector3> GenerateOptimizedSeeds(int numSeeds, float minDistance)
        {
            List<Vector3> seedPoints = new List<Vector3>();

            // Generate evenly spaced points using Poisson Disk Sampling
            seedPoints = GeneratePoissonDiskSeeds(minDistance, numSeeds);
            return seedPoints;
        }
        private List<Vector3> GeneratePoissonDiskSeeds(float minDistance, int maxAttempts)
        {
            List<Vector3> seedPoints = new List<Vector3>();

            // Define triangle bounds
            float minX = Mathf.Min(point1.x, point2.x, point3.x);
            float maxX = Mathf.Max(point1.x, point2.x, point3.x);
            float minZ = Mathf.Min(point1.z, point2.z, point3.z);
            float maxZ = Mathf.Max(point1.z, point2.z, point3.z);

            // Poisson Disk Sampling
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 candidate = GetRandomPointInTriangle();

                // Ensure minimum distance constraint
                bool valid = true;
                foreach (Vector3 point in seedPoints)
                {
                    if (Vector3.Distance(point, candidate) < minDistance)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    seedPoints.Add(candidate);
                }

                if (seedPoints.Count >= maxAttempts) break;
            }

            return seedPoints;
        }
        public Vector3 GetRandomPointInTriangle()
        {
            float r1 = UnityEngine.Random.Range(0.1f, 1f);
            float r2 = UnityEngine.Random.Range(0.1f, 1f);

            if (r1 + r2 > 1)
            {
                r1 = 1 - r1;
                r2 = 1 - r2;
            }

            return point1
                + r1 * (point2 - point1)
                + r2 * (point3 - point1);
        }
        private List<Vector3> GenerateStreamline(Vector3 startPosition)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3 currentPosition = startPosition;

            for (int i = 0; i < maxStreamlineSteps; i++)
            {
                Vector3 direction = SpatialCalculations.RungeKutta4(currentPosition,gradientPoints,streamlineStepSize);
                float velocityMagnitude = direction.magnitude;

                // Stop if the velocity is too small
                if (velocityMagnitude < 0.001f)
                    break;

                float stepSize = Mathf.Clamp(velocityMagnitude * 0.05f, 0.005f, 0.05f);

                points.Add(currentPosition);
                currentPosition += direction * stepSize;

                // Stop if streamline exits the triangle
                bool isPointInTriangle = SpatialCalculations.IsPointInTriangle(currentPosition, point1, point2, point3);
                if (!isPointInTriangle) break;
            }

            return points;
        }
        public List<Vector3> GetBorderPoints() { return new List<Vector3>() { point1, point2, point3 }; }

    }
}
