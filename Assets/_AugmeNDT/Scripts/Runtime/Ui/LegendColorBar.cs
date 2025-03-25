using MathNet.Numerics;
using System.Linq;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Class is responsible for creating a color scalar bar for data visualization.
    /// </summary>
    public class LegendColorBar
    {
        public GameObject colorScalarBarContainer;
        public GameObject colorBarPrefab;
        public GameObject text3DPrefab;
        public Vector3 containerSize = new Vector3(0.5f, 1f, 0.4f); // Dimensions of the entire color bar
        public Color[] colorScheme;
        public int percentOfSpacing = 2;    // Percentage of spacing between each block

        public LegendColorBar()
        {
            text3DPrefab = (GameObject)Resources.Load("Prefabs/Text3D");
            colorBarPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Legends/ColorBar");
        }

        /// <summary>
        /// Create color scalar bar with numeric min-max values.
        /// </summary>
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

                // Generate value range for the block
                double[] boundaries = ScaleColor.GetCategoricalColorValueRange(i, minMaxValue[0], minMaxValue[1], colorScheme.Length);

                // Add numeric label to bottom
                Vector3 pos = currentPosition + new Vector3((cubeSize.x / 1.5f), -(cubeSize.y / 2f), 0f);
                CreateValueText(pos, boundaries[0].Round(3).ToString());

                // Add top label for last block
                if (i == colorScheme.Length - 1)
                {
                    Vector3 topPos = currentPosition + new Vector3((cubeSize.x / 1.5f), (cubeSize.y / 2f), 0f);
                    CreateValueText(topPos, boundaries[1].Round(3).ToString());
                }

                currentPosition.y += cubeSize.y + spacing;
            }

            // Add single-facing title
            GameObject colorScalarBarTitle = GameObject.Instantiate(text3DPrefab, currentPosition + new Vector3(0f, spacing, 0f), Quaternion.identity, colorScalarBarContainer.transform);
            TextMesh titleText = colorScalarBarTitle.GetComponent<TextMesh>();
            titleText.text = title;
            titleText.anchor = TextAnchor.LowerCenter;
            titleText.fontSize = 250;

            return colorScalarBarContainer;
        }

        /// <summary>
        /// Create color scalar bar with a mid value included.
        /// </summary>
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

                double[] boundaries = ScaleColor.GetCategoricalColorValueRange(i, minMaxValue[0], midValue, minMaxValue[1], colorScheme.Length);

                Vector3 pos = currentPosition + new Vector3((cubeSize.x / 1.5f), -(cubeSize.y / 2f), 0f);
                CreateValueText(pos, boundaries[0].Round(3).ToString());

                if (i == colorScheme.Length - 1)
                {
                    Vector3 topPos = currentPosition + new Vector3((cubeSize.x / 1.5f), (cubeSize.y / 2f), 0f);
                    CreateValueText(topPos, boundaries[1].Round(3).ToString());
                }

                currentPosition.y += cubeSize.y + spacing;
            }

            // Add single-facing title
            GameObject colorScalarBarTitle = GameObject.Instantiate(text3DPrefab, currentPosition + new Vector3(0f, spacing, 0f), Quaternion.identity, colorScalarBarContainer.transform);
            TextMesh titleText = colorScalarBarTitle.GetComponent<TextMesh>();
            titleText.text = title;
            titleText.anchor = TextAnchor.LowerCenter;
            titleText.fontSize = 250;

            return colorScalarBarContainer;
        }

        /// <summary>
        /// Create color scalar bar with custom labels instead of numeric values.
        /// </summary>
        public GameObject CreateColorScalarBar(Vector3 position, string title, string[] labels, Color[] colorScheme, int percentOfSpacing = 2)
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

                // Add label on all four sides of the block
                CreateValueTextOnAllSides(currentPosition, cubeSize, labels[i]);

                currentPosition.y += cubeSize.y + spacing;
            }

            // Add single-facing title
            GameObject colorScalarBarTitle = GameObject.Instantiate(text3DPrefab, currentPosition + new Vector3(0f, spacing, 0f), Quaternion.identity, colorScalarBarContainer.transform);
            TextMesh titleText = colorScalarBarTitle.GetComponent<TextMesh>();
            titleText.text = title;
            titleText.anchor = TextAnchor.LowerCenter;
            titleText.fontSize = 250;

            return colorScalarBarContainer;
        }

        /// <summary>
        /// Instantiates 3D text on all four sides (front, back, left, right) of a block.
        /// </summary>
        private void CreateValueTextOnAllSides(Vector3 centerPosition, Vector3 cubeSize, string text)
        {
            float offset = cubeSize.z / 2f + 0.01f;
            float sideOffset = cubeSize.x / 2f + 0.01f;

            Vector3[] directions = new Vector3[]
            {
                new Vector3(0, 0, -offset),    // front
                new Vector3(0, 0, offset),     // back
                new Vector3(sideOffset, 0, 0), // right
                new Vector3(-sideOffset, 0, 0) // left
            };

            Vector3[] rotations = new Vector3[]
            {
                new Vector3(0, 0, 0),       // front
                new Vector3(0, 180, 0),     // back
                new Vector3(0, -90, 0),     // right
                new Vector3(0, 90, 0)       // left
            };

            for (int i = 0; i < directions.Length; i++)
            {
                Vector3 pos = centerPosition + directions[i];
                Quaternion rot = Quaternion.Euler(rotations[i]);

                GameObject label = GameObject.Instantiate(text3DPrefab, pos, rot, colorScalarBarContainer.transform);
                TextMesh tm = label.GetComponent<TextMesh>();
                tm.text = text;
                tm.fontSize = 150;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
            }
        }

        /// <summary>
        /// Instantiates 3D text at a given position with left alignment.
        /// </summary>
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
