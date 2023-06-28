using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace AugmeNDT{
    public class MDDLegend : MonoBehaviour
    {
        public TextMesh xtitle;
        public TextMesh ytitle;
        public List<TextMesh> xRangeLabels;
        public List<TextMesh> yRangeLabels;

        public List<GameObject> colorBars;  // Ordered from top to bottom and left to right 

        private MDDGlyphColorLegend mddGlyphColorLegend;

        public void SetMDDGlyphColorLegend(MDDGlyphColorLegend mddGlyphColorLegend)
        {
            this.mddGlyphColorLegend = mddGlyphColorLegend;
        }

        public void SetColorBarInteraction()
        {
            for (int i = 0; i < colorBars.Count; i++)
            {
                var id = i;
                var onTouchReceiver = colorBars[id].GetComponent<Interactable>().AddReceiver<InteractableOnTouchReceiver>();
                onTouchReceiver.OnTouchStart.AddListener(() => mddGlyphColorLegend.ChangeView(id));
            }

        }

        public void SetXRangeLabels(string min, string center, string max)
        {
            xRangeLabels[0].text = min;
            xRangeLabels[1].text = center;
            xRangeLabels[2].text = max;
        }

        public void SetYRangeLabels(string min, string center, string max)
        {
            yRangeLabels[0].text = min;
            yRangeLabels[1].text = center;
            yRangeLabels[2].text = max;
        }

        public void SetXTitle(string title)
        {
            xtitle.text = title;
        }

        public void SetYTitle(string title)
        {
            ytitle.text = title;
        }

    }
}
