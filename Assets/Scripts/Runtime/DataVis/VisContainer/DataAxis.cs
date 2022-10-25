using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static Vis;

public enum Direction
{
    X, // From 0 to +x
    Y, // From 0 to +y 
    Z, // From 0 to +z 
}

public class DataAxis
{
    [SerializeField]
    private GameObject axisLinePrefab;

    [SerializeField]
    private GameObject axisInstance;
    [SerializeField]
    private Scale dataScale;
    [SerializeField]
    private AxisTicks axisTicks;
    [SerializeField]
    private TextMesh axisLabel;

    private float tickLabelOffset = 0.03f;
    private Direction axisDirection = Direction.X;



    public GameObject CreateAxis(Transform visContainer, string axisTitle, Direction direction, Scale dataScale, int numberOfTicks)
    {
        axisLinePrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Axis");

        axisInstance = GameObject.Instantiate(axisLinePrefab, visContainer.position, Quaternion.identity, visContainer);

        //axisLabel = axisInstance.GetComponent<TextMesh>();
        //TODO: Add as reference?
        axisLabel = axisInstance.GetComponentInChildren<TextMesh>();
        axisLabel.text = axisTitle;

        this.dataScale = dataScale;
        CreateAxisTicks(axisInstance.transform, dataScale, numberOfTicks);

        //Rotate after Tick generation
        axisDirection = direction;
        axisInstance.name = axisDirection.ToString() + " Axis";
        ChangeAxisDirection(axisInstance, direction);

        return axisInstance;
    }

    //TODO: rotate Text
    //TODO: Show Axis on closer side and grid on side further away
    private void ChangeAxisDirection(GameObject axis, Direction dir)
    {
        switch (dir)
        {
            case Direction.X:
                //Nothing To do
            break;

            case Direction.Y:
                axis.transform.Rotate(0, 0, 90);    //Rotate 90 degree in z
                Transform axisTitle = axis.transform.Find("Title");
                axisTitle.localPosition = new Vector3(axisTitle.localPosition.x, (-axisTitle.localPosition.y), axisTitle.localPosition.z);
                
                foreach (var tickObject in axisTicks.tickList)
                {
                    Transform tickLabel = tickObject.transform.Find("TickLabel");
                    tickLabel.localPosition = new Vector3(tickLabel.localPosition.x, (- tickLabel.localPosition.y), tickLabel.localPosition.z);
                }
                break;

            case Direction.Z:
                axis.transform.Rotate(0, -90, 0);    //Rotate 90 degree in y
                axis.transform.Find("Title").Rotate(0, 180, 0);

                foreach (var tickObject in axisTicks.tickList)
                {
                    Transform tickLabel = tickObject.transform.Find("TickLabel");
                    tickLabel.Rotate(0, 180, 0);
                }
                break;

            default:
                //Use X
                break;
        }
    }

    private AxisTicks CreateAxisTicks(Transform axisTransform, Scale dataScale, int numberOfTicks)
    {
        axisTicks = new AxisTicks();
        axisTicks.CreateTicks(axisTransform, dataScale, numberOfTicks);

        return axisTicks;
    }


}
