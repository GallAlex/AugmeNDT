// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT{
    public class HandlerUtility : MonoBehaviour
    {
        public GameObject container;
        public List<TextMesh> textMeshes;
        public string handlerName;

        void Start()
        {
            //Get name of Dataset for Object
            handlerName = container.name;
        }

        private void SetHandlerText()
        {
            // Run trough all TextMeshes and set the text
            foreach (TextMesh textMesh in textMeshes)
            {
                textMesh.text = handlerName;
            }
        }

    }
}
