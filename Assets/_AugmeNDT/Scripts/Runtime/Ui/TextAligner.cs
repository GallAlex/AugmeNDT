using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Class is used to align Text (TextMesh) with predefined positions if they overlap.
    /// It needs the positions of the Texts and the Texts itself.
    /// </summary>
    public class TextAligner : MonoBehaviour
    {
        public TextMesh textMesh;
        public float size;
        private GameObject text3DPrefab;

        private List<GameObject> textLabels;
        private List<string> labelText;
        private List<Vector3> labelPositions;


        void Update()
        {
            size = GetTextMeshWidth(textMesh);
        }

        public TextAligner()
        {
            text3DPrefab = (GameObject)Resources.Load("Prefabs/Text3D");
        }

        public void CreateLabel(string text, Vector3 position, Transform transform)
        {
            CreateLabelObject(position, transform, text);
        }


        private void CreateLabelObject(Vector3 position, Transform transform, string text)
        {
            GameObject label = GameObject.Instantiate(text3DPrefab, position, Quaternion.identity, transform);
            TextMesh labelText = label.GetComponent<TextMesh>();
            labelText.text = text;
            labelText.anchor = TextAnchor.MiddleLeft;
            labelText.fontSize = 50;
        }

        private float GetTextMeshWidth(TextMesh textMesh)
        {
            string[] textLines = textMesh.text.Split(' ');
            int widestLineWidth = 0;

            // Iterate through each line of text
            foreach (string textLine in textLines)
            {
                int width = 0;

                // Iterate through each symbol in the current text line
                foreach (char symbol in textLine)
                {
                    if (textMesh.font.GetCharacterInfo(symbol, out CharacterInfo charInfo, textMesh.fontSize, textMesh.fontStyle))
                        width += charInfo.advance;
                }

                if (widestLineWidth <= 0 || width > widestLineWidth)
                    widestLineWidth = width;
            }

            // Multiplied by 0.1 to make the size of this match the bounds size of meshes (which is 10x larger)
            return widestLineWidth * textMesh.characterSize * Mathf.Abs(textMesh.transform.lossyScale.x) * 0.1f;
        }
    }
}
