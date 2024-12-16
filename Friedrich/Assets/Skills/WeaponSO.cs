using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Battle/Weapon")]
public class WeaponSO : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public List<SkillSO> skills;

    // New fields
    public int damage;
    public int powerBonus; // Add this new field
    public List<CharacterType> compatibleCharacterTypes;
    public GameObject weaponPrefab; // Reference to the weapon's prefab
}

public enum WeaponType
{
    Greatsword,
    FireStaff
    // Add other weapon types as needed
}
