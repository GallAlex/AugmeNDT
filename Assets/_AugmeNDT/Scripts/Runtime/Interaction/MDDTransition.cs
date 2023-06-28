using System;
using UnityEngine;

namespace AugmeNDT{
    public class MDDTransition : MonoBehaviour
    {
        VisMDDGlyphs mddVis;
        Camera mainCamera;
        bool isTransitioned = false;


        public void SetMDDVis(VisMDDGlyphs vis)
        {
            mddVis = vis;
            mainCamera = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            if (mddVis != null && (mddVis.visContainerObject.transform.hasChanged || mainCamera.transform.hasChanged))
            {
                Vector3 rotation = mddVis.visContainerObject.transform.eulerAngles;

                if (NearlyOrthogonalViewAngle(mddVis.visContainerObject.transform))
                {
                    if (!isTransitioned)
                    {
                        Debug.Log("Transition fits to 2D");
                        mddVis.ApplyMDDTransition(true);
                        isTransitioned = true; 
                    }
                }
                else
                {
                    if (isTransitioned)
                    {
                        Debug.Log("Transition fits to 3D");
                        mddVis.ApplyMDDTransition(false);
                        isTransitioned = false;
                    }
                }
            }
        }


        private bool NearlyOrthogonalViewAngle(Transform visObject)
        {
            //Define Plane at bottom of Vis for calculation
            //TODO: Plane is only at the bottom so UP is not correct when showing the Vis looking from bottom
            Plane plane = new Plane(visObject.up, visObject.position);
            bool isAbove = false;

            //Check if camera is above.... or below the plane
            isAbove = plane.GetSide(mainCamera.transform.position);

            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

            // If camera intersects the plane
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 posOnPlane = ray.direction * distance;
                //Debug.DrawRay(mainCamera.transform.position, posOnPlane, Color.green);

                //Use Abs to ignore if looking from below or above
                double dotProduct = Math.Abs(Vector3.Dot(ray.direction, plane.normal));

                if(dotProduct >= 0.91 && dotProduct <= 1) return true;
            }

            return false;
        }



    }
}
