using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestJournalUI : MonoBehaviour
{
    public static QuestJournalUI Instance { get; private set; }
    
    [Header("Panel References")]
    public GameObject journalPanel;
    public GameObject questListPanel;
    public GameObject questDescriptionPanel;
    
    [Header("Quest Category Buttons")]
    public Button mainQuestsButton;
    public Button sideQuestsButton;
    public Button completedQuestsButton;
    
    [Header("Quest Description Elements")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescription;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questRequirements;
    public TextMeshProUGUI questRequirementsText;
    public TextMeshProUGUI questHintText;
    
    [Header("Quest List Container")]
    public Transform questListContent;
    public GameObject questButtonPrefab;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        questDescriptionPanel.SetActive(false);
        journalPanel.SetActive(false);
        questTitleText.gameObject.SetActive(false);
        questDescription.gameObject.SetActive(false);
        questDescriptionText.gameObject.SetActive(false);
        questRequirements.gameObject.SetActive(false);
        questRequirementsText.gameObject.SetActive(false);
        questHintText.gameObject.SetActive(false);
        SetupButtonListeners();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleJournal();
        }
    }

    private void SetupButtonListeners()
    {
        mainQuestsButton.onClick.AddListener(() => ShowQuestsByType(true));
        sideQuestsButton.onClick.AddListener(() => ShowQuestsByType(false));
        completedQuestsButton.onClick.AddListener(ShowCompletedQuests);
    }

    // Add this to your QuestJournalUI Start method
    private void SetupQuestListLayout()
    {
        // Get or add Grid Layout Group
        GridLayoutGroup gridLayout = questListContent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = questListContent.gameObject.AddComponent<GridLayoutGroup>();
        }

        // Configure Grid Layout
        gridLayout.cellSize = new Vector2(200f, 40f); // Adjust size as needed
        gridLayout.spacing = new Vector2(10f, 10f);
        gridLayout.padding = new RectOffset(10, 10, 10, 10);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;
    }

    private void ToggleJournal()
    {
        bool isOpening = !journalPanel.activeSelf;
        journalPanel.SetActive(isOpening);
        Time.timeScale = isOpening ? 0f : 1f;
        
        if (isOpening)
        {
            ShowQuestsByType(true); // Show main quests by default
            questDescriptionPanel.SetActive(false);
            questDescription.gameObject.SetActive(false);
            questTitleText.gameObject.SetActive(false);
            questDescriptionText.gameObject.SetActive(false);
            questRequirements.gameObject.SetActive(false);
            questRequirementsText.gameObject.SetActive(false);
            questHintText.gameObject.SetActive(false);
        }
    }

    private void ShowQuestsByType(bool isMainQuest)
    {
        ClearQuestList();
        questDescriptionPanel.SetActive(false);
        questDescriptionPanel.SetActive(false);
        questDescription.gameObject.SetActive(false);
        questTitleText.gameObject.SetActive(false);
        questDescriptionText.gameObject.SetActive(false);
        questRequirements.gameObject.SetActive(false);
        questRequirementsText.gameObject.SetActive(false);    
        questHintText.gameObject.SetActive(false);
        
        var quests = QuestManager.Instance.GetActiveQuestsByType(isMainQuest);
        
        foreach (var quest in quests)
        {
            CreateQuestButton(quest);
        }
    }

    private void ShowCompletedQuests()
    {
        ClearQuestList();
        questDescriptionPanel.SetActive(false);
        var quests = QuestManager.Instance.GetCompletedQuests();
        
        foreach (var quest in quests)
        {
            CreateQuestButton(quest);
        }
    }


    private void CreateQuestButton(QuestSO quest)
    {
        GameObject buttonObj = Instantiate(questButtonPrefab, questListContent);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        
        buttonText.text = quest.title;
        button.onClick.AddListener(() => ShowQuestDetails(quest));
    }
        

    private void ShowQuestDetails(QuestSO quest)
    {
        questTitleText.text = quest.title;
        questDescriptionText.text = quest.description;
        questDescription.gameObject.SetActive(true);
        questTitleText.gameObject.SetActive(true);
        questDescriptionText.gameObject.SetActive(true);
        questRequirements.gameObject.SetActive(true);
        questRequirementsText.gameObject.SetActive(true);
        questHintText.gameObject.SetActive(true);
        
        if (QuestManager.Instance.IsQuestActive(quest.questID))
        {
            int currentSegment = QuestManager.Instance.GetCurrentSegmentIndex(quest.questID);
            questRequirementsText.text = quest.segments[currentSegment].requirementText;
            questHintText.text = quest.segments[currentSegment].navigationHint;
        }
        else if (QuestManager.Instance.IsQuestCompleted(quest.questID))
        {
            questRequirementsText.text = "Quest Completed";
            questHintText.text = "";
        }
        
        questDescriptionPanel.SetActive(true);
    }

    private void ClearQuestList()
    {
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }
    }
}