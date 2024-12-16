using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;
using UnityEngine.SceneManagement;


public class Enemy : MonoBehaviour
{
    private CinemachineImpulseSource impulseSource;
    public string enemyDataPath;
    public EnemyData enemyData; // Reference to the EnemyData ScriptableObject
    public bool isDead = false; // boolean tracker if the enemy is dead
    public string uniqueID; // unique ID for enemies.
    public EnemySkillSystem skillSystem;
    private float healThresholdValue;

    private const float HEAL_THRESHOLD_PERCENT = 0.25f; // When to heal (25% health)
    private const float MIN_HEAL_PERCENT = 0.30f;      // Minimum heal amount (30%)
    private const float MAX_HEAL_PERCENT = 0.40f;      // Maximum heal amount (40%)
    private int healCount = 0;
    public int MAX_HEAL_USES = 2;

    private float attackSlideDistance = 6.5f;
    private float slideSpeed = 10f;
    private Vector3 originalPosition;
    private Transform enemyTransform;
    private bool isAnimating = false;

    public List<Transform> patrolPoints; // Assign in Inspector
    private int currentPatrolIndex = 0;
    public float patrolSpeed = 2f;
    private bool isPatrolling = true;

    public List<EnemySkill> availableSkills = new List<EnemySkill>();
    private int skillUseCount = 0;
    public int maxSkillUses = 2; // Configurable max uses   

    public int fateCrystalsReward = 50;

    public bool IsAnimating()
    {
        return isAnimating;
    }

    void Awake()
    {
        // Load enemy data from Resources folder
        if (string.IsNullOrEmpty(enemyDataPath))
        {
            Debug.LogError("Enemy data path not set!");
            return;
        }

        enemyData = Resources.Load<EnemyData>(enemyDataPath);
        if (enemyData == null)
        {
            Debug.LogError($"Failed to load enemy data from path: {enemyDataPath}");
            return;
        }

        // Load skill system
        string skillSystemPath = $"ScriptableObjects/{enemyData.characterType}Skills";
        skillSystem = Resources.Load<EnemySkillSystem>(skillSystemPath);
    }
    void Start()
    {
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            StartCoroutine(Patrol());
        }
        impulseSource = GetComponent<CinemachineImpulseSource>();
        healThresholdValue = enemyData.VitalityValue * skillSystem.healThreshold;
        // Initialize transform and position
        enemyTransform = transform;
        originalPosition = transform.position;
        if (enemyData == null)
        {
            Debug.LogError("Enemy Data is null! Make sure it's assigned.");
            return;
        }

        if (skillSystem == null)
        {
            Debug.LogError("Skill System is null! Make sure it's assigned.");
            return;
        }

