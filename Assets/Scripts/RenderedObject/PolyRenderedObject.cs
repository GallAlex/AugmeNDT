using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;


public class PolyRenderedObject : MonoBehaviour
{
    private GameObject polyObject;

    public async Task CreateObject(GameObject container, PolyFiberData polyFiberDataset)
    {
        polyObject = Instantiate((GameObject)Resources.Load("Prefabs/PolyPrefab"));
        polyObject.transform.SetParent(container.transform);
    }

    public void CreateObject(GameObject container, PolyPoreData polyPoreDataset)
    {
        //
    }
}
