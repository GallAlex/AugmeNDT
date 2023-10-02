using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

namespace AugmeNDT{
    /// <summary>
    /// Class helps defining colors and offers certtain color schemes
    /// </summary>
    public class ColorHelper
    {

        // Extended Virvidis going from darker to brighter
        public static Color[] viridisYellowHue = new[] { ReturnColorFromHex("#cac16e"), ReturnColorFromHex("#e4d54f"), ReturnColorFromHex("#fde725") };
        public static Color[] viridisGreenHue = new[] { ReturnColorFromHex("#3b5e5c"), ReturnColorFromHex("#337875"), ReturnColorFromHex("#21918c") };
        public static Color[] viridisPurpleHue = new[] { ReturnColorFromHex("#440154"), ReturnColorFromHex("#5d176e"), ReturnColorFromHex("#773887") };

        // MDD-Glyph: Multi-Hue Sequential from bright to dark
        public static Color[] redMultiHueValues = new[] { ReturnColorFromHex("#FDBB84"), ReturnColorFromHex("#E34A33"), ReturnColorFromHex("#97483F") };
        public static Color[] blueMultiHueValues = new[] { ReturnColorFromHex("#A6BDDB"), ReturnColorFromHex("#2B8CBE"), ReturnColorFromHex("#1F618E") };
        public static Color[] violetMultiHueValues = new[] { ReturnColorFromHex("#E7C3CC"), ReturnColorFromHex("#8856A7"), ReturnColorFromHex("#5E407A") };

        // MDD-Glyph: Single-Hue from bright to dark (derived from the multi-hue values)
        public static Color[] redSingleHueValues01 = new[] { ReturnColorFromHex("#E6A39C"), ReturnColorFromHex("#97473F"), ReturnColorFromHex("#551009") };
        public static Color[] redSingleHueValues02 = new[] { ReturnColorFromHex("#FF9686"), ReturnColorFromHex("#E34B33"), ReturnColorFromHex("#981702") };
        public static Color[] redSingleHueValues03 = new[] { ReturnColorFromHex("#FFE6D1"), ReturnColorFromHex("#FDBB84"), ReturnColorFromHex("#B76C2D") };

        public static Color[] blueSingleHueValues01 = new[] { ReturnColorFromHex("#5B8DB0"), ReturnColorFromHex("#1F608E"), ReturnColorFromHex("#053659") };
        public static Color[] blueSingleHueValues02 = new[] { ReturnColorFromHex("#76B9DD"), ReturnColorFromHex("#2B8BBE"), ReturnColorFromHex("#065884") };
        public static Color[] blueSingleHueValues03 = new[] { ReturnColorFromHex("#F5F9FD"), ReturnColorFromHex("#A6BDDB"), ReturnColorFromHex("#4E709C") };

        public static Color[] violetSingleHueValues01 = new[] { ReturnColorFromHex("#A08DB1"), ReturnColorFromHex("#5E407A"), ReturnColorFromHex("#32154D") };
        public static Color[] violetSingleHueValues02 = new[] { ReturnColorFromHex("#D2B6E3"), ReturnColorFromHex("#8856A7"), ReturnColorFromHex("#572179") };
        public static Color[] violetSingleHueValues03 = new[] { ReturnColorFromHex("#FAF7F8"), ReturnColorFromHex("#E7C3CC"), ReturnColorFromHex("#A86273") };

        // Stacked Histogram ChangeIndicator: From ligth blue to dark blue
        public static Color[] blueHueValues = new[] { ReturnColorFromHex("#deebf7"), ReturnColorFromHex("#9ecae1"), ReturnColorFromHex("#3182bd") };
        // Stacked Histogram ChangeIndicator: From ligth red to dark red
        public static Color[] redHueValues = new[] { ReturnColorFromHex("#fee0d2"), ReturnColorFromHex("#fc9272"), ReturnColorFromHex("#de2d26")};

        // Stacked Histogram ChangeIndicator: Diverging Scheme from from green to purple over gray
        public static Color[] divergingValues = new[] { ReturnColorFromHex("#88d8b0"), ReturnColorFromHex("#2a9d8f"), ReturnColorFromHex("#264653"), ReturnColorFromHex("#808080"), ReturnColorFromHex("#c77dff"), ReturnColorFromHex("#9e00ff"), ReturnColorFromHex("#4a148c") };


        /// <summary>
        /// Method takes in string with hex value of color and returns color between 0-1
        /// Strings have to begin with '#' 
        /// </summary>
        /// <param name="hexColor"></param>
        /// <returns></returns>
        public static Color ReturnColorFromHex(string hexColor)
        {
            Color newCol;
            if (!ColorUtility.TryParseHtmlString(hexColor, out newCol)) Debug.LogError("ColorHelper: Color could not be parsed from hex string: " + hexColor);

            return newCol;
        }

        /// <summary>
        /// Method takes in color and returns a hex string (starting with '#')
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static string ReturnHexFromColor(Color col)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(col);
        }

        /// <summary>
        /// Method takes in int array with color values between 0-255
        /// If no alpha value is provided it is set to 255
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color ReturnColorFromInt(int[] color)
        {
            Color newCol;

            //Check if color has fourth value
            if (color.Length < 4)
            {
                newCol = new Color(color[0] / 255.0f, color[1] / 255.0f, color[2] / 255.0f, 255.0f);
            }
            else
            {
                newCol = new Color(color[0] / 255.0f, color[1] / 255.0f, color[2] / 255.0f, color[3] / 255.0f);
            }

            return newCol;
        }

    }
}