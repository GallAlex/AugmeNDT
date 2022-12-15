using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class holds information related to the positioning and scaling of Objects in Unity for Hololens 2 
/// </summary>
public static class GlobalScaleAndPos
{

    /// <summary>
    /// Sets the gameobject to the best viewing position for the  Hololens 2
    /// </summary>
    /// <param name="gameObjecTransform"></param>
    public static void SetToBestInitialStartPos(Transform gameObjecTransform)
    {
        gameObjecTransform.position = new Vector3(-0.2f, 0.1f, 0.5f);
    }

    /// <summary>
    /// Rescales the size of the current object to be at maximum 20 centimeters
    /// </summary>
    /// <param name="gameObjecTransform"></param>
    /// <param name="currentSize"></param>
    public static void SetToBestInitialScale(Transform gameObjecTransform, Vector3 currentSize)
    {
        ResizeRealtiveObject(gameObjecTransform, 0.2f, currentSize);
    }

    /// <summary>
    /// Rescales Object size based on the biggest side length. The biggest side length equals the targetSize.
    /// </summary>
    /// <param name="gameObjecTransform"></param>
    /// <param name="targetSize">New Sidelength of the Maximum sidelength</param>
    /// <param name="currentSize">Current size (not scale) aof the object</param>
    public static void ResizeRealtiveObject(Transform gameObjecTransform, float targetSize, Vector3 currentSize)
    {
        float maxScale = Mathf.Max(currentSize.x, currentSize.y, currentSize.z);
        float scaleFactor = targetSize / maxScale;

        gameObjecTransform.localScale = new Vector3(currentSize.x * scaleFactor, currentSize.y * scaleFactor, currentSize.z * scaleFactor);

    }

    /// <summary>
    /// Rescales Uniform and Non-Uniform Object based on their size to given scale.
    /// Devides  by the biggest side length
    /// </summary>
    /// <param name="gameObjecTransform"></param>
    /// <param name="targetSize">Maximum size a sidelength can have</param>
    /// <param name="currentSize">Current size (not scale) aof the object</param>
    public static void ResizeAbsolutMeshObject(Transform gameObjecTransform, float targetSize, Vector3 currentSize)
    {
        float maxScale = Mathf.Max(currentSize.x, currentSize.y, currentSize.z);
        float scaleFactor = targetSize / maxScale;

        gameObjecTransform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }

    public static void ResizeBoxCollider(Transform gameObjecTransform, BoxCollider boxColl, Vector3 currentSize, Vector3 currentCenter)
    {
        boxColl.center = new Vector3(currentCenter.x * gameObjecTransform.localScale.x, currentCenter.y * gameObjecTransform.localScale.y, currentCenter.z * gameObjecTransform.localScale.z);
        boxColl.size = new Vector3(currentSize.x * gameObjecTransform.localScale.x, currentSize.y * gameObjecTransform.localScale.y, currentSize.z * gameObjecTransform.localScale.z);

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
