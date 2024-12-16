using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DariusTestQuest", menuName = "Quest System/Quests/DariusTestQuest")]
public class DariusTestQuest : QuestSO
{
    private void OnEnable()
    {
        questID = 1;
        title = "Prove Your Worth";
        description = "Prove your worth to Captain Darius by eliminating Lesser Demons.";
        requiredLevel = 1;
        
        segments = new List<QuestSegment>
        {
            new QuestSegment
            {
                segmentName = "Meet Darius",
                description = "Speak with Captain Darius Blackvale",
                navigationHint = "Captain Darius can be found in Wyrmspire Hall",
                completionType = SegmentCompletionType.TalkToNPC,
                targetNPCID = "DARIUS_BLACKVALE"
            },
            new QuestSegment
            {
                segmentName = "Find Demon Territory",
                description = "Travel to the Outskirts of Yggdrasil",
                navigationHint = "Use the Statue of Valkirith to reach the Outskirts",
                completionType = SegmentCompletionType.VisitLocation,
                targetLocationName = "Outskirts of Yggdrasil",
                targetSceneName = "SampleScene"  // Your actual scene name
            },
            new QuestSegment
            {
                segmentName = "Eliminate Demons",
                description = "Defeat 5 Lesser Demons",
                navigationHint = "Lesser Demons can be found in the Outskirts",
                completionType = SegmentCompletionType.KillEnemies,
                targetEnemyType = CharacterType.LesserDemon,
                requiredKills = 5,
                currentKills = 0
            },
            new QuestSegment
            {
                segmentName = "Report Back",
                description = "Report your success to Captain Darius",
                navigationHint = "Return to Darius in Wyrmspire Hall",
                completionType = SegmentCompletionType.TalkToNPC,
                targetNPCID = "DARIUS_BLACKVALE"
            }
        };

        rewardType = RewardType.FateCrystals;
        fateCrystalReward = 500;
    }
}