using UnityEngine;

namespace AugmeNDT
{
    [System.Serializable]
    public class TopologyConfigData
    {
        [Header("Scene Number")]
        public int SceneNumber = 2;

        [Header("MHD Dimensions")]
        public Vector3 mhdDimension = new Vector3(24, 16, 45);

        [Header("SubRegion Borders")]
        public Vector3 lowerCornerOfSubRegion = new Vector3(0, 0, 5);
        public Vector3 upperCornerOfSubRegion = new Vector3(24, 8, 40);

        [Header("Data Paths")]
        public string mhdPath = @"C:/Users/ozdag/OneDrive/Desktop/smallDATA/fibers.mhd";

        [Header("Critical Point Color Settings")]
        public Color sinkColor = Color.blue;
        public Color sourcePointColor = Color.red;
        public Color saddle1_PointColor = Color.yellow;
        public Color saddle2_PointColor = Color.magenta;

        [Header("Critical Point Label Settings")]
        public string label_sink = "Sink";
        public string label_source = "Source";
        public string label_saddle1 = "1-Saddle";
        public string label_saddle2 = "2-Saddle";

        [Header("Vector Field Color Settings")]
        public Color ColorOfVectorObject = Color.gray;
        public Color ColorOfStreamLines = Color.white;

        [Header("3D Streamline Parameters")]
        public int numberOfStreamline = 550;
        public float StepSizeOfStreamline = 0.0033f;
        public int maxStepOfStreamline = 142;

        [Header("3D Sub-region region-border")]
        public bool showBorderof3DSub_Region = false;
        public Color ColorOf_Borderof3DSub_Region = Color.green;

        [Header("Settings of All Vector Field_Vector Visualization")]
        public float localScaleRate = 0.3f;
        public int arrowsPerFrame = 50;

        public bool UseDynamicVectorField = false;
        public int maxVectorCount = 20000;
        public int minVectorCount = 1000;
        public float scaleChangeThreshold = 0.1f;
        public float vectorDensity = 0.6f;

        [Header("Settings of All Vector Field_Critical Points Visualization")]
        public float criticalPoints_localScaleRate = 0.006f;

        public bool UseDynamicCriticalPoints = false;
        public float cp_dynamic_localScaleFactor = 0.006f;
        public int cp_dynamic_maxPointCount = 10000;
        public int cp_dynamic_minPointCount = 400;
        public float cp_dynamic_scaleChangeThreshold = 0.1f;
    }
}
