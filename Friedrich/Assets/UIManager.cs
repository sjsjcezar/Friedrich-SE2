using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManger : MonoBehaviour
{
    private static UIManger instance;

    void Awake()
    {
        // Singleton pattern to make sure only one UI persists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);  // Destroy duplicate UI manager
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Keep UI across scenes
        }
    }

    // Detect when the scene changes
    void Update()
    {
        // Get the name of the current scene
        string sceneName = SceneManager.GetActiveScene().name;
        
        // Check if we are in the battleScene
        if (sceneName == "battleScene")
        {
            Destroy(gameObject);  // Destroy UI manager in battleScene
        }
    }
}
