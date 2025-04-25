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
        public GameObject CreateInteractiveColorScalarBar(Vector3 position, string title, string[] labels, Color[] colorScheme, int percentOfSpacing = 2)
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
                MakeInteractive(colorBarInstance, i);

                currentPosition.y += cubeSize.y + spacing;
            }

            // Add single-facing title with draggable functionality
            GameObject colorScalarBarTitle = GameObject.Instantiate(text3DPrefab, currentPosition + new Vector3(0f, spacing, 0f), Quaternion.identity, colorScalarBarContainer.transform);
            colorScalarBarTitle.name = "DraggableTitle";
            TextMesh titleText = colorScalarBarTitle.GetComponent<TextMesh>();
            titleText.text = title;
            titleText.anchor = TextAnchor.LowerCenter;
            titleText.fontSize = 250;

            // Make the title draggable
            MakeTitleDraggable(colorScalarBarTitle);

            // Make the entire container draggable as well
            MakeContainerDraggable(colorScalarBarContainer);

            return colorScalarBarContainer;
        }

        /// <summary>
        /// Makes the title GameObject draggable.
        /// </summary>
        private void MakeTitleDraggable(GameObject titleObject)
        {
            // Add box collider to the title
            BoxCollider titleCollider = titleObject.AddComponent<BoxCollider>();

            // Adjust the collider size based on the text mesh
            TextMesh textMesh = titleObject.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                // Get approximate text bounds
                MeshRenderer renderer = titleObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    titleCollider.size = renderer.bounds.size;
                    titleCollider.center = renderer.bounds.center - titleObject.transform.position;
                }
                else
                {
                    // Default size if renderer is not available
                    titleCollider.size = new Vector3(1f, 0.3f, 0.1f);
                }
            }

            // Add ObjectManipulator component for drag functionality
            ObjectManipulator manipulator = titleObject.AddComponent<ObjectManipulator>();

            // Configure ObjectManipulator for dragging
            manipulator.HostTransform = colorScalarBarContainer.transform; // This makes the entire container move
            manipulator.AllowFarManipulation = true;

            // Visual feedback when interacting with the title
            GameObject highlightObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            highlightObj.name = "Highlight_Title";
            highlightObj.transform.SetParent(titleObject.transform);
            highlightObj.transform.localPosition = Vector3.zero;
            highlightObj.transform.localRotation = Quaternion.identity;

            // Scale the highlight object to match the title's collider
            highlightObj.transform.localScale = titleCollider.size * 1.05f;

            // Create highlighting material
            Material highlightMat = new Material(Shader.Find("Standard"));
            highlightMat.color = new Color(0.5f, 0.5f, 1f, 0.3f);
            highlightMat.SetFloat("_Mode", 3); // Transparent mode
            highlightMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            highlightMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            highlightMat.SetInt("_ZWrite", 0);
            highlightMat.DisableKeyword("_ALPHATEST_ON");
            highlightMat.EnableKeyword("_ALPHABLEND_ON");
            highlightMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            highlightMat.renderQueue = 3000;

            // Apply material to highlight object
            highlightObj.GetComponent<Renderer>().material = highlightMat;

            // Start with highlight inactive
            highlightObj.SetActive(false);

            // Add focus handler to toggle highlight visibility
            TitleFocusHandler focusHandler = titleObject.AddComponent<TitleFocusHandler>();
            focusHandler.highlightObject = highlightObj;
        }

        /// <summary>
        /// Makes the entire container draggable.
        /// </summary>
        private void MakeContainerDraggable(GameObject container)
        {
            // Add a box collider to the container
            BoxCollider containerCollider = container.AddComponent<BoxCollider>();

            // Calculate bounds to include all children
            Bounds bounds = new Bounds(container.transform.position, Vector3.zero);
            Renderer[] renderers = container.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            // Set the collider size to encompass all children
            containerCollider.size = bounds.size;
            containerCollider.center = bounds.center - container.transform.position;

            // Set the collider as trigger so it doesn't interfere with child colliders
            containerCollider.isTrigger = true;

            // Add ObjectManipulator component for dragging
            ObjectManipulator manipulator = container.AddComponent<ObjectManipulator>();
            manipulator.AllowFarManipulation = true;

            // Optional: Add bounds control for scaling/rotating
            // BoundsControl boundsControl = container.AddComponent<BoundsControl>();
            // boundsControl.BoundsOverride = containerCollider;
            // boundsControl.RotateHandlesConfig.ShowRotationHandleForX = true;
            // boundsControl.RotateHandlesConfig.ShowRotationHandleForY = true;
            // boundsControl.RotateHandlesConfig.ShowRotationHandleForZ = true;
        }

        private void MakeInteractive(GameObject colorBar, int buttonIndex)
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
                // GlobalScaleAndPos method for sizing
                GlobalScaleAndPos.ResizeBoxCollider(
                    colorBar.transform,
                    boundsColl,
                    renderer.bounds.size,
                    renderer.bounds.center - colorBar.transform.position
                );
            }

            // Create highlight object
            GameObject highlightObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            highlightObj.name = "Highlight_" + colorBar.name;
            highlightObj.transform.SetParent(colorBar.transform);
            highlightObj.transform.localPosition = Vector3.zero;
            highlightObj.transform.localRotation = Quaternion.identity;

            // Make highlight object slightly larger than the color cube
            highlightObj.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);

            // Create appropriate material for highlighting
            Material highlightMat = new Material(Shader.Find("Standard"));
            highlightMat.color = new Color(1f, 1f, 1f, 0.3f);
            highlightMat.SetFloat("_Mode", 3); // Transparent mode
            highlightMat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            highlightMat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            highlightMat.SetInt("_ZWrite", 0);
            highlightMat.DisableKeyword("_ALPHATEST_ON");
            highlightMat.EnableKeyword("_ALPHABLEND_ON");
            highlightMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            highlightMat.renderQueue = 3000;

            // Apply the material
            highlightObj.GetComponent<Renderer>().material = highlightMat;

            // Initially hidden
            highlightObj.SetActive(false);

            // ColorBarButtonConfig component
            ColorBarButtonConfig buttonConfig = colorBar.AddComponent<ColorBarButtonConfig>();
            buttonConfig.buttonIndex = buttonIndex;
            buttonConfig.highlightObject = highlightObj;

            // Make it interactable
            var interactable = colorBar.AddComponent<Microsoft.MixedReality.Toolkit.UI.Interactable>();
            interactable.OnClick.AddListener(() => buttonConfig.OnButtonPressed());
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

    /// <summary>
    /// Focus handler for the title object to provide visual feedback.
    /// </summary>
    public class TitleFocusHandler : MonoBehaviour, Microsoft.MixedReality.Toolkit.Input.IMixedRealityFocusHandler
    {
        public GameObject highlightObject; // Reference to the highlight object

        public void OnFocusEnter(Microsoft.MixedReality.Toolkit.Input.FocusEventData eventData)
        {
            // Show highlight when focused
            if (highlightObject != null)
                highlightObject.SetActive(true);
        }

        public void OnFocusExit(Microsoft.MixedReality.Toolkit.Input.FocusEventData eventData)
        {
            // Hide highlight when focus is lost
            if (highlightObject != null)
                highlightObject.SetActive(false);
        }
    }

    public class ColorBarButtonConfig : MonoBehaviour, Microsoft.MixedReality.Toolkit.Input.IMixedRealityFocusHandler
    {
        public int buttonIndex { get; set; }
        public GameObject highlightObject; // Highlight object reference

        // Action delegate for method to be assigned externally
        public System.Action<int> onButtonPressedCallback;

        public void OnFocusEnter(Microsoft.MixedReality.Toolkit.Input.FocusEventData eventData)
        {
            // Show highlight when focused
            if (highlightObject != null)
                highlightObject.SetActive(true);
        }

        public void OnFocusExit(Microsoft.MixedReality.Toolkit.Input.FocusEventData eventData)
        {
            // Hide highlight when focus is lost
            if (highlightObject != null)
                highlightObject.SetActive(false);
        }

        public void OnButtonPressed()
        {
            onButtonPressedCallback?.Invoke(buttonIndex);
        }
    }
}