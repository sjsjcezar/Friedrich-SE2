using UnityEngine;
using System;

public class NonWeaponItem : MonoBehaviour
{
    public string uniqueID;
    public string itemType; // Added this field
    public bool isEquipped; // Ensure consistency with WeaponItem if necessary

    private void Awake()
    {
        uniqueID = Guid.NewGuid().ToString();
        isEquipped = false; // Initialize as not equipped
    }
}