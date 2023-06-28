using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT{
    /// <summary>
    /// The class controls what happens during an interaction with the VisMDDGlyph visualization. 
    /// </summary>
    public class VisMDDGlyphInteractor : VisInteractor
    {
        private VisMDDGlyphs visMddGlyphs;

        public VisMDDGlyphInteractor(VisMDDGlyphs visMddGlyphs)
        {
            this.visMddGlyphs = visMddGlyphs;
        }

        public override void OnTouch(string ID)
        {
            int selectedGlyph = VisInteractor.GetIDNumber(ID);

            // Highlight selected Glyph
            DataMark dataMark = visMddGlyphs.visContainer.dataMarkList[selectedGlyph];
            var glyphObject = dataMark.GetDataMarkInstance();

            // Add Outline Component if not already present
            var outline = glyphObject.GetComponent<Outline>() != null ? glyphObject.GetComponent<Outline>() : glyphObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = Color.green;
            outline.OutlineWidth = 3f;
        
            // Select all values == fibers which are covered by the encoded range in this Glyph 
            // As Glyphs are numbered by Order of Attributes, selectedGlyph ID equals Attribute ID
            // Todo: Maybe use Scale to get from Glyph values in Chart back to Data Values
            List<int> selectedFiberIds = visMddGlyphs.GetFiberIDsFromIQRRange(selectedGlyph, 1);

            Debug.Log("[" + selectedFiberIds.Count + "] Selected Fibers");

            //Check if Glyph is currently selected
            // If selected Unhighlight
            if (dataMark.selected)
            {
                dataMark.selected = false;
                outline.enabled = false;
            
                // Call Method of DataVisGroup Interactor
                // Color Polyfibers of selected FiberIds
                visMddGlyphs.GetDataVisGroup().HighlightPolyFibers(selectedFiberIds, Color.white);
            }
            // If not selected Highlight
            else
            {
                outline.enabled = true;
                dataMark.selected = true;

                // Call Method of DataVisGroup Interactor
                // Color Polyfibers of selected FiberIds
                visMddGlyphs.GetDataVisGroup().HighlightPolyFibers(selectedFiberIds, Color.green);
            }
        
        }

        public override void OnClick(string ID)
        {
        
        }


    
    }
}
