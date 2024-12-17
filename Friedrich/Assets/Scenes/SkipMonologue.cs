using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SkipMonologue : MonoBehaviour
{
    [SerializeField] private Button skipButton;
    [SerializeField] private float delayBeforeShowing = 3f;

    private void Start()
    {
        skipButton.gameObject.SetActive(false);

        StartCoroutine(ShowSkipButton());
    }

    private System.Collections.IEnumerator ShowSkipButton()
    {
        yield return new WaitForSeconds(delayBeforeShowing);
        skipButton.gameObject.SetActive(true);

        skipButton.onClick.AddListener(SkipToLoadingScene);
    }

    private void SkipToLoadingScene()
    {
        SceneManager.LoadScene("LoadingScene");
    }
}
