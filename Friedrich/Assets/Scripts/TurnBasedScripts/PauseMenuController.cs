using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public Button resumeButton;
    public Button saveButton;
    public Button loadButton;
    public Button settingsButton;
    public Button mainMenuButton;
    public Button quitButton;

    private SaveLoadManager saveLoadManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        resumeButton.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);
        loadButton.gameObject.SetActive(false);
        settingsButton.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        
        saveLoadManager = FindObjectOfType<SaveLoadManager>();
        
        // Add listeners to buttons
        resumeButton.onClick.AddListener(Resume);
        saveButton.onClick.AddListener(SaveGame);
        loadButton.onClick.AddListener(LoadGame);
        settingsButton.onClick.AddListener(OpenSettings);
        mainMenuButton.onClick.AddListener(LoadMainMenu);
        quitButton.onClick.AddListener(QuitGame);

        // Ensure menu is hidden at start
        pauseMenuUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Check if any other UI panels are active
            if (StatueController.Instance != null && StatueController.Instance.statueUIPanel.activeSelf)
            {
                // Don't pause if statue UI is open
                return;
            }

            if (VialController.Instance != null && VialController.Instance.vialUIPanel.activeSelf)
            {
                // Don't pause if vial UI is open
                return;
            }

            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        resumeButton.gameObject.SetActive(false);
        saveButton.gameObject.SetActive(false);
        loadButton.gameObject.SetActive(false);
        settingsButton.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        
        // Re-enable player movement if it was disabled
        var player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.enabled = true;
        }
        pauseMenuUI.SetActive(false);
        FindObjectOfType<VialOverworldManager>()?.SetUIVisibility(true);
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        resumeButton.gameObject.SetActive(true);
        saveButton.gameObject.SetActive(true);
        loadButton.gameObject.SetActive(true);
        settingsButton.gameObject.SetActive(true);
        mainMenuButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        
        // Disable player movement while paused
        var player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.enabled = false;
        }

        FindObjectOfType<VialOverworldManager>()?.SetUIVisibility(false);
        
        pauseMenuUI.SetActive(true);
    }

    void SaveGame()
    {
        saveLoadManager.SaveGame();
    }

    void LoadGame()
    {
        saveLoadManager.LoadGame();
        Resume(); // Resume after loading
    }

    void OpenSettings()
    {
        Debug.Log("Settings menu not implemented yet");
    }

    void LoadMainMenu()
    {
        Debug.Log("Main menu loading not implemented yet");
    }

    void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}