using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.Rendering.Universal;

public enum SkillType
{
    None,
    RapidAssault,
    HackingSlash,
    CrescentSlash,
}

public class PlayerSkillManager : MonoBehaviour
{
  
    private SkillType currentSkill = SkillType.None;
    public WeaponSO equippedWeapon;
    private BattleManager battleManager;
    private RapidAssaultFeedback rapidAssaultFeedback;
    private HackingSlashFeedback hackingSlashFeedback;
    private Transform playerTransform;
    private Vector3 originalPosition;
    public bool isPerformingSkill = false;
    private float skillTimer = 0f;
    private int hitCount = 0;
    private int hackingSlashHitCount = 0; 
    public ParticleSystem lightningEffect;
    public ParticleSystem lightningBurst;
    public ParticleSystem lightningDash;
    public SpriteRenderer playerSpriteRenderer;

    [Header("Hacking Slash Skill")]
    public GameObject skillEffectPrefab;

    public Light2D[] slashLights; // Array to hold the slash lights
    public float lightDuration = 0.3f; // How long the light stays visible
    private System.Random random = new System.Random(); // For random selection of lights

    private float targetStart = 0.4f; 
    private float targetEnd = 0.6f; 

    public float totalDamageDealt = 0; // Variable for storing damage calculation
    public int spacebarPressCount = 0; // Variable added


    private CinemachineImpulseSource impulseSource;

    public void Start()
    {

        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        battleManager = FindObjectOfType<BattleManager>();
        BattleManager.Instance.audioSource = gameObject.AddComponent<AudioSource>(); 
        if (GlobalVariables.equippedWeapon != null)
        {
            equippedWeapon = GlobalVariables.equippedWeapon;
        }
        battleManager = FindObjectOfType<BattleManager>();
        playerTransform = transform;
        originalPosition = playerTransform.position;

        battleManager.audioSource = GetComponent<AudioSource>();
        
        if (battleManager.timingSlider == null)
            Debug.LogWarning("Please assign the timingSlider in the Inspector.");
        if (battleManager.blueBarImage == null)
            Debug.LogWarning("Please assign the blueBarImage in the Inspector.");

        lightningEffect.gameObject.SetActive(false);  // Disable the particle system initially
        lightningBurst.gameObject.SetActive(false);
        lightningDash.gameObject.SetActive(false);
        rapidAssaultFeedback = FindObjectOfType<RapidAssaultFeedback>();
        hackingSlashFeedback = FindObjectOfType<HackingSlashFeedback>();
        if (rapidAssaultFeedback == null)
        {
            Debug.LogError("RapidAssaultFeedback script not found in the scene!");
        }

    }

    public void EquipWeapon(WeaponSO weapon)
    {
        equippedWeapon = weapon;
        GlobalVariables.equippedWeapon = weapon; // Save to global variables
    }

