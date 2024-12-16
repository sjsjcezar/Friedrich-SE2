using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Character Prefabs")]
    public GameObject playerBoyPrefab;
    public GameObject playerGirlPrefab;

    [Header("Character Display")]
    public GameObject boyCharacterDisplay; // Visual representation of the boy character
    public GameObject girlCharacterDisplay; // Visual representation of the girl character

    [Header("UI Elements")]
    public Button boyButton;
    public Button girlButton;
    public Button confirmButton;
    public GameObject nameInputPanel;
    public TMP_InputField nameInputField;
    public TMP_Text namePromptText;
    public Button submitNameButton;
    public Button cancelNameButton;
    public Button yesButton;
    public Button noButton;
   // public GameObject classSelectionPanel;

    private GameObject selectedCharacterPrefab;
    private string playerName = "";

    [Header("Error Message")]
    public GameObject errorPanel;
    public TMP_Text errorText;

    private const int MAX_NAME_LENGTH = 8;
    private const float ERROR_DISPLAY_TIME = 2f;


    [Header("Cutscene")]
    public CutsceneManager cutsceneManager;
    public string nextSceneName = "GameWorld";

    void Start()
    {
        // Set default character to girl and show only the girl model
        selectedCharacterPrefab = playerGirlPrefab;
        ShowCharacterDisplay();
        UpdateCharacterSelection();

        // Hide name input panel initially
        nameInputPanel.SetActive(false);

        // Set up button listeners
        boyButton.onClick.AddListener(() => SelectCharacter(playerBoyPrefab));
        girlButton.onClick.AddListener(() => SelectCharacter(playerGirlPrefab));
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmButtonClick);
        submitNameButton.onClick.AddListener(OnNameEntered);
        cancelNameButton.onClick.AddListener(HideNameInput);
        yesButton.onClick.AddListener(OnConfirmJourneyStart);
        noButton.onClick.AddListener(HideNameInput);

        // Hide Yes/No buttons initially
        nameInputPanel.SetActive(false);
        submitNameButton.gameObject.SetActive(false);
        cancelNameButton.gameObject.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        errorPanel.SetActive(false);
        boyButton.gameObject.SetActive(true);

        ShowGenderButtons();
        HideNameInput();
        HideError();
    }

    private void SelectCharacter(GameObject characterPrefab)
    {
        selectedCharacterPrefab = characterPrefab;
        UpdateCharacterSelection();
        ShowCharacterDisplay();
    }


    private void UpdateCharacterSelection()
    {
        // Update the UI to reflect the currently selected character
        boyButton.interactable = selectedCharacterPrefab != playerBoyPrefab;
        girlButton.interactable = selectedCharacterPrefab != playerGirlPrefab;
    }

    private void ShowCharacterDisplay()
    {
        // Show or hide character displays based on the selected character
        if (selectedCharacterPrefab == playerBoyPrefab)
        {
            boyCharacterDisplay.SetActive(true);
            girlCharacterDisplay.SetActive(false);
        }
        else if (selectedCharacterPrefab == playerGirlPrefab)
        {
            boyCharacterDisplay.SetActive(false);
            girlCharacterDisplay.SetActive(true);
        }
    }

    private void OnConfirmButtonClick()
    {
        if (selectedCharacterPrefab == playerBoyPrefab)
        {
            ShowError("Sorry! We don't have animations for this :(");
        }
        else
        {
            ShowNameInput();
        }
    }
    private void ShowNameInput()
    {
        nameInputPanel.SetActive(true);
        namePromptText.gameObject.SetActive(true);
        nameInputField.gameObject.SetActive(true);
        namePromptText.text = "What is your name?";
        nameInputField.interactable = true;
        nameInputField.text = "";
        submitNameButton.gameObject.SetActive(true);
        cancelNameButton.gameObject.SetActive(true);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
    }


    private void HideNameInput()
    {
        nameInputPanel.SetActive(false);
        namePromptText.gameObject.SetActive(false);
        nameInputField.gameObject.SetActive(false);
        submitNameButton.gameObject.SetActive(false);
        cancelNameButton.gameObject.SetActive(false);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
    }

    private void ShowGenderButtons()
    {
        boyButton.gameObject.SetActive(true);
        girlButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);
    }

    private void HideGenderButtons()
    {
        boyButton.gameObject.SetActive(false);
        girlButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
    }

    public void OnNameEntered()
    {
        playerName = nameInputField.text.Trim();
        if (ValidateName(playerName))
        {
            namePromptText.text = $"Are you ready to begin your journey, {playerName}?";
            nameInputField.gameObject.SetActive(false);
            submitNameButton.gameObject.SetActive(false);
            cancelNameButton.gameObject.SetActive(false);
            yesButton.gameObject.SetActive(true);
            noButton.gameObject.SetActive(true);
        }
    }
    
    private bool ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            ShowError("Please enter a name.");
            return false;
        }
        if (name.Length > MAX_NAME_LENGTH)
        {
            ShowError($"Name must be {MAX_NAME_LENGTH} characters or less.");
            return false;
        }
        if (name.Length == 1)
        {
            ShowError("Name must be at least 2 characters long.");
            return false;
        }
        if (!Regex.IsMatch(name, @"^[a-zA-Z]+$"))
        {
            ShowError("Name can only contain letters.");
            return false;
        }
        return true;
    }

    private void ShowError(string message)
    {
        errorText.text = message;
        errorPanel.SetActive(true);
        errorText.gameObject.SetActive(true);
        StartCoroutine(HideErrorAfterDelay());
    }

    private void HideError()
    {
        errorPanel.SetActive(false);
        errorText.gameObject.SetActive(false);
    }

    private IEnumerator HideErrorAfterDelay()
    {
        yield return new WaitForSeconds(ERROR_DISPLAY_TIME);
        HideError();
    }

    public void OnConfirmJourneyStart()
    {
        HideGenderButtons();
        HideNameInput(); // Hide UI elements
       StartCoroutine(StartCutsceneSequence());
    }

    public void OnDeclineJourneyStart()
    {
        // Go back to name input
        ShowNameInput();
    }


    private IEnumerator StartCutsceneSequence()
    {
        // Start the cutscene directly without character instantiation
        cutsceneManager.StartCutscene();

        // Wait for the cutscene to finish
        yield return new WaitUntil(() => cutsceneManager.IsCutsceneFinished);

        // Transition to the next scene
        SceneManager.LoadScene(nextSceneName);
    }
}

/*
    public void ShowClassSelection()
    {
        // Show class selection based on the character chosen
        classSelectionPanel.SetActive(true);
        if (selectedCharacterPrefab == playerBoyPrefab)
        {
            // Show boy-specific classes
            // e.g., warriorBoy and mageBoy
        }
        else if (selectedCharacterPrefab == playerGirlPrefab)
        {
            // Show girl-specific classes
            // e.g., warriorGirl and mageGirl
        }
    }
}
*/
