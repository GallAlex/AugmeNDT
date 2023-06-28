using UnityEngine;

namespace AugmeNDT{
    /// <summary>
    /// Class handles settings needed at the start of the application and acts as main class for the application
    /// </summary>
    public class MainHandler : MonoBehaviour
    {
        // SceneObjectHandler controls all, non UI, objects in the scene
        [SerializeField]
        private SceneObjectHandler sceneObjectHandler;
        // SceneUIHandler controls the UI elements and actions in the scene
        [SerializeField]
        private SceneUIHandler sceneUIHandler;


        // Called only once during the lifetime of the script instance (loading of a scene)
        void Awake()
        {
            //Check if Handlers are set
            if (sceneUIHandler == null || sceneObjectHandler == null)
            {
                Debug.LogError("SceneUIHandler or sceneObjectHandler not set!");
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
            sceneUIHandler.SetSceneObjectHandler(sceneObjectHandler);
        }

    }
}
