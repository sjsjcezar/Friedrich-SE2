using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopTemplate : MonoBehaviour
{
    public TMP_Text titleTxt;
    public TMP_Text stat1Txt;
    public TMP_Text stat2Txt;
    public TMP_Text costTxt;
    public Image weaponImage;  // Reference to the UI Image component
    
    private WeaponSO weaponSO;
    private GameObject weaponPrefab;
    public ShopItemSO shopItem;

    public Button purchaseButton;   

    public void SetupShopItem(ShopItemSO shopItem)
    {
        if (shopItem == null) return;

        // Get the WeaponItem component from the prefab
        if (shopItem.weaponPrefabs != null)
        {
            WeaponItem weaponItem = shopItem.weaponPrefabs.GetComponent<WeaponItem>();
            if (weaponItem != null && weaponItem.weaponSO != null)
            {
                weaponSO = weaponItem.weaponSO;
                weaponPrefab = shopItem.weaponPrefabs;

                // Set the weapon information
                titleTxt.text = weaponSO.weaponName;
                stat1Txt.text = $"Damage: {weaponSO.damage}";
                stat2Txt.text = $"Power Bonus: {weaponSO.powerBonus}";
                costTxt.text = $"{shopItem.baseCost} FC";

                // Set the weapon image if available
                Image prefabImage = weaponPrefab.GetComponent<Image>();
                if (prefabImage != null && prefabImage.sprite != null)
                {
                    weaponImage.sprite = prefabImage.sprite;
                }

                
            }

            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(() => ShopManager.instance.PurchaseItem(shopItem));
        }
    }

    public void OnPurchase()
    {
        if (ShopManager.instance != null)
        {
            ShopManager.instance.PurchaseItem(shopItem);
        }
        else
        {
            Debug.LogError("ShopManager instance is not found!");
        }
    }

    public WeaponSO GetWeaponSO()
    {
        return weaponSO;
    }

    public GameObject GetWeaponPrefab()
    {
        return weaponPrefab;
    }
}