using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    public TMP_Text loadingText; // Reference to the TMP_Text component
    public float delay = 0.5f; // Delay between updates

    private void Start()
    {
        if (loadingText == null)
        {
            loadingText = GetComponent<TMP_Text>(); // Automatically get TMP_Text if not assigned
        }

        StartCoroutine(AnimateLoadingText());
    }

    private IEnumerator AnimateLoadingText()
    {
        string baseText = "Loading";
        int dotCount = 0;

        while (true)
        {
            loadingText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4; // Cycle through 0 to 3 dots
            yield return new WaitForSeconds(delay); // Wait for the specified delay
        }
    }
}
