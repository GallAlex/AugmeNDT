using MathNet.Numerics;
using System.Linq;
using UnityEngine;

namespace AugmeNDT{
    /// <summary>
    /// Class is responsible to create a color scalar bar for a visualization
    /// </summary>
    public class LegendColorBar
    {
        public GameObject colorScalarBarContainer;
        public GameObject colorBarPrefab;
        public GameObject text3DPrefab;
        public Vector3 containerSize = new Vector3(0.5f, 1f, 0.4f);
        public Color[] colorScheme;         // Used Colors
        public int percentOfSpacing = 2;    // 2% of the container size

        public LegendColorBar()
        {
            text3DPrefab = (GameObject)Resources.Load("Prefabs/Text3D");
            colorBarPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Legends/ColorBar");
        }

        public GameObject CreateColorScalarBar(Vector3 position, string title, double[] minMaxValue, int percentOfSpacing, Color[] colorScheme)
        {
            colorScalarBarContainer = new GameObject("Color Scheme");
            colorScalarBarContainer.transform.position = position;
        
            float spacing = containerSize.y * ((float)percentOfSpacing / 100.0f);
            float spacingSize = (colorScheme.Length - 1) * spacing;
            float ySize = (containerSize.y - spacingSize) / colorScheme.Length;

            Vector3 cubeSize = new Vector3(containerSize.x, ySize, containerSize.z);
            Vector3 currentPosition = colorScalarBarContainer.transform.position;

            for (int i = 0; i < colorScheme.Length; i++)
            {
                GameObject colorBarInstance = GameObject.Instantiate(colorBarPrefab, currentPosition, Quaternion.identity, colorScalarBarContainer.transform);
                colorBarInstance.transform.localScale = cubeSize;
                colorBarInstance.name = "Color Bar " + i;
                colorBarInstance.GetComponent<Renderer>().material.color = colorScheme[i];

                //Calculate min and max value for current colorBar
                double[] boundaries = ScaleColor.GetCategoricalColorValueRange(i, minMaxValue[0], minMaxValue[1], colorScheme.Length);
                //double[] boundaries = ScaleColor.GetCategoricalColorValueRange(i, minMaxValue[0], 0.0, minMaxValue[1], colorScheme.Length);

                // Create cube text object
                Vector3 pos = currentPosition + new Vector3((cubeSize.x / 1.5f), -(cubeSize.y / 2f), 0f);
                CreateValueText(pos, boundaries[0].Round(3).ToString());

                // Create max value text object
                if (i == colorScheme.Length - 1)
                {
                    Vector3 topPos = currentPosition + new Vector3((cubeSize.x / 1.5f), (cubeSize.y / 2f), 0f);
                    CreateValueText(topPos, boundaries[1].Round(3).ToString());
                }

                currentPosition.y += cubeSize.y + spacing;
            }

            //Add title
            GameObject colorScalarBarTitle = GameObject.Instantiate(text3DPrefab, currentPosition + new Vector3(0f, spacing, 0f), Quaternion.identity, colorScalarBarContainer.transform);
            TextMesh titleText = colorScalarBarTitle.GetComponent<TextMesh>();
            titleText.text = title;
            titleText.anchor = TextAnchor.LowerCenter;
            titleText.fontSize = 250;

            return colorScalarBarContainer;
        }

        public GameObject CreateColorScalarBar(Vector3 position, string title, double[] minMaxValue, double midValue, int percentOfSpacing, Color[] colorScheme)
        {
            colorScalarBarContainer = new GameObject("Color Scheme");
            colorScalarBarContainer.transform.position = position;

            float spacing = containerSize.y * ((float)percentOfSpacing / 100.0f);
            float spacingSize = (colorScheme.Length - 1) * spacing;
            float ySize = (containerSize.y - spacingSize) / colorScheme.Length;

            Vector3 cubeSize = new Vector3(containerSize.x, ySize, containerSize.z);
            Vector3 currentPosition = colorScalarBarContainer.transform.position;

            for (int i = 0; i < colorScheme.Length; i++)
            {
                GameObject colorBarInstance = GameObject.Instantiate(colorBarPrefab, currentPosition, Quaternion.identity, colorScalarBarContainer.transform);
                colorBarInstance.transform.localScale = cubeSize;
                colorBarInstance.name = "Color Bar " + i;
                colorBarInstance.GetComponent<Renderer>().material.color = colorScheme[i];

                //Calculate min and max value for current colorBar
                double[] boundaries = ScaleColor.GetCategoricalColorValueRange(i, minMaxValue[0], midValue, minMaxValue[1], colorScheme.Length);

                // Create cube text object
                Vector3 pos = currentPosition + new Vector3((cubeSize.x / 1.5f), -(cubeSize.y / 2f), 0f);
                CreateValueText(pos, boundaries[0].Round(3).ToString());

                // Create max value text object
                if (i == colorScheme.Length - 1)
                {
                    Vector3 topPos = currentPosition + new Vector3((cubeSize.x / 1.5f), (cubeSize.y / 2f), 0f);
                    CreateValueText(topPos, boundaries[1].Round(3).ToString());
                }

                currentPosition.y += cubeSize.y + spacing;
            }

            //Add title
            GameObject colorScalarBarTitle = GameObject.Instantiate(text3DPrefab, currentPosition + new Vector3(0f, spacing, 0f), Quaternion.identity, colorScalarBarContainer.transform);
            TextMesh titleText = colorScalarBarTitle.GetComponent<TextMesh>();
            titleText.text = title;
            titleText.anchor = TextAnchor.LowerCenter;
            titleText.fontSize = 250;

            return colorScalarBarContainer;
        }

        private void CreateValueText(Vector3 position, string text)
        {
            GameObject colorBarText = GameObject.Instantiate(text3DPrefab, position, Quaternion.identity, colorScalarBarContainer.transform);
            TextMesh barText = colorBarText.GetComponent<TextMesh>();
            barText.text = text;
            barText.anchor = TextAnchor.MiddleLeft;
            barText.fontSize = 150;
        }

    }
}