        // Check the global status to see if this enemy is already defeated or fled
        if (GlobalVariables.enemyStatus.ContainsKey(uniqueID))
        {
            isDead = GlobalVariables.enemyStatus[uniqueID];
            if (isDead)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            // If this is a new enemy, initialize its status as alive
            isDead = false;
            GlobalVariables.enemyStatus[uniqueID] = false;
        }
    }
    IEnumerator Patrol()
    {
        while (isPatrolling && !isDead)
        {
            Transform targetPatrolPoint = patrolPoints[currentPatrolIndex];
            while (Vector3.Distance(transform.position, targetPatrolPoint.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPatrolPoint.position, patrolSpeed * Time.deltaTime);
                yield return null;
            }

            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            yield return new WaitForSeconds(1f); // Pause at each patrol point
        }
    }

    public void SetUniqueID(string id)
    {
        uniqueID = id; // Set the unique ID
    }
    
    public void HandleAttack(PlayerStats target)
    {
        if (target == null)
        {
            Debug.LogError("Target is null in HandleAttack!");
            return;
        }

        if (enemyTransform == null || !gameObject.activeInHierarchy)
        {
            Debug.LogError($"{enemyData.enemyName} transform is null or game object is inactive!");
            return;
        }

        // Update action text to indicate the enemy is preparing to act
        BattleManager.Instance.UpdateActionText($"{enemyData.enemyName} is preparing to act...");

        // Check if the enemy should heal
        float currentHealthPercent = (float)enemyData.Vitality / enemyData.VitalityValue;
        if (currentHealthPercent <= HEAL_THRESHOLD_PERCENT && healCount < MAX_HEAL_USES)
        {
            float healPercent = Random.Range(MIN_HEAL_PERCENT, MAX_HEAL_PERCENT);
            int healAmount = Mathf.RoundToInt(enemyData.VitalityValue * healPercent);
            StartCoroutine(Heal(healAmount));
            return;
        }

        // Determine whether to attack or use a skill based on a 40% chance
        float skillDecisionChance = 0.6f; // 60% chance to use a skill
        bool shouldUseSkill = Random.value < skillDecisionChance;

        if (shouldUseSkill && skillSystem != null && skillSystem.availableSkills.Count > 0)
        {
            var usableSkills = skillSystem.availableSkills
                .Where(s => s.isUnlocked && s.currentUses < s.maxUses)
                .ToList();

            if (usableSkills.Count > 0)
            {
                EnemySkill selectedSkill = usableSkills[Random.Range(0, usableSkills.Count)];
                BattleManager.Instance.UpdateActionText($"{enemyData.enemyName} prepares to use {selectedSkill.skillName}!");
                StartCoroutine(UseSkill(selectedSkill, target));
                return;
            }
            else
            {
                // No usable skills available, default to attack
                BattleManager.Instance.UpdateActionText($"{enemyData.enemyName} wants to use a skill but none are available. Defaulting to attack.");
            }
        }

        // Default to a normal attack
        BattleManager.Instance.UpdateActionText($"{enemyData.enemyName} attacks the player!");
        StartCoroutine(SlideAndAttack(target));
    }


    public IEnumerator HandleSkill(PlayerStats playerStats, GameObject playerObject)
    {
        BattleManager.Instance.UpdateActionText($"{enemyData.enemyName} casts a skill!");
        if (skillUseCount >= maxSkillUses)
        {
            BattleManager.Instance.UpdateActionText($"{enemyData.enemyName} ran out of skill uses!");
            HandleAttack(playerStats);
            yield return null;
        }
        var usableSkills = availableSkills.FindAll(s => s.isUnlocked);
        BattleManager.Instance.SetOpacity(0.04f);
        yield return new WaitForSeconds(0.5f);

        if (usableSkills.Count > 0)
        {
            EnemySkill selectedSkill = usableSkills[Random.Range(0, usableSkills.Count)];
            
            BattleManager.Instance.UpdateActionText($"{enemyData.enemyName} casts {selectedSkill.skillName}!");
            
            if (selectedSkill.skillPrefab != null)
            {
                // Apply position offsets
                Vector3 spawnPosition = enemyTransform.position + new Vector3(selectedSkill.xOffset, selectedSkill.yOffset, 0f);
                Instantiate(selectedSkill.skillPrefab, spawnPosition, Quaternion.identity);
                CameraShake.instance.TriggerCameraShake(impulseSource);
                CameraShake.instance.TriggerCameraShake(impulseSource);
                CameraShake.instance.TriggerCameraShake(impulseSource);
                CameraShake.instance.TriggerCameraShake(impulseSource);
                CameraShake.instance.TriggerCameraShake(impulseSource);
                CameraShake.instance.TriggerCameraShake(impulseSource);
                yield return new WaitForSeconds(0.5f);
            }

            playerStats.TakeDamage(selectedSkill.damage);
            skillUseCount++;
        }
        else
        {
            HandleAttack(playerStats);
        }
        BattleManager.Instance.SetOpacity(1f);
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator SlideAndAttack(PlayerStats playerStats)
    {
        yield return new WaitForSeconds(0.3f);
        isAnimating = true;

        // Slide to player
        Vector3 targetPosition = originalPosition + Vector3.left * attackSlideDistance;
        while (Vector3.Distance(enemyTransform.position, targetPosition) > 0.1f)
        {
            enemyTransform.position = Vector3.MoveTowards(enemyTransform.position, targetPosition, slideSpeed * Time.deltaTime);
            yield return null;
        }

        // Perform attack
        int damage = Mathf.Max(
            enemyData.baseDamage + Mathf.RoundToInt(enemyData.Power * enemyData.PowerMultiplier) - playerStats.Fortitude,
            Mathf.RoundToInt(0.1f * enemyData.baseDamage)   
        );

        Debug.Log($"{enemyData.enemyName} attacked the player. Damage dealt: {damage}");
        playerStats.TakeDamage(damage);
        if (playerStats.Vitality <= 0)
        {
            StartCoroutine(HandlePlayerDeath(playerStats));
            yield break;
        }

        // Update player health slider after attack
        FindObjectOfType<HealthManager>()?.UpdatePlayerHealth(playerStats.Vitality, playerStats.maxVitality);

        // Small pause after hitting
        yield return new WaitForSeconds(0.4f);

        // Slide back
        while (Vector3.Distance(enemyTransform.position, originalPosition) > 0.1f)
        {
            enemyTransform.position = Vector3.MoveTowards(enemyTransform.position, originalPosition, slideSpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure we're exactly at the original position
        enemyTransform.position = originalPosition;
        isAnimating = false;
        yield return new WaitForSeconds(0.5f);
        // **Removed NextTurn Call**
        // BattleManager.Instance.NextTurn();
    }

    public void TakeDamage(int damage)
    {
        enemyData.Vitality -= damage;
        if (enemyData.Vitality < 0) enemyData.Vitality = 0;

        // Update enemy health slider after taking damage
        FindObjectOfType<HealthManager>()?.UpdateEnemyHealth(enemyData.Vitality);
        Debug.Log($"{enemyData.enemyName} took {damage} damage. Current Vitality: {enemyData.Vitality}");

        if (enemyData.Vitality <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(HandleDeath());
        }
    }

    public void DecideAction(PlayerStats playerTarget)
    {
        if (isDead)
        {
            // Enemy is dead; no further actions
            return;
        }
        // Check if should heal first
        if (skillSystem.canHeal && !skillSystem.hasUsedHeal && 
            enemyData.Vitality <= (enemyData.VitalityValue * skillSystem.healThreshold))
        {
            int healAmount = Mathf.RoundToInt(enemyData.VitalityValue * (skillSystem.healPercentage / 100f));
            StartCoroutine(Heal(healAmount));
            skillSystem.hasUsedHeal = true;
            return;
        }

        // Decide between normal attack and skill
        float decision = Random.Range(0.0f, 1.0f);
        
        // Increase skill usage chance to 70%
        if (decision < 0.35f)
        {
            // Get available skills (not maxed out uses and unlocked)
            var usableSkills = skillSystem.availableSkills
                .Where(skill => skill.isUnlocked && skill.currentUses < skill.maxUses)
                .ToList();

            if (usableSkills.Count > 0)
            {
                EnemySkill selectedSkill = usableSkills[Random.Range(0, usableSkills.Count)];
                StartCoroutine(UseSkill(selectedSkill, playerTarget));
                return;
            }
        }

        // Default to normal attack
        HandleAttack(playerTarget);
    }

    private IEnumerator UseSkill(EnemySkill skill, PlayerStats target)
    {
        BattleManager.Instance.DisablePrimaryActionUI();
        BattleManager.Instance.UpdateActionText($"{enemyData.enemyName} casts {skill.skillName}!");
        yield return new WaitForSeconds(1.5f);
        Animator enemyAnimator = GetComponent<Animator>();
        
        if (skillUseCount >= maxSkillUses)
        {
            BattleManager.Instance.UpdateActionText($"{enemyData.enemyName} ran out of skill uses!");
            HandleAttack(target);
            yield break;
        }
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("CastSkill");
        }
        BattleManager.Instance.SetOpacity(0.04f);
        yield return new WaitForSeconds(1f);

        GameObject instantiatedSkill = null;
        if (skill.skillPrefab != null)
        {
            
            // Apply position offsets
            Vector3 spawnPosition = enemyTransform.position + new Vector3(skill.xOffset, skill.yOffset, 0f);
            instantiatedSkill = Instantiate(skill.skillPrefab, spawnPosition, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(ShakeDuringSkill(0.02f)); // 1.5 seconds of shake
        }
        int finalDamage = skill.damage;

        // Apply Damage Mitigation based on skill category
        if (skill.category == SkillCategory.Physical)
        {
            finalDamage = target.MitigatePhysicalDamage(skill.damage);
            Debug.Log($"Physical Skill Damage after Mitigation: {finalDamage}");
        }
        else if (skill.category == SkillCategory.Magical)
        {
            finalDamage = target.MitigateMagicalDamage(skill.damage);
            Debug.Log($"Magical Skill Damage after Mitigation: {finalDamage}");
        }

        target.TakeDamage(finalDamage);
        if (target.Vitality <= 0)
        {
            StartCoroutine(HandlePlayerDeath(target));
            yield break;
        }
        skill.currentUses++;
        skillUseCount++;

        yield return new WaitForSeconds(1f);
        if (instantiatedSkill != null)
        {
            Destroy(instantiatedSkill);
        }

        BattleManager.Instance.SetOpacity(1f);
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator ShakeDuringSkill(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            CameraShake.instance.TriggerCameraShake(impulseSource);
            yield return new WaitForSeconds(0.05f); // Shake frequency
            elapsedTime += 0.05f;
        }
    }

    private string[] healMessages = new string[]
    {
        "{0} uses Heal for {1} HP!",
        "{0} is CHEATING!!!",
        "Bro is NOT A SOULS BOSS!! {0} healed for {1} HP!!",
        "Oh no! {0} healed for {1} HP!",
        "Healed for {1} HP!",
        "{0} is CHEATING! Why {0} healing {1} HP!!",
        "{0} healed for {1} HP! Something light!"
    };

    private IEnumerator Heal(int amount)
    {
        healCount++;
        int oldHealth = enemyData.Vitality;
        enemyData.Vitality = Mathf.Min(enemyData.VitalityValue, enemyData.Vitality + amount);
        int actualHealAmount = enemyData.Vitality - oldHealth;
        // Update the enemy's health UI
        FindObjectOfType<HealthManager>()?.UpdateEnemyHealth(enemyData.Vitality);
        string selectedMessage = healMessages[Random.Range(0, healMessages.Length)];
        string formattedMessage = string.Format(selectedMessage, enemyData.enemyName, actualHealAmount);        
        BattleManager.Instance.UpdateActionText(formattedMessage);
        yield return new WaitForSeconds(1f);  
    }

       public IEnumerator HandleDeath()
       {
           // Trigger death animation
           Animator animator = GetComponent<Animator>();
           if (animator != null)
           {
               animator.SetTrigger("Death");
           }
           PlayerStatsHolder playerStatsHolder = FindObjectOfType<PlayerStatsHolder>();
            if (playerStatsHolder != null && playerStatsHolder.playerStats != null)
            {
                playerStatsHolder.playerStats.AddFateCrystals(fateCrystalsReward);
            }
            else
            {
                Debug.LogError("PlayerStatsHolder or PlayerStats is missing. Cannot reward Fate Crystals.");
            }

           BattleManager.Instance.isBattleActive = false;
           yield return new WaitForSeconds(0.7f);
           BattleManager.Instance.TriggerBattleWon();

           // Update GlobalVariables to mark enemy as defeated
           if (GlobalVariables.enemyStatus.ContainsKey(uniqueID))
           {
               GlobalVariables.enemyStatus[uniqueID] = true;
           }
           else
           {
               GlobalVariables.enemyStatus.Add(uniqueID, true);
           }

           // Disable the enemy GameObject
           isDead = true;
           gameObject.SetActive(false);
       }

       private IEnumerator HandlePlayerDeath(PlayerStats playerStats)
    {
        // Disable battle UI and actions
        BattleManager.Instance.isBattleActive = false;
        BattleManager.Instance.DisablePrimaryActionUI();
        BattleManager.Instance.UpdateActionText("You have been defeated!");

        // Wait for the defeat message
        yield return new WaitForSeconds(2f);

        // Set spawn point for Yggdrasil
        GlobalVariables.lastTeleportSpawnID = "outskirts_spawn";
        
        // Restore player's stats
        playerStats.Vitality = playerStats.maxVitality;
        playerStats.Intelligence = playerStats.maxIntelligence;
        
        // Stop battle music and load Yggdrasil scene
        AudioManager.Instance.StopBattleMusic();
        SceneManager.LoadScene("YggdrasilSampleScene");
    }

}