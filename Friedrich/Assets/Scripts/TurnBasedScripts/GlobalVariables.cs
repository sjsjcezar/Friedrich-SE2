using System.Collections.Generic;
using UnityEngine;

public static class GlobalVariables
{
    // Player Unique ID
    public static Dictionary<string, string> playerData = new Dictionary<string, string>();
    
    // Player stats dictionary
    public static Dictionary<string, PlayerStats> playerStatsReferences = new Dictionary<string, PlayerStats>();
    
    // Enemy dictionaries
    public static Dictionary<string, bool> enemyStatus = new Dictionary<string, bool>();
    public static Dictionary<string, string> enemyUniqueIDs = new Dictionary<string, string>();
    public static Dictionary<string, EnemyData> enemyDataReferences = new Dictionary<string, EnemyData>();
    public static Dictionary<string, EnemySkillSystem> enemySkillReferences = new Dictionary<string, EnemySkillSystem>();
    public static List<EnemySpawnPoint> enemySpawnPoints = new List<EnemySpawnPoint>();

    public static WeaponSO equippedWeapon;

    public static void ResetEquippedWeapon()
    {
        equippedWeapon = null;
    }
    
    public static string lastTeleportSpawnID;
    public static bool isStatueTeleporting = false;

    public static int currentHealVialCount;
    public static int currentManaVialCount;

    public static int healVialCount;
    public static int manaVialCount;

    // **Time Management**
    public static float seconds = 0f;
    public static int minutes = 0;
    public static int hours = 8; // Default start time (8:00 AM)
    public static int days = 1;
    public static int dayOfWeek = 1; // 1=Mondas, 2=Loredas, ..., 7=Sundar
    public static int month = 1;
    public static int year = 1;

    public static float timeTick = 1f; // Adjust this to speed up or slow down time
    public static Vector3 lastOverworldPosition;
    public static bool hasStoredPosition = false;

    // Day names
    public static readonly string[] dayNames = 
    {
        "Mondas",    // 1
        "Loredas",   // 2
        "Wendas",    // 3
        "Thurdas",   // 4
        "Frindar",   // 5
        "Brithondas",// 6
        "Sundar"     // 7
    };

    // Month names
    public static readonly string[] monthNames = 
    {
        "Frosvael",   // January
        "Thaloril",   // February
        "Elenara",    // March
        "Virethil",   // April
        "Sylphara",   // May
        "Solarya",    // June
        "Rivendawn",  // July
        "Flarrowen",  // August
        "Galyndor",   // September
        "Thyrian",    // October
        "Nolvara",    // November
        "Winterael"   // December
    };

    // Days in each month
    public static readonly int[] monthDays = 
    {
        31, // Frosvael
        28, // Thaloril
        31, // Elenara
        30, // Virethil
        31, // Sylphara
        30, // Solarya
        31, // Rivendawn
        31, // Flarrowen
        30, // Galyndor
        31, // Thyrian
        30, // Nolvara
        31  // Winterael
    };
}