    public IEnumerator PerformRapidAssault()
    {
        Animator animator = playerTransform.GetComponent<Animator>();
        rapidAssaultFeedback.comboCounterText.gameObject.SetActive(true);
        rapidAssaultFeedback.hypeText.gameObject.SetActive(true);
        isPerformingSkill = true;
        skillTimer = 0f;
        hitCount = 0;

        SkillSO rapidAssault = equippedWeapon.skills[0];
        Vector3 targetPosition = originalPosition + Vector3.right * rapidAssault.slideDistance;
        BattleManager.Instance.SetOpacity(0.04f);

        SpeedIllusionCast(0);
        // Slide to enemy
        animator.ResetTrigger("WarriorIdleRight");
        animator.SetTrigger("DashForward");
        lightningEffect.gameObject.SetActive(true);
        lightningBurst.gameObject.SetActive(true);
        lightningDash.gameObject.SetActive(true);
        
        CameraShake.instance.TriggerCameraShake(impulseSource);
        while (Vector3.Distance(playerTransform.position, targetPosition) > 0.1f)
        {
                playerTransform.position = Vector3.MoveTowards(
                playerTransform.position,
                targetPosition,
                rapidAssault.slideSpeed * Time.deltaTime
            );
            yield return null;
        }
        SpeedIllusionCast(500);
        // Rapid assault phase
        animator.ResetTrigger("DashForward"); // Clear the DashForward trigger
        while (skillTimer < rapidAssault.duration)
        { 
            SpeedIllusionCast(0);
            lightningEffect.gameObject.SetActive(true);
            skillTimer += Time.deltaTime;

            // Check for spacebar input
            if (Input.GetKeyDown(KeyCode.Space) && isPerformingSkill)
            {
                SpeedIllusionCast(500);
                CameraShake.instance.TriggerCameraShake(impulseSource);
                hitCount++;
                AudioManager.Instance.PlaySlashSound();
                StartCoroutine(PerformSlashAnimation());  // Trigger the slash animation 
                battleManager.DealDamage(rapidAssault.damagePerHit);  // Deal damage with each slash
                StartCoroutine(SpawnSlashLight());
                rapidAssaultFeedback.IncrementCombo();
                rapidAssaultFeedback.OnSpacePressed();
            }
            else if (!Input.GetKey(KeyCode.Space) && isPerformingSkill)
            {
                rapidAssaultFeedback.OnSpaceReleased();
                SpeedIllusionCast(150);
                // Trigger idle slash if space is not pressed
                TriggerIdleSlash();
            }
        

            yield return null;
        }
        lightningEffect.gameObject.SetActive(false);
        lightningBurst.gameObject.SetActive(false);
        lightningDash.gameObject.SetActive(false);

        // Display results
        Debug.Log($"Rapid Assault completed! Total hits: {hitCount}");

        // Slide back
        animator.ResetTrigger("Slash");
        animator.ResetTrigger("IdleSlash");
        animator.SetTrigger("DashBack");
        lightningBurst.gameObject.SetActive(true);
        while (Vector3.Distance(playerTransform.position, originalPosition) > 0.1f)
        {
            rapidAssaultFeedback.OnSpaceReleased();
            SpeedIllusionCast(0);
            playerTransform.position = Vector3.MoveTowards(
                playerTransform.position, 
                originalPosition, 
                rapidAssault.slideSpeedBack * Time.deltaTime
            );
            yield return null;
        }

        playerTransform.position = originalPosition;
        isPerformingSkill = false;

        // After finishing the rapid assault, go back to idle
        if(!isPerformingSkill)
        {   
            animator.ResetTrigger("DashBack");
            TriggerIdle();
        }
        SpeedIllusionCast(500);
        BattleManager.Instance.SetOpacity(1f);
        yield return new WaitForSeconds(0.4f);
        lightningEffect.gameObject.SetActive(false);
        lightningBurst.gameObject.SetActive(false);
        rapidAssaultFeedback.ResetCombo();
        rapidAssaultFeedback.comboCounterText.gameObject.SetActive(false);
        rapidAssaultFeedback.hypeText.gameObject.SetActive(false);
        battleManager.EndPlayerTurn();
    }

    private IEnumerator SpawnSlashLight()
    {
        // Select a random light from the array
        int randomIndex = random.Next(slashLights.Length);
        Light2D selectedLight = slashLights[randomIndex];

        // Activate the light
        selectedLight.gameObject.SetActive(true);

        // Wait for the specified duration
        yield return new WaitForSeconds(lightDuration);

        // Deactivate the light after the duration
        selectedLight.gameObject.SetActive(false);
    }

    public void SetCurrentSkill(SkillType skillType)
    {
        currentSkill = skillType;
    }

    public void UseRapidAssault()
    {
        SetCurrentSkill(SkillType.RapidAssault);
        StartCoroutine(PerformRapidAssault());
    }

    public void UseHackingSlash()
    {
        SetCurrentSkill(SkillType.HackingSlash);
        StartCoroutine(PerformHackingSlash());
    }

    public void UseCrescentSlash()
    {
        SetCurrentSkill(SkillType.CrescentSlash);
        StartCoroutine(PerformCrescentSlash());
    }

    public void SpeedIllusionCast(float opacity)
    {
        Color color = playerSpriteRenderer.color;
        color.a = opacity / 255f;  // Normalize opacity to the range [0, 1]
        playerSpriteRenderer.color = color;
    }

    private IEnumerator BlinkEffect(float blinkInterval)
    {
        while (!Input.GetKey(KeyCode.Space) && isPerformingSkill)
        {
            SpeedIllusionCast(0);    // Make sprite invisible
            yield return new WaitForSeconds(blinkInterval);
            
            SpeedIllusionCast(500);  // Make sprite visible again
            yield return new WaitForSeconds(blinkInterval);
        }

        SpeedIllusionCast(500);  // Ensure sprite is visible when blinking ends
    }

    private void TriggerIdleSlash()
    {
        Animator animator = playerTransform.GetComponent<Animator>();
         animator.SetTrigger("IdleSlash");
    }

    private void TriggerIdle()
    {
        Animator animator = playerTransform.GetComponent<Animator>();
        animator.SetTrigger("WarriorIdleRight"); // Trigger the idle animation
    }

    private IEnumerator PerformSlashAnimation()
    {
        Animator animator = playerTransform.GetComponent<Animator>();
        animator.SetTrigger("Slash"); // Trigger the slash animation
        yield return null;
    }

