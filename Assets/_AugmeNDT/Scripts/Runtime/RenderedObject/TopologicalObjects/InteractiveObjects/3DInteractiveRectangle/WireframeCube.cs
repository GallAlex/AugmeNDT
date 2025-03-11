namespace AugmeNDT
{
    using UnityEngine;

    /// <summary>
    /// Creates and manages a wireframe cube with resizable faces
    /// </summary>
    public class WireframeCube : MonoBehaviour
    {
        public Vector3 cubeSize = new Vector3(11.39501f, 5.990006f, 13.065f);
        private LineRenderer lineRenderer;
        private GameObject[] resizeHandles = new GameObject[6]; // One for each face

        /// <summary>
        /// Initializes the wireframe cube and its resize handles
        /// </summary>
        void Start()
        {
            // Set up line renderer component for drawing the wireframe
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 16;
            lineRenderer.loop = false;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.useWorldSpace = false;
            UpdateWireframe();

            // Create handles for resizing each face
            CreateResizeHandles();
        }

        /// <summary>
        /// Creates resize handles for each face of the cube
        /// </summary>
        void CreateResizeHandles()
        {
            // Define the center position of each face
            Vector3[] faceCenters = new Vector3[]
            {
                new Vector3(0, 0, cubeSize.z / 2),   // Front
                new Vector3(0, 0, -cubeSize.z / 2),  // Back
                new Vector3(cubeSize.x / 2, 0, 0),   // Right
                new Vector3(-cubeSize.x / 2, 0, 0),  // Left
                new Vector3(0, cubeSize.y / 2, 0),   // Top
                new Vector3(0, -cubeSize.y / 2, 0)   // Bottom
            };

            // Create a handle for each face
            for (int i = 0; i < 6; i++)
            {
                resizeHandles[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                resizeHandles[i].transform.SetParent(transform);
                resizeHandles[i].transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                resizeHandles[i].transform.localPosition = faceCenters[i];
                resizeHandles[i].GetComponent<Renderer>().material.color = Color.red;
                resizeHandles[i].AddComponent<ResizeHandle>().Init(this, i);
            }
        }

        /// <summary>
        /// Resizes a specific face of the cube
        /// </summary>
        /// <param name="faceIndex">Index of the face to resize (0-5)</param>
        /// <param name="amount">Amount to resize by</param>
        public void ResizeFace(int faceIndex, float amount)
        {
            Vector3 resizeDirection = Vector3.zero;

            // Determine resize direction based on face index
            switch (faceIndex)
            {
                case 0: resizeDirection = new Vector3(0, 0, 1); break;  // Front
                case 1: resizeDirection = new Vector3(0, 0, -1); break; // Back
                case 2: resizeDirection = new Vector3(1, 0, 0); break;  // Right
                case 3: resizeDirection = new Vector3(-1, 0, 0); break; // Left
                case 4: resizeDirection = new Vector3(0, 1, 0); break;  // Top
                case 5: resizeDirection = new Vector3(0, -1, 0); break; // Bottom
            }

            // Apply resize and update the wireframe
            cubeSize += resizeDirection * amount;
            UpdateWireframe();
        }

        /// <summary>
        /// Updates the wireframe vertices based on current cube size
        /// </summary>
        void UpdateWireframe()
        {
            // Calculate the eight corners of the cube
            Vector3[] corners = new Vector3[8];
            float x = cubeSize.x / 2, y = cubeSize.y / 2, z = cubeSize.z / 2;

            corners[0] = new Vector3(-x, -y, -z);  // Back bottom left
            corners[1] = new Vector3(x, -y, -z);   // Back bottom right
            corners[2] = new Vector3(x, y, -z);    // Back top right
            corners[3] = new Vector3(-x, y, -z);   // Back top left
            corners[4] = new Vector3(-x, -y, z);   // Front bottom left
            corners[5] = new Vector3(x, -y, z);    // Front bottom right
            corners[6] = new Vector3(x, y, z);     // Front top right
            corners[7] = new Vector3(-x, y, z);    // Front top left

            // Define the edges connecting the corners
            Vector3[] edges = new Vector3[]
            {
                corners[0], corners[1], corners[2], corners[3], corners[0],  // Back face
                corners[4], corners[5], corners[6], corners[7], corners[4],  // Front face
                corners[0], corners[4], corners[1], corners[5], corners[2], corners[6], corners[3], corners[7]  // Connecting edges
            };

            // Update the line renderer with the new edges
            lineRenderer.positionCount = edges.Length;
            lineRenderer.SetPositions(edges);
        }
    }
}