using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // Direct references to UI objects
    public GameObject timeCanvas;
    public GameObject overworldUI;
    public GameObject pauseMenu;
    public GameObject inventoryUI;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("UIManager initialized");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene Loaded: {scene.name}");
        bool shouldEnable = scene.name != "BattleScene";
        
        // Directly manage UI objects
        if (timeCanvas != null) 
        {
            timeCanvas.SetActive(shouldEnable);
            Debug.Log($"Time Canvas set to {shouldEnable}");
        }
        
        if (overworldUI != null) 
        {
            overworldUI.SetActive(shouldEnable);
            Debug.Log($"Overworld UI set to {shouldEnable}");
        }
        
        if (pauseMenu != null) 
        {
            pauseMenu.SetActive(shouldEnable);
            Debug.Log($"Pause Menu set to {shouldEnable}");
        }
        
        if (inventoryUI != null) 
        {
            inventoryUI.SetActive(shouldEnable);
            Debug.Log($"Inventory UI set to {shouldEnable}");
        }
    }
}