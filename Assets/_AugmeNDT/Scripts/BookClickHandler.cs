using AugmeNDT;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BookClickHandler : MonoBehaviour
{
    public FlipPage flipPage;
    public PopulateIndexPage populateIndexPage;
    public GameObject pagePopulatorObject;
    private PagePopulator pagePopulator; 
    public string folderPath;
    [SerializeField] GameObject UserInterface;

    [SerializeField] GameObject closedBook;

    private float interactionStartTime;
    private const float clickThreshold = 0.2f; 

    public void OnSelectEnter()
    {
       
        interactionStartTime = Time.time;
    }

    public void OnSelectExit()
    {
        
        float interactionDuration = Time.time - interactionStartTime;
        if (interactionDuration < clickThreshold)
        {
            OnBookClicked();
        }
    }
    // This method is called when the book is clicked
    public void OnBookClicked()
    {

        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("Folder path is empty or null!");
        }
        else
        {

            Debug.Log("Book was clicked! Path: " + folderPath);
            UserInterface.SetActive(false);
            // Add logic to handle the book click event, such as opening the book or loading content
            closedBook.SetActive(true);
           // pagePopulator.PopulatePages(folderPath);
           if(flipPage == null)
            {
                Debug.Log("flipPage instance not found");
            }
            flipPage.SetFolderPath(folderPath);
            populateIndexPage.setFolderPath(folderPath);


        }
    }
}