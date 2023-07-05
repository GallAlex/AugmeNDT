using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT{
    public class MDDGlyphColorLegend
    {
        public GameObject colorLegendInstance;
        public MDDLegend instanceScript;
        public GameObject colorLegendPrefab;
        public GameObject text3DPrefab;

        private VisMDDGlyphs mddGlyph;

        private double[] minMaxSkewness;
        private double[] minMaxKurtosis;

        // BreakPoints from https://brownmath.com/stat/shape.htm#Skewness
        // Acceptable limits of ±2 (Trochim & Donnelly, 2006; Field, 2000 & 2009; Gravetter & Wallnau, 2014)
        /* https://www.researchgate.net/post/What_is_the_acceptable_range_of_skewness_and_kurtosis_for_normal_distribution_of_data
         * Trochim, W. M., & Donnelly, J. P. (2006). The research methods knowledge base (3rd ed.). Cincinnati, OH:Atomic Dog.
         * Gravetter, F., & Wallnau, L. (2014). Essentials of statistics for the behavioral sciences (8th ed.). Belmont, CA: Wadsworth.
         * Field, A. (2000). Discovering statistics using spss for windows. London-Thousand Oaks- New Delhi: Sage publications.
         * Field, A. (2009). Discovering statistics using SPSS. London: SAGE.
         */
        // Skewness Thresholds
        private const double leftSkewedThreshold = -2.0;      // min skewness
        private const double symmetricThreshold = 0.0;
        private const double rightSkewedThreshold = 2.0;      // max skewness

        // Kurtosis Thresholds
        private const double platykurticThreshold = -2.0;     // min kurtosis
        private const double mesokurticThreshold = 0.0;
        private const double leptokurticThreshold = 2.0;      // max kurtosis

        // Threshold Offset
        private const double thresholdOffset = 0.000000;

        // Current Thresholds with defaults
        private double minSkewness = leftSkewedThreshold;      
        private double centerSkewness = symmetricThreshold;
        private double maxSkewness = rightSkewedThreshold;      

        private double minKurtosis = platykurticThreshold;     
        private double centerKurtosis = mesokurticThreshold;
        private double maxKurtosis = leptokurticThreshold;      

        bool detailedViewOn = false;

        // Color Schemes & Textures
        private List<Color[]> categoricColorSchemeList;         // Stores the color scheme for each category from left to right and the individual colors from top to bottom (9 colors)
        private List<List<Color[]>> extendedColorSchemeList;    // Stores the extended (new) color scheme based on a selected category from left to right and the individual colors from top to bottom (9 colors)
        private List<Color[]> currentColorScheme;               // The color scheme that is currently used for calculating the colors of the glyphs
        private Texture[] texturesToSet;



        public MDDGlyphColorLegend(VisMDDGlyphs mddGlyph)
        {
            text3DPrefab = (GameObject)Resources.Load("Prefabs/Text3D");
            colorLegendPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/MDDGlyphColorLegend");

            this.mddGlyph = mddGlyph;

            // Initialize the textures
            texturesToSet = new Texture[7];
            texturesToSet[0] = (Texture)Resources.Load("Icons/RightSkewed_peaky");
            texturesToSet[1] = (Texture)Resources.Load("Icons/RightSkewed_flat");
            texturesToSet[2] = (Texture)Resources.Load("Icons/Symmetric_peaky");
            texturesToSet[3] = (Texture)Resources.Load("Icons/Symmetric_normal");
            texturesToSet[4] = (Texture)Resources.Load("Icons/Symmetric_flat");
            texturesToSet[5] = (Texture)Resources.Load("Icons/LeftSkewed_peaky");
            texturesToSet[6] = (Texture)Resources.Load("Icons/LeftSkewed_flat");

            // Intialize the color schemes
            categoricColorSchemeList = new List<Color[]>() { ColorHelper.violetMultiHueValues, ColorHelper.blueMultiHueValues, ColorHelper.redMultiHueValues };
            currentColorScheme = categoricColorSchemeList;

            extendedColorSchemeList = new List<List<Color[]>>(3);
            extendedColorSchemeList.Add(new List<Color[]>() { ColorHelper.violetSingleHueValues01, ColorHelper.violetSingleHueValues02, ColorHelper.violetSingleHueValues03 });
            extendedColorSchemeList.Add(new List<Color[]>() { ColorHelper.blueSingleHueValues01, ColorHelper.blueSingleHueValues02, ColorHelper.blueSingleHueValues03 });
            extendedColorSchemeList.Add(new List<Color[]>() { ColorHelper.redSingleHueValues01, ColorHelper.redSingleHueValues02, ColorHelper.redSingleHueValues03 });

            CreateColorLegend();
        }

        private void CreateColorLegend()
        {
            detailedViewOn = false;

            colorLegendInstance = GameObject.Instantiate(colorLegendPrefab);
            instanceScript = colorLegendInstance.GetComponent<MDDLegend>();
            instanceScript.SetMDDGlyphColorLegend(this);
            instanceScript.SetColorBarInteraction();

            SetLegendColoring();
        }

        public GameObject GetColorLegend()
        {
            return colorLegendInstance;
        }

        public void SetMinMaxSkewKurtValues(double[] minMaxSkewness, double[] minMaxKurtosis)
        {
            this.minMaxSkewness = minMaxSkewness;
            this.minMaxKurtosis = minMaxKurtosis;
        }
    
        public void SetLegendColoring()
        {
            Transform colorBarContainer = colorLegendInstance.transform.Find("ColorBars");

            int currentTex = 0;
            int currentcolorBar = 0;

            foreach (Transform colorBars in colorBarContainer.transform)
            {
                int currentCube = 0;

                foreach (Transform colorCube in colorBars)
                {
                    bool hasQuads = false;
                    colorCube.GetComponent<Renderer>().material.color = currentColorScheme[currentcolorBar][currentCube];

                    foreach (Transform quad in colorCube)
                    {
                        hasQuads = true;
                        quad.GetComponent<Renderer>().material.SetTexture("_MainTex", texturesToSet[currentTex]);
                    }

                    if (hasQuads) currentTex++;
                    currentCube++;
                }

                currentcolorBar++;
            }
        }

        /// <summary>
        /// Method Calculates the resulting Color based on Skewness and Kurtosis
        /// Kurtosis:
        /// The excess kurtosis is defined as kurtosis minus 3
        /// excess kurtosis = 0: symmetric, Normal distribution
        /// excess kurtosis > 0: more prominent peaks, fatter tails - more outliers
        /// excess kurtosis < 0: more flat peaks, thinner tails - fewer outliers
        /// Skewness:
        /// skewness = 0: symmetric
        /// skewness > 0: right skewed, more Values that are smaller than the mean,
        /// skewness < 0: left skewed, more Values that are bigger than the mean,
        /// </summary>
        /// <param name="skewness"></param>
        /// <param name="kurtosis"></param>
        /// <returns></returns>
        public Color GetColoring(double skewness, double kurtosis)
        {
            // Define final color by checking kurtosis Value (Error Value Green)
            Color finalColor = new Color(0, 1.0f, 0);

            if (detailedViewOn)
            {
                finalColor = GetDetailedColoring(skewness, kurtosis);
            }
            else
            {
                finalColor = GetCategoricColoring(skewness, kurtosis);
            }


            return finalColor;
        }

        public void ChangeView(int colorBar)
        {
            if (detailedViewOn)
            {
                SetCategoricView(colorBar);
            }
            else
            {
                SetDetailedView(colorBar);
            }

            //Trigger the change in the MDDGlyphs
            mddGlyph.CreateMDDGlyphColors(this);
            mddGlyph.ChangeDataMarks();
        }

        private Color GetCategoricColoring(double skewness, double kurtosis)
        {

            // Indices for the color scheme
            int skewnessColorId = -1;
            int kurtosisColorId = -1;

            // We take a value between -1 and 1 as we have 3 colors/categories (-1,0,1)
            if (kurtosis <= minKurtosis)
            {
                kurtosisColorId = 2;
            }
            else if (kurtosis < maxKurtosis && kurtosis > minKurtosis)
            {
                kurtosisColorId = 1;
            }
            else if (kurtosis >= maxKurtosis)
            {
                kurtosisColorId = 0;
            }


            // Skewness
            if (skewness <= minSkewness)
            {
                skewnessColorId = 2;
            }
            else if (skewness < maxSkewness && skewness > minSkewness)
            {
                skewnessColorId = 1;
            }
            else if (skewness >= maxSkewness)
            {
                skewnessColorId = 0;
            }


            return currentColorScheme[skewnessColorId][kurtosisColorId];
        }

        private Color GetDetailedColoring(double skewness, double kurtosis)
        {

            // If detailed View is on, we only show the glyphs that are within the range of the selected skewness and kurtosis
            if (!(skewness <= maxSkewness && skewness >= minSkewness))
            {
                //Debug.Log(skewness + " < " + maxSkewness + " && " + skewness + " >= " + minSkewness);
                return new Color(0, 1.0f, 0);
            }

            if (!(kurtosis <= maxKurtosis && kurtosis >= minKurtosis))
            {
                //Debug.Log(kurtosis + " < " + maxKurtosis + " && " + kurtosis + " >= " + minKurtosis);
                return new Color(0, 1.0f, 0);
            }
            //TODO: Is the range correct?

            // Note: We have to invert the color scheme (switch min with max)
            int skewnesIndex = ScaleColor.GetCategoricalColorIndex(skewness, maxSkewness, minSkewness, currentColorScheme.Count);
            int kurtosisIndex = ScaleColor.GetCategoricalColorIndex(kurtosis, maxKurtosis, minKurtosis, currentColorScheme[skewnesIndex].Length);


            //Debug.Log("Skewness colorIndex: " + colorIndex + " | ratio: " + ratio + " | skewness: " + skewness + " | minSkewness: " + minSkewness + " | maxSkewness: " + maxSkewness);

            //var invertedList = currentColorScheme;
            //invertedList.Reverse();
            //var invertedArray = currentColorScheme[colorIndex].Reverse().ToArray();

            // Kurtosis
            //return ScaleColor.GetCategoricalColor(kurtosis, minKurtosis, maxKurtosis, currentColorScheme[colorIndex]);

            return currentColorScheme[skewnesIndex][kurtosisIndex];
        }


        private void SetCategoricView(int colorBar)
        {
            detailedViewOn = false;
            currentColorScheme = categoricColorSchemeList;

            // Reset Thresholds
            minSkewness = leftSkewedThreshold;
            centerSkewness = symmetricThreshold;
            maxSkewness = rightSkewedThreshold;

            minKurtosis = platykurticThreshold;
            centerKurtosis = mesokurticThreshold;
            maxKurtosis = leptokurticThreshold;

            // Reset Range Labels
            instanceScript.SetXRangeLabels($"> {minSkewness:0.00}", $"{centerSkewness:0.00}", $"{maxSkewness:0.00} >");
            instanceScript.SetYRangeLabels($"{minKurtosis:0.00} <", $"{centerKurtosis:0.00}", $"< {maxKurtosis:0.00}");
            // Reset titles
            instanceScript.SetXTitle("Skewness");
            instanceScript.SetYTitle("Kurtosis");


            SetLegendColoring();
        }


        private void SetDetailedView(int colorBar)
        {
            detailedViewOn = true;

            //############ Skewness ############

            // Right Skewed (starting from max skewness)
            if (colorBar >= 0 && colorBar <= 2)
            {
                currentColorScheme = extendedColorSchemeList[0];

                // From biggest Skewness to old maximum Skewness
                maxSkewness = (minMaxSkewness[1] >= rightSkewedThreshold) ? minMaxSkewness[1] : rightSkewedThreshold;
                minSkewness = rightSkewedThreshold;
                // value = ratio * (maxValue - minValue) + minValue
                centerSkewness = 0.5 * (maxSkewness - minSkewness) + minSkewness;

                // Set new Thresholds
                instanceScript.SetXRangeLabels($"<= {minSkewness:0.00}", $"{centerSkewness:0.00}", $"{maxSkewness:0.00} >=");
            }
            // Symmetric skewed ( between min and max skewness - excluding min and max)
            else if (colorBar >= 3 && colorBar <= 5)
            {
                currentColorScheme = extendedColorSchemeList[1];

                // From center to exactly the minimum skewness and exactly the maximum skewness.
                minSkewness = leftSkewedThreshold;
                maxSkewness = rightSkewedThreshold;

                // Set new Thresholds
                instanceScript.SetXRangeLabels($"] {minSkewness:0.00}", $"> {centerSkewness:0.00} >", $"{maxSkewness:0.00} [");
            }
            // Left skewed (starting from min skewness)
            else if (colorBar >= 6 && colorBar <= 8)
            {
                currentColorScheme = extendedColorSchemeList[2];

                minSkewness = (minMaxSkewness[0] <= leftSkewedThreshold) ? minMaxSkewness[0] : leftSkewedThreshold; 
                maxSkewness = leftSkewedThreshold;
                centerSkewness = 0.5 * (maxSkewness - minSkewness) + minSkewness;

                // Set new Thresholds
                instanceScript.SetXRangeLabels($">= {minSkewness:0.00}", $"{centerSkewness:0.00}", $"{maxSkewness:0.00} >=");
            }


            //############ Kurtosis ############

            // Leptokurtic (max kurtosis)
            if (colorBar == 0 || colorBar == 3 || colorBar == 6)
            {
                maxKurtosis = (minMaxKurtosis[1] >= leptokurticThreshold) ? minMaxKurtosis[1] : leptokurticThreshold; ;
                minKurtosis = leptokurticThreshold;
                centerKurtosis = 0.5 * (maxKurtosis - minKurtosis) + minKurtosis;

                // Set new Thresholds
                instanceScript.SetYRangeLabels($"{minKurtosis:0.00} <=", $"{centerKurtosis:0.00}", $">= {maxKurtosis:0.00}");
            }
            // Mesokurtic
            else if (colorBar == 1 || colorBar == 4 || colorBar == 7)
            {
                // From center to exactly the minimum skewness and exactly the maximum skewness.
                minKurtosis = platykurticThreshold;
                maxKurtosis = leptokurticThreshold;

                // Set new Thresholds
                instanceScript.SetYRangeLabels($"] {minKurtosis:0.00}", $"> {centerKurtosis:0.00} >", $"{maxKurtosis:0.00} [");
            }
            // Platykurtic (min kurtosis)
            else if (colorBar == 2 || colorBar == 5 || colorBar == 8)
            {
                minKurtosis = (minMaxKurtosis[0] <= platykurticThreshold) ? minMaxKurtosis[0] : platykurticThreshold; 
                maxKurtosis = platykurticThreshold;
                centerKurtosis = 0.5 * (maxKurtosis - minKurtosis) + minKurtosis;

                // Set new Thresholds
                instanceScript.SetYRangeLabels($"{minKurtosis:0.00} >=", $"{centerKurtosis:0.00}", $"<= {maxKurtosis:0.00} ");
            }

            // Set new titles
            instanceScript.SetXTitle("Skewness \n[Detailed View]");
            instanceScript.SetYTitle("Kurtosis \n[Detailed View]");

            SetLegendColoring();
        }

    }
}
