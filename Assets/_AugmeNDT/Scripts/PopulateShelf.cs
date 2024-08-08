using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

namespace AugmeNDT
{
    public class ShelfPopulator : MonoBehaviour
    {
        [SerializeField]
        private GameObject bookPrefab; // The book prefab to instantiate

        [SerializeField]
        private Transform[] slots; // Array to hold slot transforms

        public string folderPath= "C:/Users/anugu/Downloads/Main"; // Path to the folder containing subfolders

        void Start()
        {
            PopulateShelf();
            Debug.Log("Hi start is being callec");
        }

        void PopulateShelf()
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError("Directory does not exist: " + folderPath);
                return;
            }

            string[] folders = Directory.GetDirectories(folderPath);

            for (int i = 0; i < folders.Length && i < slots.Length; i++)
            {
                GameObject book = Instantiate(bookPrefab, slots[i].position, slots[i].rotation, slots[i]);
                // Optional: Set the name of the book to the folder name
                // Reset local scale to match the slot's scale
                book.transform.localScale = Vector3.one;
                book.name = Path.GetFileName(folders[i]);



                // Find the Text component at a specific path in the book prefab
                TextMeshPro bookText = book.transform.Find("BookName").GetComponent<TextMeshPro>(); // Change this to the actual p



                if (bookText != null)
                {
                    bookText.text = Path.GetFileName(folders[i]);
                }
                else
                {
                    Debug.LogError("Text component not found at the specified path in book prefab.");
                }
                //Assign the folder path to the BookClickHandler
               BookClickHandler clickHandler = book.GetComponent<BookClickHandler>();
                if (clickHandler != null)
                {
                    clickHandler.folderPath = folders[i];
                    Debug.Log("Set folder path: " + folders[i]);
                }
                else
                {
                    Debug.LogError("BookClickHandler component not found on the book object.");
                }

            }
        }
    }
}

