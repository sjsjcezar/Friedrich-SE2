using System;

[Serializable]
public class ItemData
{
    public string itemName;
    public string itemType;      // "Weapon", "HealthVial", "ManaVial", etc.
    public string uniqueID;      // Unique identifier for each item
    public bool isEquipped;      // Indicates if the item is currently equipped
}