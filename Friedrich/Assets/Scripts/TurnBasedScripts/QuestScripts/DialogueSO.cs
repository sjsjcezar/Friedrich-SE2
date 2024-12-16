using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue")]
public class DialogueSO : ScriptableObject
{
    public string npcName;
    public DialogueNode baseDialogue;
    public List<QuestSpecificDialogue> questDialogues = new List<QuestSpecificDialogue>();
    public List<DialogueOption> questOptions = new List<DialogueOption>();
    
    [Header("NPC Greetings & Farewells")]
    public List<DialogueContentSO> greetings;
    public List<DialogueContentSO> farewells;

    [System.Serializable]
    public class DialogueNode
    {
        public DialogueContentSO mainDialogue;
        public List<DialogueOption> dialogueOptions = new List<DialogueOption>();
        public Action onDialogueComplete;
    }

    [System.Serializable]
    public class DialogueOption
    {
        public string optionText;
        public DialogueContentSO dialogueContent;
        public DialogueSO nextDialogue;
        public string requiredSegmentID;  // Which segment this option appears in
        public bool isQuestRelated;
        public bool hasBeenSelected;
        public bool isRepeatable;  // If true, button stays until segment changes. If false, removed after first use.
        public Action onSelectAction;
    }

    [System.Serializable]
    public class QuestSpecificDialogue
    {
        public string segmentID;
        public DialogueContentSO dialogueContent;
        public bool isOneTime;
        public bool hasBeenShown;
    }


    public bool HasQuestDialogue()
    {
        return questDialogues != null && questDialogues.Count > 0;
    }

    public DialogueContentSO GetRandomGreeting()
    {
        if (greetings == null || greetings.Count == 0)
            return null;
        return greetings[Random.Range(0, greetings.Count)];
    }

    public DialogueContentSO GetRandomFarewell()
    {
        if (farewells == null || farewells.Count == 0)
            return null;
        return farewells[Random.Range(0, farewells.Count)];
    }

}