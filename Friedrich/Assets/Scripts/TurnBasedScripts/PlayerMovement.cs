    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class PlayerMovement : MonoBehaviour
    {
        public static PlayerMovement instance;
        public float speed = 5f;
        public CharacterType playerType; 
        public string uniqueID; // Unique player ID
        public PlayerStats playerStats; // Reference to PlayerStats ScriptableObject

        public Rigidbody2D rb;
        public Animator animator;

        Vector2 movement;
        Vector2 lastMovementDirection = Vector2.down; // Default facing down
        bool isMoving = false;
        
        private InventoryController inventoryController;
        public GameObject inventoryObject;
        public float sprintSpeed = 8f; // Adjust this value as needed
        public static bool isReturningFromBattle = false;
        public PlayerPositionData positionData;

        public static bool isInTransitionToBattle = false;
        [Header("Battle Transition")]
        public GameObject leftTransitionPanel;
        public GameObject rightTransitionPanel;
        public float transitionSpeed = 10f;
        private Vector3 battlePosition = new Vector3(-6f, 0f, 0f);

        private bool hasSetBattlePosition = false;
        

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true; 

        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);

        if (inventoryObject != null)
        {
            inventoryObject.SetActive(false);
        }

        bool hasStaticPosition = GlobalVariables.hasStoredPosition;
        bool hasScriptablePosition = positionData != null && positionData.hasStoredPosition;
        
        Debug.Log($"Static position available: {hasStaticPosition}");
        Debug.Log($"ScriptableObject position available: {hasScriptablePosition}");

        // Only restore position if we're returning from battle
        if (isReturningFromBattle && SceneManager.GetActiveScene().name != "BattleScene")
        {
            Debug.Log($"RESTORE - Current Position Before Restore: {transform.position}");
            Debug.Log($"RESTORE - ScriptableObject Stored Position: {positionData.lastOverworldPosition}");
            
            if (positionData != null && positionData.hasStoredPosition)
            {
                Vector3 storedPos = positionData.lastOverworldPosition;
                Debug.Log($"RESTORE - About to set position to: {storedPos}");
                transform.position = storedPos;
                Debug.Log($"RESTORE - Position after setting: {transform.position}");
                positionData.ClearPosition();
            }
            isReturningFromBattle = false;
        }
    }

    
    void Awake()
    {
        // Check if a unique ID has been set in GlobalVariables; if not, assign a default value or handle it as needed
        if (GlobalVariables.playerData.ContainsKey("PlayerID"))
        {
            uniqueID = GlobalVariables.playerData["PlayerID"];
        }
        else
        {
            // Determine the default unique ID based on the player prefab type
            if (playerType == CharacterType.Mage)
            {
                uniqueID = "110011"; // Default ID for Mage
                Debug.Log("Player ID not found in GlobalVariables. Setting the unique ID '110011' to Mage Prefab.");
            }
            else if (playerType == CharacterType.Warrior)
            {
                uniqueID = "001100"; // Default ID for Warrior
                Debug.Log("Player ID not found in GlobalVariables. Setting the unique ID '001100'to Warrior Prefab.");
            }
            else
            {
                uniqueID = "Forbidden"; // Fallback default ID if no player prefab
                Debug.LogWarning("No player found. Setting a fallback default unique ID.");
            }

            // Store the unique ID in GlobalVariables
            GlobalVariables.playerData["PlayerID"] = uniqueID;
            
        }

        // Store in GlobalVariables to access across scenes without regenerating
        GlobalVariables.playerData["PlayerID"] = uniqueID;

        if (inventoryObject != null)
        {
            inventoryObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Inventory Object not assigned in the Inspector!");
        }

        // Load player stats based on player type
        if (playerType == CharacterType.Mage)
        {
            playerStats = Resources.Load<PlayerStats>("ScriptableObjects/MageStats");
        }
        else if (playerType == CharacterType.Warrior)
        {
            playerStats = Resources.Load<PlayerStats>("ScriptableObjects/WarriorStats");
        }

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats is null. Check the path and ensure it is in the 'Resources' folder.");
        }
    }

    void Update()
    {
        if (!ShouldDisableMovement())
        {
            hasSetBattlePosition = false;
            // Get input
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            // Normalize diagonal movement
            if (movement.magnitude > 1f)
                movement.Normalize();

            // Check if moving
            isMoving = movement.sqrMagnitude > 0;

            // Determine current speed based on sprint input
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;

            // Update last movement direction if moving
            if (isMoving)
            {
                lastMovementDirection = movement.normalized;
                // Set walking animation parameters
                animator.SetFloat("Horizontal", movement.x);
                animator.SetFloat("Vertical", movement.y);
                
                // Adjust animation speed when sprinting
                animator.speed = Input.GetKey(KeyCode.LeftShift) ? 1.8f : 1f;
            }
            else
            {
                animator.speed = 1f;
            }

            // Set idle animation parameters based on last movement
            animator.SetFloat("IdleHorizontal", lastMovementDirection.x);
            animator.SetFloat("IdleVertical", lastMovementDirection.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);

            // Remove the Transform.Translate line since we're using Rigidbody2D for movement

            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }
        }
        else
        {
            // In battle scene or transition, reset all movement
            movement = Vector2.zero;
            rb.velocity = Vector2.zero;
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
            animator.SetFloat("Speed", 0);
            animator.speed = 1f;

            if (IsInBattleScene() && !hasSetBattlePosition)
            {
                transform.position = battlePosition;
                hasSetBattlePosition = true;
            }
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;
        Vector2 targetVelocity = movement * currentSpeed;
        
        // Smoothly interpolate between current velocity and target velocity
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, 0.7f);
    }

    public void SavePositionForBattle()
    {
        if (SceneManager.GetActiveScene().name != "BattleScene")
        {
            // Save to both static and ScriptableObject
            GlobalVariables.lastOverworldPosition = transform.position;
            GlobalVariables.hasStoredPosition = true;
            
            if (positionData != null)
            {
                positionData.SavePosition(transform.position);
            }
            
            Debug.Log($"Position saved to both storage methods: {transform.position}");
        }
    }


    private void ToggleInventory()
    {
        if (inventoryObject != null)
        {
            bool currentState = inventoryObject.activeSelf;
            inventoryObject.SetActive(!currentState);
            
            // Toggle VialOverworldManager UI visibility
            var vialManager = FindObjectOfType<VialOverworldManager>();
            if (vialManager != null)
            {
                vialManager.SetUIVisibility(currentState); // inverse of inventory state
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemyScript = other.GetComponent<Enemy>();
            string enemyID = enemyScript.uniqueID;
            AudioManager.Instance.StartBattleMusic();

            // Check if this enemy was already defeated or fled
    //        if (GlobalVariables.enemyStatus.ContainsKey(enemyID) && GlobalVariables.enemyStatus[enemyID])
  //          {
    //            Debug.Log("Enemy already defeated or fled, ignoring.");
   //             return;
   //         }

            BattleTransition(other.gameObject, enemyID);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Instead of immediately zeroing velocity, apply a small bounce-back effect
        Vector2 normal = collision.contacts[0].normal;
        rb.velocity = Vector2.Reflect(rb.velocity, normal) * 0.2f; // Reduce bounce intensity with 0.2f
    }

    private void BattleTransition(GameObject enemy, string enemyID)
    {
        isInTransitionToBattle = true;
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        SavePositionForBattle();
        
        if (enemyScript.enemyData != null)
        {
            StartCoroutine(PlayTransitionAnimation(() => {
                CharacterType enemyType = enemyScript.enemyData.characterType;
                
                PlayerPrefs.SetString("PlayerType", playerType.ToString());
                PlayerPrefs.SetString("EnemyID", enemyID);
                PlayerPrefs.SetString("EnemyType", enemyType.ToString());

                GlobalVariables.enemyDataReferences[enemyID] = enemyScript.enemyData;
                if (enemyScript.skillSystem != null)
                {
                    GlobalVariables.enemySkillReferences[enemyID] = enemyScript.skillSystem;
                }

                SceneManager.LoadScene("BattleScene");
                transform.position = battlePosition;
            }));
        }
        else
        {
            Debug.LogError("Enemy data is null during battle transition!");
            isInTransitionToBattle = false;
        }
    }

    private IEnumerator PlayTransitionAnimation(System.Action onComplete)
    {
        // Pause the game at start of transition
        Time.timeScale = 0f;
        isInTransitionToBattle = true;
        
        // Activate panels
        leftTransitionPanel.SetActive(true);
        rightTransitionPanel.SetActive(true);
        
        // Get panel start positions
        RectTransform leftRect = leftTransitionPanel.GetComponent<RectTransform>();
        RectTransform rightRect = rightTransitionPanel.GetComponent<RectTransform>();
        
        // Set initial positions (at screen edges)
        leftRect.anchoredPosition = new Vector2(-Screen.width, 0);
        rightRect.anchoredPosition = new Vector2(Screen.width, 0);
        
        // Use unscaledTime for animation since game is paused
        float elapsedTime = 0f;
        float closeDuration = 1f;
        
        while (elapsedTime < closeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / closeDuration;
            float smoothProgress = progress * progress * (3f - 2f * progress);
            
            // Move panels from edges to center
            leftRect.anchoredPosition = Vector2.Lerp(new Vector2(-Screen.width, 0), Vector2.zero, smoothProgress);
            rightRect.anchoredPosition = Vector2.Lerp(new Vector2(Screen.width, 0), Vector2.zero, smoothProgress);
            yield return null;
        }

        // Pause while closed using unscaled time
        float pauseTime = 1f;
        yield return new WaitForSecondsRealtime(pauseTime);

        // Load battle scene while panels are closed
        onComplete?.Invoke();
        
        // Resume time - battle scene will handle its own timing
        Time.timeScale = 1f;
    }

    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene Loaded: {scene.name}, IsReturningFromBattle: {isReturningFromBattle}");
        Debug.Log($"Current Position at Scene Load: {transform.position}");
        
        if (scene.name != "BattleScene")
        {
            // Always ensure movement is enabled outside battle
            enabled = true;
            Time.timeScale = 1f;
            isInTransitionToBattle = false;

            // Reset to original idle animation
            if (animator != null)
            {
                animator.ResetTrigger("WarriorIdleRight");
                animator.ResetTrigger("Idle");
                animator.ResetTrigger("IdleSlash");
                animator.ResetTrigger("Slash");
                animator.ResetTrigger("Slashing");
                animator.ResetTrigger("HackingSlash");
                animator.ResetTrigger("DashForward");
                animator.ResetTrigger("DashBack");
                animator.SetTrigger("OriginalIdle");
            }

            // Handle position restoration if returning from battle
            if (isReturningFromBattle)
            {
                if (leftTransitionPanel != null && rightTransitionPanel != null)
                {
                    RectTransform leftRect = leftTransitionPanel.GetComponent<RectTransform>();
                    RectTransform rightRect = rightTransitionPanel.GetComponent<RectTransform>();
                    
                    leftRect.anchoredPosition = new Vector2(-Screen.width, 0);
                    rightRect.anchoredPosition = new Vector2(Screen.width, 0);
                    leftTransitionPanel.SetActive(false);
                    rightTransitionPanel.SetActive(false);
                }

                if (positionData != null && positionData.hasStoredPosition)
                {
                    Vector3 storedPos = positionData.lastOverworldPosition;
                    transform.position = storedPos;
                    positionData.ClearPosition();
                }
                isReturningFromBattle = false;
            }
        }
    }

    private bool IsInBattleScene()
    {
        return SceneManager.GetActiveScene().name == "BattleScene";
    }

    private bool ShouldDisableMovement()
    {
        return IsInBattleScene() || isInTransitionToBattle;
    }
}