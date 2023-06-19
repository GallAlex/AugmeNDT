using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Class handles the loaded datasets and their respective renderings and data visualizations
/// </summary>
public class SceneObjectHandler : MonoBehaviour
{
    // Used to load data in various file formats
    private FileLoadingManager fileLoadingManager;

    // Parent Container which stores all DataVisGroups in the scene
    [SerializeField] 
    private GameObject sceneObjectsContainer;
    // GridObjectCollection used to arrange all DataVisGroups in the scene
    private GridObjectCollection gridColl;
    
    // Stores all loaded data & its derived representations
    [SerializeField]
    private List<DataVisGroup> dataVisGroups;

    // Combines stored groups for combined representations
    [SerializeField]
    private Dictionary<int, DataVisGroup> multiGroups;

    // When introducing multiple DataVisGroups rearrange first the singular Objects in each group
    private bool rearrangeObjects = true;

    // Called only once during the lifetime of the script instance (loading of a scene)
    void Awake()
    {
        // Initialize all members
        fileLoadingManager = new FileLoadingManager();
        dataVisGroups = new List<DataVisGroup>();
        multiGroups = new Dictionary<int, DataVisGroup>();

        sceneObjectsContainer = new GameObject("Scene Objects");
    }

    void Update()
    {
        
        //Move to individual vis Object(monobehaviour ?)
        if (dataVisGroups.Count > 0)
        {
            foreach (DataVisGroup group in dataVisGroups)
            {
                group.AlignGridPositions();
            }
        }
        
    }

    /// <summary>
    /// Returns the amount of loaded dataVisGroups
    /// </summary>
    /// <returns></returns>
    public int GetAmountOfDataVisGroups()
    {
        return dataVisGroups.Count;
    }

    /// <summary>
    /// Loads a file based on the picked file path and renders all possible representations
    /// </summary>
    /// <returns></returns>
    public async Task<string> LoadObject()
    {
        // Start async Loadings
        string filePath = await fileLoadingManager.StartPicker();
        
        // If file loading failed
        if (filePath == "")
        {
            return null;
        }

        Debug.Log("Filepath found is: " + filePath);

        bool loadingSucceded = await fileLoadingManager.LoadDataset();

        //## Wait for Loadings to finish ##
        if (!loadingSucceded)
        {
            Debug.LogError("Loading aborted!");
        }

        // Add Group
        dataVisGroups.Add(fileLoadingManager.GetDataVisGroup());
        int lastIndex = dataVisGroups.Count - 1;

        // Render all representations (whose data is available)
        dataVisGroups[lastIndex].RenderAll(VisType.MDDGlyphs);


        dataVisGroups[lastIndex].ArrangeObjectsSpatially();
        
        // Add Group to sceneObjectsContainer container
        dataVisGroups[lastIndex].GetGroupContainer().transform.parent = sceneObjectsContainer.transform;
        ArrangeGroupsSpatially();

        return filePath;
    }

    /// <summary>
    /// Method iterates through all selected groups and adds them to the multiGroups dictionary (if found in the dataVisGroups list).
    /// </summary>
    /// <param name="selectedGroups"></param>
    public void CreateMultiGroup(List<int> selectedGroups)
    {
        foreach (var selection in selectedGroups)
        {
            DataVisGroup temp = dataVisGroups.FirstOrDefault(groupID => groupID.GetGroupID() == selection);

            if (temp != null) multiGroups.Add(selection, temp);
        }
    }

    /// <summary>
    /// Arranges all groups in a [2 by n] grid and moves it to the best initial start position
    /// </summary>
    public void ArrangeGroupsSpatially()
    {
        
        if (rearrangeObjects)
        {
            // First arrange all individual groups again
            foreach (var group in dataVisGroups)
            {
                group.ArrangeObjectsSpatially();
            }
        }

        if (gridColl == null)
        {
            gridColl = sceneObjectsContainer.AddComponent<GridObjectCollection>();
            //gridColl.SurfaceType = ObjectOrientationSurfaceType.Cylinder;
            gridColl.CellWidth = 0.60f; //Todo: Use twice the biggest size of the individual grids in the groups
            gridColl.CellHeight = 0.3f; //Todo: Use twice the biggest size of the individual grids in the groups
            gridColl.SortType = CollationOrder.ChildOrder;
            gridColl.Layout = LayoutOrder.ColumnThenRow;
            gridColl.Columns = 2;
            gridColl.Anchor = LayoutAnchor.BottomLeft;
            gridColl.AnchorAlongAxis = true;
        }
        
        gridColl.UpdateCollection();

        GlobalScaleAndPos.SetToBestInitialStartPos(sceneObjectsContainer.transform);
    }

    //#####################     VOLUME METHODS      #####################

    public void ChangeVolumeShader(int selectedGroup, Shader shader)
    {
        //TODO: Change int selectedVolume to reference of Object during interaction
        dataVisGroups[selectedGroup].ChangeVolumeShader(shader);
    }

    //#####################     POLY MODEL METHODS  #####################

    public void HighlightPolyFibers(int selectedGroup, List<int> fiberIDs, Color selectedColor)
    {
        dataVisGroups[selectedGroup].HighlightPolyFibers(fiberIDs, selectedColor);
    }

    //#####################     VIS CHART METHODS   #####################

    /// <summary>
    /// Returns the amount of attributes the Abstract of the selected group
    /// </summary>
    /// <param name="selectedGroup"></param>
    /// <returns></returns>
    public int GetAttributeCount(int selectedGroup)
    {
        //Todo:
        return 0;
    }

    /// <summary>
    /// Adds a new abstract visualization object to the selected group and renders it
    /// </summary>
    /// <param name="selectedGroup"></param>
    /// <param name="visType"></param>
    public void AddAbstractVisObject(int selectedGroup, VisType visType)
    {
        dataVisGroups[selectedGroup].RenderAbstractVisObject(visType);
        //Arrange Vis objects
        dataVisGroups[selectedGroup].ArrangeObjectsSpatially();
    }

    /// <summary>
    /// Adds a new abstract visualization object to the selected group and renders it
    /// </summary>
    /// <param name="selectedGroup"></param>
    /// <param name="visType"></param>
    public void AddAbstractVisObject(int selectedGroup, VisType visType, int[] visualizedAttributes)
    {
        dataVisGroups[selectedGroup].RenderAbstractVisObject(visType, visualizedAttributes);
        //Arrange Vis objects
        dataVisGroups[selectedGroup].ArrangeObjectsSpatially();
    }

    /// <summary>
    /// Renders a 4D abstract visualization object combining all csv files found in the groups
    /// </summary>
    public void RenderAbstractVisObjectForMultiGroup()
    {
        Debug.Log("Render Abstract Vis Object of MultiGroup");

        Vis vis = new VisMDDGlyphs();

        foreach (var group in multiGroups)
        {
            //vis.AppendData(group.Value.GetPolyData().ExportForDataVis());
            vis.AppendData(group.Value.GetAbstractCsvData());
        }

        vis.CreateVis(new GameObject("MultiGroupVis"));
    }

    public void ChangeAxis(int selectedGroup, int selectedVis,  int axisID, int selectedDimension, int numberOfTicks)
    {
        //TODO: Change int selectedVis to reference of Object during interaction
        dataVisGroups[selectedGroup].ChangeAxis(selectedVis, axisID, selectedDimension, numberOfTicks);
    }

}
