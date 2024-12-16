using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController instance;

    void Awake()
    {
        // Check if an instance already exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);  // Destroy the duplicate
        }
        else
        {
            instance = this;  // Set this as the instance
            DontDestroyOnLoad(gameObject);  // Ensure it persists
        }
    }

    // Your other game controller logic here...
}
