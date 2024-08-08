using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

namespace AugmeNDT
{

    public class PopulateIndexPage : MonoBehaviour
    {
        public GameObject datasetPrefab; // Prefab for the dataset UI element
        public Transform contentPanel; // Panel where the dataset UI elements will be instantiated
        public string folderPath; // Path to the folder containing dataset files

        // Start is called before the first frame update
        void Start()
        {
            PopulatePage(folderPath);
        }
        public void setFolderPath(string path)
        {
            folderPath = path;
        }
        void PopulatePage(string folderPath)
        {
            // Check if the folder exists
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError("Folder does not exist: " + folderPath);
                return;
            }

            // Get all files in the folder
            string[] files = Directory.GetFiles(folderPath);
            float yPos = 540f;
            // Iterate through each file
            foreach (string filePath in files)
            {
                // Extract metadata from the file name or contents
                string fileName = Path.GetFileNameWithoutExtension(filePath); // Example: DatasetName
                print(fileName);

                // Instantiate dataset UI element from prefab
                GameObject datasetObject = Instantiate(datasetPrefab, contentPanel);

                datasetPrefab.SetActive(true);

                // Access UI elements of the instantiated datasetObject
                TextMeshProUGUI nameLabel = datasetObject.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                
                if (nameLabel != null)
                {
                    // Set metadata to UI elements
                    nameLabel.text = fileName;
                }
                else
                {
                    Debug.LogError("Title does not have a TextMeshProUGUI component.");
                }
                // Update vertical position
                datasetObject.transform.localPosition = new Vector3(60f, yPos, 0f);

                // Increase vertical position for the next entry
                yPos -= 90f;

                // Move the contentPanel's position downwards to accommodate the new UI element
                RectTransform contentRectTransform = contentPanel.GetComponent<RectTransform>();
                contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, contentRectTransform.sizeDelta.y + 10f);


            }
        }

    }
}
