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
            string mainFolder = "D:\\TestData\\DemoData_Hololens\\4D_FiberData\\";
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

            sceneUIHandler.ChangeVisTicks();

            Debug.Log("Finished loading " + filePaths.Count + " Files");
        }

        private async Task Scenario1()
        {
            string mainFolder = "D:\\TestData\\DemoData_Hololens\\FCP_Daten_Gall_sGFCR0\\";
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

            sceneUIHandler.ChangeVisTicks();

            Debug.Log("Finished loading " + filePaths.Count + " Files");
        }

    }
}
