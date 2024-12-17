using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class StatueController : MonoBehaviour
{
    public static StatueController Instance;
    private Animator playerAnimator;

    public GameObject statueUIPanel;
    private bool isPlayerNearby = false;
    private PlayerStatsHolder playerStatsHolder;
    private PlayerStats playerStats => playerStatsHolder?.playerStats;
    private PlayerMovement playerMovement;   

    [Header("Interactive UI Elements")]
    public GameObject interactPanel;
    public TextMeshProUGUI interactText;

    [Header("UI Elements")]
    public Button teleportButton;
    public Button levelUpButton;
    public Button vialsButton;
    public Button upgradeSpellsButton;
    public Button leaveButton;
    public Button teleport2Button;
    public TextMeshProUGUI textWherePlayerIs;

    [Header("Teleport UI Elements")]
    public GameObject teleportOptionsPanel;
    public Button yggdrasilButton;
    public GameObject yggdrasilGreyedOutPanel;
    public Button outskirtsButton;
    public GameObject outskirtsGreyedOutPanel;
    public Button backButton;
    public GameObject teleport2GreyedOutPanel;

    [Header("Teleport2 UI Elements")]
    public GameObject teleport2OptionsPanel;
    public Button castleButton;
    public GameObject castleGreyedOutPanel;
    public Button blacksmithButton;
    public GameObject blacksmithGreyedOutPanel;
    public Button back2Button;

    [Header("Level Up UI Elements")]
    public GameObject levelUpPanel;
    public TextMeshProUGUI levelUpCounterText;
    public TextMeshProUGUI currentCrystalCounterText;
    public TextMeshProUGUI crystalsNeededCounterText;
    public TextMeshProUGUI currentVitalityText;
    public TextMeshProUGUI currentPowerText;
    public TextMeshProUGUI currentFortitudeText;
    public TextMeshProUGUI currentArcaneText;
    public Button actualLevelUpButton;
    public Button leaveLevelUpButton;

    [Header("Stats Display")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;
    public TextMeshProUGUI physResText;
    public TextMeshProUGUI arcaneResText;

    [Header("Enemy Prefabs")]
    public GameObject lesserDemonPrefab;
    public GameObject testSubjectPrefab;

    [Header("Teleport Effects")]
    public AudioSource audioSource;
    public AudioClip teleportSound;
    public float teleportSoundDelay = 0.5f; // Delay before playing sound during black screen
    public float blackScreenDuration = 1.5f; // How long to stay on black screen

    public Image fadeImage;
    public float fadeDuration = 1f;
    private bool isFading = false;

    [Header("Transition Panels")]
    public GameObject leftTransitionPanel;
    public GameObject rightTransitionPanel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (interactPanel != null)
        {
            interactPanel.SetActive(false);
        }

        statueUIPanel.SetActive(false);
        vialsButton.gameObject.SetActive(false);
        teleportButton.gameObject.SetActive(false);
	teleport2Button.gameObject.SetActive(false);
        levelUpButton.gameObject.SetActive(false);
        upgradeSpellsButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
        vialsButton.onClick.AddListener(OpenVialController);
        leaveButton.onClick.AddListener(LeaveStatue);
        levelUpPanel.SetActive(false);

        levelUpButton.gameObject.SetActive(false);
        levelUpCounterText.gameObject.SetActive(false);
        actualLevelUpButton.gameObject.SetActive(false);
        leaveLevelUpButton.gameObject.SetActive(false);
        currentCrystalCounterText.gameObject.SetActive(false);
        crystalsNeededCounterText.gameObject.SetActive(false);
        currentVitalityText.gameObject.SetActive(false);
        currentPowerText.gameObject.SetActive(false);
        currentFortitudeText.gameObject.SetActive(false);
        currentArcaneText.gameObject.SetActive(false);

        hpText.gameObject.SetActive(false);
        mpText.gameObject.SetActive(false);
        physResText.gameObject.SetActive(false);
        arcaneResText.gameObject.SetActive(false);

        levelUpButton.onClick.AddListener(OpenLevelUpPanel);
        actualLevelUpButton.onClick.AddListener(LevelUpPlayer);
        leaveLevelUpButton.onClick.AddListener(CloseLevelUpPanel);

        outskirtsButton.onClick.AddListener(TeleportToOutskirts);
        backButton.onClick.AddListener(BackFromTeleportOptions);
        yggdrasilButton.onClick.AddListener(TeleportToYggdrasil);
        teleportOptionsPanel.SetActive(false);
	
	castleButton.onClick.AddListener(TeleportToCastle);
	back2Button.onClick.AddListener(BackFromTeleport2Options);
	blacksmithButton.onClick.AddListener(TeleportToBlacksmith);
	teleport2OptionsPanel.SetActive(false);
        
        // Initialize fade image
        Color color = fadeImage.color;
        color.a = 0;
        fadeImage.color = color;
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && !isFading)
        {
            if (playerStats != null)
            {
                StartCoroutine(FadeAndShowUI());
            }
            else
            {
                Debug.LogWarning("PlayerStats reference is missing. Cannot replenish Vitality and Intelligence.");
            }
        }
    }

    private void OpenTeleportOptions()
    {
        // Disable existing UI elements
        statueUIPanel.SetActive(false);
        vialsButton.gameObject.SetActive(false);
        teleportButton.gameObject.SetActive(false);
	teleport2Button.gameObject.SetActive(false);
        levelUpButton.gameObject.SetActive(false);
        upgradeSpellsButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);

        // Show teleport options
        teleportOptionsPanel.SetActive(true);

        // Update teleport buttons based on current scene
        UpdateTeleportButtons();
    }

    private void OpenTeleport2Options()
    {
        statueUIPanel.SetActive(false);
        vialsButton.gameObject.SetActive(false);
        teleportButton.gameObject.SetActive(false);
	teleport2Button.gameObject.SetActive(false);
        levelUpButton.gameObject.SetActive(false);
        upgradeSpellsButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
        teleport2OptionsPanel.SetActive(true);
        UpdateTeleport2Buttons();
    }

    private void UpdateTeleportButtons()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene.Equals("YggdrasilSampleScene", System.StringComparison.OrdinalIgnoreCase))
        {
            // Disable Yggdrasil button and show greyed panel
            yggdrasilButton.interactable = false;
            SetButtonTextColor(yggdrasilButton, Color.gray);
            if (yggdrasilGreyedOutPanel != null)
                yggdrasilGreyedOutPanel.SetActive(true);

            // Enable Outskirts button and hide greyed panel
            outskirtsButton.interactable = true;
            SetButtonTextColor(outskirtsButton, Color.white);
            if (outskirtsGreyedOutPanel != null)
                outskirtsGreyedOutPanel.SetActive(false);

            // Update location text color
            if (textWherePlayerIs != null)
                textWherePlayerIs.color = Color.white;
        }
        else if (currentScene.Equals("SampleScene", System.StringComparison.OrdinalIgnoreCase))
        {
            // Enable Yggdrasil button and hide greyed panel
            yggdrasilButton.interactable = true;
            SetButtonTextColor(yggdrasilButton, Color.white);
            if (yggdrasilGreyedOutPanel != null)
                yggdrasilGreyedOutPanel.SetActive(false);

            // Disable Outskirts button and show greyed panel
            outskirtsButton.interactable = false;
            SetButtonTextColor(outskirtsButton, Color.gray);
            if (outskirtsGreyedOutPanel != null)
                outskirtsGreyedOutPanel.SetActive(true);

            // Update location text color
            if (textWherePlayerIs != null)
                textWherePlayerIs.color = Color.white;
        }
        else
        {
            // Enable both buttons and hide greyed panels
            yggdrasilButton.interactable = true;
            outskirtsButton.interactable = true;
            SetButtonTextColor(yggdrasilButton, Color.white);
            SetButtonTextColor(outskirtsButton, Color.white);
            
            if (yggdrasilGreyedOutPanel != null)
                yggdrasilGreyedOutPanel.SetActive(false);
            if (outskirtsGreyedOutPanel != null)
                outskirtsGreyedOutPanel.SetActive(false);

            // Update location text color
            if (textWherePlayerIs != null)
                textWherePlayerIs.color = Color.white;
        }
    }

    private void UpdateTeleport2Buttons()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // First handle the Teleport 2 Button state based on scene
        if (currentScene.Equals("SampleScene", System.StringComparison.OrdinalIgnoreCase))
        {
            // Disable Teleport 2 Button in SampleScene
            teleport2Button.interactable = false;
            SetButtonTextColor(teleport2Button, Color.gray);
            if (teleport2GreyedOutPanel != null)
                teleport2GreyedOutPanel.SetActive(true);

            if (textWherePlayerIs != null)
            textWherePlayerIs.color = Color.white;

            if (castleGreyedOutPanel != null)
                castleGreyedOutPanel.SetActive(true);
            if (blacksmithGreyedOutPanel != null)
                blacksmithGreyedOutPanel.SetActive(true);
        }
        else if (currentScene.Equals("YggdrasilSampleScene", System.StringComparison.OrdinalIgnoreCase))
        {
            // Enable Teleport 2 Button in other scenes
            teleport2Button.interactable = true;
            SetButtonTextColor(teleport2Button, Color.white);
            if (teleport2GreyedOutPanel != null)
                teleport2GreyedOutPanel.SetActive(false);

            if (textWherePlayerIs != null)
            textWherePlayerIs.color = Color.white;
            
            if (castleGreyedOutPanel != null)
                castleGreyedOutPanel.SetActive(false);
            if (blacksmithGreyedOutPanel != null)
                blacksmithGreyedOutPanel.SetActive(false);
        }


        // Update location text color
        if (textWherePlayerIs != null)
            textWherePlayerIs.color = Color.white;
    }

    private void SetButtonTextColor(Button button, Color color)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = color;
        }
    }

    private void TeleportToYggdrasil()
    {
        GlobalVariables.lastTeleportSpawnID = "outskirts_spawn";
        StartCoroutine(TeleportWithEffects("YggdrasilSampleScene"));
    }

    private void TeleportToOutskirts()
    {
        GlobalVariables.lastTeleportSpawnID = "yggdrasil_spawn";
        StartCoroutine(TeleportWithEffects("SampleScene"));
    }

