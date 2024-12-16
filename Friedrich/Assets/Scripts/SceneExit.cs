using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneExit : MonoBehaviour
{
    public string sceneToLoad;
    public string exitName;
    
    [Header("UI Elements")]
    public GameObject interactPanel;
    public TextMeshProUGUI interactText;
    
    private bool playerInRange = false;
    
    void Start()
    {
        // Ensure panel starts hidden
        if (interactPanel != null)
            interactPanel.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            PlayerPrefs.SetString("LastExitName", exitName);
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {     
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPanel != null)
            {
                interactPanel.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPanel != null)
                interactPanel.SetActive(false);
        }
    }
}