    //end of rapid assault
    public IEnumerator PerformHackingSlash()
    {
        Animator animator = playerTransform.GetComponent<Animator>();
        SkillSO hackingSlash = equippedWeapon.skills.Find(skill => skill.skillName == "Hacking Slash");
        hackingSlash.maxHits = 5;
        hackingSlashHitCount = 0;
        float elapsedTime = 0f;

        // Store original position and calculate target left position
        Vector3 originalPosition = playerTransform.position;
        Vector3 targetPosition = originalPosition + Vector3.left * 1f; // Adjust 0.5f as needed for distance
        SpeedIllusionCast(0);
        lightningBurst.gameObject.SetActive(true);
        lightningEffect.gameObject.SetActive(true);
        lightningDash.gameObject.SetActive(true);
        // Move to target position at the start of the skill
        float moveDuration = 0.2f; // Time to move to the left
        float moveTime = 0f;
        while (moveTime < moveDuration)
        {
            playerTransform.position = Vector3.Lerp(originalPosition, targetPosition, moveTime / moveDuration);
            moveTime += Time.deltaTime;
            yield return null;
        }
        CameraShake.instance.TriggerCameraShake(impulseSource);
        playerTransform.position = targetPosition; // Ensure it reaches the exact target position

        battleManager.timingSlider.value = 0;

        isPerformingSkill = true;
        yield return new WaitForSeconds(0.4f);
        BattleManager.Instance.SetOpacity(0.04f);

        while (elapsedTime < hackingSlash.duration && hackingSlashHitCount < hackingSlash.maxHits)
        {
            elapsedTime += Time.deltaTime;
            lightningBurst.gameObject.SetActive(true);
            lightningEffect.gameObject.SetActive(true);
            lightningDash.gameObject.SetActive(true);
            
            // Access timingSlider and blueBarImage from battleManager
            battleManager.timingSlider.value = Mathf.PingPong(elapsedTime * battleManager.sliderSpeed * 2f, 1.0f);
            battleManager.blueBarImage.fillAmount = battleManager.timingSlider.value;
            animator.SetTrigger("IdleSlash");
            hackingSlashFeedback.OnSpaceReleasedHack();
            SpeedIllusionCast(500);
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpeedIllusionCast(150);
                animator.ResetTrigger("HackingSlash");
                animator.ResetTrigger("IdleSlash");
                if (battleManager.timingSlider.value >= targetStart && battleManager.timingSlider.value <= targetEnd)
                {
                    hackingSlashFeedback.IncrementHit();
                    hackingSlashHitCount++;
                    AudioManager.Instance.PlaySlashSound();
                    hackingSlashFeedback.OnSpacePressedHack();

                    CameraShake.instance.TriggerCameraShake(impulseSource);
                    CameraShake.instance.TriggerCameraShake(impulseSource);
                    CameraShake.instance.TriggerCameraShake(impulseSource);
                    CameraShake.instance.TriggerCameraShake(impulseSource);

                    lightningBurst.gameObject.SetActive(true);
                    lightningEffect.gameObject.SetActive(true);
                    lightningDash.gameObject.SetActive(true);

                    animator.SetTrigger("HackingSlash");
                    battleManager.DealDamage(hackingSlash.damage); 
                    BattleManager.Instance.PlaySuccessfulHitSound();

                    Debug.Log($"Successful hit {hackingSlashHitCount}! Damage dealt: {hackingSlash.damage}");
                    StartCoroutine(MoveSkillEffectToEnemy());
                }
                else
                {
                    hackingSlashFeedback.OnSpaceReleasedHack();
                    animator.SetTrigger("IdleSlash");
                    Debug.Log("Missed hit!");
                    BattleManager.Instance.PlayUnsuccessfulHitSound();
                    hackingSlash.maxHits--;
                    Debug.Log($"Missed! Remaining hits: {hackingSlash.maxHits}");
                }
            }
            yield return null;
        }

        // Return to original position after skill completes
        SpeedIllusionCast(0);
        moveTime = 0f;
        while (moveTime < moveDuration)
        {
            playerTransform.position = Vector3.Lerp(targetPosition, originalPosition, moveTime / moveDuration);
            moveTime += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = originalPosition; // Ensure it reaches the exact original position
        lightningBurst.gameObject.SetActive(false);
        lightningEffect.gameObject.SetActive(false);
        lightningDash.gameObject.SetActive(false);
        BattleManager.Instance.SetOpacity(0.1f);
        lightningBurst.gameObject.SetActive(true);
        lightningEffect.gameObject.SetActive(true);
        lightningDash.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        animator.ResetTrigger("IdleSlash");
        if (hackingSlashHitCount >= hackingSlash.maxHits)
        {
            yield return new WaitForSeconds(0.4f); 
        }
        animator.ResetTrigger("IdleSlash");
        animator.ResetTrigger("HackingSlash");
        animator.SetTrigger("WarriorIdleRight");
        SpeedIllusionCast(500);
        battleManager.timingSlider.value = 0;

        isPerformingSkill = false;
        battleManager.timingSlider.gameObject.SetActive(false);
        battleManager.blueBarImage.gameObject.SetActive(false);
        battleManager.border.gameObject.SetActive(false);

        lightningBurst.gameObject.SetActive(false);
        lightningEffect.gameObject.SetActive(false);
        lightningDash.gameObject.SetActive(false);

        hackingSlashFeedback.ResetAfterSkill();
        hackingSlashFeedback.hitCounterText.gameObject.SetActive(false);
        hackingSlashFeedback.feedbackText.gameObject.SetActive(false);
        BattleManager.Instance.SetOpacity(1f);

        yield return new WaitForSeconds(0.4f);
        battleManager.EndPlayerTurn();
    }

