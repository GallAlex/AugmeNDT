using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace AugmeNDT
{
    public class OpenBook : MonoBehaviour
    {
        [SerializeField] Button openBtn;
        [SerializeField] Button closebtn;

        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip openBook;

        [SerializeField] GameObject openedBook;
        [SerializeField] GameObject insideBackCover;

        private bool isCloseClicked;
        private Vector3 rotationVector;
        private bool isOpenClicked;
        private DateTime startTime;
        private DateTime endTime;
        // Start is called before the first frame update
        void Start()
        {
            if (openBtn != null)
            {
                openBtn.onClick.AddListener(() => openBtn_Click());
            }
            AppEvent.CloseBook += new EventHandler(closeBook_Click);
        }

        // Update is called once per frame
        void Update()
        {
            if (isOpenClicked || isCloseClicked)
            {

                transform.Rotate(rotationVector * Time.deltaTime);
                endTime = DateTime.Now;

                if (isOpenClicked)
                {
                    if ((endTime - startTime).TotalSeconds >= 1)
                    {
                        isOpenClicked = false;
                        gameObject.SetActive(false);
                        insideBackCover.SetActive(false);
                        openedBook.SetActive(true);
                    }
                }
                if (isCloseClicked)
                {
                    if ((endTime - startTime).TotalSeconds >= 1)
                    {
                        isCloseClicked = false;
                         
                    }

                }
               
            }
        }

        private void openBtn_Click()
        {
            isOpenClicked = true;
            startTime = DateTime.Now;
            rotationVector = new Vector3(0, 180, 0);
            PlaySound();

        }

     
        private void closeBook_Click(object sender, EventArgs e)
        {
            print("close book clicked in open book");

            gameObject.SetActive(true);
            openedBook.SetActive(false);
            insideBackCover.SetActive(true);
            isCloseClicked = true;
            startTime = DateTime.Now;
            rotationVector = new Vector3(0, -180, 0);
            PlaySound();

       
        }
        private void PlaySound()
        {
            if ((audioSource != null) && (openBook != null))
            {
                audioSource.PlayOneShot(openBook);
            }
        }
    }
}
