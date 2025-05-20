// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AugmeNDT{
    using MixedReality.Toolkit;
    using MixedReality.Toolkit.UX;
    using UnityEngine.XR.Interaction.Toolkit;

    /// <summary>
    /// Class handles the interaction with the MDDLegend and sets the text of the legend
    /// </summary>
    public class MDDLegendInteractable : MonoBehaviour
    {
        public TextMeshPro xtitle;
        public TextMeshPro ytitle;
        public List<TextMeshPro> xRangeLabels;
        public List<TextMeshPro> yRangeLabels;

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
                // TODO: MRTK3 Rework
                var id = i;
                var onTouchReceiver = colorBars[id].GetComponent<PressableButton>();
                onTouchReceiver.selectEntered.AddListener((onTouch) => mddGlyphColorLegend.ChangeView(id));

                //onTouchReceiver.OnTouchStart.AddListener(() => mddGlyphColorLegend.ChangeView(id));
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
