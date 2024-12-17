using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }
    private InventoryUICanvas inventoryUICanvas;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public GameObject equipmentPanel;
    public GameObject weaponSlotPrefab; // Weapon Equipment Slot
    public int slotCount;
    public int equipmentSlotCount = 1;
    public GameObject[] itemPrefabs;

    [Header("Stats Display")]
    public TextMeshProUGUI levelText1;
    public TextMeshProUGUI classText1;
    public GameObject statsDisplayPanel;
    public TextMeshProUGUI hpText1;
    public TextMeshProUGUI mpText1;
    public TextMeshProUGUI physResText1;
    public TextMeshProUGUI arcaneResText1;
    public TextMeshProUGUI dmgText1;

    public TextMeshProUGUI currentVitalityText1;
    public TextMeshProUGUI currentPowerText1;
    public TextMeshProUGUI currentPowerBonusText1;
    public TextMeshProUGUI currentFortitudeText1;
    public TextMeshProUGUI currentArcaneText1;
    [Header("Fate Crystal")]
    public TextMeshProUGUI fateCrystalText;

    public PlayerStats playerStats;

    private List<string> carriedItems = new List<string>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        UpdateStatsDisplay();
    }

    void Start()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not assigned!");
        }

        InitializeInventory();
        InitializeEquipment();
    }



    private void UpdateStatsDisplay()
    {
        PlayerStatsHolder statsHolder = PlayerStatsHolder.Instance;
        if (statsHolder != null && statsHolder.playerStats != null)
        {
            PlayerStats playerStats = statsHolder.playerStats;
            
            // Update attributes
            levelText1.text = $"{playerStats.Level}";
            classText1.text = $"{playerStats.characterType}";
            currentVitalityText1.text = $"{playerStats.vitalityStat}";
            currentPowerText1.text = $"{playerStats.Power}";
            
            // Update power bonus text
            int powerBonus = playerStats.equippedWeapon != null ? playerStats.equippedWeapon.powerBonus : 0;
            currentPowerBonusText1.text = powerBonus > 0 ? $"(+{powerBonus})" : "(+0)";
            
            currentFortitudeText1.text = $"{playerStats.Fortitude}";
            currentArcaneText1.text = $"{playerStats.ArcaneStat}";
            dmgText1.text = $"{playerStats.currentDamage}";

            // Update base stats
            hpText1.text = $"{playerStats.maxVitality}";
            mpText1.text = $"{playerStats.maxIntelligence}";

            // Update resistances
            float physicalResistance = playerStats.Fortitude * 0.5f;
            float arcaneResistance = playerStats.ArcaneStat * 0.5f;
            physResText1.text = $"{physicalResistance:F1}%";
            arcaneResText1.text = $"{arcaneResistance:F1}%";

            fateCrystalText.text = $"{playerStats.currentCrystalsHeld}";
        }
    }

 public int GetCurrentFateCrystals()
    {
        return playerStats != null ? playerStats.currentCrystalsHeld : 0;
    }

