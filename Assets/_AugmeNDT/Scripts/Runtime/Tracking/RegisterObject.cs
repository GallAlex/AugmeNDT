using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using Windows.Storage;
#endif

namespace AugmeNDT
{
    public class RegisterObject : MonoBehaviour
    {
        public GameObject registeredObjectContainer;
        public bool loaded = false;

        // Handler
        public SceneObjectHandler sceneObjectHandler;
        public SceneUIHandler sceneUIHandler;

        public async void LoadObject()
        {
            if (!loaded)
            {

                string path = "";
#if !UNITY_EDITOR && UNITY_WSA_10_0
            path = "C:\\Data\\Users\\alexander.gall@chello.at\\Pictures\\Datasets\\4D_FiberData\\10min_01.csv";
            //var picturesPath = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            //path = picturesPath.SaveFolder.Path + "\\Datasets\\4D_FiberData\\10min_01.csv";
            
#endif
#if UNITY_EDITOR
            path = "D:\\TestData\\DemoData_Hololens\\4D_FiberData\\10min_01.csv";
#endif
            await sceneObjectHandler.LoadPreSelectedObject(path);
            

            GameObject registeObject = GameObject.Find("DataVisGroup_0");
            registeredObjectContainer.transform.position = Vector3.zero;

            registeObject.transform.parent = registeredObjectContainer.transform;
            loaded = true; 
            }
        }

    }
}
