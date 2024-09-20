using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace AugmeNDT
{
    public class BookTransition : MonoBehaviour
    {
        public GameObject closedBook;    // The 3D closed book object
        public GameObject openBook2D;    // The 2D open book object
        public Button openButton;        // Button to trigger the opening
       
        

        // Update is called once per frame
        void Open3dBook()
        {
            closedBook.SetActive(false);

            //Position the 2D book in the same place as the 3D book
            openBook2D.transform.position = closedBook.transform.position;
            openBook2D.transform.rotation = closedBook.transform.rotation;

            // Show the open 2D book
            openBook2D.SetActive(true);
        }
    }
}
