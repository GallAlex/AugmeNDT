using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

/// <summary>
/// Class handels the created Visualizations in the Scene (Charts, Volumes, Poly Models, ...) regarding interactions, visibility,... </summary>
public class SceneVisHandler : MonoBehaviour
{
    [SerializeField]
    private List<VolumeRenderedObject> volumeRenderedObjectList;        // Stores all loaded & rendered Volumes
    [SerializeField]
    private List<PolyFiberRenderedObject> polyFiberRenderedObjectList;  // Stores all loaded & rendered Poly Fiber Models
    [SerializeField]
    public List<Vis> visObjectList;                                    // Stores all loaded & rendered Visulaizations

    private SceneFileHandler refSceneFileHandler;   // Reference to the SceneFileHandler

    void Update()
    {
        foreach (var vis in visObjectList)
        {
            vis.UpdateVis();
        }
    }

    /// <summary>
    /// Sets a reference to the scenFileHandler
    /// </summary>
    /// <param name="sceneFileHandler"></param>
    public void SetSceneFileHandler(SceneFileHandler sceneFileHandler)
    {
        refSceneFileHandler = sceneFileHandler;
    }

    public void UpdateRenderedObjects(FileLoadingManager flManager)
    {
        volumeRenderedObjectList = flManager.volumeRenderedObjectList;
        polyFiberRenderedObjectList = flManager.polyFiberRenderedObjectList;
        visObjectList = flManager.visList;
    }


    //#####################     VOLUME METHODS      #####################

    public void ChangeVolumeShader(int selectedVolume, Shader shader)
    {
        //TODO: Change int selectedVolume to reference of Object during interaction
        volumeRenderedObjectList[selectedVolume].ChangeShader(shader);
    }

    //#####################     POLY MODEL METHODS  #####################


    //#####################     VIS CHART METHODS   #####################

    public void ChangeAxis(int selectedVis, int axisID, int selectedDimension, int numberOfTicks)
    {
        //TODO: Change int selectedVis to reference of Object during interaction
        visObjectList[selectedVis].ChangeAxisAttribute(axisID, selectedDimension, numberOfTicks);
    }

}
