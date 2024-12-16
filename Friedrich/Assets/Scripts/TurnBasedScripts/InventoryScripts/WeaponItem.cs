using UnityEngine;
using System;

public class WeaponItem : MonoBehaviour
{
    public WeaponSO weaponSO;
    public string uniqueID;  // Add this field
    
    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = System.Guid.NewGuid().ToString();
        }
    }
}