using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{

    public static ShopManager instance;
    public TMP_Text fateCrystalsUI;
    //public ShopItemSO[] shopItemSO;
    public GameObject[] shopPanelsGO;
    public ShopTemplate[] shopPanels;
    
    
    public GameObject shopUI;
    public PlayerStats playerStats; // Reference to PlayerStats script
    public Button leaveButton; // Reference to the "Leave" button in the shop UI
    public CanvasGroup shopCanvasGroup; // CanvasGroup for fading effect

    // Add reference to the BlacksmithInteraction script
    public BlacksmithInteraction blacksmithInteraction;


    void Awake()
{
    if (instance == null)
    {
        instance = this;
    }
    else
    {
        Destroy(gameObject);  // Prevent duplicate instances
    }
}
    void Start()
    {
    
        leaveButton.onClick.AddListener(CloseShop);
        //for (int i = 0; i < shopItemSO.Length; i++)
            //shopPanelsGO[i].SetActive(true); 
        UpdateFateCrystalsUI();
        LoadPanels();
        CheckPurchasable();
        shopUI.SetActive(false);
    }

public void CheckPurchasable()
{
    foreach (var panel in shopPanels)  // Assuming shopPanels is a collection of ShopTemplate
    {
        if (panel.shopItem != null && panel.purchaseButton != null)
        {
            panel.purchaseButton.interactable = playerStats.currentCrystalsHeld >= panel.shopItem.baseCost;
        }
        else
        {
            Debug.LogError("ShopItem or PurchaseButton is null for panel: " + panel.name);
        }
    }
}

    public void PurchaseItem(ShopItemSO item)
    {
        if (item == null)
        {
            Debug.LogError("Attempted to purchase a null ShopItemSO.");
            return;
        }

        // Check if player has enough crystals
        if (playerStats.currentCrystalsHeld >= item.baseCost)
        {
            playerStats.currentCrystalsHeld -= item.baseCost; // Deduct cost
            UpdateFateCrystalsUI();

            // Add item to inventory
            if (item.weaponPrefabs != null)
            {
                InventoryController.Instance.AddPurchasedItemToInventory(item.weaponPrefabs);
                Debug.Log($"Purchased '{item.title}' and added it to the inventory.");
            }
            else
            {
                Debug.LogWarning($"No weapon prefab assigned for '{item.title}'.");
            }

            CheckPurchasable(); // Update button interactability
        }
        else
        {
            Debug.LogWarning($"Not enough Fate Crystals to purchase '{item.title}'. Cost: {item.baseCost}, Available: {playerStats.currentCrystalsHeld}");
        }
    }


    public void LoadPanels()
    {
        for (int i = 0; i < shopItemSO.Length; i++)
        {
            if (shopItemSO[i] != null)
            {
                shopPanels[i].SetupShopItem(shopItemSO[i]);
            }
            else
            {
                Debug.LogError($"Shop item SO at index {i} is null!");
            }
        }
    }

public void OpenShop()
{
    if (shopUI != null)
    {
        shopUI.SetActive(true);  // Ensure the shop UI is activated
        shopCanvasGroup.alpha = 1; // Ensure full visibility
        Debug.Log("Shop UI is now visible.");
    }
    else
    {
        Debug.LogError("Shop UI is not assigned!");
    }
}

private bool isShopOpen = false;

public void CloseShop()
{
    StartCoroutine(FadeOutShopUI());
    isShopOpen = false; // Reset shop state
}


   public void OnLeaveShopClicked()
    {
        // Fade out and close the shop UI when "Leave" button is clicked
        StartCoroutine(FadeOutShopUI());

    }

    private IEnumerator FadeOutShopUI()
    {
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            shopCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        shopCanvasGroup.alpha = 0;
        shopUI.SetActive(false);

        if (blacksmithInteraction != null)
        {
            blacksmithInteraction.OnShopClosed();
        }
        else
        {
            Debug.LogError("BlacksmithInteraction is not assigned!");
        }
    }


    private void UpdateFateCrystalsUI()
    {
        if (fateCrystalsUI != null && playerStats != null)
        {
            fateCrystalsUI.text = "Crystals: " + playerStats.currentCrystalsHeld.ToString();
        }
    }
}
