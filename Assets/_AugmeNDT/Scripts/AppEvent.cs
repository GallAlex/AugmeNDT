using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace AugmeNDT
{
    public static class AppEvent
    {
        public static event EventHandler CloseBook;

        public static void CloseBookFunction()
        {
            if (CloseBook != null) {

                CloseBook(new object(), new EventArgs());
            }
            else
            {
                Debug.Log("it's null");
            }
        }
    }
}
