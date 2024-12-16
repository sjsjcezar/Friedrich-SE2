using UnityEngine;

public class Slot : MonoBehaviour
{
    public GameObject currentItem; // The item currently held in the slot
    public bool isEquipmentSlot = false; // Flag to identify equipment slots
    public string allowedItemType; // e.g., "Weapon"
}