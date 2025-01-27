using UnityEngine;
using UnityEngine.UI;

public class UIButtonManager : MonoBehaviour
{
    public static UIButtonManager Instance; 

    [SerializeField] private GameObject buttonPrefab; 
    [SerializeField] private Transform buttonParent; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CreateButton(string buttonText, Vector2 position, UnityEngine.Events.UnityAction onClickAction)
    {
        if (buttonPrefab == null)
        {
            Debug.LogError("UIButtonManager: Button prefab is not assigned!");
            return;
        }

        if (buttonParent == null)
        {
            buttonParent = GameObject.Find("Canvas").transform;
            if (buttonParent == null)
            {
                Debug.LogError("UIButtonManager: No Canvas found in scene!");
                return;
            }
        }

        GameObject newButton = Instantiate(buttonPrefab, buttonParent);
        newButton.name = "UIButton_" + buttonText;

        RectTransform rectTransform = newButton.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        Text buttonTextComponent = newButton.GetComponentInChildren<Text>();
        if (buttonTextComponent != null)
        {
            buttonTextComponent.text = buttonText;
        }

        Button buttonComponent = newButton.GetComponent<Button>();
        buttonComponent.onClick.AddListener(onClickAction);

        Debug.Log($"UIButtonManager: button created -> {buttonText}");
    }
}
