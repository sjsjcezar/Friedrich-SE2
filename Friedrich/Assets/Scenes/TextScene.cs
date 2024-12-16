using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TextSceneManager : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string text;
        public AudioClip voiceLine;
        public float pauseAfter; // Additional pause after this line
    }

    public TMP_Text monologueText;
    public float typingSpeed = 0.05f;
    public float defaultPauseBetweenLines = 0.5f;
    public AudioSource audioSource;

    [Header("Transition Elements")]
    public GameObject leftTransitionPanel;
    public GameObject rightTransitionPanel;

    [Header("Dialogue")]
    public DialogueLine[] dialogueLines;

    void Start()
    {
        StartCoroutine(PlayMonologue());
    }

    private IEnumerator PlayMonologue()
    {
        foreach (DialogueLine line in dialogueLines)
        {
            // Clear previous text
            monologueText.text = "";

            // Play voice line if available
            if (line.voiceLine != null && audioSource != null)
            {
                audioSource.PlayOneShot(line.voiceLine);
            }

            // Type out the text
            foreach (char c in line.text)
            {
                monologueText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }

            // Wait for voice line to finish if it's playing
            if (line.voiceLine != null && audioSource.isPlaying)
            {
                yield return new WaitUntil(() => !audioSource.isPlaying);
            }

            // Additional pause after line
            float pauseDuration = line.pauseAfter > 0 ? line.pauseAfter : defaultPauseBetweenLines;
            yield return new WaitForSeconds(pauseDuration);
        }

        StartCoroutine(TransitionToGame());
    }

    private IEnumerator TransitionToGame()
    {
        var musicManager = GetComponent<TextSceneMusic>();
        if (musicManager != null)
        {
            StartCoroutine(musicManager.FadeOutMusic());
        }
        leftTransitionPanel.SetActive(true);
        rightTransitionPanel.SetActive(true);
        
        RectTransform leftRect = leftTransitionPanel.GetComponent<RectTransform>();
        RectTransform rightRect = rightTransitionPanel.GetComponent<RectTransform>();
        
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

        SceneManager.LoadScene("LoadingScene");
    }
}