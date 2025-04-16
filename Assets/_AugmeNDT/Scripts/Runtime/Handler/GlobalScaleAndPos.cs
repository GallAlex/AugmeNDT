using UnityEngine;

namespace AugmeNDT{
    /// <summary>
    /// Class holds information related to the positioning and scaling of Objects in Unity for Hololens 2 
    /// </summary>
    public static class GlobalScaleAndPos
    {
        // Head height as defined by XROrigin 
        public static float HeadHeight = 1.6f;

        /// <summary>
        /// Sets the gameobject to the best viewing position for the  Hololens 2
        /// </summary>
        /// <param name="gameObjecTransform"></param>
        public static void SetToBestInitialStartPos(Transform gameObjecTransform)
        {
            gameObjecTransform.position = new Vector3(0f, HeadHeight, 0.5f);
        }

        /// <summary>
        /// Rescales the size of the current object to be at maximum 20 centimeters
        /// </summary>
        /// <param name="gameObjecTransform"></param>
        /// <param name="currentSize"></param>
        public static void SetToBestInitialScale(Transform gameObjecTransform, Vector3 currentSize)
        {
            ResizeObjectRelative(gameObjecTransform, 0.2f, currentSize);
        }

        /// <summary>
        /// Rescales a GameObject by calculating a scaling factor based on the ratio between the target size and the object's largest current dimension.
        /// The dimensions of the object are increased by the ratio maintaining the original proportions.
        /// </summary>
        /// <param name="gameObjecTransform"></param>
        /// <param name="targetSize">New Sidelength of the Maximum sidelength</param>
        /// <param name="currentSize">Current size (not scale) of the object</param>
        public static void ResizeObjectRelative(Transform gameObjecTransform, float targetSize, Vector3 currentSize)
        {
            float maxScale = Mathf.Max(currentSize.x, currentSize.y, currentSize.z);
            float scaleFactor = targetSize / maxScale;
            gameObjecTransform.localScale = new Vector3(currentSize.x * scaleFactor, currentSize.y * scaleFactor, currentSize.z * scaleFactor);

        }


        public static void ResizeBoxCollider(Transform gameObjecTransform, BoxCollider boxColl, Vector3 currentSize, Vector3 currentCenter)
        {
            boxColl.center = new Vector3(currentCenter.x * gameObjecTransform.localScale.x, currentCenter.y * gameObjecTransform.localScale.y, currentCenter.z * gameObjecTransform.localScale.z);
            boxColl.size = new Vector3(currentSize.x * gameObjecTransform.localScale.x, currentSize.y * gameObjecTransform.localScale.y, currentSize.z * gameObjecTransform.localScale.z);

        }

        /// <summary>
        /// Uses the GetBoundsOfParentAndChildren method to expand the BoxCollider of the Parent Object to fits all its children
        /// </summary>
        /// <param name="parentObj"></param>
        public static void FitBoxColliderToChildren(GameObject parentObj)
        {
            // If the parent object has no BoxCollider, add one
            BoxCollider parentColl = parentObj.GetComponent<BoxCollider>() != null ? parentObj.GetComponent<BoxCollider>() : parentObj.AddComponent<BoxCollider>();

            Bounds newBounds = GetBoundsOfParentAndChildren(parentObj);
        
            parentColl.center = newBounds.center;
            parentColl.size = newBounds.size;
        }

        /// <summary>
        /// Returns the Bounds of the Parent Object which fits all its children
        /// Starts from the bounds of the parent object and expands it iteratively
        /// </summary>
        /// <param name="parentObj"></param>
        /// <returns></returns>
        public static Bounds GetBoundsOfParentAndChildren(GameObject parentObj)
        {
            Bounds bounds = new Bounds(parentObj.transform.position, Vector3.zero);
            foreach (MeshRenderer renderer in parentObj.GetComponentsInChildren<MeshRenderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return new Bounds(bounds.center - parentObj.transform.position, bounds.size);
        }

        /// <summary>
        /// Moves the Gameobjects Pivot from the Center (0,0,0), to start the content of the Gameobject
        /// on its lower front left point.
        /// </summary>
        /// <param name="gameObjecTransform"></param>
        public static void MoveOriginToLowerFrontLeftPoint(Transform gameObjecTransform)
        {
            float newX = gameObjecTransform.localPosition.x + gameObjecTransform.localScale.x / 2.0f;
            float newY = gameObjecTransform.localPosition.y + gameObjecTransform.localScale.y / 2.0f;
            float newZ = gameObjecTransform.localPosition.z + gameObjecTransform.localScale.z / 2.0f;

            gameObjecTransform.localPosition = new Vector3(newX, newY, newZ);
        }

    }
}
