// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{

    /// <summary>
    /// This class is used to calculate certain ancer points on an object.
    /// </summary>
    public static class AncerPointCalc
    {

        /// <summary>
        /// Possible Ancor Points for the X-Axis.
        /// </summary>
        public enum AncorPointX
        {
            Left,
            Right,
            Center,
        }

        /// <summary>
        /// Possible Ancor Points for the Y-Axis.
        /// </summary>
        public enum AncorPointY
        {
            Top,
            Bottom,
            Center,
        }

        /// <summary>
        /// Possible Ancor Points for the Z-Axis.
        /// </summary>
        public enum AncorPointZ
        {
            Front,
            Back,
            Center,
        }

        /// <summary>
        /// Method returns the position of the given ancor point based on the given Transform of an object.
        /// The method assumes that the object is centered around the origin.
        /// </summary>
        /// <param name="objectTransform"></param>
        /// <param name="ancorPoint"></param>
        /// <returns></returns>
        public static Vector3 GetAncorPoint(Transform objectTransform, AncorPointX xAncor, AncorPointY yAncor,
            AncorPointZ zAncor)
        {
            Vector3 halfScale = objectTransform.localScale / 2.0f;
            Vector3 translationVec = BuildTranslationVec(halfScale, xAncor, yAncor, zAncor);

            Vector3 ancorPoint = objectTransform.localPosition + translationVec;

            // Based on the given ancor point descriptions, the vector for translation is calculated and the ancor point on the object is returned.
            return ancorPoint;
        }


        private static Vector3 BuildTranslationVec(Vector3 halfScale, AncorPointX xAncor, AncorPointY yAncor, AncorPointZ zAncor)
        {
            Vector3 translationVec = Vector3.zero;

            switch (xAncor)
            {
                case AncorPointX.Left:
                    translationVec.x = -halfScale.x;
                    break;
                case AncorPointX.Right:
                    translationVec.x = halfScale.x;
                    break;
                case AncorPointX.Center:
                    translationVec.x = 0.0f;
                    break;
                default:
                    Debug.LogError("AncorPointX: Ancor not found");
                    break;
            }

            switch (yAncor)
            {
                case AncorPointY.Top:
                    translationVec.y = halfScale.y;
                    break;
                case AncorPointY.Bottom:
                    translationVec.y = -halfScale.y;
                    break;
                case AncorPointY.Center:
                    translationVec.y = 0.0f;
                    break;
                default:
                    Debug.LogError("AncorPointY: Ancor not found");
                    break;
            }

            switch (zAncor)
            {
                case AncorPointZ.Front:
                    translationVec.z = -halfScale.z;
                    break;
                case AncorPointZ.Back:
                    translationVec.z = halfScale.z;
                    break;
                case AncorPointZ.Center:
                    translationVec.z = 0.0f;
                    break;
                default:
                    Debug.LogError("AncorPointZ: Ancor not found");
                    break;
            }

            return translationVec;
        }

    }
}
