using System.Collections.Generic;
using UnityEngine;

public class DataMarkAxisLine
{
    private GameObject dataMarkAxisLinePrefab;
    private GameObject dataMarkAxisLineInstance;

    private Bounds visBounds;

    public DataMarkAxisLine()
    {
        dataMarkAxisLinePrefab = (GameObject)Resources.Load("Prefabs/DataMarkAxisLine");
    }

    // Gets DataMark and draws on different Positions a Line from the DataMark to the Value on the X, Y, Z Axis
    public GameObject DrawAxisLine(Transform container, Bounds bounds, DataMark dataMark)
    {
        visBounds = bounds;

        // Create DataMarkAxisLine Container
        GameObject axisLineContainer = new GameObject("Data Mark " + dataMark.GetDataMarkId() + " Line");
        axisLineContainer.transform.parent = container.transform;

        for (int i = 0; i < 4; i++)
        {
            dataMarkAxisLineInstance = GameObject.Instantiate(dataMarkAxisLinePrefab, container.position, Quaternion.identity, axisLineContainer.transform);
            dataMarkAxisLineInstance.name = i.ToString() + "_" + dataMark.GetDataMarkId().ToString();

            //Cylinder Object
            CylinderObjectVis cylinderObjectVis = new CylinderObjectVis();

            var mesh = cylinderObjectVis.CreateMesh("Data Mark Line Mesh", 0.001f, CalculateLinePoints(dataMark, i));
            dataMarkAxisLineInstance.GetComponent<MeshFilter>().mesh = mesh;
        }
        

        //var mesh02 = cylinderObjectVis.CreateMesh("Highlight Line", 0.001f, new List<Vector3>() { bottomPosition, yAxisPosition });

        // Create X LineRenderer
        //GameObject line01 = new GameObject("Line 01");
        //line01.transform.parent = highlightContainer.transform;
        //var filter01 = line01.AddComponent<MeshFilter>();
        //line01.AddComponent<MeshRenderer>();
        //filter01.mesh = mesh01;

        //LineRenderer lineRendererX = line01.AddComponent<LineRenderer>();
        //lineRendererX.SetPosition(0, bottomPosition);
        //lineRendererX.SetPosition(1, xAxisPosition);
        //lineRendererX.material = new Material(Shader.Find("Sprites/Default"));
        //lineRendererX.widthMultiplier = 0.001f;
        //lineRendererX.useWorldSpace = false;
        //lineRendererX.alignment = LineAlignment.TransformZ;

        // Create Y LineRenderer
        //GameObject line02 = new GameObject("Line 02");
        //line02.transform.parent = highlightContainer.transform;
        //var filter02 = line02.AddComponent<MeshFilter>();
        //line02.AddComponent<MeshRenderer>();
        //filter02.mesh = mesh02;

        //LineRenderer lineRendererY = line02.AddComponent<LineRenderer>(); ;
        //lineRendererY.SetPosition(0, bottomPosition);
        //lineRendererY.SetPosition(1, yAxisPosition);
        //lineRendererY.transform.parent = highlightContainer.transform;
        //lineRendererY.material = new Material(Shader.Find("Sprites/Default"));
        //lineRendererY.widthMultiplier = 0.001f;
        //lineRendererY.useWorldSpace = false;
        //lineRendererY.alignment = LineAlignment.TransformZ;

        return null;
    }

    /// <summary>
    /// Calculates the Line Points on the DataMark to the Axis
    /// </summary>
    /// <returns></returns>
    private List<Vector3> CalculateLinePoints(DataMark dataMark, int axis)
    {
        // Get Bottom, Top and Center Position of DataMark
        //TODO: Check Anchor (if top + bottom or center)
        Vector3 bottomPosition = dataMark.dataChannel.position;
        Vector3 topPosition = dataMark.dataChannel.position + new Vector3(0, dataMark.dataChannel.size.y, 0);
        Vector3 centerPosition = dataMark.GetDataMarkInstance().transform.localPosition;

        // Extension size (which goes beyond Axis)
        float extensionSize = 0.1f;

        // Always uses the left, bottom, front Axes
        Vector3 xAxisPos = bottomPosition - new Vector3(0, bottomPosition.y, 0);
        Vector3 xAxisExtensionPos = xAxisPos - new Vector3(0, 0, extensionSize);
        
        Vector3 yAxisPos1 = bottomPosition - new Vector3(bottomPosition.x, 0, 0);
        Vector3 yAxisExtensionPos1 = yAxisPos1 - new Vector3(extensionSize, 0, 0);
        
        Vector3 yAxisPos2 = topPosition - new Vector3(topPosition.x, 0, 0);
        Vector3 yAxisExtensionPos2 = yAxisPos2 - new Vector3(extensionSize, 0, 0);
        
        Vector3 zAxisPos = xAxisPos - new Vector3(bottomPosition.x, 0, 0);
        Vector3 zAxisExtensionPos = zAxisPos - new Vector3(extensionSize, 0, 0);

        if(axis == 0) return new List<Vector3>() { bottomPosition, xAxisPos, xAxisExtensionPos };
        if(axis == 1) return new List<Vector3>() { bottomPosition, yAxisPos1, yAxisExtensionPos1 };
        if(axis == 2) return new List<Vector3>() { topPosition, yAxisPos2, yAxisExtensionPos2 };
        if (axis == 3) return new List<Vector3>() { xAxisPos, zAxisPos, zAxisExtensionPos };

        return null;
    }


}