    public string GetCurrentSkillName()
    {
        if (currentSkill == SkillType.RapidAssault)
            return "Rapid Assault";
        else if (currentSkill == SkillType.HackingSlash)
            return "Hacking Slash";
        else if (currentSkill == SkillType.CrescentSlash)
            return "Crescent Slash";
        
        return "Unknown Skill";
    }

    private IEnumerator MoveSkillEffectToEnemy()
    {
        // Get the enemy's position (you'll need to provide this)
        Vector3 enemyPosition = GetEnemyPosition();

        // Instantiate the skill effect prefab at the player's position, slightly offset to the right
        Vector3 spawnPosition = playerTransform.position + Vector3.right * 0.5f;
        Quaternion spawnRotation = Quaternion.Euler(skillEffectPrefab.transform.rotation.eulerAngles);

        GameObject skillEffect = Instantiate(skillEffectPrefab, spawnPosition, spawnRotation);

        // Move the skill effect towards a position slightly beyond the enemy's position
        Vector3 targetPosition = enemyPosition + Vector3.right * 5f; // Adjust the offset value as needed
        float elapsedTime = 0f;
        float duration = 0.5f; // Adjust this value to control the movement speed

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            skillEffect.transform.position = Vector3.Lerp(spawnPosition, targetPosition, elapsedTime / duration);
            yield return null;
        }

        // Fade out the skill effect
        float fadeOutDuration = 0.2f; // Adjust this value to control the fade out duration
        float startAlpha = skillEffect.GetComponent<SpriteRenderer>().color.a; // Assuming the skill effect has a SpriteRenderer component
        float elapsedFadeOutTime = 0f;

        while (elapsedFadeOutTime < fadeOutDuration)
        {
            elapsedFadeOutTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedFadeOutTime / fadeOutDuration);
            Color color = skillEffect.GetComponent<SpriteRenderer>().color;
            color.a = alpha;
            skillEffect.GetComponent<SpriteRenderer>().color = color;
            yield return null;
        }
        // Destroy the skill effect after fading out
        Destroy(skillEffect);
    }

    private Vector3 GetEnemyPosition()
    {
        // Replace this with your own logic to get the enemy's position
        return Vector3.zero;
    }
    public IEnumerator PerformCrescentSlash()
    {
        Animator animator = playerTransform.GetComponent<Animator>();
        isPerformingSkill = true;
        SkillSO crescentSlashSkill = equippedWeapon.skills.Find(skill => skill.skillName == "Crescent Slash");
        float elapsedTime = 0f;
        int requiredPressCount = crescentSlashSkill.requiredPressCount;
        float reducedDamagePercentage = crescentSlashSkill.reducedDamagePercentage;
        battleManager.crescentSlider.gameObject.SetActive(true);
        battleManager.whiteBarImage.gameObject.SetActive(true);

        while (elapsedTime < crescentSlashSkill.duration)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (spacebarPressCount < requiredPressCount)
                {
                    spacebarPressCount++;
                    BattleManager.Instance.crescentSlider.value = (float)spacebarPressCount / requiredPressCount;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (spacebarPressCount >= requiredPressCount)
        {
            battleManager.DealDamage(crescentSlashSkill.crescentDamage);
            Debug.Log("Crescent Slash success! Full damage: " + crescentSlashSkill.crescentDamage);
        }
        else
        {
           battleManager.DealDamage((int)(crescentSlashSkill.crescentDamage * (1 - crescentSlashSkill.reducedDamagePercentage / 100)));
            Debug.Log("Crescent Slash partial success! Reduced damage: " + crescentSlashSkill.crescentDamage * (1 - crescentSlashSkill.reducedDamagePercentage / 100));
        }

        isPerformingSkill = false;
        battleManager.whiteBarImage.gameObject.SetActive(false);
        battleManager.crescentSlider.gameObject.SetActive(false);
        battleManager.crescentSlider.value = 0; 
        battleManager.EndPlayerTurn();
    }


    public bool IsPerformingSkill()
    {
        return isPerformingSkill;
    }
}
