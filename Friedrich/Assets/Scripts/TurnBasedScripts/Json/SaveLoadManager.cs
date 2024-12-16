using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    private string saveFilePath;

    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        DontDestroyOnLoad(gameObject); // Make persistent across scenes
    }

    void Start()
    {
        Application.targetFrameRate = 144;  // Set FPS cap to 144
    }


    public void SaveGame()
    {
        if (PlayerStatsHolder.Instance == null || PlayerStatsHolder.Instance.playerStats == null)
        {
            Debug.LogError("PlayerStatsHolder or PlayerStats is not assigned.");
            return;
        }

        if (InventoryController.Instance == null)
        {
            Debug.LogError("InventoryController instance is not found.");
            return;
        }

        PlayerSaveData saveData = new PlayerSaveData
        {
            level = PlayerStatsHolder.Instance.playerStats.Level,
            playerTypePrefab = PlayerStatsHolder.Instance.playerStats.characterType.ToString(),
            currentCrystalsHeld = PlayerStatsHolder.Instance.playerStats.currentCrystalsHeld,
            lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,  // Save current scene
            playerStats = new PlayerSaveData.PlayerStatsData

            {
                characterType = PlayerStatsHolder.Instance.playerStats.characterType,
                baseDamage = PlayerStatsHolder.Instance.playerStats.baseDamage,
                currentDamage = PlayerStatsHolder.Instance.playerStats.currentDamage,
                vitality = PlayerStatsHolder.Instance.playerStats.Vitality,
                maxVitality = PlayerStatsHolder.Instance.playerStats.maxVitality,
                vitalityStat = PlayerStatsHolder.Instance.playerStats.vitalityStat,
                power = PlayerStatsHolder.Instance.playerStats.Power,
                basePower = PlayerStatsHolder.Instance.playerStats.basePower,
                fortitude = PlayerStatsHolder.Instance.playerStats.Fortitude,
                intelligence = PlayerStatsHolder.Instance.playerStats.Intelligence,
                maxIntelligence = PlayerStatsHolder.Instance.playerStats.maxIntelligence,
                arcaneStat = PlayerStatsHolder.Instance.playerStats.ArcaneStat,
                powerMultiplier = PlayerStatsHolder.Instance.playerStats.PowerMultiplier,
                level = PlayerStatsHolder.Instance.playerStats.Level,
                currentCrystalsHeld = PlayerStatsHolder.Instance.playerStats.currentCrystalsHeld,
                crystalsNeededForNextLevel = PlayerStatsHolder.Instance.playerStats.crystalsNeededForNextLevel
            },
            vials = new PlayerSaveData.VialsData
            {
                healVialCount = GlobalVariables.healVialCount,
                manaVialCount = GlobalVariables.manaVialCount,
                remainingVialCount = VialController.Instance.remainingVialCount,
                currentHealVialCount = GlobalVariables.currentHealVialCount,
                currentManaVialCount = GlobalVariables.currentManaVialCount
            },
            playerPosition = new PlayerSaveData.Vector3Data(PlayerStatsHolder.Instance.transform.position),
            carriedItems = GetInventoryItemData(),
            playerID = GlobalVariables.playerData.ContainsKey("PlayerID") ? GlobalVariables.playerData["PlayerID"] : string.Empty
        };

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Game Saved to {saveFilePath}\n{json}");
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogError($"Save file not found at {saveFilePath}");
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(json);

        if (saveData == null)
        {
            Debug.LogError("Failed to parse save data.");
            return;
        }

        // Restore player position
        if (PlayerStatsHolder.Instance != null)
        {
            PlayerStatsHolder.Instance.transform.position = saveData.playerPosition.ToVector3();
        }

        // Restore player stats
        if (PlayerStatsHolder.Instance != null && PlayerStatsHolder.Instance.playerStats != null)
        {
            RestorePlayerStats(saveData);
        }
        else
        {
            Debug.LogError("PlayerStatsHolder or PlayerStats is not assigned.");
            return;
        }

        // Restore vials
        if (VialController.Instance != null)
        {
            RestoreVials(saveData);
        }
        else
        {
            Debug.LogError("VialController instance is not found.");
        }

        // Restore inventory
        if (InventoryController.Instance != null)
        {
            RestoreInventory(saveData);
        }

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != saveData.lastSceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(saveData.lastSceneName);
        }

        // Restore Player ID
        RestorePlayerID(saveData);

        Debug.Log($"Game Loaded from {saveFilePath}\n{json}");
    }

    private void RestorePlayerStats(PlayerSaveData saveData)
    {
        var stats = PlayerStatsHolder.Instance.playerStats;
        var savedStats = saveData.playerStats;

        stats.Level = saveData.level;
        stats.characterType = (CharacterType)Enum.Parse(typeof(CharacterType), saveData.playerTypePrefab);
        stats.currentCrystalsHeld = saveData.currentCrystalsHeld;
        stats.baseDamage = savedStats.baseDamage;
        stats.currentDamage = savedStats.currentDamage;
        stats.Vitality = savedStats.vitality;
        stats.maxVitality = savedStats.maxVitality;
        stats.vitalityStat = savedStats.vitalityStat;
        stats.Power = savedStats.power;
        stats.basePower = savedStats.basePower;
        stats.Fortitude = savedStats.fortitude;
        stats.Intelligence = savedStats.intelligence;
        stats.maxIntelligence = savedStats.maxIntelligence;
        stats.ArcaneStat = savedStats.arcaneStat;
        stats.PowerMultiplier = savedStats.powerMultiplier;
        stats.crystalsNeededForNextLevel = savedStats.crystalsNeededForNextLevel;
    }

    private void RestoreVials(PlayerSaveData saveData)
    {
        GlobalVariables.healVialCount = saveData.vials.healVialCount;
        GlobalVariables.manaVialCount = saveData.vials.manaVialCount;
        GlobalVariables.currentHealVialCount = saveData.vials.currentHealVialCount;
        GlobalVariables.currentManaVialCount = saveData.vials.currentManaVialCount;
        VialController.Instance.remainingVialCount = saveData.vials.remainingVialCount;
    }

    private void RestoreInventory(PlayerSaveData saveData)
    {
        InventoryController.Instance.ClearInventory();
        
        foreach (var itemData in saveData.carriedItems)
        {
            InventoryController.Instance.AddItemToInventory(itemData);
        }
    }

    private void RestorePlayerID(PlayerSaveData saveData)
    {
        if (!string.IsNullOrEmpty(saveData.playerID))
        {
            if (GlobalVariables.playerData.ContainsKey("PlayerID"))
            {
                GlobalVariables.playerData["PlayerID"] = saveData.playerID;
            }
            else
            {
                GlobalVariables.playerData.Add("PlayerID", saveData.playerID);
            }
        }
    }

    private List<ItemData> GetInventoryItemData()
    {
        List<ItemData> items = new List<ItemData>();
        var inventoryPanel = InventoryController.Instance.inventoryPanel;
        var equipmentPanel = InventoryController.Instance.equipmentPanel;

        // Get items from inventory
        foreach (Transform child in inventoryPanel.transform)
        {
            AddItemToList(items, child, false);
        }

        // Get equipped items
        foreach (Transform child in equipmentPanel.transform)
        {
            AddItemToList(items, child, true);
        }

        return items;
    }

    private void AddItemToList(List<ItemData> items, Transform slotTransform, bool isEquipped)
    {
        Slot slot = slotTransform.GetComponent<Slot>();
        if (slot != null && slot.currentItem != null)
        {
            WeaponItem weaponItem = slot.currentItem.GetComponent<WeaponItem>();
            if (weaponItem != null && weaponItem.weaponSO != null)
            {
                items.Add(new ItemData 
                { 
                    itemName = weaponItem.weaponSO.weaponName,
                    itemType = "Weapon",
                    uniqueID = weaponItem.uniqueID,
                    isEquipped = isEquipped
                });
            }
            else
            {
                var itemName = slot.currentItem.name.Replace("(Clone)", "").Trim();
                items.Add(new ItemData 
                { 
                    itemName = itemName,
                    itemType = slot.currentItem.tag,
                    uniqueID = slot.currentItem.GetComponent<NonWeaponItem>()?.uniqueID ?? Guid.NewGuid().ToString(),
                    isEquipped = false
                });
            }
        }
    }
}