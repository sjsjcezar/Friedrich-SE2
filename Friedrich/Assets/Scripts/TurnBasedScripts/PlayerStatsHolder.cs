using UnityEngine;

public class PlayerStatsHolder : MonoBehaviour
{
    public static PlayerStatsHolder Instance { get; private set; }

    public PlayerStats playerStats;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Uncomment the next line if you want this object to persist across scenes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats is not assigned in PlayerStatsHolder.");
        }
        else
        {
            playerStats.InitializeVitals(playerStats.Vitality, playerStats.ArcaneStat);
        }
    }
}