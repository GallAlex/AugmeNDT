using UnityEngine;

namespace AugmeNDT{
    using System.IO;

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
            OuputDeviceInformation();
        }
    
        // Initialize and assign the classes to the handlers
        public void Initialize()
        {
            sceneUIHandler.SetSceneObjectHandler(sceneObjectHandler);
        }

        private void OuputDeviceInformation()
        {
            string deviceName = SystemInfo.deviceName;
            double graphicsMemorySizeMB = SystemInfo.graphicsMemorySize;
            int maxTextureSize = SystemInfo.maxTextureSize;
            double maxGraphicsBufferSizeMB = (double)SystemInfo.maxGraphicsBufferSize / 1024.0f / 1024.0f;
            string persistentDataPath = Application.persistentDataPath;

            string deviceInformation = $"DeviceName: {deviceName}, GraphicsMemorySize: {graphicsMemorySizeMB} MB, MaxTextureSize: {maxTextureSize}, MaxGraphicsBufferSize: {maxGraphicsBufferSizeMB:F2} MB, Application.persistentDataPath is: {persistentDataPath}";

            Debug.Log("########## Device Information ##########\n" + deviceInformation);
        }

    }
}
