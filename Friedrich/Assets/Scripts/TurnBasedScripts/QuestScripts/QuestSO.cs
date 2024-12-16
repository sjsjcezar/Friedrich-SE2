using UnityEngine;
using System.Collections.Generic;

public enum QuestState { Requirements_Not_Met, Can_Start, In_Progress, Can_Finish, Finished }
public enum RewardType { FateCrystals, Weapon }
public enum SegmentCompletionType { TalkToNPC, KillEnemies, VisitLocation }
public enum QuestType { Main, Side }

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
public class QuestSO : ScriptableObject
{
    public int questID;
    public string title;
    public string description;
    public QuestType questType;
    
    [Header("Requirements")]
    public int requiredLevel;
    public List<QuestSO> prerequisiteQuests;
    
    [Header("Reward")]
    public RewardType rewardType;
    public int fateCrystalReward;
    public GameObject weaponPrefabReward;
    
    [Header("Quest Flow")]
    public List<QuestSegment> segments;
    public string[] navigationHints; // Corresponds to each segment

    [Header("Main Quest Sequence")]
    public bool isMainQuest = false;
    public QuestSO nextMainQuestInSequence;
}