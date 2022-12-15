using UnityEngine;

public class SceneMainHandler : MonoBehaviour
{
    // SceneUIHandler controls the UI elements in the scene
    public SceneUIHandler sceneUIHandler;
    // SceneVisHandler controls the visualization of the loaded data
    public SceneVisHandler sceneVisHandler;
    // FileLoadingManager handles the loading of the data
    private FileLoadingManager fileLoadingManager;


    // Called only once during the lifetime of the script instance (loading of a scene)
    void Awake()
    {
        //Check if Handlers are set
        if (sceneUIHandler == null || sceneVisHandler == null)
        {
            Debug.LogError("SceneUIHandler or SceneVisHandler not set!");
        }

        Initialize();
    }

    // Initialize and assign the classes to the handlers
    public void Initialize()
    {
        // Initialize the FileLoadingManager
        fileLoadingManager = new FileLoadingManager();
        
        // Assign the FileLoadingManager to the SceneUIHandler
        //sceneUIHandler.fileLoadingManager = fileLoadingManager;
    }

}
