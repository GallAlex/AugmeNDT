using UnityEngine;

/// <summary>
/// Class stores the values of a polygonal model for fibers
/// </summary>
public class PolyFiberData : ScriptableObject, IPolygonDataset
{

    private int[] label;
    private double[] realX1;
    private double[] realY1;
    private double[] realZ1;
    private double[] realX2;
    private double[] realY2;
    private double[] realZ2;
    private double[] straightLength;
    private double[] curvedLength;
    private double[] diameter;
    private double[] surfaceArea;
    private double[] volume;
    private int[] seperatedFibre;
    private int[] curvedFibre;

    #region Getter/Setter

    public int[] Label
    {
        get => label;
        set => label = value;
    }

    public double[] RealX1
    {
        get => realX1;
        set => realX1 = value;
    }

    public double[] RealY1
    {
        get => realY1;
        set => realY1 = value;
    }

    public double[] RealZ1
    {
        get => realZ1;
        set => realZ1 = value;
    }

    public double[] RealX2
    {
        get => realX2;
        set => realX2 = value;
    }

    public double[] RealY2
    {
        get => realY2;
        set => realY2 = value;
    }

    public double[] RealZ2
    {
        get => realZ2;
        set => realZ2 = value;
    }

    public double[] StraightLength
    {
        get => straightLength;
        set => straightLength = value;
    }

    public double[] CurvedLength
    {
        get => curvedLength;
        set => curvedLength = value;
    }

    public double[] Diameter
    {
        get => diameter;
        set => diameter = value;
    }

    public double[] SurfaceArea
    {
        get => surfaceArea;
        set => surfaceArea = value;
    }

    public double[] Volume
    {
        get => volume;
        set => volume = value;
    }

    public int[] SeperatedFibre
    {
        get => seperatedFibre;
        set => seperatedFibre = value;
    }

    public int[] CurvedFibre
    {
        get => curvedFibre;
        set => curvedFibre = value;
    }

    #endregion

    public PolyFiberData()
    {

    }

    public override string ToString()
    {
        string values = "label = " + label.ToString() + "\n";
        values += "realX1 = " + realX1.ToString() + "\n";
        values += "realY1 = " + realY1.ToString() + "\n";
        values += "realZ1 = " + realZ1.ToString() + "\n";
        values += "realX2 = " + realX2.ToString() + "\n";
        values += "realY2 = " + realY2.ToString() + "\n";
        values += "realZ2 = " + realZ2.ToString() + "\n";
        values += "straightLength = " + straightLength.ToString() + "\n";
        values += "curvedLength = " + curvedLength.ToString() + "\n";
        values += "diameter = " + diameter.ToString() + "\n";
        values += "surfaceArea = " + surfaceArea.ToString() + "\n";
        values += "volume = " + volume.ToString() + "\n";
        values += "seperatedFibre = " + seperatedFibre.ToString() + "\n";
        values += "curvedFibre = " + curvedFibre.ToString() + "\n";

        return base.ToString() + values;
    }
}
