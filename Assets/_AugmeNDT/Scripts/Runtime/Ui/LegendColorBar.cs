using MathNet.Numerics;
using UnityEngine;
using UnityEngine.Rendering;
using Microsoft.MixedReality.Toolkit.UI;

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
        /// Create color scalar bar with custom labels instead of numeric values,
        /// and make each color bar interactive as a button.
        /// The title is now interactive for dragging the entire color bar.
        /// </summary>
        public GameObject CreateInteractiveColorScalarBar(Vector3 position, string title, 
            string[] labels, Color[] colorScheme, System.Action<int> calledFunction,int percentOfSpacing = 2)
        {
            colorScalarBarContainer = new GameObject("Color Scheme");
            colorScalarBarContainer.transform.position = position;

            containerSize = new Vector3(0.8f, 1f, 0.6f);
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

                // Make the color bar interactive by adding required components
                MakeInteractive(colorBarInstance, i, calledFunction);

                currentPosition.y += cubeSize.y + spacing;
            }

            Vector3 titlePosition = currentPosition + new Vector3(0f, spacing, 0f);

            // Add single-facing title with draggable functionality
            GameObject colorScalarBarTitle = GameObject.Instantiate(text3DPrefab, titlePosition, Quaternion.identity, colorScalarBarContainer.transform);
            colorScalarBarTitle.name = "DraggableTitle";
            TextMesh titleText = colorScalarBarTitle.GetComponent<TextMesh>();
            titleText.text = title;
            titleText.anchor = TextAnchor.LowerCenter;
            titleText.fontSize = 250;

            // Make the entire container draggable as well
            MakeContainerDraggable(titlePosition, cubeSize, colorScalarBarContainer);

            return colorScalarBarContainer;
        }

        /// <summary>
        /// Makes the entire container draggable.
        /// </summary>
        private void MakeContainerDraggable(Vector3 titlePosition, Vector3 cubeSize, GameObject container)
        {

            // Add a box collider to the container
            BoxCollider containerCollider = container.AddComponent<BoxCollider>();
            containerCollider.size = cubeSize;
            containerCollider.center = new Vector3(0, titlePosition.y + cubeSize.y + 1f, 0);

            // Set the collider as trigger so it doesn't interfere with child colliders
            containerCollider.isTrigger = true;

            // Add ObjectManipulator component for dragging
            ObjectManipulator manipulator = container.AddComponent<ObjectManipulator>();
            manipulator.AllowFarManipulation = true;
        }

        /// <summary>
        /// Makes the color bar GameObject interactive with simple selection functionality.
        /// </summary>
        private void MakeInteractive(GameObject colorBar, int buttonIndex, System.Action<int> calledFunction)
        {
            // Add BoxCollider (if it doesn't exist)
            BoxCollider boundsColl = colorBar.GetComponent<BoxCollider>();
            if (boundsColl == null)
            {
                boundsColl = colorBar.AddComponent<BoxCollider>();
                boundsColl.enabled = true;
            }

            // Size the collider based on renderer bounds
            Renderer renderer = colorBar.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Get the exact bounds from renderer
                Bounds exactBounds = renderer.bounds;

                // First convert world center to local position
                Vector3 localCenter = colorBar.transform.InverseTransformPoint(exactBounds.center);

                // Set size and center directly
                boundsColl.size = colorBar.transform.InverseTransformVector(exactBounds.size);
                boundsColl.center = localCenter; // Don't subtract position - that's causing the offset

                // Debug log to verify calculations
                Debug.Log($"Collider center set to: {boundsColl.center}, size: {boundsColl.size}");
            }

            // Store original color for toggling
            Color originalColor = renderer.material.color;

            // Add interactable component for click functionality
            var interactable = colorBar.AddComponent<Microsoft.MixedReality.Toolkit.UI.Interactable>();
            interactable.OnClick.AddListener(() => CustomCalledFunction(calledFunction, buttonIndex));

            // Add components for direct hand interaction in VR
            colorBar.AddComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionGrabbable>();
            colorBar.AddComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionTouchableVolume>();
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

        // New method to toggle selection similar to ToggleSelection in DuplicateStreamLine2D
        private void CustomCalledFunction(System.Action<int> calledFunction,int buttonIndex)
        {
            calledFunction?.Invoke(buttonIndex);
        }
    }
}