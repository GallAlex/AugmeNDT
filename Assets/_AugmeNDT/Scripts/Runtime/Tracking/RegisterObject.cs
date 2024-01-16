using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

                foreach (var path in GatherPath())
                {
                    await sceneObjectHandler.LoadPreSelectedObject(path);
                }

                GameObject registeObject = GameObject.Find("DataVisGroup_0").transform.Find("FiberModel").gameObject;
                registeObject.transform.position = Vector3.zero;
                
                registeObject.transform.parent = registeredObjectContainer.transform;
                loaded = true; 
            }
        }

        private List<string> GatherPath()
        {
            string mainFolder = "";
            List<string> filePaths;

#if !UNITY_EDITOR && UNITY_WSA_10_0
            mainFolder = "C:\\Data\\Users\\alexander.gall@chello.at\\Pictures\\Datasets\\4D_FiberData\\";
            //var picturesPath = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            //path = picturesPath.SaveFolder.Path + "\\Datasets\\4D_FiberData\\10min_01.csv";

            filePaths = new List<string>(){
                mainFolder + "10min_01.csv",
                //mainFolder + "5-Segmented.mhd",
                //mainFolder + "5-Segmented.csv",
            };
            
#endif
#if UNITY_EDITOR
            mainFolder = "D:\\TestData\\DemoData_Hololens\\4D_FiberData\\";

            filePaths = new List<string>(){
                mainFolder + "10min_01.csv",
                //mainFolder + "5-Segmented.mhd",
                //mainFolder + "5-Segmented.csv",
            };
#endif

            return filePaths;
        }

    }
}
