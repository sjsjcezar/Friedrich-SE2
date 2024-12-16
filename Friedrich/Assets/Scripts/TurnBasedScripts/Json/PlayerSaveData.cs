using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public int level;
    public string playerTypePrefab;
    public int currentCrystalsHeld;
    public PlayerStatsData playerStats;
    public VialsData vials;
    public EquippedWeaponData equippedWeapon;
    public List<ItemData> carriedItems;
    public string playerID;
    public Vector3Data playerPosition;
    public string lastSceneName;

    [Serializable]
    public class PlayerStatsData
    {
        public CharacterType characterType;
        public int baseDamage;
        public int currentDamage;
        public int vitality;
        public int maxVitality;
        public int vitalityStat;
        public int power;
        public int basePower;
        public int fortitude;
        public int intelligence;
        public int maxIntelligence;
        public int arcaneStat;
        public float powerMultiplier;
        public int level;
        public int currentCrystalsHeld;
        public int crystalsNeededForNextLevel;
    }

    [Serializable]
    public class VialsData
    {
        public int healVialCount;        // Allocated heal vials
        public int manaVialCount;        // Allocated mana vials
        public int remainingVialCount;
        public int currentHealVialCount; // Current heal vials being held
        public int currentManaVialCount; // Current mana vials being held
    }

    [Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3Data(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }


    [Serializable]
    public class EquippedWeaponData
    {
        public string itemName;
        public string itemType;
        public string uniqueID;
    }

}