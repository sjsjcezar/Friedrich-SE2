using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySkill
{
    public string skillName;
    public int damage;
    public int maxUses;
    public int currentUses;
    public GameObject skillPrefab;
    public bool isUnlocked; // Whether this enemy can use this skill
    public SkillCategory category; // Specifies skill type

    // New position offset properties
    public float xOffset = 0f; // Horizontal offset relative to enemy's position
    public float yOffset = 0f; // Vertical offset relative to enemy's position
}

[CreateAssetMenu(fileName = "NewEnemySkillSystem", menuName = "Enemy/Skill System")]
public class EnemySkillSystem : ScriptableObject
{
    public List<EnemySkill> availableSkills = new List<EnemySkill>();
    public bool canHeal = true;
    public float healThreshold = 0.3f; // 30% health threshold
    public bool hasUsedHeal = false;
    public int healPercentage = 30; // Heals for 30% of max health

    // Method to reset currentUses for all skills
    public void ResetSkillUses()
    {
        foreach (var skill in availableSkills)
        {
            skill.currentUses = 0;
        }
    }
}