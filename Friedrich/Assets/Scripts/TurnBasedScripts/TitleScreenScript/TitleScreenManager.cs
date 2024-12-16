using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button newGameButton;
    public Button quitButton;
    public GameObject updatePanel;
    public Button proceedButton;
    
    [Header("Transition Elements")]
    public GameObject leftTransitionPanel;
    public GameObject rightTransitionPanel;
    public float transitionSpeed = 10f;

    private void Start()
    {
        // Initialize UI
        updatePanel.SetActive(false);
        
        // Add button listeners
        newGameButton.onClick.AddListener(ShowUpdatePanel);
        proceedButton.onClick.AddListener(StartNewGame);
        quitButton.onClick.AddListener(() => Application.Quit());
    }

    private void ShowUpdatePanel()
    {
        updatePanel.SetActive(true);
    }

    private void StartNewGame()
    {
        StartCoroutine(NewGameSequence());
    }

    private IEnumerator NewGameSequence()
    {
        // Start transition animation
        leftTransitionPanel.SetActive(true);
        rightTransitionPanel.SetActive(true);
        
        RectTransform leftRect = leftTransitionPanel.GetComponent<RectTransform>();
        RectTransform rightRect = rightTransitionPanel.GetComponent<RectTransform>();
        
        // Start from edges
        leftRect.anchoredPosition = new Vector2(-Screen.width, 0);
        rightRect.anchoredPosition = new Vector2(Screen.width, 0);

        // Move panels to center
        float elapsedTime = 0f;
        float duration = 0.5f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float smoothProgress = progress * progress * (3f - 2f * progress);
            
            leftRect.anchoredPosition = Vector2.Lerp(new Vector2(-Screen.width, 0), Vector2.zero, smoothProgress);
            rightRect.anchoredPosition = Vector2.Lerp(new Vector2(Screen.width, 0), Vector2.zero, smoothProgress);
            
            yield return null;
        }

        // Load text scene
        SceneManager.LoadScene("TextScene");
    }
}