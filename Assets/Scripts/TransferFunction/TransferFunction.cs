using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferFunction : ScriptableObject
{

    private const int TEXTURE_WIDTH = 512;
    private const int TEXTURE_HEIGHT = 2;

    public List<TFColourControlPoint> colourCPs = new List<TFColourControlPoint>();

    public List<TFAlphaControlPoint> alphaCPs = new List<TFAlphaControlPoint>();

    private Texture2D texture = null;

    private Color[] tfCols;







}
