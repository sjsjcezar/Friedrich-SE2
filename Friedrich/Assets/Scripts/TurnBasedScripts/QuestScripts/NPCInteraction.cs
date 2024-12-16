using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static DialogueSO;


public class NPCInteraction : MonoBehaviour
{
    public NPCSO npcData;
    public DialogueSO defaultDialogue;
    private bool playerInRange = false;
    public static bool playerIsInConversation = false;

    private QuestManager questManager;
    private DialogueUI dialogueUI;

    void Start()
    {
        questManager = FindObjectOfType<QuestManager>();
        dialogueUI = FindObjectOfType<DialogueUI>();
        questManager = QuestManager.Instance;
        if (questManager == null)
        {
            Debug.LogError("QuestManager not found in the scene!");
        }

        dialogueUI = FindObjectOfType<DialogueUI>();
        if (dialogueUI == null)
        {
            Debug.LogError("DialogueUI not found in the scene!");
        }

        if (npcData == null)
        {
            Debug.LogError("NPCSO not assigned to NPCInteraction!");
        }

        if (defaultDialogue == null)
        {
            Debug.LogError("DialogueSO not assigned to NPCInteraction!");
        }
    }

    private void Update()
    {
        // Only allow interaction if DialogueUI says we can
        if (playerInRange && !playerIsInConversation && Input.GetKeyDown(KeyCode.E) && dialogueUI.canInteract)
        {
            InteractWithNPC();
        }
}

    private void InteractWithNPC()
    {
        if (dialogueUI == null || defaultDialogue == null) return;

        playerIsInConversation = true;

        var activeQuest = GetActiveQuestForNPC();
        DialogueNode dialogueToShow = new DialogueNode();
        dialogueToShow.dialogueOptions = new List<DialogueOption>();

        // Add base dialogue options first
        if (defaultDialogue.baseDialogue != null && defaultDialogue.baseDialogue.dialogueOptions != null)
        {
            dialogueToShow.dialogueOptions.AddRange(defaultDialogue.baseDialogue.dialogueOptions);
        }

        // If there's an active quest and current segment matches this NPC
        if (activeQuest != null)
        {
            int currentProgress = QuestManager.Instance.GetQuestProgress(activeQuest.questID);
            if (currentProgress >= activeQuest.segments.Count)
            {
                Debug.LogWarning($"Quest progress ({currentProgress}) exceeds segment count ({activeQuest.segments.Count})");
                return;
            }

            var currentSegment = activeQuest.segments[currentProgress];
            if (currentSegment == null)
            {
                Debug.LogError("Current segment is null!");
                return;
            }
            
            // If this is a TalkToNPC segment and matches this NPC
            if (currentSegment.completionType == SegmentCompletionType.TalkToNPC && 
                currentSegment.targetNPCID == npcData.npcID)
            {
                if (defaultDialogue.questOptions == null)
                {
                    Debug.LogError("Quest options list is null in DialogueSO!");
                    return;
                }

                // Find matching quest option for this segment
                var matchingOption = defaultDialogue.questOptions.FirstOrDefault(
                    opt => opt.requiredSegmentID == currentSegment.segmentID
                );

                if (matchingOption != null)
                {
                    // Create a new option instance to modify its action
                    var questOption = new DialogueOption
                    {
                        optionText = matchingOption.optionText,
                        dialogueContent = matchingOption.dialogueContent,
                        nextDialogue = matchingOption.nextDialogue,
                        requiredSegmentID = matchingOption.requiredSegmentID,
                        isQuestRelated = true,
                        hasBeenSelected = matchingOption.hasBeenSelected
                    };

                    // Set up the onSelectAction to complete the segment
                    questOption.onSelectAction = () =>
                    {
                        if (!currentSegment.isCompleted)
                        {
                            currentSegment.isCompleted = true;
                            UpdateQuestProgress(activeQuest);
                            if (QuestNotificationUI.Instance != null)
                            {
                                // Use the proper notification methods instead of direct ShowNotification
                                QuestNotificationUI.Instance.ShowSegmentCompletedNotification(activeQuest, currentSegment);
                                
                                int newProgress = QuestManager.Instance.GetQuestProgress(activeQuest.questID);
                                if (newProgress < activeQuest.segments.Count)
                                {
                                    var nextSegment = activeQuest.segments[newProgress];
                                    if (!nextSegment.isCompleted) // Only show if not completed
                                    {
                                        QuestNotificationUI.Instance.ShowNewObjectiveNotification(nextSegment);
                                    }
                                }
                            }
                        }
                    };

                    // Add the quest-specific dialogue option at the start
                    dialogueToShow.dialogueOptions.Insert(0, questOption);
                }
                else
                {
                    Debug.LogWarning($"No matching dialogue option found for segment ID: {currentSegment.segmentID}");
                }
            }
        }

        dialogueUI.DisplayDialogue(defaultDialogue, dialogueToShow, npcData);
    }

    private QuestSO GetActiveQuestForNPC()
    {
        foreach (var quest in QuestManager.Instance.GetActiveQuests())
        {
            var currentSegment = quest.segments[QuestManager.Instance.GetQuestProgress(quest.questID)];
            if (currentSegment.completionType == SegmentCompletionType.TalkToNPC &&
                currentSegment.targetNPCID == npcData.npcID)
            {
                return quest;
            }
        }
        return null;
    }

    private void UpdateQuestProgress(QuestSO quest)
    {
        var currentSegmentIndex = QuestManager.Instance.GetQuestProgress(quest.questID);
        var segment = quest.segments[currentSegmentIndex];
        if (segment.completionType == SegmentCompletionType.TalkToNPC &&
            segment.targetNPCID == npcData.npcID)
        {
            segment.isCompleted = true;
            QuestManager.Instance.CheckQuestCompletion(quest.questID);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}