public bool DeductFateCrystals(int amount)
    {
        if (playerStats != null && playerStats.currentCrystalsHeld >= amount)
        {
            playerStats.currentCrystalsHeld -= amount;
            Debug.Log($"Deducted {amount} Fate Crystals. Remaining: {playerStats.currentCrystalsHeld}");
            return true;
        }
        Debug.LogWarning("Not enough Fate Crystals to deduct.");
        return false;
    }



    public void InitializeInventory()
    {
        for(int i = 0; i < slotCount; i++)
        {
            Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>();
            slot.isEquipmentSlot = false; // Mark as inventory slot

            if(i < itemPrefabs.Length)
            {
                GameObject item = Instantiate(itemPrefabs[i], slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = item;

                // Assuming each item prefab has a WeaponItem component
                WeaponItem weaponItem = item.GetComponent<WeaponItem>();
                if (weaponItem != null)
                {
                    carriedItems.Add(weaponItem.weaponSO.weaponName);
                }
            }
        }
    }

    void InitializeEquipment()
    {
        for(int i = 0; i < equipmentSlotCount; i++)
        {
            Slot slot = Instantiate(weaponSlotPrefab, equipmentPanel.transform).GetComponent<Slot>();
            slot.isEquipmentSlot = true; // Mark as equipment slot
            slot.allowedItemType = "Weapon"; // Only allow weapons
        }
    }

    public List<string> GetCarriedItems()
    {
        return new List<string>(carriedItems);
    }

    /// <summary>
    /// Clears the current inventory.
    /// </summary>
    public void ClearInventory()
    {
        carriedItems.Clear();

        // Optionally, remove items from UI slots as well
        foreach (Transform child in inventoryPanel.transform)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
        }
    }


    /// <summary>
    /// Adds an item to the inventory by its name.
    /// </summary>
    public void AddItemToInventory(ItemData itemData)
    {
        foreach (Transform child in inventoryPanel.transform)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject prefabToSpawn = null;
                
                if (itemData.itemType == "Weapon")
                {
                    prefabToSpawn = FindWeaponPrefab(itemData.itemName);
                }
                else
                {
                    prefabToSpawn = FindItemPrefab(itemData.itemName);
                }

                if (prefabToSpawn != null)
                {
                    GameObject item = Instantiate(prefabToSpawn, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    slot.currentItem = item;

                    // Set the unique ID
                    if (itemData.itemType == "Weapon")
                    {
                        var weaponItem = item.GetComponent<WeaponItem>();
                        if (weaponItem != null)
                        {
                            weaponItem.uniqueID = itemData.uniqueID;
                            if (itemData.isEquipped)
                            {
                                MoveItemToEquipmentSlot(slot, weaponItem);
                            }
                        }
                    }
                    else
                    {
                        var nonWeaponItem = item.GetComponent<NonWeaponItem>();
                        if (nonWeaponItem != null)
                        {
                            nonWeaponItem.uniqueID = itemData.uniqueID;
                        }
                    }
                    break;
                }
            }
        }
    }

    public void AddQuestReward(GameObject weaponPrefab)
    {
        Debug.Log("Adding quest reward to inventory");
        if (weaponPrefab == null)
        {
            Debug.LogError("Weapon prefab is null!");
            return;
        }

        foreach (Transform child in inventoryPanel.transform)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                // Create a new instance of the weapon prefab
                GameObject item = Instantiate(weaponPrefab, slot.transform);
                WeaponItem weaponItem = item.GetComponent<WeaponItem>();
                
                if (weaponItem != null && weaponItem.weaponSO != null)
                {
                    // Set up the item in the slot
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    slot.currentItem = item;
                    
                    // Only generate new ID if the prefab doesn't already have one
                    WeaponItem prefabWeaponItem = weaponPrefab.GetComponent<WeaponItem>();
                    if (prefabWeaponItem != null && !string.IsNullOrEmpty(prefabWeaponItem.uniqueID))
                    {
                        weaponItem.uniqueID = prefabWeaponItem.uniqueID;
                    }
                    else
                    {
                        weaponItem.uniqueID = System.Guid.NewGuid().ToString();
                    }
                    
                    carriedItems.Add(weaponItem.weaponSO.weaponName);
                    Debug.Log($"Quest reward added to inventory: {weaponItem.weaponSO.weaponName} with ID: {weaponItem.uniqueID}");

                    if (!itemPrefabs.Contains(weaponPrefab))
                    {
                        var newItemPrefabs = new GameObject[itemPrefabs.Length + 1];
                        itemPrefabs.CopyTo(newItemPrefabs, 0);
                        newItemPrefabs[itemPrefabs.Length] = weaponPrefab;
                        itemPrefabs = newItemPrefabs;
                        Debug.Log($"Added {weaponItem.weaponSO.weaponName} prefab to itemPrefabs array");
                    }
                    
                    return;
                }
                else
                {
                    Debug.LogError("Weapon prefab does not have WeaponItem component or WeaponSO!");
                    Destroy(item);
                }
            }
        }
        Debug.LogWarning("No empty slots available in inventory!");
    }

    private void MoveItemToEquipmentSlot(Slot originalSlot, WeaponItem weaponItem)
    {
        if (weaponItem == null)
        {
            Debug.LogError("Failed to equip: WeaponItem is null");
            return;
        }

        foreach (Transform child in equipmentPanel.transform)
        {
            Slot equipSlot = child.GetComponent<Slot>();
            if (equipSlot != null && equipSlot.isEquipmentSlot)
            {
                // Handle existing equipped weapon
                if (equipSlot.currentItem != null)
                {
                    // Store the currently equipped weapon
                    GameObject existingWeapon = equipSlot.currentItem;
                    WeaponItem existingWeaponItem = existingWeapon.GetComponent<WeaponItem>();

                    // Move existing weapon back to original slot
                    existingWeapon.transform.SetParent(originalSlot.transform);
                    existingWeapon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    originalSlot.currentItem = existingWeapon;
                }

                // Equip new weapon
                weaponItem.transform.SetParent(equipSlot.transform);
                weaponItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                equipSlot.currentItem = weaponItem.gameObject;
                originalSlot.currentItem = null;
                
                PlayerStatsHolder.Instance.playerStats.EquipWeapon(weaponItem.weaponSO);
                Debug.Log($"Successfully equipped weapon: {weaponItem.weaponSO.weaponName} with ID: {weaponItem.uniqueID}");
                return;
            }
        }

        Debug.LogWarning("Failed to equip weapon: No valid equipment slot found");
    }

    private GameObject FindWeaponPrefab(string weaponName)
    {
        // First try to find in itemPrefabs
        GameObject prefab = itemPrefabs.FirstOrDefault(prefab => {
            WeaponItem weaponItem = prefab.GetComponent<WeaponItem>();
            return weaponItem != null && weaponItem.weaponSO != null && 
                weaponItem.weaponSO.weaponName.Equals(weaponName, StringComparison.OrdinalIgnoreCase);
        });

        // If not found in itemPrefabs, try WeaponDatabase
        if (prefab == null && WeaponDatabase.Instance != null)
        {
            prefab = WeaponDatabase.Instance.GetWeaponPrefabByName(weaponName);
            if (prefab != null)
            {
                // Add to itemPrefabs for future use
                var newItemPrefabs = new GameObject[itemPrefabs.Length + 1];
                itemPrefabs.CopyTo(newItemPrefabs, 0);
                newItemPrefabs[itemPrefabs.Length] = prefab;
                itemPrefabs = newItemPrefabs;
            }
        }

        return prefab;
    }

    /*public void AddPurchasedItemToInventory(GameObject itemPrefab)
    {
        if (itemPrefab == null)
        {
            Debug.LogError("Attempted to add a null prefab to the inventory.");
            return;
        }

        foreach (Transform child in inventoryPanel.transform)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                // Instantiate the item and add it to the slot
                GameObject item = Instantiate(itemPrefab, slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = item;

                // Log the success
                Debug.Log($"Added '{itemPrefab.name}' to inventory.");
                return;
            }
        }

        // If no slots are available
        Debug.LogWarning("Inventory is full! Cannot add item.");
    }*/

    public void AddPurchasedItemToInventory(GameObject itemPrefab)
{
    if (itemPrefab == null)
    {
        Debug.LogError("Attempted to add a null prefab to the inventory.");
        return;
    }

    // Check if the item already exists in the inventory
    foreach (Transform child in inventoryPanel.transform)
    {
        Slot slot = child.GetComponent<Slot>();
        if (slot != null && slot.currentItem != null && slot.currentItem.name == itemPrefab.name)
        {
            Debug.Log($"Item {itemPrefab.name} already exists in inventory.");
            return; // Item already exists, no need to add again
        }
    }

    // Look for the first empty slot to add the new item
    foreach (Transform child in inventoryPanel.transform)
    {
        Slot slot = child.GetComponent<Slot>();
        if (slot != null && slot.currentItem == null)
        {
            // Instantiate the item prefab in the slot
            GameObject item = Instantiate(itemPrefab, slot.transform); // Instantiate the weapon
            item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Position it at the center of the slot

            slot.currentItem = item; // Set the item to the slot's current item
            Debug.Log($"Added '{itemPrefab.name}' to inventory.");
            return;
        }
    }


    private GameObject FindItemPrefab(string itemName)
    {
        return itemPrefabs.FirstOrDefault(prefab => 
            prefab.name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
    }


    public void EquipWeaponByID(string uniqueID)
    {
        // Find the weapon in the inventory first
        foreach (Transform invChild in inventoryPanel.transform)
        {
            Slot invSlot = invChild.GetComponent<Slot>();
            if (invSlot != null && invSlot.currentItem != null)
            {
                WeaponItem weaponItem = invSlot.currentItem.GetComponent<WeaponItem>();
                if (weaponItem != null && weaponItem.uniqueID == uniqueID)
                {
                    // Find an equipment slot
                    foreach (Transform eqChild in equipmentPanel.transform)
                    {
                        Slot eqSlot = eqChild.GetComponent<Slot>();
                        if (eqSlot != null && eqSlot.isEquipmentSlot)
                        {
                            // Store the weapon we want to equip
                            GameObject weaponToEquip = invSlot.currentItem;

                            // Handle existing equipped weapon first
                            if (eqSlot.currentItem != null)
                            {
                                // Move currently equipped weapon to inventory slot
                                GameObject existingWeapon = eqSlot.currentItem;
                                existingWeapon.transform.SetParent(invSlot.transform);
                                existingWeapon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                                invSlot.currentItem = existingWeapon; // Make sure to assign back
                            }

                            // Now equip the new weapon
                            weaponToEquip.transform.SetParent(eqSlot.transform);
                            weaponToEquip.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                            eqSlot.currentItem = weaponToEquip;

                            // Update player stats
                            if (PlayerStatsHolder.Instance != null && PlayerStatsHolder.Instance.playerStats != null)
                            {
                                PlayerStatsHolder.Instance.playerStats.EquipWeapon(weaponItem.weaponSO);
                                Debug.Log($"Successfully equipped weapon: {weaponItem.weaponSO.weaponName}");
                            }
                            return; // Exit after equipping
                        }
                    }
                    break; // Exit after finding the weapon
                }
            }
        }
    }

}