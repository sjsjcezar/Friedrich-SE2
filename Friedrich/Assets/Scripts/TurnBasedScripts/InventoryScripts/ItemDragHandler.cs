using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private PlayerStats playerStats;
    private float lastClickTime = 0f;
    private float doubleClickTimeThreshold = 0.3f;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        PlayerStatsHolder statsHolder = FindObjectOfType<PlayerStatsHolder>();
        if (statsHolder != null)
        {
            playerStats = statsHolder.playerStats;
        }
        else
        {
            Debug.LogError("PlayerStatsHolder not found in the scene.");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick <= doubleClickTimeThreshold)
        {
            HandleDoubleClick();
        }
            
        lastClickTime = Time.time;
    }

    private void HandleDoubleClick()
    {
        WeaponItem weaponItem = GetComponent<WeaponItem>();
        if (weaponItem == null) return;

        Slot currentSlot = transform.parent.GetComponent<Slot>();
        if (currentSlot == null) return;

        // If we're in inventory, try to equip
        if (!currentSlot.isEquipmentSlot)
        {
            TryEquipWeapon(currentSlot, weaponItem);
        }
        // If we're in equipment slot, try to move to first empty inventory slot
        else
        {
            TryUnequipWeapon(currentSlot);
        }
    }

    private void TryEquipWeapon(Slot originalSlot, WeaponItem weaponItem)
    {
        if (!weaponItem.weaponSO.compatibleCharacterTypes.Contains(playerStats.characterType))
        {
            Debug.Log($"This weapon can't be equipped by {playerStats.characterType}");
            return;
        }

        // Find the equipment slot
        foreach (Transform child in InventoryController.Instance.equipmentPanel.transform)
        {
            Slot equipSlot = child.GetComponent<Slot>();
            if (equipSlot != null && equipSlot.isEquipmentSlot)
            {
                HandleWeaponEquip(equipSlot, originalSlot, weaponItem);
                break;
            }
        }
    }

    private void TryUnequipWeapon(Slot equipmentSlot)
    {
        // Find first empty inventory slot
        foreach (Transform child in InventoryController.Instance.inventoryPanel.transform)
        {
            Slot inventorySlot = child.GetComponent<Slot>();
            if (inventorySlot != null && !inventorySlot.isEquipmentSlot && inventorySlot.currentItem == null)
            {
                // Move to inventory slot
                transform.SetParent(inventorySlot.transform);
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                inventorySlot.currentItem = gameObject;
                equipmentSlot.currentItem = null;
                
                // Unequip from player stats
                playerStats.UnequipWeapon();
                UpdateStatsUI();
                break;
            }
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = null;
        if (eventData.pointerEnter != null)
        {
            // Try to get slot from the object itself
            dropSlot = eventData.pointerEnter.GetComponent<Slot>();
            
            // If no slot found, check if we're hovering over an item in a slot
            if (dropSlot == null)
            {
                WeaponItem weaponItem = eventData.pointerEnter.GetComponent<WeaponItem>();
                if (weaponItem != null)
                {
                    dropSlot = weaponItem.GetComponentInParent<Slot>();
                }
            }
            
            // If still no slot found, check parent
            if (dropSlot == null)
            {
                dropSlot = eventData.pointerEnter.GetComponentInParent<Slot>();
            }
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (dropSlot != null)
        {
            HandleItemPlacement(dropSlot, originalSlot);
        }
        else
        {
            ReturnToOriginalPosition();
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
    
    private void HandleItemPlacement(Slot dropSlot, Slot originalSlot)
    {
        // If dropping into weapon slot
        if (dropSlot.isEquipmentSlot && dropSlot.allowedItemType == "Weapon")
        {
            WeaponItem weaponItem = GetComponent<WeaponItem>();
            if (weaponItem != null)
            {
                HandleWeaponEquip(dropSlot, originalSlot, weaponItem);
            }
            else
            {
                ReturnToOriginalPosition();
            }
        }
        // If moving from weapon slot to inventory
        else if (originalSlot.isEquipmentSlot && originalSlot.allowedItemType == "Weapon")
        {
            HandleWeaponUnequip(dropSlot, originalSlot);
        }
        // Normal inventory swap
        else
        {
            HandleInventorySwap(dropSlot, originalSlot);
        }
    }
    
    private void HandleWeaponEquip(Slot dropSlot, Slot originalSlot, WeaponItem weaponItem)
    {
        // Check if weapon is compatible with player's character type
        if (!weaponItem.weaponSO.compatibleCharacterTypes.Contains(playerStats.characterType))
        {
            Debug.Log($"This weapon can't be equipped by {playerStats.characterType}");
            ReturnToOriginalPosition();
            return;
        }

        // Remove any duplicate weapons in the equipment slot
        foreach (Transform child in dropSlot.transform)
        {
            WeaponItem existingWeaponItem = child.GetComponent<WeaponItem>();
            if (existingWeaponItem != null && existingWeaponItem.uniqueID == weaponItem.uniqueID)
            {
                Destroy(child.gameObject);
            }
        }

        // Handle existing weapon in equipment slot (swap)
        if (dropSlot.currentItem != null && dropSlot.currentItem != gameObject)
        {
            // Swap items
            dropSlot.currentItem.transform.SetParent(originalSlot.transform);
            originalSlot.currentItem = dropSlot.currentItem;
            dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        // Equip new weapon and update stats
        transform.SetParent(dropSlot.transform);
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 1); // Set position to (0, 1)
        dropSlot.currentItem = gameObject;

        // Update player stats
        playerStats.EquipWeapon(weaponItem.weaponSO);

        // Update UI with current stats
        UpdateStatsUI();
    }


    private void HandleWeaponUnequip(Slot dropSlot, Slot originalSlot)
    {
        // Prevent direct unequipping - only allow swapping
        Debug.Log("Cannot unequip weapon directly. Swap with another compatible weapon instead.");
        ReturnToOriginalPosition();
    }
    
    private void HandleInventorySwap(Slot dropSlot, Slot originalSlot)
    {
        if (dropSlot.currentItem != null)
        {
            // Swap items
            dropSlot.currentItem.transform.SetParent(originalSlot.transform);
            originalSlot.currentItem = dropSlot.currentItem;
            dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        transform.SetParent(dropSlot.transform);
        dropSlot.currentItem = gameObject;
    }

    private void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    public void UpdateStatsUI()
    {
        InventoryController inventoryController = FindObjectOfType<InventoryController>();
        if (inventoryController != null)
        {
            // Update attributes
            inventoryController.currentVitalityText1.text = $"{playerStats.vitalityStat}";
            inventoryController.currentPowerText1.text = $"{playerStats.Power}";
            inventoryController.currentFortitudeText1.text = $"{playerStats.Fortitude}";
            inventoryController.currentArcaneText1.text = $"{playerStats.ArcaneStat}";
            inventoryController.dmgText1.text = $"{playerStats.currentDamage}";

            // Update power bonus text
            int powerBonus = playerStats.equippedWeapon != null ? playerStats.equippedWeapon.powerBonus : 0;
            inventoryController.currentPowerBonusText1.text = $"(+{powerBonus})";

            // Update base stats
            inventoryController.hpText1.text = $"{playerStats.maxVitality}";
            inventoryController.mpText1.text = $"{playerStats.maxIntelligence}";

            // Update resistances
            float physicalResistance = playerStats.Fortitude * 0.5f;
            float arcaneResistance = playerStats.ArcaneStat * 0.5f;
            inventoryController.physResText1.text = $"{physicalResistance:F1}%";
            inventoryController.arcaneResText1.text = $"{arcaneResistance:F1}%";
        }
    }
}