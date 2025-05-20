// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Class handles evaluation scenarios
    /// </summary>
    public class EvaluationHandler : MonoBehaviour
    {
        public bool isEvaluation = false;
        public int scenarioID = 0;

        // Handler
        public SceneObjectHandler sceneObjectHandler;
        public SceneUIHandler sceneUIHandler;

        public GameObject dataLoadingMenu;

        // Start is called before the first frame update
        void Start()
        {
            if (!isEvaluation) return;

            LoadScenario(scenarioID);

        }

        private void LoadScenario(int scenarioID)
        {
            dataLoadingMenu.SetActive(false);

            switch (scenarioID)
            {
                case 0:
                    Scenario0();
                    break;
                case 1:
                    Scenario1();
                    break;
            }
        }

        private async Task Scenario0()
        {
            string mainFolder = GetPlatformDependentMainPath() + "\\4D_FiberData\\";
            List<string> filePaths = new List<string>(){
                mainFolder + "10min_01.csv",
                mainFolder + "60min_01.csv",
                mainFolder + "10min_02.csv",
                mainFolder + "60min_02.csv",
            };

            foreach (var path in filePaths)
            {
                await sceneObjectHandler.LoadPreSelectedObject(path);

            }

            sceneUIHandler.Create4DVis();

            Debug.Log("Finished loading " + filePaths.Count + " Files");
        }

        private async Task Scenario1()
        {
            string mainFolder = GetPlatformDependentMainPath() + "/FCP_Daten_Gall_sGFCR0/";
            List<string> filePaths = new List<string>(){
                mainFolder + "0N.csv",
                mainFolder + "132N.csv",
                mainFolder + "228N.csv",
                mainFolder + "263N.csv",
                mainFolder + "299N.csv",
                mainFolder + "334N.csv",
                mainFolder + "369N.csv",
                mainFolder + "404N.csv",
            };

            foreach (var path in filePaths)
            {
                await sceneObjectHandler.LoadPreSelectedObject(path);

            }

            sceneUIHandler.Create4DVis();

            Debug.Log("Finished loading " + filePaths.Count + " Files");
        }

        private string GetPlatformDependentMainPath()
        {
            // Different Paths for different platforms
            #if UNITY_EDITOR
            return "D:\\TestData\\DemoData_Hololens";
            #endif
            #if !UNITY_EDITOR && UNITY_ANDROID
            return "/storage/emulated/0/Datasets/";
            #endif
            #if !UNITY_EDITOR && UNITY_WSA_10_0
            throw new System.NotImplementedException("No main path for Hololens");
            #endif
            #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            return "D:\\TestData\\DemoData_Hololens";
            #endif

            throw new System.NotImplementedException("No main path for this platform");
        }

    }
}
