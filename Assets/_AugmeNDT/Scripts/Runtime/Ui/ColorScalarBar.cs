using MathNet.Numerics;
using UnityEngine;

namespace AugmeNDT{
    /// <summary>
    /// Class is responsible to create a color scalar bar for a visualization
    /// </summary>
    public class ColorScalarBar
    {
        public GameObject colorScalarBarContainer;
        public GameObject colorBarPrefab;
        public GameObject text3DPrefab;
        public Vector3 containerSize = new Vector3(0.1f, 0.3f, 0.1f);
        public Color[] colorScheme;         // Used Colors
        public int percentOfSpacing = 1;    // 1% of the container size

        public ColorScalarBar()
        {
            text3DPrefab = (GameObject)Resources.Load("Prefabs/Text3D");
            colorBarPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/ColorBar");
        }

        public GameObject CreateColorScalarBar(Vector3 position, string title, double[] minMaxValue, int percentOfSpacing, Color[] colorScheme)
        {
            colorScalarBarContainer = new GameObject("Color Bar");
            colorScalarBarContainer.transform.position = position;
        
            float spacing = containerSize.y * ((float)percentOfSpacing / 100.0f);
            float ySize = (containerSize.y / colorScheme.Length) - (colorScheme.Length - 1) * spacing;

            Vector3 cubeSize = new Vector3(containerSize.x, ySize, containerSize.z);
            Vector3 currentPosition = colorScalarBarContainer.transform.position;

            for (int i = 0; i < colorScheme.Length; i++)
            {
                GameObject colorBarInstance = GameObject.Instantiate(colorBarPrefab, currentPosition, Quaternion.identity, colorScalarBarContainer.transform);
                colorBarInstance.transform.localScale = cubeSize;
                colorBarInstance.GetComponent<Renderer>().material.color = colorScheme[i];

                //Calculate current value for the colorBar based on min and max value
                //TODO: Replace with scale linear?
                double ratio = (double)i / (colorScheme.Length - 1);
                double currentVal = ratio * (minMaxValue[1] - minMaxValue[0]) + minMaxValue[0];

                // Create cube text object
                GameObject colorBarText = GameObject.Instantiate(text3DPrefab, currentPosition + new Vector3(cubeSize.x + spacing, 0f, 0f), Quaternion.identity, colorScalarBarContainer.transform);
                TextMesh barText = colorBarText.GetComponent<TextMesh>();
                barText.text = currentVal.Round(3).ToString();
            
                currentPosition.y += cubeSize.y + spacing;
            }

            //Add title
            GameObject colorScalarBarTitle = GameObject.Instantiate(text3DPrefab, currentPosition + new Vector3(0f, spacing, 0f), Quaternion.identity, colorScalarBarContainer.transform);
            TextMesh titleText = colorScalarBarTitle.GetComponent<TextMesh>();
            titleText.text = title;

            return colorScalarBarContainer;
        }


    }
}
