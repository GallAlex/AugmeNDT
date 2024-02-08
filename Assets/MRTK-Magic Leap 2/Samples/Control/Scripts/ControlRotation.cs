// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using MagicLeap.MRTK.DeviceManagement.Input;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.MRTK.Samples
{
    public class ControlRotation : MonoBehaviour
    {
        float speed = 20f;

        float sensitivity = 100f;

        // Scale factor used to compute direction vector for rotation. (higher scaleFactor results in a bigger rotation)
        float scaleFactor = 30f;

        float colorLerpTime = 3f;

        void LerpCubeColor(float pressure)
        {
            Color randColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

            Renderer cubeRenderer = this.GetComponent<Renderer>();

            cubeRenderer.material.color = Color.Lerp(cubeRenderer.material.color, randColor, colorLerpTime * Time.deltaTime);
        }

        public void RotateOnPositionChange(BaseInputEventData eventData)
        {
            Vector3 direction;
            if (eventData.MixedRealityInputAction.AxisConstraint == AxisType.DualAxis)
            {
                InputEventData<Vector2> data = (InputEventData<Vector2>)eventData;

                // Get the touch vector from the controller. 
                Vector2 touchVector = data.InputData;

                // Invert axis to get desired, touch-to-rotation behavior.  
                direction = new Vector3(touchVector.y, -touchVector.x, touchVector.y) * scaleFactor;

                // Obtain current rotation of cube object.
                Quaternion currentRotation = this.transform.rotation;

                // Perform rotation using currentRotation & computed direction. 
                this.transform.rotation = Quaternion.Slerp(currentRotation, Quaternion.Euler(direction) * currentRotation, Time.deltaTime * speed);
            }
        }

        public void LerpOnPress(BaseInputEventData eventData)
        {
            if (eventData.MixedRealityInputAction.AxisConstraint == AxisType.SingleAxis)
            {
                InputEventData<float> data = (InputEventData<float>)eventData;
                // Obtain touch pressure from controller scaled by desired sensitivity (used to decide whether or not to Lerp to a random color). 
                float LastPressure = data.InputData * sensitivity;

                if (LastPressure > 10)
                {
                    LerpCubeColor(LastPressure);
                }
            }
        }
    }
}