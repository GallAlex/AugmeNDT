using TMPro;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;

namespace AugmeNDT
{
    
    public class ContainerHandle : MonoBehaviour
    {
        public GameObject handle;
        public BoundsControl boundsControl;
        public List<TextMeshPro> sideText;

        void Start()
        {
            //Set the bounds control to the container (parent object)
            boundsControl.Target = this.transform;
        }

        public void Remove()
        {
            sideText = null;
            Destroy(handle);
        }

        public void SetSideText(string text)
        {
            foreach (TextMeshPro textMesh in sideText)
            {
                textMesh.text = text;
            }
        }

    }
}
