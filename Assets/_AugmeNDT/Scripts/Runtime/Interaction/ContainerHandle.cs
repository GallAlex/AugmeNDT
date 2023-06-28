using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    public class ContainerHandle : MonoBehaviour
    {
        public GameObject handle;
        public List<TextMesh> sideText;

        public void Remove()
        {
            sideText = null;
            Destroy(handle);
        }

        public void SetSideText(string text)
        {
            foreach (TextMesh textMesh in sideText)
            {
                textMesh.text = text;
            }
        }

    }
}
