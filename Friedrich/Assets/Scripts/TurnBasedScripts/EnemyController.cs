using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    public float chaseRange = 5f; // Range within which the enemy will chase the player
    public float speed = 3f; // Speed of the enemy
    private float chaseTimer = 0f; // Timer to track how long since last collision

    void Start()
    {
        // Disable the EnemyController script if in the BattleScene
        if (SceneManager.GetActiveScene().name == "BattleScene")
        {
            this.enabled = false; // Disable the script
        }
    }

    void Update()
    {
        // Only chase if not in the BattleScene
        if (SceneManager.GetActiveScene().name != "BattleScene")
        {
            // Find the player object tagged "Player"
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
                
                if (distanceToPlayer < chaseRange)
                {
                    // Chase the player
                    transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
                    chaseTimer = 0f; // Reset the timer when chasing
                }
                else
                {
                    // Increment the timer when not chasing
                    chaseTimer += Time.deltaTime;
                    if (chaseTimer > 3f)
                    {
                        // Logic to stop chasing can be added here
                    }
                }
            }
            else
            {
                Debug.LogWarning("Player not found! Ensure the player is tagged 'Player'.");
            }
        }
    }

    void OnEnable()
    {
        // Re-enable the EnemyController script if not in the BattleScene
        if (SceneManager.GetActiveScene().name != "BattleScene")
        {
            this.enabled = true; // Enable the script
        }
    }

    void OnDisable()
    {
        // Disable the EnemyController script when entering the BattleScene
        if (SceneManager.GetActiveScene().name == "BattleScene")
        {
            this.enabled = false; // Disable the script
        }
    }
}