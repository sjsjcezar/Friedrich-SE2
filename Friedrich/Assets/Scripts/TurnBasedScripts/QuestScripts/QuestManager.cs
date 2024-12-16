using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    private Dictionary<int, QuestSO> activeQuests = new Dictionary<int, QuestSO>();
    private Dictionary<int, int> questProgress = new Dictionary<int, int>(); // QuestID -> Current Segment Index
    private Dictionary<int, QuestState> questStates = new Dictionary<int, QuestState>();

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


    private void Update()
    {
        if (activeQuests.Count > 0)
        {
            foreach (var quest in activeQuests.Values)
            {
                CheckQuestProgress(quest);
            }
        }
    }

    private void CheckQuestProgress(QuestSO quest)
    {
        int currentSegmentIndex = questProgress[quest.questID];
        QuestSegment currentSegment = quest.segments[currentSegmentIndex];

        switch (currentSegment.completionType)
        {
            case SegmentCompletionType.VisitLocation:
                CheckLocationProgress(quest, currentSegment);
                break;
            case SegmentCompletionType.KillEnemies:
                // Progress is updated through OnEnemyKilled
                break;
        }
    }

    public List<QuestSO> GetActiveQuestsByType(bool isMainQuest)
    {
        return activeQuests.Values
            .Where(q => isMainQuest ? q.questType == QuestType.Main : q.questType == QuestType.Side)
            .ToList();
    }

    public List<QuestSO> GetCompletedQuests()
    {
        return questStates
            .Where(q => q.Value == QuestState.Finished)
            .Select(q => Resources.Load<QuestSO>($"Quests/{q.Key}"))
            .ToList();
    }


    private void CheckLocationProgress(QuestSO quest, QuestSegment segment)
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == segment.targetSceneName)
        {
            Debug.Log($"Player reached target location: {segment.targetSceneName}");
            CompleteQuestSegment(quest.questID);
        }
    }


    public void OnNPCInteraction(string npcID)
    {
        Debug.Log($"Interacting with NPC: {npcID}");
        // Create a copy of the quest values to avoid modification during enumeration
        var activeQuestsList = activeQuests.Values.ToList();
        
        foreach (var quest in activeQuestsList)
        {
            int currentSegmentIndex = questProgress[quest.questID];
            QuestSegment currentSegment = quest.segments[currentSegmentIndex];

            if (currentSegment.completionType == SegmentCompletionType.TalkToNPC &&
                currentSegment.targetNPCID == npcID)
            {
                Debug.Log($"NPC interaction successful for quest: {quest.title}");
                CompleteQuestSegment(quest.questID);
            }
        }
    }

    public void OnEnemyKilled(CharacterType enemyType)
    {
        foreach (var quest in activeQuests.Values)
        {
            int currentSegmentIndex = questProgress[quest.questID];
            QuestSegment currentSegment = quest.segments[currentSegmentIndex];

            if (currentSegment.completionType == SegmentCompletionType.KillEnemies &&
                currentSegment.targetEnemyType == enemyType)
            {
                currentSegment.currentKills++;
                Debug.Log($"Enemy killed: {enemyType}. Progress: {currentSegment.currentKills}/{currentSegment.requiredKills}");
                
                if (currentSegment.currentKills >= currentSegment.requiredKills)
                {
                    CompleteQuestSegment(quest.questID);
                }
            }
        }
    }

    public void StartQuest(QuestSO quest)
    {
        if (!CanStartQuest(quest))
        {
            Debug.Log($"Cannot start quest: {quest.title}. Requirements not met.");
            return;
        }

        activeQuests[quest.questID] = quest;
        questProgress[quest.questID] = 0;
        questStates[quest.questID] = QuestState.In_Progress;
        
        // Only show start notification and first objective if it's not completed
        QuestNotificationUI.Instance.ShowQuestStartNotification(quest);
        if (!quest.segments[0].isCompleted)
        {
            QuestNotificationUI.Instance.ShowNewObjectiveNotification(quest.segments[0]);
        }
    }

    public void CheckQuestCompletion(int questID)
    {
        if (!activeQuests.ContainsKey(questID)) return;

        var quest = activeQuests[questID];
        var currentSegmentIndex = GetQuestProgress(questID);
        
        // Check if current segment is complete
        if (quest.segments[currentSegmentIndex].isCompleted)
        {
            // Move to next segment
            questProgress[questID] = currentSegmentIndex + 1;
            
            // Check if quest is complete
            if (currentSegmentIndex + 1 >= quest.segments.Count)
            {
                CompleteQuest(questID);
            }
        }
    }

    private void CompleteQuestSegment(int questID)
    {
        var quest = activeQuests[questID];
        int currentSegmentIndex = questProgress[questID];
        var currentSegment = quest.segments[currentSegmentIndex];
        
        // Mark current segment as completed
        currentSegment.isCompleted = true;
        QuestNotificationUI.Instance.ShowSegmentCompletedNotification(quest, currentSegment);
        
        // Check if this completion would finish the quest
        if (currentSegmentIndex >= quest.segments.Count - 1)
        {
            CompleteQuest(questID);
            return;
        }

        // If not the last segment, update progress and show next objective
        questProgress[questID] = currentSegmentIndex + 1;
        
        // Show next segment notification if quest is not completed and next segment isn't completed
        if (!IsQuestCompleted(questID))
        {
            var nextSegment = quest.segments[questProgress[questID]];
            if (!nextSegment.isCompleted) // Only show if next segment isn't already completed
            {
                QuestNotificationUI.Instance.ShowNewObjectiveNotification(nextSegment);
            }
        }
    }

        
    private void CompleteQuest(int questID)
    {
        if (!activeQuests.ContainsKey(questID)) return;

        var quest = activeQuests[questID];
        
        // First handle the weapon reward if it exists
        if (quest.weaponPrefabReward != null)
        {
            Debug.Log($"Adding weapon reward: {quest.weaponPrefabReward.GetComponent<WeaponItem>().weaponSO.weaponName}");
            InventoryController.Instance.AddQuestReward(quest.weaponPrefabReward);
        }

        // Then handle fate crystals if they exist
        if (quest.fateCrystalReward > 0)
        {
            Debug.Log($"Adding {quest.fateCrystalReward} Fate Crystals");
            PlayerStatsHolder.Instance.playerStats.AddFateCrystals(quest.fateCrystalReward);
        }

        // Update quest state
        questStates[questID] = QuestState.Finished;
        activeQuests.Remove(questID);
        questProgress.Remove(questID);

        // Show completion notification
        QuestNotificationUI.Instance.ShowQuestCompletedNotification(quest);

        // Start next quest in sequence if it exists
        if (quest.isMainQuest && quest.nextMainQuestInSequence != null && CanStartQuest(quest.nextMainQuestInSequence))
        {
            StartQuest(quest.nextMainQuestInSequence);
        }
    }

    public void CheckEnemyKill(Enemy enemy)
    {
        if (enemy == null || enemy.enemyData == null) return;
        
        CharacterType killedEnemyType = enemy.enemyData.characterType;
        Debug.Log($"Enemy killed: {killedEnemyType}");
        OnEnemyKilled(killedEnemyType);
    }

    public List<QuestSO> GetActiveQuests()
    {
        return activeQuests.Values.ToList();
    }

    public int GetQuestProgress(int questID)
    {
        return questProgress.ContainsKey(questID) ? questProgress[questID] : 0;
    }


    public bool IsQuestActive(int questID)
    {
        return activeQuests.ContainsKey(questID);
    }

    public int GetCurrentSegmentIndex(int questID)
    {
        return questProgress.ContainsKey(questID) ? questProgress[questID] : 0;
    }
    

    public bool IsQuestActiveOrCompleted(int questID)
    {
        return activeQuests.ContainsKey(questID) || 
            (questStates.ContainsKey(questID) && questStates[questID] == QuestState.Finished);
    }


    public bool IsQuestCompleted(int questID)
    {
        return questStates.ContainsKey(questID) && questStates[questID] == QuestState.Finished;
    }

    private bool CanStartQuest(QuestSO quest)
    {
        // Check level requirement
        if (PlayerStatsHolder.Instance.playerStats.Level < quest.requiredLevel)
            return false;

        // Check prerequisite quests
        foreach (var prereq in quest.prerequisiteQuests)
        {
            if (!questStates.ContainsKey(prereq.questID) || 
                questStates[prereq.questID] != QuestState.Finished)
                return false;
        }

        return true;
    }
}