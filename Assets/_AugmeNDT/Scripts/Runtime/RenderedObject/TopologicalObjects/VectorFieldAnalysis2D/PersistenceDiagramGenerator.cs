using AugmeNDT;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using UnityEngine;

public class PersistenceDiagramGenerator : MonoBehaviour
{
    // Public visualization parameters exposed in the Unity Inspector
    [Header("Visualization Settings")]
    [SerializeField] private float diagramSize = 0.5f;
    [SerializeField] private Color axisColor = Color.green;
    [SerializeField] private Color diagonalColor = Color.red;
    [SerializeField] private Color markerColor = Color.green;
    [SerializeField] private Color persistenceLineColor = Color.white;
    [SerializeField] private Color labelColor = Color.white;
    [SerializeField] private float lineWidth = 0.005f;
    [SerializeField] private float axisWidth = 0.005f;
    [SerializeField] private int numAxisMarkers = 5;
    [SerializeField] private float axisMarkerSize = 0.02f;
    [SerializeField] private float labelSize = 0.02f;
    [SerializeField] private float labelOffset = 0.03f;

    private BoundsControl boundsControl;

    // Stores the list of birth-death point pairs for persistence diagram
    private List<PersistencePoint> persistencePoints = new List<PersistencePoint>();

    // Used to normalize birth and death values to fit within diagram bounds
    private float maxBirth = 100000;
    private float maxDeath = 110000;

    // Root container object to group diagram components
    private Transform container;

    void Start()
    {
        // Load persistence data from CSV file via the topological data handler
        persistencePoints = TopologicalDataObject.instance.ttkCalculation.LoadPersistenceCSV("cells.csv");

        // Generate the full persistence diagram
        CreatePersistenceDiagram();
    }

    private void CreatePersistenceDiagram()
    {
        // Create an empty GameObject to hold all diagram components
        container = new GameObject("PersistenceDiagram").transform;
        container.SetParent(transform);
        container.position = GameObject.Find("Volume").transform.position + new Vector3(1, 0, 0);
        container.localRotation = Quaternion.identity;
        container.localScale = Vector3.one;

        // Add interaction components from MRTK
        SetupBoundsControl(container.gameObject);

        float globalMax = Mathf.Max(maxBirth, maxDeath); // Normalize range

        CreateAxes(globalMax);          // Create X and Y axes
        CreateDiagonalLine();          // Create birth = death diagonal
        CreatePersistenceLines(globalMax); // Plot each persistence pair
        CreateAxisTitles();            // Add axis labels (X/Y)
    }

    private void CreateAxisTitles()
    {
        // Create and position the X axis title
        GameObject xTitle = new GameObject("X_Axis_Title");
        xTitle.transform.SetParent(container);
        xTitle.transform.localPosition = new Vector3(diagramSize / 2, -labelOffset * 2.5f, 0);

        TextMesh xTextMesh = xTitle.AddComponent<TextMesh>();
        xTextMesh.text = "Birth";
        xTextMesh.color = labelColor;
        xTextMesh.fontSize = 12;
        xTextMesh.characterSize = labelSize;
        xTextMesh.alignment = TextAlignment.Center;
        xTextMesh.anchor = TextAnchor.MiddleCenter;

        MeshRenderer meshRenderer = xTitle.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.allowOcclusionWhenDynamic = false;

        // Create and position the Y axis title
        GameObject yTitle = new GameObject("Y_Axis_Title");
        yTitle.transform.SetParent(container);
        yTitle.transform.localPosition = new Vector3(-labelOffset * 2.5f, diagramSize / 2, 0);
        yTitle.transform.localEulerAngles = new Vector3(0, 0, 90); // Rotate vertically

        TextMesh yTextMesh = yTitle.AddComponent<TextMesh>();
        yTextMesh.text = "Death";
        yTextMesh.color = labelColor;
        yTextMesh.fontSize = 12;
        yTextMesh.characterSize = labelSize;
        yTextMesh.alignment = TextAlignment.Center;
        yTextMesh.anchor = TextAnchor.MiddleCenter;

        MeshRenderer meshRendererY = yTitle.GetComponent<MeshRenderer>();
        if (meshRendererY != null)
            meshRendererY.allowOcclusionWhenDynamic = false;
    }

