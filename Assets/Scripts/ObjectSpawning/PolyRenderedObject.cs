using UnityEngine;


public class PolyRenderedObject : MonoBehaviour
{
    private GameObject polyVolume;

    public GameObject CreateObject(IPolygonDataset polyDataset)
    {
        polyVolume = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        return polyVolume;
    }
}
