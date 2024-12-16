using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestSegment
{
 [Header("Segment Information")]
    public string segmentName;
    public string description;
    public string segmentID;  // e.g., "Darius_BlackvaleInitial", "Darius_BlackvaleEnd"
    
    [Header("Player Guidance")]
    [TextArea(3, 5)]
    public string requirementText;  // What player needs to do
    [TextArea(2, 4)]
    public string navigationHint;   // How to do it
    
    [Header("Completion Logic")]
    public SegmentCompletionType completionType;
    public bool isCompleted;
    
    // NPC-related
    [Header("NPC Interaction")]
    public string targetNPCID;
    public DialogueContentSO segmentDialogue;  // specific dialogue for this segment
    
    // Location-related
    [Header("Location Visit")]
    public string targetLocationName;
    public string targetSceneName;
    public float locationReachDistance = 5f;
    
    // Enemy-related
    [Header("Enemy Kills")]
    public CharacterType targetEnemyType;
    public int requiredKills;
    public int currentKills;
}