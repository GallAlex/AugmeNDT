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
using Microsoft.MixedReality.Toolkit.XRSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using UnityEngine.XR.MagicLeap.Meshing;

namespace MagicLeap.MRTK.SpatialAwareness
{
    public enum GeneralMeshRenderMode
    {
        None,
        Colored,
        PointCloud,
        Occlusion,
        Wireframe
    }

    [MixedRealityDataProvider(
        typeof(IMixedRealitySpatialAwarenessSystem),
        SupportedPlatforms.Android,
        "MagicLeap Spatial Mesh Observer")]
    [HelpURL("https://docs.microsoft.com/windows/mixed-reality/mrtk-unity/features/spatial-awareness/spatial-awareness-getting-started")]
    public class MagicLeapSpatialMeshObserver :
        BaseSpatialMeshObserver
    {
        /// <summary>
        /// An event which is invoked whenever a new mesh is added
        /// </summary>
        public event Action<GameObject> MeshAdded;

        /// <summary>
        /// An event which is invoked whenever an existing mesh is updated (regenerated).
        /// </summary>
        public event Action<GameObject> MeshUpdated;

        /// <summary>
        /// Altering the mesh profile data at runtime may require calling ForceUpdateMeshData() to clear visuals;
        /// </summary>
        public MagicLeapSpatialMeshObserverProfile Profile;

        private MeshingSubsystemComponent subsystemComponent;

        private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

        private GameObject mainCamera = null;

        private GameObject meshParent = null;
        private GameObject meshingSubsystemParent = null;

        private XRInputSubsystem inputSubsystem;
        private bool permissionsGranted = false;
        private bool permissionRequested = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="registrar">The <see cref="IMixedRealityServiceRegistrar"/> instance that loaded the service.</param>
        /// <param name="name">Friendly name of the service.</param>
        /// <param name="priority">Service priority. Used to determine order of instantiation.</param>
        /// <param name="profile">The service's configuration profile.</param>
        public MagicLeapSpatialMeshObserver(
            IMixedRealitySpatialAwarenessSystem spatialAwarenessSystem,
            string name = null,
            uint priority = DefaultPriority,
            BaseMixedRealityProfile profile = null) : base(spatialAwarenessSystem, name, priority, profile)
        {
        }

        public override void Enable()
        {
            base.Enable();

            Profile = ConfigurationProfile as MagicLeapSpatialMeshObserverProfile;
            if (Profile == null)
            {
                Debug.LogWarning($"Use the `MagicLeapSpatialMeshObserverProfile` configuration to set Magic Leap specific meshing settings. Default settings will be used.");

                Profile = new MagicLeapSpatialMeshObserverProfile();
            }

            if (meshParent == null)
            {
                meshParent = new GameObject("MeshParent");
            }

            if (meshingSubsystemParent == null)
            {
                meshingSubsystemParent = new GameObject("MeshingSubsystem");
            }

            inputSubsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRInputSubsystem>();
            inputSubsystem.trackingOriginUpdated += OnTrackingOriginChanged;

            mainCamera = Camera.main.gameObject;

            meshingSubsystemParent.transform.position = mainCamera.transform.position;

            UpdateBounds();

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            inputSubsystem.trackingOriginUpdated -= OnTrackingOriginChanged;

            permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
            subsystemComponent.meshAdded -= HandleOnMeshAdded;
            subsystemComponent.meshUpdated -= HandleOnMeshUpdated;

        }

        public override void Update()
        {
            if(Profile == null)
            {
                return;
            }

            if(Profile != null && !permissionRequested)
            {
                permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
                permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
                permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
                permissionRequested = true;

                MLPermissions.RequestPermission(MLPermission.SpatialMapping, permissionCallbacks);
                return;
            }

            if (!permissionsGranted)
            {
                return;
            }

            base.Update();

            if (Profile.Follow)
            {
                meshingSubsystemParent.transform.position = mainCamera.transform.position;
            }

            if(Profile.IsBounded && meshingSubsystemParent.transform.localScale != Profile.BoundedExtentsSize ||
                !Profile.IsBounded && meshingSubsystemParent.transform.localScale != Profile.BoundlessExtentsSize)
            {
                UpdateBounds();
            }
        }

        public void ForceUpdateMeshData()
        {

            subsystemComponent.meshPrefab = Profile.MeshPrefab;
            subsystemComponent.computeNormals = Profile.ComputeNormals;
            subsystemComponent.density = Profile.Density;
            subsystemComponent.meshParent = meshParent.transform;
            subsystemComponent.requestedMeshType = Profile.MeshType;
            subsystemComponent.fillHoleLength = Profile.FillHoleLength;
            subsystemComponent.planarize = Profile.Planarize;
            subsystemComponent.disconnectedComponentArea = Profile.DisconnectedComponentArea;
            subsystemComponent.meshQueueSize = Profile.MeshQueueSize;
            subsystemComponent.pollingRate = Profile.PollingRate;
            subsystemComponent.batchSize = Profile.BatchSize;
            subsystemComponent.requestVertexConfidence = Profile.RequestVertexConfidence;
            subsystemComponent.removeMeshSkirt = Profile.RemoveMeshSkirt;

            subsystemComponent.DestroyAllMeshes();
            subsystemComponent.RefreshAllMeshes();
            UpdateBounds();

        }

        public MeshingSubsystemComponent GetActiveMeshingComponent()
        {
            return subsystemComponent;
        }

        private void GeneralRendering(MeshRenderer meshRenderer)
        {
            // Toggle the GameObject(s) and set the correct materia based on the current RenderMode.
            if (Profile.GeneralRenderMode == GeneralMeshRenderMode.None)
            {
                meshRenderer.enabled = false;
            }
            else if (Profile.GeneralRenderMode == GeneralMeshRenderMode.PointCloud)
            {
                meshRenderer.enabled = true;
                meshRenderer.material = Profile.GeneralPointCloudMaterial;
            }
            else if (Profile.GeneralRenderMode == GeneralMeshRenderMode.Colored)
            {
                meshRenderer.enabled = true;
                meshRenderer.material = Profile.GeneralColoredMaterial;
            }
            else if (Profile.GeneralRenderMode == GeneralMeshRenderMode.Occlusion)
            {
                meshRenderer.enabled = true;
                meshRenderer.material = Profile.GeneralOcclusionMaterial;
            }
            else if (Profile.GeneralRenderMode == GeneralMeshRenderMode.Wireframe)
            {
                meshRenderer.enabled = true;
                meshRenderer.material = Profile.GeneralWireframeMaterial;
            }
        }

        private void UpdateBounds()
        {
            meshingSubsystemParent.transform.localScale = Profile.IsBounded ? Profile.BoundedExtentsSize : Profile.BoundlessExtentsSize;
        }

        private void OnPermissionDenied(string permission)
        {
            if (permission == MLPermission.SpatialMapping)
            {
                Debug.LogError($"Failed to create Meshing Subsystem due to missing or denied {permission} permission. Please add to manifest. Disabling script.");
            }
        }

        private void OnPermissionGranted(string permission)
        {
            if (permission == MLPermission.SpatialMapping)
            {
                permissionsGranted = true;
                Debug.Log($"Permission: {permission} granted.");

                subsystemComponent = meshingSubsystemParent.gameObject.AddComponent<MeshingSubsystemComponent>();

                subsystemComponent.meshPrefab = Profile.MeshPrefab;
                subsystemComponent.computeNormals = Profile.ComputeNormals;
                subsystemComponent.density = Profile.Density;
                subsystemComponent.meshParent = meshParent.transform;
                subsystemComponent.requestedMeshType = Profile.MeshType;
                subsystemComponent.fillHoleLength = Profile.FillHoleLength;
                subsystemComponent.planarize = Profile.Planarize;
                subsystemComponent.disconnectedComponentArea = Profile.DisconnectedComponentArea;
                subsystemComponent.meshQueueSize = Profile.MeshQueueSize;
                subsystemComponent.pollingRate = Profile.PollingRate;
                subsystemComponent.batchSize = Profile.BatchSize;
                subsystemComponent.requestVertexConfidence = Profile.RequestVertexConfidence;
                subsystemComponent.removeMeshSkirt = Profile.RemoveMeshSkirt;
                subsystemComponent.meshAdded += HandleOnMeshAdded;
                subsystemComponent.meshUpdated += HandleOnMeshUpdated;
            }
        }

        private void OnTrackingOriginChanged(XRInputSubsystem inputSubsystem)
        {

            subsystemComponent.DestroyAllMeshes();
            subsystemComponent.RefreshAllMeshes();

        }

        private void HandleOnMeshAdded(UnityEngine.XR.MeshId meshId)
        {
            if (subsystemComponent.meshIdToGameObjectMap.ContainsKey(meshId))
            {
                if (MeshAdded != null)
                {
                    MeshAdded(subsystemComponent.meshIdToGameObjectMap[meshId]);
                }

                if (Profile.UseGeneralRendering)
                {
                    GeneralRendering(subsystemComponent.meshIdToGameObjectMap[meshId].GetComponent<MeshRenderer>());
                }
            }
        }

        private void HandleOnMeshUpdated(UnityEngine.XR.MeshId meshId)
        {
            if (subsystemComponent.meshIdToGameObjectMap.ContainsKey(meshId))
            {
                if (MeshUpdated != null)
                {
                    MeshUpdated(subsystemComponent.meshIdToGameObjectMap[meshId]);
                }

                if (Profile.UseGeneralRendering)
                {
                    GeneralRendering(subsystemComponent.meshIdToGameObjectMap[meshId].GetComponent<MeshRenderer>());
                }
            }
        }

        /// <summary>
        /// Not yet fully implemented. Will be in future releases.
        /// </summary>
        private MeshingSubsystem.Extensions.MLMeshing.MeshBlockRequest[] CustomBlockRequests(MeshingSubsystem.Extensions.MLMeshing.MeshBlockInfo[] blockInfos)
        {
            var blockRequests = new MeshingSubsystem.Extensions.MLMeshing.MeshBlockRequest[blockInfos.Length];
            for (int i = 0; i < blockInfos.Length; ++i)
            {
                var blockInfo = blockInfos[i];
                var distanceFromCamera = Vector3.Distance(mainCamera.transform.position, blockInfo.pose.position);
                if (distanceFromCamera > 1)
                    blockRequests[i] = new MeshingSubsystem.Extensions.MLMeshing.MeshBlockRequest(blockInfo.id, MeshingSubsystem.Extensions.MLMeshing.LevelOfDetail.Minimum);
                else
                    blockRequests[i] = new MeshingSubsystem.Extensions.MLMeshing.MeshBlockRequest(blockInfo.id, MeshingSubsystem.Extensions.MLMeshing.LevelOfDetail.Maximum);
            }

            return blockRequests;
        }
    }
}