using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Handles the visualization of vector fields using arrow prefabs.
    /// Uses gradient data to create 3D arrows.
    /// </summary>
    public class VectorFieldObjectVis : MonoBehaviour
    {
        public GameObject arrowPrefab; // Prefab for the arrow representation of vectors
        public static VectorFieldObjectVis Instance; // Singleton instance

        private TopologicalDataObject topologicalDataInstance; // Reference to the topological data instance containing gradient data
        private List<GameObject> arrows = new List<GameObject>(); // List to store all created arrows for toggling visibility
        
        private Transform container;         // Parent container for all arrows in the scene
        
        // Flags to check if arrows are already created and if they are currently hidden
        private bool arrowscalculated = false;
        private bool arrowshidden = false;
        private static ArrowObjectVis arrowObjectVisInstance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        void Start()
        {
            if (topologicalDataInstance == null)
                topologicalDataInstance = TopologicalDataObject.Instance;

            if (arrowObjectVisInstance == null)
                arrowObjectVisInstance = ArrowObjectVis.Instance;
            container = new GameObject("GeneralVectorFieldArrows").transform;
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
                arrows = arrowObjectVisInstance.CreateArrows(topologicalDataInstance.gradientList, arrowPrefab, container);
                arrowscalculated = true;
                arrowshidden = false;
            }
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
