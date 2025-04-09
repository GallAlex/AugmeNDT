using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Handles the visualization of vector fields using arrow prefabs.
    /// Uses gradient data to create 3D arrows.
    /// </summary>
    public class VectorFieldObjectVis : MonoBehaviour
    {
        // Singleton instance
        public static VectorFieldObjectVis instance;

        // Reference to the topological data instance containing gradient data
        private static TopologicalDataObject topologicalDataInstance;
        
        // Reference to the VectorObjectVis instance containing gradient data
        private static VectorObjectVis arrowObjectVisInstance;
        
        // List to store all created arrows for toggling visibility
        private List<GameObject> arrows = new List<GameObject>();
        
        // Parent container for all arrows in the scene
        private Transform container;

        // Flags to check if arrows are already created and if they are currently hidden
        private bool arrowscalculated = false;
        private bool arrowshidden = false;

        public static float localScaleRate = 0.3f;

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        void Start()
        {
            if (topologicalDataInstance == null)
                topologicalDataInstance = TopologicalDataObject.instance;

            if (arrowObjectVisInstance == null)
                arrowObjectVisInstance = VectorObjectVis.instance;
        }

        /// <summary>
        /// Generates and visualizes the vector field using gradient data.
        /// If arrows are already created, it simply makes them visible.
        /// Otherwise, it generates the arrows from `gradientList` in `TopologicalDataObject`.
        /// </summary>
        public void Visualize()
        {
            if (arrowscalculated)
            {
                ShowVectorField();
            }
            else
            {
                if (container == null)
                    SetContainer();

                arrows = arrowObjectVisInstance.CreateArrows(topologicalDataInstance.gradientList, container, localScaleRate);
                arrowscalculated = true;
                arrowshidden = false;
            }
        }

        private void SetContainer()
        {
            Transform fibers = GameObject.Find("DataVisGroup_0/fibers.raw").transform;

            GameObject generalVectorFieldArrows = new GameObject("GeneralVectorFieldArrows");
            container = generalVectorFieldArrows.transform;
            container.SetParent(fibers);
        }

        /// <summary>
        /// Hides all arrows in the vector field without destroying them.
        /// </summary>
        public void HideVectorField()
        {
            if (arrowshidden)
                return;

            arrows.ForEach(x => x.SetActive(false));
            arrowshidden = true;
        }

        /// <summary>
        /// Makes all arrows in the vector field visible again.
        /// </summary>
        public void ShowVectorField()
        {
            if (!arrowshidden)
                return;

            arrows.ForEach(x => x.SetActive(true));
            arrowshidden = false;
        }
    }
}
