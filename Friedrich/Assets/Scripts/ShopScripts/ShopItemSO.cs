using UnityEngine;

[CreateAssetMenu(fileName = "ShopMenu", menuName = "Scriptable Objects/New Shop Item", order = 1)]
public class ShopItemSO : ScriptableObject
{
    public string title;  // This will be auto-populated from WeaponSO
    public int baseCost;
    public GameObject weaponPrefabs;  // This should have WeaponItem component with WeaponSO reference

    // Helper function to setup from WeaponSO
    public void SetupFromWeaponSO(WeaponSO weaponSO, int cost)
    {
        if (weaponSO != null)
        {
            title = weaponSO.weaponName;
            baseCost = cost;
            weaponPrefabs = weaponSO.weaponPrefab;
        }
    }
}