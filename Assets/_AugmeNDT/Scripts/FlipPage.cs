using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;       
using System;

namespace AugmeNDT

{
    public class FlipPage : MonoBehaviour
    {
        private PagePopulator pagePopulator;

        public void SetPagePopulator(PagePopulator populator)
        {
            pagePopulator = populator;
        }

        private enum ButtonType
        {
            forwardBtn1,
            forwardBtn,
            prevBtn
        }
        [SerializeField] Button ForwardButton;
        [SerializeField] Button ForwardButton1;
        [SerializeField] Button PrevButton;
        [SerializeField] Button CloseButton;

        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip flipPage;
        [SerializeField] GameObject indexPage;



        private int currentPageIndex = -1;
        private List<GameObject> pages = new List<GameObject>();

        private Vector3 rotationVector;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private bool isClicked;


        private DateTime startTime;
        private DateTime endTime;
        // Start is called before the first frame update
        void Start()
        {
            pagePopulator = GetComponent<PagePopulator>();

            if (pagePopulator == null)
            {
                Debug.LogError("PagePopulator component not found!");
                return;
            }
            pagePopulator.PopulatePages();
            pages = pagePopulator.GetPages();
            //if (pages.Count == 0)
            //{
            //    Debug.LogError("No pages found!");
            //    return;
            //}

            startRotation = transform.rotation;
            startPosition = transform.position;

            if (ForwardButton != null)
            {
                ForwardButton.onClick.AddListener(() => turnOnePage_Click(ButtonType.forwardBtn));
            }
            if (ForwardButton1 != null)
            {
                ForwardButton1.onClick.AddListener(() => turnOnePage_Click(ButtonType.forwardBtn));
            }
            else
            {
                Debug.Log("forward button null");
            }
            if (PrevButton != null)
            {
                PrevButton.onClick.AddListener(() => turnOnePage_Click(ButtonType.prevBtn));
            }

            if (CloseButton != null)
            {
                CloseButton.onClick.AddListener(() => CloseBookBtn_Click());
            }
            ShowCurrentPage();
        }

        // Update is called once per frame
        void Update()
        {
            if (isClicked)
            {
                transform.Rotate(rotationVector * Time.deltaTime);
                endTime = DateTime.Now;

                if ((endTime - startTime).TotalSeconds >= 1)
                {
                    isClicked = false;
                    transform.rotation = startRotation;
                    transform.position = startPosition;

                    ShowCurrentPage();
                }
            }
        }
        private void turnOnePage_Click(ButtonType type)
        {

            //if(currentPageIndex <= -1)
            //{
            //    PrevButton.gameObject.SetActive(false);
            //}

            if (type == ButtonType.forwardBtn && currentPageIndex < pages.Count - 1)
            {
                currentPageIndex++;
            }
            else if (type == ButtonType.prevBtn && currentPageIndex > -1)
            {
                currentPageIndex--;
            }
           

            isClicked = true;
            startTime = DateTime.Now;

            if (type == ButtonType.forwardBtn)
            {
                Vector3 newRotation = new Vector3(startRotation.x, 180, startRotation.z);
                transform.rotation = Quaternion.Euler(newRotation);

                rotationVector = new Vector3(0, 180, 0);
            }
            else if (type == ButtonType.prevBtn)
            {
               
                rotationVector = new Vector3(0, -180, 0);

            }

            PlaySound();
        }

        public void ShowCurrentPage()
        {
            
            Debug.Log($"Current Page Index: {currentPageIndex}");
            //Hide all pages
            foreach (var page in pages)
            {
                page.SetActive(false);
            }

            // Hide the index page
            indexPage.SetActive(false);

            if (currentPageIndex == -1)
            {
                indexPage.SetActive(true);
            }

            // Show only the current page
            else if (currentPageIndex >= 0 && currentPageIndex < pages.Count)
            {
                pages[currentPageIndex].SetActive(true);

                PagePopulator pagePopulator = GetComponent<PagePopulator>(); 
                if(pagePopulator== null)
                {
                    Debug.Log("null");
                }
                // Ensure ForwardButton1 is assigned the correct listener and is visible
                //Button forwardButton1 = pages[currentPageIndex].transform.Find("Canvas/DynamicElements/ForwardButton1")?.GetComponent<Button>();
                //if (forwardButton1 != null)
                //{
                //    forwardButton1.onClick.RemoveAllListeners(); // Clear existing listeners to avoid duplication
                //    forwardButton1.onClick.AddListener(() => turnOnePage_Click(ButtonType.forwardBtn));

                //    // Bring ForwardButton1 to the front
                //    forwardButton1.transform.SetAsLastSibling();

                //    // Add debug log to verify the button is being found
                //    Debug.Log("ForwardButton1 found and listener assigned.");
                //}
                //else
                //{
                //    Debug.LogWarning("ForwardButton1 not found on current page.");
                //}


                pagePopulator.GetFileInfoAtIndex(currentPageIndex);

            }
        }
        private void PlaySound()
        {
            if((audioSource != null) && flipPage != null)
            {
                audioSource.PlayOneShot(flipPage);

            }
        }

        private void CloseBookBtn_Click()
        {
            AppEvent.CloseBookFunction();
        }
    
    }

}
