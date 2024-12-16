using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro; 

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance; // Singleton instance
    public GameObject magePlayerPrefab;
    public GameObject warriorPlayerPrefab;
    public GameObject lesserDemonPrefab;
    public GameObject testSubjectPrefab;

    private Enemy currentEnemyScript;
    private PlayerMovement playerMovement; // Reference to PlayerMovement
    private PlayerStats playerStats; // Reference to PlayerStats
    private CharacterType characterType;

    public GameObject currentEnemy; // Reference to the current enemy
    public GameObject player; // Reference to the current player
    private HealthManager healthManager;

    public bool isPlayerTurn = true;
    public bool isBattleActive = true; // Prevent actions when battle is over
    private float attackSlideDistance = 7f; // Distance to slide
    
    private float slideSpeed = 21f; // Speed of the slide
    public Transform playerTransform;
    private Vector3 originalPosition; // Store player's starting position
    private bool isAnimating = false;  // Track if an animation is playing

    public PlayerSkillManager playerSkillManager;

    [Header("Buttons for Turnbased")]
    public Button attackButton;
    public Button skillsButton;
    public Button flaskButton;    
    public Button healVialButton; 
    public Button manaVialButton;
    public Button backButtonVials; 
    public Button fleeButton;
    public Button rapidAssaultButton;
    public Button hackingSlashButton;
    public Button crescentSlashButton;
    public Button backButtonSkill;

    [Header("Panels For UI Turnbased")]
    public GameObject actionTrackerUI;
    public GameObject playerTabUI;
    public GameObject vialsTabUI;

    [Header("Action Text Settings")]
    private bool battleWon = false;
    private bool waitingForKeyPress = false;

    [Header("Global Light")]
    public UnityEngine.Rendering.Universal.Light2D globalLight;
    public float transitionDuration = 0.5f;


    [Header("Texts")]
    public TMP_Text counterForHealBattle;
    public TMP_Text counterForManaBattle;
    public TMP_Text actionText;

    // Add these at the top of the BattleManager class
    [Header("Hacking Slash Values")]
    public Slider timingSlider; // Reference to the UI Slider for the meter
    public Image blueBarImage;  // Reference for the blue fill area
    public float sliderSpeed = 12f; // Speed for the slider movement
    public Image border;
    public AudioSource audioSource; 
    public AudioClip[] successfulHitSounds;
    public AudioClip unsuccessfulHitSound; // Sound to play on miss

    [Header("Crescent Slash Values")]
    public Slider crescentSlider; // Reference to the slider in Crescent Slash
    public Image whiteBarImage; // Reference to white bar in UI

    private EnemyData enemyData;

    [Header("Transition Panels")]
    public GameObject leftTransitionPanel;
    public GameObject rightTransitionPanel;
    

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // If there is already an instance, destroy this one
        }

        globalLight = FindGlobalLight();
    }


    void Start()
    {   
        if (globalLight == null)
        {
            Debug.LogError("Global Light2D is not assigned!");
        }
        else
        {
            globalLight.intensity = 1; // Start with light off
        }
        audioSource = gameObject.AddComponent<AudioSource>(); 
        timingSlider.gameObject.SetActive(false);
        blueBarImage.gameObject.SetActive(false);
        border.gameObject.SetActive(false);

        crescentSlider.gameObject.SetActive(false);
        whiteBarImage.gameObject.SetActive(false);

        playerSkillManager = FindObjectOfType<PlayerSkillManager>();
        healthManager = FindObjectOfType<HealthManager>();
        string playerType = PlayerPrefs.GetString("PlayerType");
        Debug.Log("Player Type Retrieved: " + playerType);

        string enemyType = PlayerPrefs.GetString("EnemyType");

        Debug.Log("Player Type: " + playerType);
        Debug.Log("Enemy Type: " + enemyType);

        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerStats = playerMovement.playerStats;

            // Access PlayerStats from GlobalVariables using unique ID
            if (GlobalVariables.playerStatsReferences.ContainsKey(playerMovement.uniqueID))
            {
                playerStats = GlobalVariables.playerStatsReferences[playerMovement.uniqueID];
            }
        }

        if (GlobalVariables.playerData.ContainsKey("PlayerID"))
        {
            string playerID = GlobalVariables.playerData["PlayerID"];
            Debug.Log("Loaded Player ID from GlobalVariables: " + playerID);
        }
        if (rapidAssaultButton)
        {
            rapidAssaultButton.onClick.AddListener(() => StartCoroutine(HandleRapidAssault()));
        }
        if (hackingSlashButton)
        {
            hackingSlashButton.onClick.AddListener(() => StartCoroutine(StartHackingSlash()));
        }
        if (crescentSlashButton)
        {
            crescentSlashButton.onClick.AddListener(() => StartCrescentSlash());
        }

        flaskButton.onClick.AddListener(OnFlaskButtonPressed);
        healVialButton.onClick.AddListener(UseHealVial);
        manaVialButton.onClick.AddListener(UseManaVial);

        skillsButton.onClick.AddListener(EnableSkillButtons);
        backButtonSkill.onClick.AddListener(EnablePrimaryActionButtons);
        
        flaskButton.onClick.AddListener(EnableVialButtons);
        backButtonVials.onClick.AddListener(DisableVialButtons);

        UpdateActionText("What will you do?");

        // Initial button setup
        EnablePrimaryActionButtons();
        DisableVialButtons();
        UpdateVialCountersUI();

        StartBattle((CharacterType)System.Enum.Parse(typeof(CharacterType), playerType), (CharacterType)System.Enum.Parse(typeof(CharacterType), enemyType));

        healVialButton.gameObject.SetActive(false);
        manaVialButton.gameObject.SetActive(false);

        GameObject persistentCanvas = GameObject.Find("DialogueCanvas"); // Replace with your canvas name
        if (persistentCanvas != null)
        {
            leftTransitionPanel = persistentCanvas.transform.Find("LeftTransitionPanel").gameObject;
            rightTransitionPanel = persistentCanvas.transform.Find("RightTransitionPanel").gameObject;
        }
        StartCoroutine(PlayOpeningAnimation());
        
    }

    private void Update()
    {
        if (waitingForKeyPress && battleWon && Input.GetKeyDown(KeyCode.E))
        {
            string enemyID = PlayerPrefs.GetString("EnemyID");
            if (GlobalVariables.enemyDataReferences.ContainsKey(enemyID))
            {
                EnemyData enemyData = GlobalVariables.enemyDataReferences[enemyID];
                enemyData.Vitality = enemyData.VitalityValue;
            }

            if (currentEnemyScript != null && currentEnemyScript.skillSystem != null)
            {
                currentEnemyScript.skillSystem.ResetSkillUses();
            }
            GlobalVariables.enemyStatus[enemyID] = true;

            EndBattle(true);

            Animator playerAnimator = playerTransform.GetComponent<Animator>();
            playerAnimator.ResetTrigger("WarriorIdleRight");
            playerAnimator.ResetTrigger("Idle");
            playerAnimator.ResetTrigger("IdleSlash");
            playerAnimator.ResetTrigger("Slash");
            playerAnimator.ResetTrigger("Slashing");
            playerAnimator.ResetTrigger("HackingSlash");
            playerAnimator.ResetTrigger("DashForward");
            playerAnimator.ResetTrigger("DashBack");

            playerAnimator.SetTrigger("OriginalIdle");
            AudioManager.Instance.StopBattleMusic();
            SceneManager.LoadScene("SampleScene");
        }

        else if (waitingForKeyPress && !battleWon && Input.GetKeyDown(KeyCode.E))
        {
            string enemyID = PlayerPrefs.GetString("EnemyID");
            if (GlobalVariables.enemyDataReferences.ContainsKey(enemyID))
            {
                EnemyData enemyData = GlobalVariables.enemyDataReferences[enemyID];
                enemyData.Vitality = enemyData.VitalityValue;
            }
            if (currentEnemyScript != null && currentEnemyScript.skillSystem != null)
            {
                currentEnemyScript.skillSystem.ResetSkillUses();
            }
            Animator playerAnimator = playerTransform.GetComponent<Animator>();
            playerAnimator.ResetTrigger("WarriorIdleRight");
            playerAnimator.ResetTrigger("Idle");
            playerAnimator.ResetTrigger("IdleSlash");
            playerAnimator.ResetTrigger("Slash");
            playerAnimator.ResetTrigger("Slashing");
            playerAnimator.ResetTrigger("HackingSlash");
            playerAnimator.ResetTrigger("DashForward");
            playerAnimator.ResetTrigger("DashBack");

            playerAnimator.SetTrigger("OriginalIdle");
            AudioManager.Instance.StopBattleMusic();
            SceneManager.LoadScene("SampleScene");
        }
        
    }

    public void HandlePlayerDeath()
    {
        isBattleActive = false;
        DisablePrimaryActionUI();
        UpdateActionText("You have been defeated!");
        StartCoroutine(PlayerDeathSequence());
    }

    private IEnumerator PlayerDeathSequence()
    {
        // Wait for a moment to show the defeat message
        yield return new WaitForSeconds(2f);
        
        // Reset all battle animations
        Animator playerAnimator = playerTransform.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // Reset all battle-related animations
            playerAnimator.ResetTrigger("WarriorIdleRight");
            playerAnimator.ResetTrigger("Idle");
            playerAnimator.ResetTrigger("IdleSlash");
            playerAnimator.ResetTrigger("Slash");
            playerAnimator.ResetTrigger("Slashing");
            playerAnimator.ResetTrigger("HackingSlash");
            playerAnimator.ResetTrigger("DashForward");
            playerAnimator.ResetTrigger("DashBack");

            // Set back to original idle state
            playerAnimator.SetTrigger("OriginalIdle");
        }
        
        // Set the spawn point ID for Yggdrasil
        GlobalVariables.lastTeleportSpawnID = "outskirts_spawn";
        
        // Reset player stats
        if (playerStats != null)
        {
            playerStats.Vitality = playerStats.maxVitality;
            playerStats.Intelligence = playerStats.maxIntelligence;
        }

        // Re-enable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        var worldTime = globalLight.GetComponent<WorldTime.WorldTime>();
        worldTime.SetOverrideIntensity(true);  // Take control temporarily
        worldTime.SetLightIntensity(1f);       // Force to full intensity
        worldTime.SetOverrideIntensity(false); // Release control back to day/night cycle
        
        // Stop battle music and load Yggdrasil scene
        AudioManager.Instance.StopBattleMusic();
        PlayerMovement.isReturningFromBattle = true; // Add this flag to ensure proper state restoration
        SceneManager.LoadScene("YggdrasilSampleScene");
    }
    private IEnumerator PlayOpeningAnimation()
    {
        // Start with time paused
        Time.timeScale = 0f;

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
        float openDuration = 0.5f;  // Faster opening in battle
        
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

    private UnityEngine.Rendering.Universal.Light2D FindGlobalLight()
    {
        // Find all Light2D components in the scene including DontDestroyOnLoad
        var allLights = FindObjectsOfType<UnityEngine.Rendering.Universal.Light2D>(true);
        
        foreach (var light in allLights)
        {
            if (light.gameObject.name == "Global Light")
            {
                return light;
            }
        }
        
        Debug.LogWarning("Global Light not found!");
        return null;
    }

    // Change from private to public
    public void UpdateActionText(string message)
    {
        if (actionText != null)
        {
            actionText.text = message;
        }
    }

    private void UpdateVialCountersUI()
    {
        counterForHealBattle.text = GlobalVariables.currentHealVialCount.ToString();
        counterForManaBattle.text = GlobalVariables.currentManaVialCount.ToString();
    }
    // Inside the BattleManager class
    public void PlaySuccessfulHitSound()
    {
        if (successfulHitSounds.Length > 0)
        {
            AudioClip clip = successfulHitSounds[Random.Range(0, successfulHitSounds.Length)];
            if (clip != null) // Ensure the clip is not null
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("Selected successful hit sound is null.");
            }
        }
        else
        {
            Debug.LogWarning("No successful hit sounds available.");
        }
    }


    public void PlayUnsuccessfulHitSound()
    {
        if (unsuccessfulHitSound != null)
        {
            audioSource.PlayOneShot(unsuccessfulHitSound);
        }
        else
        {
            Debug.LogWarning("No unsuccessful hit sound available.");
        }
    }



    public void StartBattle(CharacterType playerType, CharacterType enemyType)
    {
       // GameObject player = null;
        // Use the CharacterType enum to instantiate the correct player prefab
        switch (playerType)
        {
            case CharacterType.Mage:
                player = Instantiate(magePlayerPrefab, new Vector2(-6, 0), Quaternion.identity);
                Debug.Log("Mage Instantiated");
                break;

            case CharacterType.Warrior:
                player = PlayerMovement.instance.gameObject;
                if (player != null)
                {
                    // Reposition the existing player for the battle
                    player.transform.position = new Vector2(-6, 0);
                    player.transform.rotation = Quaternion.identity;
                    playerTransform = player.transform;
                    originalPosition = playerTransform.position;
                    
                    // Ensure PlayerSkillManager is properly initialized
                    playerSkillManager = player.GetComponent<PlayerSkillManager>();
                    if (playerSkillManager == null)
                    {
                        playerSkillManager = player.AddComponent<PlayerSkillManager>();
                    }
                    
                    // Force reinitialize PlayerSkillManager
                    StartCoroutine(ReinitializePlayerComponents());
                }
                else
                {
                    Debug.LogError("PlayerMovement instance not found!");
                    return;
                }
                break;

            default:
                Debug.LogError("Invalid player type");
                break;
        }
        if (player != null)
        {
            playerTransform = player.transform;
            originalPosition = playerTransform.position;
        }
        playerMovement = FindObjectOfType<PlayerMovement>();
        playerSkillManager = FindObjectOfType<PlayerSkillManager>();
        if (playerMovement != null)
        {
            playerStats = playerMovement.playerStats; // Ensure playerStats is assigned
            if (playerStats == null)
            {
                Debug.LogError("PlayerStats is null after instantiation. Check PlayerMovement.");
            }
        }
        else
        {
            Debug.LogError("PlayerMovement component not found on the player object.");
        }

        // Retrieve enemyID from PlayerPrefs
        string uniqueID = PlayerPrefs.GetString("EnemyID");
        enemyType = (CharacterType)System.Enum.Parse(typeof(CharacterType), PlayerPrefs.GetString("EnemyType"));
        EnemyData enemyData = null;
        // Instantiate the correct enemy prefab based on enemy type
        switch (enemyType)
        {
            case CharacterType.LesserDemon:
                currentEnemy = Instantiate(lesserDemonPrefab, new Vector2(3, 0), Quaternion.identity);      
                break;
            case CharacterType.TestSubject:
                currentEnemy = Instantiate(testSubjectPrefab, new Vector2(3, 0), Quaternion.identity);
                break;
            default:
                Debug.LogError("Invalid enemy type");
                break;
        }

        currentEnemyScript = currentEnemy.GetComponent<Enemy>();
        currentEnemyScript.SetUniqueID(uniqueID);
        
        if (GlobalVariables.enemyDataReferences.ContainsKey(uniqueID))
        {
            currentEnemyScript.enemyData = GlobalVariables.enemyDataReferences[uniqueID];
            currentEnemyScript.enemyData.Vitality = currentEnemyScript.enemyData.VitalityValue;
        }

        healthManager.InitializeSliders(
            playerStats.maxVitality, 
            playerStats.Vitality,
            playerStats.maxIntelligence,
            playerStats.Intelligence,
            currentEnemyScript.enemyData.Vitality
        );

        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // Reset any existing animation states
            playerAnimator.ResetTrigger("OriginalIdle");
            playerAnimator.ResetTrigger("Idle");
            playerAnimator.ResetTrigger("IdleSlash");
            playerAnimator.ResetTrigger("Slash");
            playerAnimator.ResetTrigger("Slashing");
            playerAnimator.ResetTrigger("DashForward");
            playerAnimator.ResetTrigger("DashBack");
            
            // Set to battle idle state
            playerAnimator.SetTrigger("WarriorIdleRight");
        }


        Animator enemyAnimator = currentEnemy.GetComponent<Animator>();
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("IdleLeft");
        }

    }
    public void SetOpacity(float targetOpacity)
    {
        if (globalLight != null)
        {
            var worldTime = globalLight.GetComponent<WorldTime.WorldTime>();
            if (worldTime != null)
            {
                worldTime.SetOverrideIntensity(true);  // Take control of the intensity
                StartCoroutine(SmoothIntensityTransition(targetOpacity, worldTime));
            }
        }
    }

    public void RestoreNormalLighting()
    {
        if (globalLight != null)
        {
            var worldTime = globalLight.GetComponent<WorldTime.WorldTime>();
            if (worldTime != null)
            {
                // Get current day/night intensity before transition
                float targetIntensity = worldTime.IsNightTime() ? worldTime.nightIntensity : worldTime.dayIntensity;
                
                // Smooth transition to target intensity before releasing control
                StartCoroutine(SmoothRestoreTransition(targetIntensity, worldTime));
            }
        }
    }

    private IEnumerator SmoothIntensityTransition(float targetIntensity, WorldTime.WorldTime worldTime)
    {
        float startIntensity = globalLight.intensity;
        float elapsed = 0;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float currentIntensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            worldTime.SetLightIntensity(currentIntensity);
            yield return null;
        }

        worldTime.SetLightIntensity(targetIntensity);
    }

    private IEnumerator SmoothRestoreTransition(float targetIntensity, WorldTime.WorldTime worldTime)
    {
        float startIntensity = globalLight.intensity;
        float elapsed = 0;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            float currentIntensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            worldTime.SetLightIntensity(currentIntensity);
            yield return null;
        }

        worldTime.SetLightIntensity(targetIntensity);
        worldTime.SetOverrideIntensity(false);  // Release control back to day/night cycle
    }

    private IEnumerator ReinitializePlayerComponents()
    {
        // Wait one frame to ensure scene is fully loaded
        yield return null;
        
        if (playerSkillManager != null)
        {
            // Force reinitialize components
            playerSkillManager.Start();
        }
    }

    private EnemyData GetEnemyData(CharacterType enemyType)
    {
        switch (enemyType)
        {
            case CharacterType.LesserDemon:
            return Resources.Load<EnemyData>("ScriptableObjects/LesserDemon"); // Adjust path as necessary
            case CharacterType.TestSubject:
                return Resources.Load<EnemyData>("ScriptableObjects/TestSubject"); // Adjust path as necessary
            default:
                Debug.LogError("No EnemyData found for this enemy type.");
                return null;
        }
    }

    public IEnumerator NextTurn()
    {
        if (isPlayerTurn)
        {
            // Player's turn ends here, switch to enemy's turn
            isPlayerTurn = false;
            yield return StartCoroutine(EnemyTurn());
        }
        else
        {
            yield return new WaitForSeconds(2f);
            // Enemy's turn ends here, switch to player's turn
            isPlayerTurn = true;
            UpdateActionText("What will you do?");
        }

        // Update health UI after action
        healthManager.UpdatePlayerHealth(playerStats.Vitality, playerStats.maxVitality);
        healthManager.UpdateEnemyHealth(currentEnemyScript.enemyData.Vitality);
    }

    // Player attacks the enemy
    public void AttackEnemy()
    {
        // Check if it's player's turn and battle is active
        if (!isPlayerTurn || !isBattleActive)
        {
            Debug.Log("Not player's turn or battle is not active!");
            return;
        }

        if (currentEnemyScript == null || playerStats == null)
        {
            Debug.LogError("Current enemy or player stats are null.");
            return;
        }

        // Get player transform if we haven't already
        if (playerTransform == null)
        {
            playerTransform = FindObjectOfType<PlayerMovement>().transform;
            originalPosition = playerTransform.position;
        }

        UpdateActionText("You attack the enemy!"); 
        StartCoroutine(SlideAndAttack());
    }


    private IEnumerator SlideAndAttack()
    {
        Animator animator = playerTransform.GetComponent<Animator>();
        // Disable buttons at start of animation
        DisablePrimaryActionUI();
        isAnimating = true;
        yield return new WaitForSeconds(0.5f);
        animator.ResetTrigger("WarriorIdleRight");
        animator.SetTrigger("DashForward");

        // Slide to enemy
        Vector3 targetPosition = originalPosition + Vector3.right * attackSlideDistance;
        while (Vector3.Distance(playerTransform.position, targetPosition) > 0.1f)
        {
            playerTransform.position = Vector3.MoveTowards(playerTransform.position, targetPosition, slideSpeed * Time.deltaTime);
            yield return null;
        }

        animator.ResetTrigger("DashForward");
        // Perform attack
        animator.SetTrigger("Slashing");
        int damage = playerStats.CalculateDamage(playerStats.currentDamage, playerStats.Power, 1, currentEnemyScript.enemyData.Fortitude);
        currentEnemyScript.TakeDamage(damage);
        yield return new WaitForSeconds(0.36f);
        animator.ResetTrigger("Slashing");
        healthManager.UpdateEnemyHealth(currentEnemyScript.enemyData.Vitality);

        // Small pause after hitting
        animator.SetTrigger("DashBack");
        

        // Slide back
        while (Vector3.Distance(playerTransform.position, originalPosition) > 0.1f)
        {
            playerTransform.position = Vector3.MoveTowards(playerTransform.position, originalPosition, slideSpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure we're exactly at the original position
        playerTransform.position = originalPosition;
        isAnimating = false;

        // Check if enemy died
        if (currentEnemyScript.isDead)
        {
            EndBattle(true);
            yield break;
        }

        // End player's turn and start enemy's turn
        isPlayerTurn = false;
        animator.SetTrigger("WarriorIdleRight"); // Trigger the idle animation
        StartCoroutine(EnemyTurn());
    }

    public void UseSkillOnEnemy()
    {
        if (!isPlayerTurn || !isBattleActive) return;

        if (currentEnemy != null && playerStats != null)
        {
            GameObject playerObject = playerTransform.gameObject;
            Enemy enemyScript = currentEnemy.GetComponent<Enemy>();
            
            string skillName = playerSkillManager.GetCurrentSkillName();
            UpdateActionText($"You cast {skillName}!");
            
            enemyScript.HandleSkill(playerStats, playerObject);
            
            isPlayerTurn = false;
            StartCoroutine(EnemyTurn());
        }
    }

    public void UseSkill(string skillName, int mpCost, int damage)
    {
        if (!playerStats.HasEnoughIntelligence(mpCost))
        {
            UpdateActionText("Not enough MP!");
            return;
        }

        UpdateActionText($"You cast {skillName}!");
        playerStats.UseIntelligence(mpCost);
        DealDamage(damage);
    }

    

    public void DealDamage(int damage)
    {
        if (currentEnemyScript != null)
        {
            currentEnemyScript.TakeDamage(damage);
            healthManager.UpdateEnemyHealth(currentEnemyScript.enemyData.Vitality);

            if (currentEnemyScript.isDead)
            {
                StartCoroutine(DelayedBattleWon());
            }
        }
    }

    private IEnumerator DelayedBattleWon()
    {
        // Wait for animations to finish
        yield return new WaitForSeconds(1f);
        EndBattle(true);
    }


    private IEnumerator HandleRapidAssault()
    {
        if (!isPlayerTurn || playerSkillManager.IsPerformingSkill()) yield break;

        // Get the skill from the equipped weapon
        SkillSO rapidAssaultSkill = playerSkillManager.equippedWeapon.skills.Find(skill => skill.skillName == "Rapid Assault");
        if (rapidAssaultSkill == null)
        {
            Debug.LogError("Rapid Assault skill not found!");
            yield break;
        }

        // Check if player has enough MP
        if (playerStats.Intelligence < rapidAssaultSkill.mpCost)
        {
            UpdateActionText("Not enough MP for Rapid Assault!");
            yield break;
        }

        // Deduct MP cost
        playerStats.UseIntelligence(rapidAssaultSkill.mpCost);
        AudioManager.Instance.PlayRapidAssaultMusic();

        UpdateActionText("You cast Rapid Assault!");
        DisablePrimaryActionUI();
        SetButtonsInteractable(false);
        playerSkillManager.lightningBurst.gameObject.SetActive(true);
        playerSkillManager.lightningEffect.gameObject.SetActive(true);
        yield return StartCoroutine(BlinkEffect(0.05f, 1.2f));
        playerSkillManager.lightningBurst.gameObject.SetActive(false);
        playerSkillManager.lightningEffect.gameObject.SetActive(false);
        yield return StartCoroutine(playerSkillManager.PerformRapidAssault());
        // Enemy turn will be started by the skill manager when it's done
    }

    public IEnumerator BlinkEffect(float blinkInterval, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            playerSkillManager.SpeedIllusionCast(0);    // Make sprite invisible
            yield return new WaitForSeconds(blinkInterval);

            playerSkillManager.SpeedIllusionCast(500);  // Make sprite visible again
            yield return new WaitForSeconds(blinkInterval);

            elapsedTime += blinkInterval * 2; // Account for on/off phases
        }

        // Ensure sprite is visible after blinking ends
        playerSkillManager.SpeedIllusionCast(500);
    }


    public IEnumerator StartHackingSlash()
    {
        if (!isPlayerTurn || playerSkillManager.IsPerformingSkill()) yield break;
        SkillSO hackingSlashSkill = playerSkillManager.equippedWeapon.skills.Find(skill => skill.skillName == "Hacking Slash");
        if (playerStats.Intelligence < hackingSlashSkill.mpCost)
        {
            UpdateActionText("Not enough MP for Hacking Slash!");
            yield break;
        }

        // Deduct MP cost
        playerStats.UseIntelligence(hackingSlashSkill.mpCost);
        UpdateActionText("You cast Hacking Slash!");

        DisablePrimaryActionUI();
        timingSlider.gameObject.SetActive(true);
        blueBarImage.gameObject.SetActive(true);
        border.gameObject.SetActive(true);

        SetButtonsInteractable(false); // Disable other buttons
        StartCoroutine(playerSkillManager.PerformHackingSlash());
    }

    public void StartCrescentSlash()
    {
        if (!isPlayerTurn || playerSkillManager.IsPerformingSkill()) return;
        DisablePrimaryActionUI();
        UpdateActionText("You cast Crescent Slash!");
        crescentSlider.gameObject.SetActive(true);
        whiteBarImage.gameObject.SetActive(true);

        SetButtonsInteractable(false); // Disable other buttons
        StartCoroutine(playerSkillManager.PerformCrescentSlash());
    }

    public void EndPlayerTurn()
    {
        isPlayerTurn = false; // End player's turn
        StartCoroutine(EnemyTurn()); // Start enemy's turn
    }

    private void OnFlaskButtonPressed()
    {
        // Toggle visibility of the Heal Vial and Mana Vial buttons
        bool isActive = !healVialButton.gameObject.activeSelf;
        healVialButton.gameObject.SetActive(isActive);
        manaVialButton.gameObject.SetActive(isActive);
    }

    public void HealPlayer(float healPercentage)
    {
        if (!isPlayerTurn || !isBattleActive) return;
        DisablePrimaryActionUI();
        healPercentage = Mathf.Clamp(healPercentage, 0f, 100f);
        int healingAmount = Mathf.FloorToInt(playerStats.maxVitality * (healPercentage / 100f));
        playerStats.Vitality = Mathf.Min(playerStats.Vitality + healingAmount, playerStats.maxVitality);
        healthManager.UpdatePlayerHealth(playerStats.Vitality, playerStats.maxVitality);
        UpdateVialCountersUI();

        isPlayerTurn = false;
        StartCoroutine(EnemyTurn());
    }

    public void UseHealVial()
    {
        if (GlobalVariables.currentHealVialCount > 0)
        {
            GlobalVariables.currentHealVialCount--;
            UpdateActionText("You used a Heal Vial!");
            HealPlayer(50);
            VialController.Instance.UpdateCounters();
        }
        else
        {
            UpdateActionText("No Heal Vials left!");
        }

    }

    public void UseManaVial()
    {
        if (GlobalVariables.currentManaVialCount > 0)
        {
            GlobalVariables.currentManaVialCount--;
            UpdateActionText("You used a Mana Vial!");
            int manaAmount = Mathf.FloorToInt(playerStats.maxIntelligence * 0.3f); // Restores 30% of max MP
            RestorePlayerIntelligence(manaAmount);
            VialController.Instance.UpdateCounters();
            UpdateVialCountersUI();
            
            isPlayerTurn = false;
            StartCoroutine(EnemyTurn());
        }
        else
        {
            UpdateActionText("No Mana Vials left!");
        }
    }

    private void RestorePlayerIntelligence(int amount)
    {
        playerStats.RestoreIntelligence(amount);
    }

    public void FleeBattle()
    {
        if (!isPlayerTurn || !isBattleActive) return;

        // 25% chance of failure
        float fleeChance = Random.Range(0f, 1f);
        if (fleeChance > 0.25f) // Flee succeeded
        {
            EndBattle(false);
        }
        else // Flee failed
        {
            Debug.Log("Flee failed!");
            UpdateActionText("Flee failed!");
            isPlayerTurn = false;
            StartCoroutine(EnemyTurn()); // Enemy's turn after failed flee
        }
    }

    private IEnumerator EnemyTurn()
    {
        Animator enemyAnimator = currentEnemy.GetComponent<Animator>();
        SetButtonsInteractable(false);
        DisablePrimaryActionUI();

        // Check if the enemy is dead before proceeding
        if (currentEnemyScript != null && !currentEnemyScript.isDead)
        {
            currentEnemyScript.HandleAttack(playerStats);
            healthManager.UpdatePlayerHealth(playerStats.Vitality, playerStats.maxVitality);
        }

        // Wait until enemy animation is complete before enabling buttons
        while (currentEnemyScript != null && currentEnemyScript.IsAnimating())
        {
            yield return null;
        }

        // If the enemy is dead, do not proceed with the turn
        if (currentEnemyScript != null && currentEnemyScript.isDead)
        {
            yield break;
        }

        // After the enemy's turn, it's the player's turn again
        isPlayerTurn = true;
        yield return new WaitForSeconds(3f);
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("IdleLeft");
        }

        // Reset action text
        UpdateActionText("What will you do?");
        SetButtonsInteractable(true);
        EnablePrimaryActionButtons();
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (attackButton) attackButton.interactable = interactable;
        if (fleeButton) fleeButton.interactable = interactable;
        if (flaskButton) flaskButton.interactable = interactable;
    }
    public void EnableSkillButtons()
    {
        // Disable primary action buttons
        attackButton.gameObject.SetActive(false);
        skillsButton.gameObject.SetActive(false);
        flaskButton.gameObject.SetActive(false);
        fleeButton.gameObject.SetActive(false);

        // Enable specific skill buttons
        rapidAssaultButton.gameObject.SetActive(true);
        hackingSlashButton.gameObject.SetActive(true);
        crescentSlashButton.gameObject.SetActive(true);
        backButtonSkill.gameObject.SetActive(true);
    }

    public void EnablePrimaryActionButtons()
    {
        // Enable primary action buttons
        attackButton.gameObject.SetActive(true);
        skillsButton.gameObject.SetActive(true);
        flaskButton.gameObject.SetActive(true);
        fleeButton.gameObject.SetActive(true);
        playerTabUI.gameObject.SetActive(true);

        // Disable skill buttons
        rapidAssaultButton.gameObject.SetActive(false);
        hackingSlashButton.gameObject.SetActive(false);
        crescentSlashButton.gameObject.SetActive(false);
        backButtonSkill.gameObject.SetActive(false);
    }

    public void DisablePrimaryActionUI()
    {
        // Enable primary action buttons
        attackButton.gameObject.SetActive(false);
        skillsButton.gameObject.SetActive(false);
        flaskButton.gameObject.SetActive(false);
        fleeButton.gameObject.SetActive(false);
        playerTabUI.gameObject.SetActive(false);

        healVialButton.gameObject.SetActive(false);
        manaVialButton.gameObject.SetActive(false);
        backButtonVials.gameObject.SetActive(false);
        vialsTabUI.gameObject.SetActive(false); 
        counterForHealBattle.gameObject.SetActive(false);
        counterForManaBattle.gameObject.SetActive(false);

        rapidAssaultButton.gameObject.SetActive(false);
        hackingSlashButton.gameObject.SetActive(false);
        crescentSlashButton.gameObject.SetActive(false);
        backButtonSkill.gameObject.SetActive(false);

    }

    public void EnableVialButtons()
    {
        // Enable vials and associated UI
        healVialButton.gameObject.SetActive(true);
        manaVialButton.gameObject.SetActive(true);
        backButtonVials.gameObject.SetActive(true);
        vialsTabUI.gameObject.SetActive(true); 
        counterForHealBattle.gameObject.SetActive(true);
        counterForManaBattle.gameObject.SetActive(true);   

        attackButton.gameObject.SetActive(false);
        skillsButton.gameObject.SetActive(false);
        flaskButton.gameObject.SetActive(false);
        fleeButton.gameObject.SetActive(false);
        playerTabUI.gameObject.SetActive(false);
        // add the counters for current heal and current mana potion in the vial controller TMP.
    }
    public void DisableVialButtons()
    {
        // Disable vials and associated UI
        healVialButton.gameObject.SetActive(false);
        manaVialButton.gameObject.SetActive(false);
        backButtonVials.gameObject.SetActive(false);
        vialsTabUI.gameObject.SetActive(false);
        counterForHealBattle.gameObject.SetActive(false);
        counterForManaBattle.gameObject.SetActive(false);

        attackButton.gameObject.SetActive(true);
        skillsButton.gameObject.SetActive(true);
        flaskButton.gameObject.SetActive(true);
        fleeButton.gameObject.SetActive(true);
        playerTabUI.gameObject.SetActive(true);
    }

    public void TriggerBattleWon()
    {
        Animator playerAnimator = playerTransform.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("WarriorIdleRight");
        }
        battleWon = true;
        waitingForKeyPress = true;
        UpdateActionText($"You have defeated {enemyData.enemyName}. Press 'E' to continue.");
        // Optionally, disable further player inputs until battle is concluded
        DisablePrimaryActionUI();

        // Update GlobalVariables to mark enemy as defeated
        string enemyID = currentEnemyScript.uniqueID;
        if (GlobalVariables.enemyStatus.ContainsKey(enemyID))
        {
            GlobalVariables.enemyStatus[enemyID] = true;
        }
        else
        {
            GlobalVariables.enemyStatus.Add(enemyID, true);
        }
    }
    
    public void EndBattle(bool playerWon)
    {
        Debug.Log("END BATTLE - Setting return flag and loading Overworld");
        isBattleActive = false;
        battleWon = playerWon;
        waitingForKeyPress = true;
        PlayerMovement.isReturningFromBattle = true;
        Debug.Log($"END BATTLE - isReturningFromBattle set to: {PlayerMovement.isReturningFromBattle}");
        SetOpacity(1f);
        playerSkillManager.lightningEffect.gameObject.SetActive(false);
        playerSkillManager.lightningBurst.gameObject.SetActive(false);
        playerSkillManager.lightningDash.gameObject.SetActive(false);
        
        // Ensure player goes to original position and animation is reset or back to idle
        if (currentEnemyScript != null)
        {
            Animator enemyAnimator = currentEnemyScript.GetComponent<Animator>();
            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Idle");
            }
        }
        QuestManager.Instance.CheckEnemyKill(currentEnemyScript);

        Animator playerAnimator = playerTransform.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // Reset all battle-related animations
            playerAnimator.ResetTrigger("WarriorIdleRight");
            playerAnimator.ResetTrigger("Idle");
            playerAnimator.ResetTrigger("IdleSlash");
            playerAnimator.ResetTrigger("Slash");
            playerAnimator.ResetTrigger("Slashing");
            playerAnimator.ResetTrigger("HackingSlash");
            playerAnimator.ResetTrigger("DashForward");
            playerAnimator.ResetTrigger("DashBack");

            // Set back to original idle state
        }
        // Add death animation for enemy
        // Add a feature where when enemy dies, prompt "enemyData.name has been defeated! [E]" and then continue to You have won!

        // Add a feature where when enemy dies, player gets experience
        string enemyID = currentEnemyScript.uniqueID;
        if (playerWon)
        {

            // Reward Fate Crystals
            PlayerStatsHolder playerStatsHolder = FindObjectOfType<PlayerStatsHolder>();
            if (playerStatsHolder != null && playerStatsHolder.playerStats != null)
            {
                playerStatsHolder.playerStats.AddFateCrystals(currentEnemyScript.fateCrystalsReward);
            }
            else
            {
                Debug.LogError("PlayerStatsHolder or PlayerStats is missing. Cannot reward Fate Crystals.");
            }

            UpdateActionText($"You have defeated {currentEnemyScript.enemyData.enemyName}. Press 'E' to continue.");
            DisablePrimaryActionUI();

            // Update GlobalVariables to mark enemy as defeated
            if (GlobalVariables.enemyStatus.ContainsKey(enemyID))
            {
                GlobalVariables.enemyStatus[enemyID] = true;
            }
            else
            {
                GlobalVariables.enemyStatus.Add(enemyID, true);
            }

            // Optional: Award experience points or other rewards here
        }
        else
        {
            UpdateActionText("You fled from the battle! Press 'E' to continue.");
            DisablePrimaryActionUI();

            // Update GlobalVariables to mark enemy as not defeated
            if (GlobalVariables.enemyStatus.ContainsKey(enemyID))
            {
                GlobalVariables.enemyStatus[enemyID] = false;
            }
            else
            {
                GlobalVariables.enemyStatus.Add(enemyID, false);
            }
        }


        // Remove Scene Load from here
        // SceneManager.LoadScene("SampleScene");
    }

}