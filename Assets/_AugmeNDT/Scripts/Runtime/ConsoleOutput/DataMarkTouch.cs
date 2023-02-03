using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.SqlServer.Server;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DataMarkTouch : MonoBehaviour
{
    private NearInteractionTouchable touchable;
    private NearInteractionTouchableVolume touchableVolume;
    private TouchHandler touchHandler;
    
    void Awake()
    {
        touchable = this.AddComponent<NearInteractionTouchable>();
        touchHandler = this.AddComponent<TouchHandler>();
    }

    void Start()
    {
        touchable.EventsToReceive = TouchableEventType.Touch;
        touchHandler.OnTouchStarted.AddListener((e) => Debug.Log("Touch Down on Object " + this.name));
        touchHandler.OnTouchCompleted.AddListener((e) => Debug.Log("Touch Up on Object " + this.name));
    }
    
    void Update()
    {

    }

    
}
