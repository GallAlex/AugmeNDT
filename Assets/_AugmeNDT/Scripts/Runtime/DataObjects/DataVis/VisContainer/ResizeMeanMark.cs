using UnityEngine;

namespace AugmeNDT{
    public class ResizeMeanMark : MonoBehaviour
    {

        public GameObject barMark;
        public Renderer barMarkRenderer;
    
    
        // Update is called once per frame
        void Update()
        {
            float medianMarkYSize = this.transform.localScale.y;
            float barMarkYSize = barMark.transform.localScale.y;

            if (medianMarkYSize > barMarkYSize)
            {
                this.transform.localScale = new Vector3(this.transform.localScale.x, barMarkYSize / 2.0f,
                    this.transform.localScale.z);
            }
            else
            {
                this.transform.localScale = new Vector3(this.transform.localScale.x, 0.5f,
                    this.transform.localScale.z);
            }

            //Make color of barMark for MeanMark darker
            Color barMarkColor = barMarkRenderer.material.color;
            this.GetComponent<Renderer>().material.color = new Color(barMarkColor.r / 4.0f, barMarkColor.g / 4.0f, barMarkColor.b / 4.0f, barMarkColor.a);
        }
    }
}
