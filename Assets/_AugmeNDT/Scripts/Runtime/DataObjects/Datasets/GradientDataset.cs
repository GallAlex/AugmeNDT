using UnityEngine;

public class GradientDataset
{
    /// <summary>
    /// Stores the gradient calculation of volumetic data on the grid
    /// </summary>
    public int ID { get; set; }
    public Vector3 Position { get; set; } // location in the coordinate system
    public Vector3 Direction { get; set; } // direction in the coordinate system
    public float Magnitude { get; set; } // average of direction magnitudes

    public GradientDataset(int id, Vector3 position, Vector3 direction, float magnitude)
    {
        ID = id;
        Position = position;
        Direction = direction;
        Magnitude = magnitude;
    }
}
