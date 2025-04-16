using UnityEngine;
using TMPro;

namespace AugmeNDT
{
    /// <summary>
    /// Handles the properties of one color segment of the color bar
    /// </summary>
    public class ColorSegment : MonoBehaviour
    {
        public GameObject colorBar;     // Colored 3d object

        public GameObject[] ticks;      // Tick marks (Upper, Center, Lower)
        public TextMeshPro[] tickText;     // Tick text (Upper, Center, Lower)

        private float xOffset = 0.05f;        // Offset for the tick

        /// <summary>
        /// Enables the ticks and sets its text.
        /// </summary>
        /// <param name="tickID"></param>
        /// <param name="tickText"></param>
        public void CreateTick(int tickID, string tickText)
        {
            ticks[tickID].SetActive(true);
            this.tickText[tickID].text = tickText;
        }

        /// <summary>
        /// Method sets the scale of the color bar and adjust all its components.
        /// Newly alignes the ticks to the new scale.
        /// </summary>
        /// <param name="scale"></param>
        public void SetColorBarScale(Vector3 scale)
        {
            foreach (var tick in ticks)
            {
                if (!tick.activeSelf) continue;

                //TODO: When Bar is scaled align the the tick x,y,z position to the new scale
                //TODO: LegendColorBar should also be able to used Color calc with midValue!!
                //TODO: Replace colorBar in LegendColorBar with new ColorSegment
                //ERROR
            }


        }

    }
}
