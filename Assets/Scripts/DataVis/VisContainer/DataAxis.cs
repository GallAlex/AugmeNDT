using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public enum Direction
{
    X, // From 0 to +x
    Y, // From 0 to +y 
    Z, // From 0 to +z 
}

public class DataAxis : MonoBehaviour
{
    [SerializeField]
    private GameObject axisLinePrefab;

    [SerializeField]
    private GameObject axisInstance;
    [SerializeField]
    private AxisTicks axisTicks;
    [SerializeField]
    private TextMesh axisLabel;

    private float tickLabelOffset = 0.03f;
    private Direction axisDirection = Direction.X;



    public GameObject CreateAxis(Transform visContainer, string axisTitle, Direction direction, int numberOfTicks)
    {
        axisLinePrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Axis");

        axisInstance = Instantiate(axisLinePrefab, visContainer.position, Quaternion.identity, visContainer);
        //axisInstance.transform.Translate(0, pos - GetLength() / 2.0f, 0);

        //axisLabel = axisInstance.GetComponent<TextMesh>();
        //TODO: Add as reference?
        axisLabel = axisInstance.GetComponentInChildren<TextMesh>();
        axisLabel.text = axisTitle;

        CreateAxisTicks(axisInstance.transform, numberOfTicks);

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

                float currentYPos = axisTicks.ticks.transform.position.y;
                Debug.Log("y coord is " + currentYPos);
                //axisTicks.ticks.transform.Translate(-currentYPos * 2, 0, 0);
            break;
            case Direction.Z:
                //axis.transform.Translate(0, 0, 1);  //Translate 1 in z
                axis.transform.Rotate(0, -90, 0);    //Rotate 90 degree in y
                axisTicks.ticks.transform.localRotation = Quaternion.Euler(0, -180.0f, 0); ;
                break;
            default:
                //Use X
                break;
        }
    }

    private AxisTicks CreateAxisTicks(Transform axisTransform, int numberOfTicks)
    {
        axisTicks = new AxisTicks();
        axisTicks.CreateTicks(axisTransform, numberOfTicks);

        return axisTicks;
    }


}
