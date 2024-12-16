using UnityEngine;

[CreateAssetMenu(fileName = "New NPC", menuName = "NPCs/NPC Data")]
public class NPCSO : ScriptableObject
{
    public string npcID;
    public string npcName;
    public string title;
    public QuestSO[] availableQuests;
    public Sprite npcImage;
}