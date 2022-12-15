using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Class is used to fill the scroll list with Buttons
/// </summary>
public class ScrollGridUtility : MonoBehaviour
{

    public GameObject buttonPrefab;
    public ScrollingObjectCollection scrollingObjectColl;
    public GridObjectCollection gridObjectColl;
    public GameObject gridObjectContainer;
    public ClippingBox clippingBox;

    void Start()
    {
        // Load Button Prefab
        if (buttonPrefab == null) buttonPrefab = (GameObject)Resources.Load("Prefabs/Button_32x96");
    }

    // Fills the Grid with Buttons with specific Text and Event
    public void FillScrollGrid(List<string> buttonNames)
    {

        //TODO: Change to dynamic loading based on looked at Vis
        //Dictionary<string, double[]> dataVal = fileLoadingManager.visList[0].dataValues;

        // Create Buttons for each Letter and add them to the Grid
        for (int bID = 0; bID < buttonNames.Count; bID++)
        {
            GameObject buttonInstance = Instantiate(buttonPrefab, gridObjectContainer.transform);
            buttonInstance.name = buttonNames[bID];
            buttonInstance.transform.SetParent(gridObjectContainer.transform, false);
            buttonInstance.GetComponentInChildren<TextMeshPro>().text = buttonNames[bID];
            buttonInstance.GetComponentInChildren<ButtonConfigHelper>().OnClick.AddListener(() => ReturnButtonId(bID));
        }

        // Add all the element's renderers to the clipping box
        Renderer[] renderersToClip = gridObjectContainer.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderersToClip.Length; i++)
        {
            clippingBox.AddRenderer(renderersToClip[i]);
        }

        UpdateScrollGridGrid();
    }

    // Updates the Grid and ScrollingObjectCollection
    public void UpdateScrollGridGrid()
    {
        gridObjectColl.UpdateCollection();
        scrollingObjectColl.UpdateContent();
    }

    public int ReturnButtonId(int buttonId)
    {
        Debug.Log("Button ID: " + buttonId);
        return buttonId;
    }

}
