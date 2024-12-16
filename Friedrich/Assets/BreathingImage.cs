using UnityEngine;
using System.Collections;

public class BreathingImage : MonoBehaviour
{
    public float scaleAmount = 1.2f; // Scale factor
    public float breathingSpeed = 1.0f; // Speed of breathing
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale; // Store original scale
        StartCoroutine(Breathe());
    }

    private IEnumerator Breathe()
    {
        while (true)
        {
            // Scale up
            yield return ScaleTo(originalScale * scaleAmount, breathingSpeed);

            // Scale down
            yield return ScaleTo(originalScale, breathingSpeed);
        }
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 initialScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        transform.localScale = targetScale; // Ensure it reaches the target scale
    }
}
