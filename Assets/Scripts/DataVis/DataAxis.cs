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
    private AxisTicks axisTicks;
    [SerializeField]
    private TextMesh axisLabel;

    private Direction axisDirection = Direction.X;



    public GameObject CreateAxis(Transform visContainer, string axisTitle, Direction direction)
    {
        axisLinePrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Axis");

        GameObject axisInstance = Instantiate(axisLinePrefab, visContainer.position, Quaternion.identity, visContainer);
        //axisInstance.transform.Translate(0, pos - GetLength() / 2.0f, 0);

        axisDirection = direction;
        ChangeAxisDirection(axisInstance, direction);

        //axisLabel = axisInstance.GetComponent<TextMesh>();
        //TODO: Add as reference?
        axisLabel = axisInstance.GetComponentInChildren<TextMesh>();
        axisLabel.text = axisTitle;

        axisInstance.name = axisDirection.ToString() + " Axis";

        CreateAxisTicks(axisInstance.transform);

        return axisInstance;
    }

    //TODO: rotate Text
    private void ChangeAxisDirection(GameObject axis, Direction dir)
    {
        switch (dir)
        {
            case Direction.X:
                //Nothing To do
            break;
            case Direction.Y:
                axis.transform.Rotate(0, 0, 90);    //Rotate 90 degree in z
            break;
            case Direction.Z:
                //axis.transform.Translate(0, 0, 1);  //Translate 1 in z
                axis.transform.Rotate(0, -90, 0);    //Rotate 90 degree in y
                break;
            default:
                //Use X
                break;
        }
    }

    private AxisTicks CreateAxisTicks(Transform axisTransform)
    {
        AxisTicks ticks = new AxisTicks();
        ticks.CreateTicks(axisTransform, 5);

        return ticks;
    }

}
