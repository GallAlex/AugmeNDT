using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferFunction : ScriptableObject
{

    private const int TEXTURE_WIDTH = 512;
    private const int TEXTURE_HEIGHT = 2;

    public List<TfColourControlPoint> colourCPs = new List<TfColourControlPoint>();

    public List<TfAlphaControlPoint> alphaCPs = new List<TfAlphaControlPoint>();

    private Texture2D texture = null;

    private Color[] tfCols;







}
