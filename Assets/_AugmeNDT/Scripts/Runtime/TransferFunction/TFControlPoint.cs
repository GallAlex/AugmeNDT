using UnityEngine;

namespace AugmeNDT{
    public struct TfColourControlPoint
    {
        public float dataValue;
        public Color colourValue;

        public TfColourControlPoint(float dataValue, Color colourValue)
        {
            this.dataValue = dataValue;
            this.colourValue = colourValue;
        }
    }

    public struct TfAlphaControlPoint
    {
        public float dataValue;
        public float alphaValue;

        public TfAlphaControlPoint(float dataValue, float alphaValue)
        {
            this.dataValue = dataValue;
            this.alphaValue = alphaValue;
        }
    }
}