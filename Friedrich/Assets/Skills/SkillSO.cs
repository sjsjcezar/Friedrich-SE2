using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Battle/Skill")]
public class SkillSO : ScriptableObject
{
    public string skillName;
    [Header("Rapid Assault Values")]
    public float duration;
    public int damagePerHit;
    public float slideDistance;
    public float slideSpeed;
    public float slideSpeedBack;

    [Header("Hacking Slash Values")]
    public int maxHits;
    public int damage;


    [Header("Crescent Slash Values")]
    public int crescentDamage;
    public int requiredPressCount;
    public float reducedDamagePercentage;

    [Header("General Skill Values")]
    public int mpCost; 
}