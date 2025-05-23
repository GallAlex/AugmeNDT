﻿using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Utilities
{
    /// <summary>
    /// Collection node is a data storage class for individual data about an object in a collection.
    /// </summary>
    [Serializable]
    public class ObjectCollectionNode
    {
        public string Name;
        public Vector2 Offset;
        public float Radius;
        public Transform Transform;
        public Collider[] Colliders;
    }
}