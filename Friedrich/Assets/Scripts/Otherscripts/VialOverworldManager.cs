using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class VialOverworldManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button vialHealButton;
    public Button vialMPButton;
    public Button arrowButton;
    public Image vialHealImage;
    public Image vialMPImage;
    public TMP_Text vialCountText;
    public TMP_Text vialNameText;
    public Image panelTop;
    public Image leftPanel;
    public Image rightPanel;

    [Header("Health/Mana Sliders")]
    public Slider healthSlider;
    public Slider manaSlider;
    public Slider staminaSlider;

    [Header("Healing Values")]
    [Range(0, 100)]
    public float healPercentage = 50f;
    [Range(0, 100)]
    public float manaPercentage = 30f;

    [Header("UI Fade Settings")]
    [Range(1f, 30f)]
    public float fadeDelay = 10f;
    [Range(0.1f, 5f)]
    public float fadeSpeed = 2f;

    private float idleTimer;
    private bool isUIVisible = true;
    private CanvasGroup uiCanvasGroup;
    private PlayerStats playerStats;
    private HealthManager healthManager;

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStatsHolder>()?.playerStats;
        healthManager = FindObjectOfType<HealthManager>();

        SetupUICanvasGroup();
        SetupInitialState();
        SetupButtonListeners();
        ResetIdleTimer();
    }

    private void SetupUICanvasGroup()
    {
        // Create a new GameObject to hold our UI elements that should fade
        GameObject uiHolder = new GameObject("UI_Fade_Holder");
        uiHolder.transform.SetParent(transform);
        
        // Add Canvas Group to the holder
        uiCanvasGroup = uiHolder.AddComponent<CanvasGroup>();
        
        // Get the original parent Canvas
        Canvas mainCanvas = healthSlider.GetComponentInParent<Canvas>();
        if (mainCanvas != null)
        {
            // Set the UI holder as a child of the main Canvas
            uiHolder.transform.SetParent(mainCanvas.transform, false);
            
            // Parent the UI elements to our holder while keeping their local positions
            healthSlider.transform.SetParent(uiHolder.transform, true);
            manaSlider.transform.SetParent(uiHolder.transform, true);
            staminaSlider.transform.SetParent(uiHolder.transform, true);
            panelTop.transform.SetParent(uiHolder.transform, true);
        }
        
        uiCanvasGroup.alpha = 1f;
    }

    private void SetupInitialState()
    {
        vialHealButton.gameObject.SetActive(true);
        vialMPButton.gameObject.SetActive(false);
        vialHealImage.gameObject.SetActive(false);
        vialMPImage.gameObject.SetActive(true);
        UpdateVialText();
    }

    private void SetupButtonListeners()
    {
        arrowButton.onClick.AddListener(SwitchVials);
        vialHealButton.onClick.AddListener(UseHealVial);
        vialMPButton.onClick.AddListener(UseManaVial);
    }

    private void Update()
    {
        HandleInput();
        HandleUIFade();
        UpdateSliders();
        UpdateVialText();
    }

    private void HandleInput()
    {
        if (PauseMenuController.GameIsPaused)
        {
            SetUIElementsActive(false);
            return;
        }
        bool isInteracting = 
            Input.GetKeyDown(KeyCode.Alpha1) || 
            Input.GetKeyDown(KeyCode.Alpha2) || 
            Input.GetKeyDown(KeyCode.H) ||
            IsMouseOverVialButtons();

        if (isInteracting)
        {
            ShowUI();
        }

        // Handle vial switching
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EnableHealVial();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EnableManaVial();
        }

        // Handle healing with 'H' key
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (vialHealButton.gameObject.activeSelf)
            {
                UseHealVial();
            }
            else
            {
                UseManaVial();
            }
        }
    }

    private bool IsMouseOverVialButtons()
    {
        return (vialHealButton != null && RectTransformUtility.RectangleContainsScreenPoint(
                   vialHealButton.GetComponent<RectTransform>(), Input.mousePosition)) ||
               (vialMPButton != null && RectTransformUtility.RectangleContainsScreenPoint(
                   vialMPButton.GetComponent<RectTransform>(), Input.mousePosition)) ||
               (arrowButton != null && RectTransformUtility.RectangleContainsScreenPoint(
                   arrowButton.GetComponent<RectTransform>(), Input.mousePosition));
    }

    private void HandleUIFade()
    {
        if (isUIVisible)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= fadeDelay)
            {
                FadeOutUI();
            }
        }
    }

    private void ShowUI()
    {
        if (!isUIVisible || uiCanvasGroup.alpha < 1f)
        {
            isUIVisible = true;
            StopAllCoroutines();
            StartCoroutine(FadeUI(1f));
        }
        ResetIdleTimer();
    }

    private void FadeOutUI()
    {
        if (isUIVisible)
        {
            isUIVisible = false;
            StartCoroutine(FadeUI(0f));
        }
    }

    private void ResetIdleTimer()
    {
        idleTimer = 0f;
    }

    private IEnumerator FadeUI(float targetAlpha)
    {
        float startAlpha = uiCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            uiCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeSpeed);
            yield return null;
        }

        uiCanvasGroup.alpha = targetAlpha;
    }

    // Your existing methods remain unchanged
    private void EnableHealVial()
    {
        vialHealButton.gameObject.SetActive(true);
        vialMPButton.gameObject.SetActive(false);
        vialHealImage.gameObject.SetActive(false);
        vialMPImage.gameObject.SetActive(true);
        UpdateVialText();
        ShowUI();
    }

    private void EnableManaVial()
    {
        vialHealButton.gameObject.SetActive(false);
        vialMPButton.gameObject.SetActive(true);
        vialHealImage.gameObject.SetActive(true);
        vialMPImage.gameObject.SetActive(false);
        UpdateVialText();
        ShowUI();
    }

    private void SwitchVials()
    {
        if (vialHealButton.gameObject.activeSelf)
        {
            EnableManaVial();
        }
        else
        {
            EnableHealVial();
        }
        ShowUI();
    }

    public void SetUIVisibility(bool visible)
    {
        SetUIElementsActive(visible);
        if (visible)
        {
            ShowUI();
        }
        else
        {
            FadeOutUI();
        }
    }

    public void SetUIElementsActive(bool active)
    {
        if (vialHealButton != null) vialHealButton.gameObject.SetActive(active);
        if (vialMPButton != null) vialMPButton.gameObject.SetActive(active && !vialHealButton.gameObject.activeSelf);
        if (arrowButton != null) arrowButton.gameObject.SetActive(active);
        if (vialHealImage != null) vialHealImage.gameObject.SetActive(active && !vialHealButton.gameObject.activeSelf);
        if (vialMPImage != null) vialMPImage.gameObject.SetActive(active && vialHealButton.gameObject.activeSelf);
        if (vialCountText != null) vialCountText.gameObject.SetActive(active);
        if (vialNameText != null) vialNameText.gameObject.SetActive(active);
        if (panelTop != null) panelTop.gameObject.SetActive(active);
        if (healthSlider != null) healthSlider.gameObject.SetActive(active);
        if (manaSlider != null) manaSlider.gameObject.SetActive(active);
        if (leftPanel != null) leftPanel.gameObject.SetActive(active);
        if (rightPanel != null) rightPanel.gameObject.SetActive(active);
        if (staminaSlider != null) staminaSlider.gameObject.SetActive(active);
    }

    private void UpdateVialText()
    {
        vialCountText.text = vialHealButton.gameObject.activeSelf ? 
            GlobalVariables.currentHealVialCount.ToString() : 
            GlobalVariables.currentManaVialCount.ToString();

        vialNameText.text = vialHealButton.gameObject.activeSelf ? 
            "Vial of Wounded Soul" : 
            "Vial of Arcane Whispers";
    }

    private void UpdateSliders()
    {
        if (playerStats != null)
        {
            healthSlider.value = (float)playerStats.Vitality / playerStats.maxVitality;
            manaSlider.value = (float)playerStats.Intelligence / playerStats.maxIntelligence;
        }
    }

    private void UseHealVial()
    {
        if (GlobalVariables.currentHealVialCount <= 0) return;

        if (playerStats != null)
        {
            int healAmount = Mathf.FloorToInt(playerStats.maxVitality * (healPercentage / 100f));
            playerStats.Vitality = Mathf.Min(playerStats.Vitality + healAmount, playerStats.maxVitality);
            GlobalVariables.currentHealVialCount--;
            
            if (healthManager != null)
            {
                healthManager.UpdatePlayerHealth(playerStats.Vitality, playerStats.maxVitality);
            }
            
            UpdateVialText();
            ShowUI();
        }
    }

    private void UseManaVial()
    {
        if (GlobalVariables.currentManaVialCount <= 0) return;

        if (playerStats != null)
        {
            int manaAmount = Mathf.FloorToInt(playerStats.maxIntelligence * (manaPercentage / 100f));
            playerStats.Intelligence = Mathf.Min(playerStats.Intelligence + manaAmount, playerStats.maxIntelligence);
            GlobalVariables.currentManaVialCount--;
            
            if (healthManager != null)
            {
                healthManager.UpdatePlayerIntelligence(playerStats.Intelligence, playerStats.maxIntelligence);
            }
            
            UpdateVialText();
            ShowUI();
        }
    }
}