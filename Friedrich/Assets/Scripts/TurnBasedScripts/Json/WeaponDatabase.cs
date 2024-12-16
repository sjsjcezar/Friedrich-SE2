using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "ScriptableObjects/WeaponDatabase", order = 1)]
public class WeaponDatabase : ScriptableObject
{
    public static WeaponDatabase Instance { get; private set; }

    public List<WeaponSO> weapons;
    public List<GameObject> weaponPrefabs;  // Add this field for prefabs

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of WeaponDatabase found!");
        }
    }

    public GameObject GetWeaponPrefabByName(string weaponName)
    {
        return weaponPrefabs.Find(wp => {
            WeaponItem weaponItem = wp.GetComponent<WeaponItem>();
            return weaponItem != null && 
                   weaponItem.weaponSO != null && 
                   weaponItem.weaponSO.weaponName.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase);
        });
    }

    public GameObject GetWeaponPrefabByID(string uniqueID)
    {
        return weaponPrefabs.Find(wp => {
            WeaponItem weaponItem = wp.GetComponent<WeaponItem>();
            return weaponItem != null && weaponItem.uniqueID == uniqueID;
        });
    }
}