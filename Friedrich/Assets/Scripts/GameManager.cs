using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine; // Make sure to add this if not already included

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Class Prefabs")]
    public GameObject warriorPrefab;
    public GameObject magePrefab;

    [Header("Cinemachine Virtual Camera")]
    public CinemachineVirtualCamera virtualCamera;

    private GameObject playerInstance;
    public string selectedClass;
    public VectorValue playerPosition;

    private void Awake()
    {
        // Ensure the GameManager is a singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

      //  LoadPlayerData();
    }

    private void Start()
    {
        // Spawn player if one doesn't already exist
        if (playerInstance == null)
        {
            SpawnPlayer();
        }
    }

    public void SetPlayerClass(string className)
    {
        selectedClass = className;
        SavePlayerData();
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        // If a player instance already exists, destroy it before spawning a new one
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }

        // Instantiate the player based on the selected class
        if (selectedClass == "Warrior" && warriorPrefab != null)
        {
            playerInstance = Instantiate(warriorPrefab, playerPosition.initialValue, Quaternion.identity);
        }
        else if (selectedClass == "Mage" && magePrefab != null)
        {
            playerInstance = Instantiate(magePrefab, playerPosition.initialValue, Quaternion.identity);
        }

        if (playerInstance != null)
        {
            DontDestroyOnLoad(playerInstance);

            // Set the virtual camera's Follow target to the newly instantiated player character
            if (virtualCamera != null)
            {
                virtualCamera.Follow = playerInstance.transform;
            }
            else
            {
                Debug.LogError("Virtual Camera reference is not assigned in the inspector!");
            }
        }
    }

  public void ChangeScene(string sceneName)
    {
        StartCoroutine(HandleSceneTransition(sceneName));
    }

    private IEnumerator HandleSceneTransition(string sceneName)
    {
        // Save player position to VectorValue before scene transition
        if (playerInstance != null)
        {
            playerPosition.initialValue = playerInstance.transform.position;
            Destroy(playerInstance); // Optional: destroy player instance before scene change to avoid duplicates
        }

        yield return SceneManager.LoadSceneAsync(sceneName);

        // Spawn a new player instance in the new scene
        SpawnPlayer();
    }

    private void SavePlayerData()
    {
        PlayerPrefs.SetString("PlayerClass", selectedClass);
    }


    private void LoadPlayerData()
    {
        selectedClass = PlayerPrefs.GetString("PlayerClass", "");
        if (!string.IsNullOrEmpty(selectedClass))
        {
            SpawnPlayer();
        }
    }
}
