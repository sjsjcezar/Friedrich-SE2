using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{

    public static ShopManager instance;
    public TMP_Text fateCrystalsUI;
    public ShopItemSO[] shopItemSO;
    public GameObject[] shopPanelsGO;
    public ShopTemplate[] shopPanels;
    public Button[] myPurchaseBtns;
    // public GameObject[] weaponPrefabs; // Reference to weapon prefab
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
    for (int i = 0; i < myPurchaseBtns.Length; i++)
    {
        int index = i;  // Capture the correct index for the button

        // Add a listener for each button, passing the correct index
        myPurchaseBtns[i].onClick.AddListener(() =>
        {
            Debug.Log($"Button {index} clicked!");  // Debug to confirm which button was clicked
            PurchaseItem(index);  // Pass the correct index to the PurchaseItem method
        });
    }
        leaveButton.onClick.AddListener(CloseShop);
        for (int i = 0; i < shopItemSO.Length; i++)
            shopPanelsGO[i].SetActive(true); 
        UpdateFateCrystalsUI();
        LoadPanels();
        CheckPurchasable();
        shopUI.SetActive(false);
    }

    public void CheckPurchasable()
    {
        for (int i = 0; i < shopItemSO.Length; i++)
        {
            if (playerStats.currentCrystalsHeld >= shopItemSO[i].baseCost)
                myPurchaseBtns[i].interactable = true;
            else
                myPurchaseBtns[i].interactable = false;
        }
    }

    public void PurchaseItem(int btnNo)
{
    int itemCost = shopItemSO[btnNo].baseCost;
    Debug.Log($"Item selected: {shopItemSO[btnNo].title}, Cost: {itemCost}");

    // Check if the player has enough Fate Crystals
    if (playerStats.currentCrystalsHeld >= itemCost)
    {
        Debug.Log($"Enough Fate Crystals to purchase {shopItemSO[btnNo].title}. Current Crystals: {playerStats.currentCrystalsHeld}");

        // Deduct the cost from PlayerStats
        playerStats.currentCrystalsHeld -= itemCost;

        // Log the new crystal count
        Debug.Log($"New Fate Crystals: {playerStats.currentCrystalsHeld}");

        // Update the UI
        UpdateFateCrystalsUI();

        // Add the purchased weapon to the inventory
        GameObject weaponPrefab = shopItemSO[btnNo].weaponPrefabs;
        if (weaponPrefab != null)
        {
            InventoryController.Instance.AddPurchasedItemToInventory(weaponPrefab);
            Debug.Log($"Added {shopItemSO[btnNo].title} to inventory.");
        }
        else
        {
            Debug.LogWarning($"No weapon prefab assigned for {shopItemSO[btnNo].title}.");
        }

        // Update purchasable UI elements
        CheckPurchasable();
    }
    else
    {
        Debug.LogWarning($"Not enough Fate Crystals to purchase {shopItemSO[btnNo].title}. Current Crystals: {playerStats.currentCrystalsHeld}, Cost: {itemCost}");
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
