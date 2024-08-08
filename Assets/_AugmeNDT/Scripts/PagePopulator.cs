using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

namespace AugmeNDT
{
    public class PagePopulator : MonoBehaviour
    {

        public GameObject pagePrefab;
         // Prefab for the page layout
        public Transform contentPanel; // Panel where the pages will be instantiated
        private string FPath;
        private List<GameObject> pages = new List<GameObject>(); // List to store the created pages



        public void PopulatePages(string folderPath)
        {
            FPath = folderPath;
            // Check if the folder exists
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError("Folder does not exist: pagepopulator " + folderPath);
                return;
            }

            // Get all files in the folder
            string[] files = Directory.GetFiles(folderPath);

            // Iterate through each file and create a page for it
            foreach (string filePath in files)
            {

                // Instantiate page prefab
                GameObject page = Instantiate(pagePrefab, contentPanel);
                pages.Add(page); // Add the page to the list

                if (page == null)
                {
                    Debug.LogError("Failed to instantiate page prefab.");
                    continue;
                }

                pages.Add(page);
            }
        }


      
        public List<GameObject> GetPages()
            {
                return pages;
            }


        public void GetFileInfoAtIndex(int index)
        {
            // Get all files in the folder
            string[] files = Directory.GetFiles(FPath);

            Debug.LogError("Getfileinfoatindex");
            FileInfo fileInfo = new FileInfo(files[index]);
            string fileName = Path.GetFileName(files[index]);
            string fileType = Path.GetExtension(files[index]);
            string lastModified = fileInfo.LastWriteTime.ToString();
            string fileSize = fileInfo.Length.ToString();

            GameObject page = pages[index];
            TextMeshProUGUI fileNameText = page.transform.Find("Canvas/DynamicElements/Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI fileTypeText = page.transform.Find("Canvas/DynamicElements/Type").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI lastModifiedText = page.transform.Find("Canvas/DynamicElements/LastModified").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI fileSizeText = page.transform.Find("Canvas/DynamicElements/Size").GetComponent<TextMeshProUGUI>();
                    
                    // Assign the retrieved values to the UI elements                    {
                    
            fileNameText.text = fileName; // Setting the fileName to the UI element
    
            fileTypeText.text = fileType; // Setting the fileType to the UI element
                    
            lastModifiedText.text = lastModified; // Setting the lastModified to the UI element
                   
            fileSizeText.text = fileSize; // Setting the fileSize to the UI element
                    

               
               
            

        }  
    }
}



