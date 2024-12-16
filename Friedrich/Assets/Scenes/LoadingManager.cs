using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public float loadingDuration = 2f;
    public TMP_Text loadingText;

    void Start()
    {
        StartCoroutine(LoadingSequence());
    }

    private IEnumerator LoadingSequence()
    {
        float elapsedTime = 0f;
        string[] loadingDots = { ".", "..", "..." };
        int dotIndex = 0;

        while (elapsedTime < loadingDuration)
        {
            if (loadingText != null)
            {
                loadingText.text = "Loading" + loadingDots[dotIndex];
                dotIndex = (dotIndex + 1) % loadingDots.Length;
            }

            yield return new WaitForSeconds(0.3f);
            elapsedTime += 0.3f;
        }

        SceneManager.LoadScene("YggdrasilSampleScene");
    }
}