using System.Collections.Generic;
using UnityEngine;

//TODO: Remove Class
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
        
            // Select all values == fibers which are covered by the encoded range in this Glyph 
            // As Glyphs are numbered by Order of Attributes, selectedGlyph ID equals Attribute ID
            // Todo: Maybe use Scale to get from Glyph values in Chart back to Data Values
            List<int> selectedFiberIds = visMddGlyphs.GetFiberIDsFromIQRRange(selectedGlyph);

            Debug.Log("[" + selectedFiberIds.Count + "] Selected Fibers");

            //Check if Glyph is currently selected
            // If selected Unhighlight
            if (dataMark.selected)
            {
                dataMark.selected = false;
            
                // Call Method of DataVisGroup Interactor
                // Color Polyfibers of selected FiberIds
                visMddGlyphs.GetDataVisGroup().HighlightPolyFibers(selectedFiberIds, Color.white);
            }
            // If not selected Highlight
            else
            {
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