private void TeleportToCastle()
{
    Vector3 targetPosition = new Vector3(332, -100, 0);
        
	GameObject persistentCanvas = GameObject.Find("DialogueCanvas"); // Replace with your canvas name
        if (persistentCanvas != null)
        {
            leftTransitionPanel = persistentCanvas.transform.Find("LeftTransitionPanel").gameObject;
            rightTransitionPanel = persistentCanvas.transform.Find("RightTransitionPanel").gameObject;
        }
        StartCoroutine(PlayOpeningAnimation());
    
   TeleportPlayer(targetPosition);
}

private void TeleportToBlacksmith()
{
    Vector3 targetPosition = new Vector3(333, -164, 0);
    
    	GameObject persistentCanvas = GameObject.Find("DialogueCanvas"); // Replace with your canvas name
        if (persistentCanvas != null)
        {
            leftTransitionPanel = persistentCanvas.transform.Find("LeftTransitionPanel").gameObject;
            rightTransitionPanel = persistentCanvas.transform.Find("RightTransitionPanel").gameObject;
        }
        StartCoroutine(PlayOpeningAnimation());

    TeleportPlayer(targetPosition);
}

private void TeleportPlayer(Vector3 targetPosition)
{
    if (playerMovement != null)
    {
        playerMovement.enabled = false;
    }

    if (playerMovement != null)
    {
        playerMovement.transform.position = targetPosition;
    }

    if (playerMovement != null)
    {
        playerMovement.enabled = true;
    }
    CleanupStatueUI();
}

    private IEnumerator PlayOpeningAnimation()
    {
        // Start with time paused
        Time.timeScale = 2f;

        // Make sure panels are active and centered
        leftTransitionPanel.SetActive(true);
        rightTransitionPanel.SetActive(true);
        
        RectTransform leftRect = leftTransitionPanel.GetComponent<RectTransform>();
        RectTransform rightRect = rightTransitionPanel.GetComponent<RectTransform>();
        
        // Start from center
        leftRect.anchoredPosition = Vector2.zero;
        rightRect.anchoredPosition = Vector2.zero;

        // Wait a frame to ensure everything is set up
        yield return null;

        // Smoothly open the panels
        float elapsedTime = 0f;
        float openDuration = 2f;  // Faster opening in battle
        
        while (elapsedTime < openDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / openDuration;
            float smoothProgress = progress * progress * (3f - 2f * progress);
            
            // Move panels from center back to edges
            leftRect.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(-Screen.width, 0), smoothProgress);
            rightRect.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(Screen.width, 0), smoothProgress);
            yield return null;
        }
        
        // Hide panels when done
        leftTransitionPanel.SetActive(false);
        rightTransitionPanel.SetActive(false);

        // Resume time for battle
        Time.timeScale = 1f;
    }

    private IEnumerator TeleportWithEffects(string sceneName)
    {
        CleanupStatueUI();
        
        // Disable player movement during transition
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            Debug.Log("PlayerMovement disabled for teleportation.");
        }
        else
        {
            Debug.LogError("PlayerMovement reference is null before teleportation.");
        }

        // Fade to black
        yield return StartCoroutine(FadeOut());

        // Play teleport sound after a short delay
        if (audioSource != null && teleportSound != null)
        {
            yield return new WaitForSeconds(teleportSoundDelay);
            audioSource.PlayOneShot(teleportSound);
            Debug.Log("Teleport sound played.");
            yield return new WaitForSeconds(teleportSound.length);
        }
        else
        {
            Debug.LogWarning("AudioSource or TeleportSound is not assigned.");
        }

        // Additional black screen duration if needed
        yield return new WaitForSeconds(blackScreenDuration);

        // Load the new scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log($"Scene '{sceneName}' loaded successfully.");

        // Wait a frame to ensure all scene objects are properly initialized
        yield return new WaitForEndOfFrame();

        // Re-acquire the PlayerMovement reference in the new scene
        playerMovement = PlayerMovement.instance;

        if (playerMovement != null)
        {
            Debug.Log("PlayerMovement instance found after scene load.");
        }
        else
        {
            Debug.LogError("PlayerMovement.instance is null after scene load.");
        }

        // Reposition player at spawn point
        RepositionPlayerAfterLoad();

        // Fade back in
        yield return StartCoroutine(FadeIn());

        // Re-enable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            Debug.Log("PlayerMovement re-enabled after teleportation.");
        }
        else
        {
            Debug.LogError("PlayerMovement reference is null. Cannot re-enable movement.");
        }

        // Ensure PlayerMovement is enabled using the static instance
        if (PlayerMovement.instance != null)
        {
            PlayerMovement.instance.enabled = true;
            Debug.Log("PlayerMovement.instance.enabled set to true.");
        }
        else
        {
            Debug.LogError("PlayerMovement.instance is null when trying to enable it.");
        }

        // Hide all statue-related UI
        CleanupStatueUI();
    }

    private void CleanupStatueUI()
    {
        statueUIPanel.SetActive(false);
        teleportOptionsPanel.SetActive(false);
	teleport2OptionsPanel.SetActive(false);
        vialsButton.gameObject.SetActive(false);
        teleportButton.gameObject.SetActive(false);
        levelUpButton.gameObject.SetActive(false);
        upgradeSpellsButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
	teleport2Button.gameObject.SetActive(false);
        
        // Make sure vial UI is visible in the overworld
        FindObjectOfType<VialOverworldManager>()?.SetUIVisibility(true);
    }

    private void RepositionPlayerAfterLoad()
    {
        // Find the TeleportSpawnPoint in the new scene
        TeleportSpawnPoint spawnPoint = FindObjectOfType<TeleportSpawnPoint>();
        if (spawnPoint != null)
        {
            PlayerMovement.instance.transform.position = spawnPoint.transform.position;
            PlayerMovement.instance.transform.rotation = spawnPoint.transform.rotation;
        }
        else
        {
            Debug.LogWarning("No TeleportSpawnPoint found in the loaded scene.");
        }
    }

    private void BackFromTeleportOptions()
    {
        // Hide teleport options
        teleportOptionsPanel.SetActive(false);
        
        // Re-enable existing UI elements
        statueUIPanel.SetActive(true);
        vialsButton.gameObject.SetActive(true);
        teleportButton.gameObject.SetActive(true);
        levelUpButton.gameObject.SetActive(true);
        upgradeSpellsButton.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);
        teleport2Button.gameObject.SetActive(true);
    }

    private void BackFromTeleport2Options()
    {
	// Hide teleport options
        teleport2OptionsPanel.SetActive(false);
        
        // Re-enable existing UI elements
        statueUIPanel.SetActive(true);
        vialsButton.gameObject.SetActive(true);
        teleportButton.gameObject.SetActive(true);
        levelUpButton.gameObject.SetActive(true);
        upgradeSpellsButton.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);
        teleport2Button.gameObject.SetActive(true);

    }


    private void OpenLevelUpPanel()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats is null. Cannot open Level Up Panel.");
            return;
        }

        levelUpPanel.SetActive(true);
        actualLevelUpButton.gameObject.SetActive(true);
        leaveLevelUpButton.gameObject.SetActive(true);
        levelUpCounterText.gameObject.SetActive(true);
        currentCrystalCounterText.gameObject.SetActive(true);
        crystalsNeededCounterText.gameObject.SetActive(true);
        currentVitalityText.gameObject.SetActive(true);
        currentPowerText.gameObject.SetActive(true);
        currentFortitudeText.gameObject.SetActive(true);
        currentArcaneText.gameObject.SetActive(true);

        hpText.gameObject.SetActive(true);
        mpText.gameObject.SetActive(true);
        physResText.gameObject.SetActive(true);
        arcaneResText.gameObject.SetActive(true);

        UpdateLevelUpUI();
    }

    private void UpdateLocationText()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (currentScene.Equals("YggdrasilSampleScene", System.StringComparison.OrdinalIgnoreCase))
        {
            textWherePlayerIs.text = "Yggdrasil";
        }
        else if (currentScene.Equals("SampleScene", System.StringComparison.OrdinalIgnoreCase))
        {
            textWherePlayerIs.text = "Yggdrasil's Outskirts";
        }
	else if (currentScene.Equals("CastleInterior", System.StringComparison.OrdinalIgnoreCase))
	{
	    textWherePlayerIs.text = "Castle";
	}
	else if(currentScene.Equals("BlacksmithInterior", System.StringComparison.OrdinalIgnoreCase))
	{
	    textWherePlayerIs.text = "Blacksmith Shop";
	}
    }

    private void CloseLevelUpPanel()
    {
        levelUpPanel.SetActive(false);
        actualLevelUpButton.gameObject.SetActive(false);
        leaveLevelUpButton.gameObject.SetActive(false);
        levelUpCounterText.gameObject.SetActive(false);
        currentCrystalCounterText.gameObject.SetActive(false);
        crystalsNeededCounterText.gameObject.SetActive(false);
        currentVitalityText.gameObject.SetActive(false);
        currentPowerText.gameObject.SetActive(false);
        currentFortitudeText.gameObject.SetActive(false);
        currentArcaneText.gameObject.SetActive(false);

        hpText.gameObject.SetActive(false);
        mpText.gameObject.SetActive(false);
        physResText.gameObject.SetActive(false);
        arcaneResText.gameObject.SetActive(false);
    }

    private void UpdateLevelUpUI()
    {
        levelUpCounterText.text = $"{playerStats.Level}";
        currentCrystalCounterText.text = $"{playerStats.currentCrystalsHeld}";
        crystalsNeededCounterText.text = $"{playerStats.crystalsNeededForNextLevel}";
        currentVitalityText.text = $"{playerStats.vitalityStat}";
        currentPowerText.text = $"{playerStats.Power}";
        currentFortitudeText.text = $"{playerStats.Fortitude}";
        currentArcaneText.text = $"{playerStats.ArcaneStat}";

        hpText.text = $"{playerStats.maxVitality}";
        mpText.text = $"{playerStats.maxIntelligence}";
        float physicalResistance = playerStats.Fortitude * 0.5f; // 0.5% per point
        float arcaneResistance = playerStats.ArcaneStat * 0.5f;  // 0.5% per point
        physResText.text = $"{physicalResistance:F1}%";
        arcaneResText.text = $"{arcaneResistance:F1}%";


        // Enable or disable the Level Up button based on crystal count
        actualLevelUpButton.interactable = playerStats.currentCrystalsHeld >= playerStats.crystalsNeededForNextLevel;
    }

    private void LevelUpPlayer()
    {
        if (playerStats.TryLevelUp())
        {
            UpdateLevelUpUI();
            Debug.Log("Player leveled up successfully!");
        }
        else
        {
            Debug.Log("Player cannot level up yet.");
        }
    }

    private IEnumerator FadeAndShowUI()
    {
        isFading = true;
        interactPanel.SetActive(false);
        
        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            
            // Set player to idle animation facing upward
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("Speed", 0);  // Ensure we're in idle state
                playerAnimator.SetFloat("IdleHorizontal", 0);  // No horizontal facing
                playerAnimator.SetFloat("IdleVertical", 1);    // Face upward
            }
        }

        FindObjectOfType<VialOverworldManager>()?.SetUIVisibility(false);
        
        yield return StartCoroutine(FadeOut());
        
        ShowStatueUI();
        AutoReplenishVials();

        // Refill player's stats
        playerStats.Vitality = playerStats.maxVitality;
        playerStats.Intelligence = playerStats.maxIntelligence;

        // Update UI
        FindObjectOfType<HealthManager>()?.UpdatePlayerHealth(playerStats.Vitality, playerStats.maxVitality);
        FindObjectOfType<HealthManager>()?.UpdatePlayerIntelligence(playerStats.Intelligence, playerStats.maxIntelligence);

        Debug.Log("Player's Vitality and Intelligence have been fully replenished.");
        StartCoroutine(RespawnEnemies());

        yield return StartCoroutine(FadeIn());
        isFading = false;
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(1f);
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    } 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (interactPanel != null)
            {
                interactPanel.SetActive(true);
            }
            isPlayerNearby = true;
            playerStatsHolder = collision.GetComponent<PlayerStatsHolder>();
            playerMovement = collision.GetComponent<PlayerMovement>();
            playerAnimator = collision.GetComponent<Animator>();
            
            if (playerStatsHolder == null)
            {
                Debug.LogError("PlayerStatsHolder component not found on the Player object.");
            }
            if (playerMovement == null)
            {
                Debug.LogError("PlayerMovement component not found on the Player object.");
            }
            if (playerAnimator == null)
            {
                Debug.LogError("Animator component not found on the Player object.");
            }
        }
    }

    private IEnumerator RespawnEnemies()
    {
        foreach (var spawnPoint in GlobalVariables.enemySpawnPoints)
        {
            if (GlobalVariables.enemyStatus.ContainsKey(spawnPoint.enemyID) && GlobalVariables.enemyStatus[spawnPoint.enemyID])
            {
                // Find existing enemy in scene with matching ID
                Enemy[] allEnemies = FindObjectsOfType<Enemy>(true); // true to include inactive objects
                Enemy existingEnemy = allEnemies.FirstOrDefault(e => e.uniqueID == spawnPoint.enemyID);

                if (existingEnemy != null)
                {
                    // Reset existing enemy
                    existingEnemy.transform.position = spawnPoint.spawnPosition;
                    existingEnemy.isDead = false;
                    existingEnemy.gameObject.SetActive(true);
                    GlobalVariables.enemyStatus[spawnPoint.enemyID] = false;
                    Debug.Log($"Respawned existing enemy {spawnPoint.enemyType} with ID {spawnPoint.enemyID}");
                }
                else
                {
                    Debug.LogError($"Could not find enemy with ID {spawnPoint.enemyID} in the scene");
                }
            }
        }
        yield return null;
    }

    private GameObject GetEnemyPrefab(CharacterType enemyType)
    {
        switch (enemyType)
        {
            case CharacterType.LesserDemon:
                return lesserDemonPrefab;
            case CharacterType.TestSubject:
                return testSubjectPrefab;
            default:
                Debug.LogError($"Invalid enemy type for respawn: {enemyType}");
                return null;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactPanel != null)
            {
                interactPanel.SetActive(false);
            }
            statueUIPanel.SetActive(false);
            vialsButton.gameObject.SetActive(false);
            playerStatsHolder = null;
        }
    }

    private void AutoReplenishVials()
    {
        // Sync current vial counts to allocated ones
        GlobalVariables.currentHealVialCount = GlobalVariables.healVialCount;
        GlobalVariables.currentManaVialCount = GlobalVariables.manaVialCount;

        Debug.Log("Vials replenished to allocated counts!");
    }

    private void OpenVialController()
    {
        // Hide statue UI elements but don't disable player movement
        statueUIPanel.SetActive(false);
        vialsButton.gameObject.SetActive(false);
        teleportButton.gameObject.SetActive(false);
        levelUpButton.gameObject.SetActive(false);
        upgradeSpellsButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
	teleport2Button.gameObject.SetActive(false);
        
        VialController.Instance.OpenVialUI();
    }

    private void LeaveStatue()
    {
        FadeAndHideUI();
    }

    private void FadeAndHideUI()
    {
        // Hide UI elements
        interactPanel.SetActive(true);
        statueUIPanel.SetActive(false);
        vialsButton.gameObject.SetActive(false);
        teleportButton.gameObject.SetActive(false);
        levelUpButton.gameObject.SetActive(false);
        upgradeSpellsButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
	teleport2Button.gameObject.SetActive(false);

        // Re-enable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        FindObjectOfType<VialOverworldManager>()?.SetUIVisibility(true);
    }

    public void ShowStatueUI()
    {
        statueUIPanel.SetActive(true);
        vialsButton.gameObject.SetActive(true);
        teleportButton.gameObject.SetActive(true);
        levelUpButton.gameObject.SetActive(true);
        upgradeSpellsButton.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);
	teleport2Button.gameObject.SetActive(true);
        UpdateLocationText();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (teleportButton != null)
        {
            teleportButton.onClick.AddListener(OpenTeleportOptions);
        }
        if (teleport2Button != null)
        {
            teleport2Button.onClick.AddListener(OpenTeleport2Options);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (teleportButton != null)
        {
            teleportButton.onClick.RemoveListener(OpenTeleportOptions);
        }
        if (teleport2Button != null)
        {
	    teleport2Button.onClick.RemoveListener(OpenTeleport2Options);
        }

    }

   private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
   {
       if (PlayerMovement.instance != null)
       {
           PlayerMovement.instance.enabled = true;
           Debug.Log("PlayerMovement.enabled set to true in OnSceneLoaded.");
       }
       else
       {
           Debug.LogError("PlayerMovement.instance is null in OnSceneLoaded.");
       }

       UpdateTeleportButtons();
       UpdateLocationText();
   }
}   