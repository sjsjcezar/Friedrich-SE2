using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public Slider playerHealthSlider;
    public Slider playerIntelligenceSlider;
    public Slider enemyHealthSlider;

    public void InitializeSliders(int playerMaxHealth, int playerCurrentHealth, int playerMaxIntelligence, int playerCurrentIntelligence, int enemyMaxHealth)
    {
        // Initialize player health slider
        playerHealthSlider.maxValue = playerMaxHealth;
        playerHealthSlider.value = playerCurrentHealth;

        // Initialize player intelligence slider
        playerIntelligenceSlider.maxValue = playerMaxIntelligence;
        playerIntelligenceSlider.value = playerCurrentIntelligence;

        // Initialize enemy health slider
        enemyHealthSlider.maxValue = enemyMaxHealth;
        enemyHealthSlider.value = enemyMaxHealth;
        
        Debug.Log("Health and Intelligence sliders initialized.");
    }

    public void UpdatePlayerHealth(int currentHealth, int maxHealth)
    {
        // Ensure the max value is correct, but avoid resetting the value to 100%
        if (playerHealthSlider.maxValue != maxHealth)
        {
            playerHealthSlider.maxValue = maxHealth; // Set max value correctly
        }

        // Update slider value based on current vitality and max vitality
        float healthPercentage = (float)currentHealth / maxHealth; // This gives a percentage (0 to 1)
        playerHealthSlider.value = healthPercentage * playerHealthSlider.maxValue; // Set the slider value proportionally
        Debug.Log($"Player Health Slider Updated: {currentHealth}/{maxHealth} - {healthPercentage * 100}%");
    }

    public void UpdatePlayerIntelligence(int currentIntelligence, int maxIntelligence)
    {
        if (playerIntelligenceSlider.maxValue != maxIntelligence)
        {
            playerIntelligenceSlider.maxValue = maxIntelligence;
        }

        float intelligencePercentage = (float)currentIntelligence / maxIntelligence;
        playerIntelligenceSlider.value = intelligencePercentage * playerIntelligenceSlider.maxValue;
        Debug.Log($"Player Intelligence Slider Updated: {currentIntelligence}/{maxIntelligence} - {intelligencePercentage * 100}%");
    }

    public void UpdateEnemyHealth(int currentHealth)
    {
        enemyHealthSlider.value = currentHealth;
        Debug.Log($"Enemy Health Slider Updated: {enemyHealthSlider.value}/{enemyHealthSlider.maxValue}");
    }
}