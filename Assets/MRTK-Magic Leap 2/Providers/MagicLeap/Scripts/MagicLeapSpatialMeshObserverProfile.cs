// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2018-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using MagicLeap.MRTK.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.MRTK.SpatialAwareness
{
    /// <summary>
    /// Configuration profile settings for spatial awareness mesh observers.
    /// </summary>
    [CreateAssetMenu(
        menuName =
            "Mixed Reality/Toolkit/Profiles/Magic Leap Spatial Awareness Mesh Observer Profile",
        fileName = "MagicLeapSpatialMeshObserverProfile", order = (int)CreateProfileMenuItemIndices.SpatialAwarenessMeshObserver)]
    [MixedRealityServiceProfile(typeof(MagicLeapSpatialMeshObserver))]
    public class MagicLeapSpatialMeshObserverProfile : MixedRealitySpatialAwarenessMeshObserverProfile
    {
        [Tooltip("Get or set the prefab which should be instantiated to create individual mesh instances. Assumed to have a mesh renderer. May have an optional mesh collider for physics.")]
        public GameObject MeshPrefab;

        [Tooltip("Should meshing follow the camera or remain at the original camera location.")]
        public bool Follow = true;

        [Tooltip("When enabled, the system will compute the normals for the triangle vertices.")]
        public bool ComputeNormals = true;

        [Tooltip("Level of detail, ranges from 0.0f to 1.0f")]
        [Range(0f, 1f)]
        public float Density = 1.0f;

        [Tooltip("Whether to generate a triangle mesh or point cloud points.")]
        public MeshingSubsystemComponent.MeshType MeshType = MeshingSubsystemComponent.MeshType.Triangles;

        [Tooltip("Boundary distance (in meters) of holes you wish to have filled.")]
        public float FillHoleLength = 1.0f;

        [Tooltip("When enabled, the system will planarize the returned mesh (planar regions will be smoothed out).")]
        public bool Planarize = false;

        [Tooltip("Any component that is disconnected from the main mesh and which has an area less than this size will be removed.")]
        public float DisconnectedComponentArea = 0.25f;

        [Tooltip("Controls the number of meshes to queue for generation at once. Larger numbers will lead to higher CPU usage.")]
        public uint MeshQueueSize = 4;

        [Tooltip("How often to check for updates, in seconds. More frequent updates will increase CPU usage.")]
        public float PollingRate = 0.25f;

        [Tooltip("How many meshes to update per batch. Larger values are more efficient, but have higher latency.")]
        public int BatchSize = 16;

        [Tooltip("When enabled, the system will generate confidence values for each vertex, ranging from 0-1.")]
        public bool RequestVertexConfidence = false;

        [Tooltip("When enabled, the mesh skirt (overlapping area between two mesh blocks) will be removed. This field is only valid when the Mesh Type is Blocks.")]
        public bool RemoveMeshSkirt = false;

        [Tooltip("Flag specifying if mesh extents are bounded.")]
        public bool IsBounded = false;

        [Tooltip("Size of the bounds extents when bounded setting is enabled.")]
        public Vector3 BoundedExtentsSize = new Vector3(2.0f, 2.0f, 2.0f);

        [Tooltip("Size of the bounds extents when bounded setting is disabled.")]
        public Vector3 BoundlessExtentsSize = new Vector3(10.0f, 10.0f, 10.0f);

        [Tooltip("If this is checked the general rendering settings below will be used. Otherwise it is assumed rendering will be taken care of in the MeshChanged event.")]
        public bool UseGeneralRendering = true;

        [Tooltip("Render mode to render mesh data with.")]
        public GeneralMeshRenderMode GeneralRenderMode = GeneralMeshRenderMode.Colored;

        [Tooltip("The material to apply for occlusion.")]
        public Material GeneralOcclusionMaterial = null;

        [Tooltip("The material to apply for colored rendering.")]
        public Material GeneralColoredMaterial = null;

        [Tooltip("The material to apply for point cloud rendering.")]
        public Material GeneralPointCloudMaterial = null;

        [Tooltip("The material to apply for wireframe rendering.")]
        public Material GeneralWireframeMaterial = null;
    }
}