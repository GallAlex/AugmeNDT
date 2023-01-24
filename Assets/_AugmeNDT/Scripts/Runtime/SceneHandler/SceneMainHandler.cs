using UnityEngine;

/// <summary>
/// Class handles settings needed at the start of the application and acts as main class for the application
/// </summary>
public class SceneMainHandler : MonoBehaviour
{
    // SceneUIHandler controls the UI elements in the scene
    [SerializeField]
    private SceneUIHandler sceneUIHandler;
    // SceneFileHandler controls the loaded files
    [SerializeField]
    private SceneFileHandler sceneFileHandler;
    // SceneVisHandler controls the visualization of the loaded data
    [SerializeField]
    private SceneVisHandler sceneVisHandler;


    // Called only once during the lifetime of the script instance (loading of a scene)
    void Awake()
    {
        //Check if Handlers are set
        if (sceneUIHandler == null || sceneVisHandler == null || sceneFileHandler == null)
        {
            Debug.LogError("SceneUIHandler, SceneFileHandler or SceneVisHandler not set!");
        }

        Initialize();
    }

    void Start()
    {
        Debug.Log("DeviceName: " + SystemInfo.deviceName);
        Debug.Log("GraphicsMemorySize: " + (double)SystemInfo.graphicsMemorySize + " MB");
        Debug.Log("MaxTextureSize: " + (double)SystemInfo.maxTextureSize);
        Debug.Log("MaxGraphicsBufferSize: " + (double)SystemInfo.maxGraphicsBufferSize / 1024.0f / 1024.0f + " MB");
    }
    
    // Initialize and assign the classes to the handlers
    public void Initialize()
    {
        sceneVisHandler.SetSceneFileHandler(sceneFileHandler);
        sceneUIHandler.SetSceneFileHandler(sceneFileHandler);
        sceneUIHandler.SetSceneVisHandler(sceneVisHandler);
    }

}
