// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System.Collections.Generic;
using System.Threading.Tasks;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace AugmeNDT{

    public class PolyFiberRenderedObject
    {

        private GameObject containerPrefab;
        private GameObject polyModelContainer;
        [SerializeField]
        private GameObject polyModel; // gameobject holding all meshes of the polygonal model
        private Material cylinderMaterial;

        // Reference to DataVisGroup
        private DataVisGroup dataVisGroup;

        //Data
        private PolyFiberData polyFiberDataset;
        float scaleFactor = 1f; //TODO: Metric unit (mm, cm, m,...) get from dataset/global setting or user

        private bool useMeshManager = true;
        private MeshManager meshManager;
        private CylinderObjectVis cylinderVis;

        Color defaultCol;                       // Sets the default color of the mesh
        private List<int> selectedFiberIDs;     // Stores the IDs of selected Fibers (e.g., colored fibers)

        public PolyFiberRenderedObject()
        {
            cylinderMaterial = new Material((Material)Resources.Load("Materials/PolyMaterial", typeof(Material)));
            containerPrefab = (GameObject)Resources.Load("Prefabs/PolyModelContainer");
            defaultCol = cylinderMaterial.color;
            selectedFiberIDs = new List<int>();
        }

        /// <summary>
        /// Gives the PolyFiberRenderedObject acces to its DataVis group
        /// </summary>
        /// <param name="group"></param>
        public void SetDataVisGroup(DataVisGroup group)
        {
            dataVisGroup = group;
        }

        /// <summary>
        /// Returns the DataVisGroup of the PolyFiberRenderedObject
        /// </summary>
        /// <returns></returns>
        public DataVisGroup GetDataVisGroup()
        {
            return dataVisGroup;
        }

        public async Task CreateObject(GameObject container, PolyFiberData dataset)
        {
            polyFiberDataset = dataset;

            polyModelContainer = GameObject.Instantiate(containerPrefab, container.transform.position, Quaternion.identity);
            polyModelContainer.transform.parent = container.transform;
            polyModelContainer.name = "FiberModel";

            polyModel = new GameObject("AllFibers_" + dataset.NumberOfFibers);
            polyModel.transform.SetParent(polyModelContainer.transform);


            //TODO: Mesh is bigger then start - end point (because of radius), Save real size - either by mhd or by min/max

            // CREATE MESH
            if (useMeshManager)
            {
                meshManager = new MeshManager();

                CreateCombinedCylinderRepresentation(dataset);

                //Resize to whole size of all meshes
                Bounds wholeFiberObjBounds = GlobalScaleAndPos.GetBoundsOfParentAndChildren(polyModel);
                
                BoxCollider boxColl = polyModelContainer.GetComponent<BoxCollider>() != null ? polyModelContainer.GetComponent<BoxCollider>() : polyModelContainer.AddComponent<BoxCollider>();

                // Fibers are drawn in different coordinate system and need to be reduced to be in meters
                float maxScale = Mathf.Max(wholeFiberObjBounds.size.x, wholeFiberObjBounds.size.y, wholeFiberObjBounds.size.z);
                float scaleFactor = 1f / maxScale;
                polyModel.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

                //Move bounding box
                //polyModel.transform.localPosition = new Vector3(-0.5f, -0.5f, 0.5f);

                //GlobalScaleAndPos.ResizeObjectRelative(polyModel.transform, 1f, wholeFiberObjBounds.size);
                GlobalScaleAndPos.ResizeBoxCollider(polyModel.transform, boxColl, wholeFiberObjBounds.size, wholeFiberObjBounds.center);

                BoundsControl boundsControl = polyModelContainer.GetComponent<BoundsControl>() != null ? polyModelContainer.GetComponent<BoundsControl>() : polyModelContainer.AddComponent<BoundsControl>();
                boundsControl.OverrideBounds = true;
                boundsControl.BoundsOverride = polyModel.transform;

            }
            else
            {
                CreateCylinderRepresentation(dataset);
            }


            GlobalScaleAndPos.SetToBestInitialScale(polyModelContainer.transform, polyModelContainer.transform.localScale);
            GlobalScaleAndPos.SetToBestInitialStartPos(polyModelContainer.transform);

        }

        /// <summary>
        /// Draws the polygonal representation with cylinders
        /// Each cylinder mesh is displayed by one gameobject.
        /// Note: Can get really slow when many meshes = gameobjects are displayed
        /// </summary>
        /// <param name="dataset"></param>
        private void CreateCylinderRepresentation(PolyFiberData dataset)
        {
            cylinderVis = new CylinderObjectVis();

            //combine = new CombineInstance[polyFiberDataset.NumberOfFibers];

            for (int fiber = 0; fiber < dataset.NumberOfFibers; fiber++)
            {
                GameObject fiberObject = new GameObject("Fiber_" + fiber);
                fiberObject.transform.SetParent(polyModel.transform);


                Mesh mesh = cylinderVis.CreateMesh(dataset.Label[fiber].ToString(), dataset.GetFiberRadius(fiber), dataset.GetFiberCoordinates(fiber));
                MeshFilter currentMeshFilter = fiberObject.AddComponent<MeshFilter>();
                currentMeshFilter.mesh = mesh;
                MeshRenderer currentMeshRenderer = fiberObject.AddComponent<MeshRenderer>();
                //currentMeshRenderer.material = new Material(Shader.Find("Mixed Reality Toolkit/Standard"));
                currentMeshRenderer.sharedMaterial = cylinderMaterial;
                //currentMeshRenderer.material.SetFloat("Vertex Colors", 1.0f);
                //ResizeToBound(polyFiberObject, fiberObject);

                //combine[fiber].mesh = mesh;
            }

            //polyFiberObject.transform.GetComponent<MeshFilter>().mesh = new Mesh();
            //polyFiberObject.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        }

        /// <summary>
        /// Draws the polygonal representation with cylinders.
        /// Uses the MeshManager class to combine the polygonal representation, which consists out of multiple meshes (cylinders), into one Mesh.
        /// If the Mesh would be too big (decision by MeshManager) it will be split into multiple parts (meshes), each hold by an gameobjects
        /// </summary>
        /// <param name="dataset"></param>
        private void CreateCombinedCylinderRepresentation(PolyFiberData dataset)
        {
            cylinderVis = new CylinderObjectVis();

            //Creation of Meshes
            List<Mesh> listOfFiberMeshes = new List<Mesh>((int)dataset.NumberOfFibers);
            for (int fiber = 0; fiber < dataset.NumberOfFibers; fiber++)
            {
                listOfFiberMeshes.Add(cylinderVis.CreateMesh(dataset.Label[fiber].ToString(), dataset.GetFiberRadius(fiber), dataset.GetFiberCoordinates(fiber)));
            }

            List<Mesh> combinedMeshes = meshManager.CreateCombinedMesh(listOfFiberMeshes);

            for (int i = 0; i < combinedMeshes.Count; i++)
            {
                GameObject fiberMeshObj = new GameObject("FiberMesh_" + i);
                fiberMeshObj.transform.SetParent(polyModel.transform);

                MeshFilter currentMeshFilter = fiberMeshObj.AddComponent<MeshFilter>();
                MeshRenderer currentMeshRenderer = fiberMeshObj.AddComponent<MeshRenderer>();

                currentMeshFilter.mesh = combinedMeshes[i];
                currentMeshRenderer.material = cylinderMaterial;
            }
        }

        /// <summary>
        /// The Method iterates through all selected fibers and highlights them.
        /// Sets the selected parts of the mesh to selection color and the other parts to default color.
        /// </summary>
        /// <param name="selectedFiberIDs"></param>
        /// <param name="selectionColor"></param>
        public void HighlightFibers(List<int> selectedFiberIDs, Color selectionColor)
        {
            //TODO: Add new selection to the still stored one?
            this.selectedFiberIDs = selectedFiberIDs;

            foreach (var fiberID in selectedFiberIDs)
            {
                MeshInteractions.ColorMesh(meshManager.GetCombinedMesh(meshManager.GetIndexOfCombinedMesh(fiberID)), meshManager.GetMeshVerticeIndices(fiberID), new Color[] { selectionColor, defaultCol });
            }

        }

        /// <summary>
        /// The Method iterates through all selected fibers and resets their highlighting (sets the mesh to default color)
        /// </summary>
        public void ResetHighlighting()
        {
            foreach (var fiberID in selectedFiberIDs)
            {
                MeshInteractions.ColorMesh(meshManager.GetCombinedMesh(meshManager.GetIndexOfCombinedMesh(fiberID)), meshManager.GetMeshVerticeIndices(fiberID), new Color[] { defaultCol, defaultCol });
            }

            selectedFiberIDs.Clear();    // Clear selected Fiber IDs
        }

        public void TranslateFibers(List<int> selectedFiberIDs, Vector3 translation)
        {
            foreach (var fiberID in selectedFiberIDs)
            {
                MeshInteractions.TranslateMesh(meshManager.GetCombinedMesh(meshManager.GetIndexOfCombinedMesh(fiberID)), meshManager.GetMeshVerticeIndices(fiberID), translation);
            }
        }

    }

}