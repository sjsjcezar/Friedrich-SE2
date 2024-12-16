using UnityEngine;

public class QuestTriggerZone : MonoBehaviour
{
    public static QuestTriggerZone instance;
    [Header("Quest Settings")]
    public QuestSO questToTrigger;
    public QuestManager questManager;
    public QuestNotificationUI questNotificationUI;

    private bool hasTriggered = false;


    private void Awake()
    {
        questManager = FindObjectOfType<QuestManager>();
        questNotificationUI = FindObjectOfType<QuestNotificationUI>();

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        
        // Check if quest can be triggered
        if (questManager.IsQuestActiveOrCompleted(questToTrigger.questID))
        {
            return;
        }

        // Trigger the quest
        questManager.StartQuest(questToTrigger);
//        questNotificationUI.ShowNotification($"New Quest: {questToTrigger.title}");
        hasTriggered = true;
        
        // Disable the collider after triggering : temporary fix
        GetComponent<Collider2D>().enabled = false;
    }
}