    private void CreateAxes(float maxValue)
    {
        // Create horizontal X axis
        GameObject xAxis = CreateLine(Vector3.zero, new Vector3(diagramSize, 0, 0), axisWidth, axisColor, "X_Axis");
        xAxis.transform.SetParent(container);

        // Create vertical Y axis
        GameObject yAxis = CreateLine(Vector3.zero, new Vector3(0, diagramSize, 0), axisWidth, axisColor, "Y_Axis");
        yAxis.transform.SetParent(container);

        // Create axis marker labels container
        GameObject labelsContainer = new GameObject("AxisLabels");
        labelsContainer.transform.SetParent(container);
        labelsContainer.transform.localPosition = Vector3.zero;
        labelsContainer.transform.localRotation = Quaternion.identity;
        labelsContainer.transform.localScale = Vector3.one;

        // Loop to create evenly spaced tick marks and labels on both axes
        for (int i = 0; i <= numAxisMarkers; i++)
        {
            float position = (float)i / numAxisMarkers * diagramSize;
            float value = (float)i / numAxisMarkers * maxValue;
            string valueText = ((int)value).ToString();

            // Create X-axis tick
            GameObject xMarker = CreateLine(
                new Vector3(position, 0, 0),
                new Vector3(position, -axisMarkerSize, 0),
                axisWidth,
                markerColor,
                $"X_Marker_{i}"
            );
            xMarker.transform.SetParent(labelsContainer.transform);

            // X-axis label
            GameObject xLabel = new GameObject($"X_Label_{i}");
            xLabel.transform.SetParent(container);
            xLabel.transform.localPosition = new Vector3(position, -labelOffset, 0);

            TextMesh xTextMesh = xLabel.AddComponent<TextMesh>();
            xTextMesh.text = valueText;
            xTextMesh.color = labelColor;
            xTextMesh.fontSize = 10;
            xTextMesh.characterSize = labelSize;
            xTextMesh.alignment = TextAlignment.Center;
            xTextMesh.anchor = TextAnchor.MiddleCenter;

            // Create Y-axis tick
            GameObject yMarker = CreateLine(
                new Vector3(0, position, 0),
                new Vector3(-axisMarkerSize, position, 0),
                axisWidth,
                markerColor,
                $"Y_Marker_{i}"
            );
            yMarker.transform.SetParent(labelsContainer.transform);

            // Y-axis label
            GameObject yLabel = new GameObject($"Y_Label_{i}");
            yLabel.transform.SetParent(container);
            yLabel.transform.localPosition = new Vector3(-labelOffset, position, 0);

            TextMesh yTextMesh = yLabel.AddComponent<TextMesh>();
            yTextMesh.text = valueText;
            yTextMesh.color = labelColor;
            yTextMesh.fontSize = 10;
            yTextMesh.characterSize = labelSize;
            yTextMesh.alignment = TextAlignment.Center;
            yTextMesh.anchor = TextAnchor.MiddleRight;
        }
    }

    // Create a red diagonal line from (0,0) to (1,1)
    private void CreateDiagonalLine()
    {
        GameObject diagonal = CreateLine(
            Vector3.zero,
            new Vector3(diagramSize, diagramSize, 0),
            lineWidth,
            diagonalColor,
            "Diagonal"
        );
        diagonal.transform.SetParent(container);
    }

    // Create container for all vertical persistence lines
    private void CreatePersistenceLines(float globalMax)
    {
        GameObject linesContainer = new GameObject("PersistenceLines");
        linesContainer.transform.SetParent(container);

        foreach (var point in persistencePoints)
        {
            // Filter invalid points
            if (point.birth > point.death || point.birth < 0)
                continue;

            // Normalize birth and death values into diagram space
            float normalizedBirth = point.birth / globalMax * diagramSize;
            float normalizedDeath = Mathf.Min(point.death / globalMax * diagramSize, diagramSize);

            // Create vertical persistence line from diagonal to death value
            GameObject line = CreateLine(
                new Vector3(normalizedBirth, normalizedBirth, 0), // start
                new Vector3(normalizedBirth, normalizedDeath, 0), // end
                lineWidth,
                persistenceLineColor,
                $"PersistenceLine_Birth{point.birth}"
            );
            line.transform.SetParent(linesContainer.transform);
        }
    }

    // Utility function to create a line renderer between two points
    private GameObject CreateLine(Vector3 start, Vector3 end, float width, Color color, string name)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(container);
        lineObj.transform.localPosition = Vector3.zero;

        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = width;
        line.endWidth = width;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = color;
        line.material = material;

        return lineObj;
    }

    // Adds MRTK interaction components to allow moving/resizing the diagram
    private void SetupBoundsControl(GameObject targetObject)
    {
        Collider existingCollider = targetObject.GetComponent<Collider>();
        if (existingCollider != null)
            DestroyImmediate(existingCollider);

        // Add a thin box collider to enable interaction
        BoxCollider boundsColl = targetObject.AddComponent<BoxCollider>();
        boundsColl.size = new Vector3(0.7f, 0.7f, 0.2f);
        boundsColl.center = new Vector3(0.2f, 0.2f, 0f);

        // Enable bounding box and manipulation behaviors
        boundsControl = targetObject.AddComponent<BoundsControl>();
        boundsControl.BoundsOverride = boundsColl;

        // Disable visual handles
        boundsControl.RotationHandlesConfig.ShowHandleForX = false;
        boundsControl.RotationHandlesConfig.ShowHandleForY = false;
        boundsControl.RotationHandlesConfig.ShowHandleForZ = false;

        boundsControl.TranslationHandlesConfig.ShowHandleForX = false;
        boundsControl.TranslationHandlesConfig.ShowHandleForY = false;
        boundsControl.TranslationHandlesConfig.ShowHandleForZ = false;

        boundsControl.enabled = false;

        // Add grabbable and manipulatable functionality
        targetObject.AddComponent<NearInteractionGrabbable>();

        ObjectManipulator manipulator = targetObject.AddComponent<ObjectManipulator>();
        manipulator.AllowFarManipulation = true;
        manipulator.OneHandRotationModeNear = ObjectManipulator.RotateInOneHandType.RotateAboutObjectCenter;
        manipulator.OneHandRotationModeFar = ObjectManipulator.RotateInOneHandType.RotateAboutObjectCenter;
        manipulator.TwoHandedManipulationType = Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Move |
                                                Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Rotate |
                                                Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Scale;
    }
}
