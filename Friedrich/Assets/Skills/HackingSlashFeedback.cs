using UnityEngine;
using TMPro;
using System.Collections;

public class HackingSlashFeedback : MonoBehaviour
{
    public TextMeshProUGUI hitCounterText;  // TMP for hit counter
    public TextMeshProUGUI feedbackText;    // TMP for feedback text

    private int successfulHits = 0;
    private Color[] feedbackColors;
    private string[] feedbackMessages;

    private Vector3 originalHitScale;       // Store the original scale of the hit counter
    private Vector3 originalFeedbackScale; // Store the original scale of the feedback text
    private Vector3 targetScale;

    private void Start()
    {
        // Initialize text visibility and scales
        hitCounterText.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);

        // Define colors for each feedback level
        feedbackColors = new Color[]
        {
            Color.white,                 // 0-1 Hits
            new Color32(0, 200, 255, 255),  // Skillz! Color
            new Color32(255, 0, 0, 255),    // Merciless!! Color
            new Color32(255, 255, 0, 255)   // FLAWLESS!!! Color
        };

        // Define messages for each feedback level
        feedbackMessages = new string[]
        {
            "Mediocre", 
            "Skillz!", 
            "Merciless!!", 
            "FLAWLESS!!!"
        };

        // Store the original scales for hit and feedback texts
        originalHitScale = hitCounterText.transform.localScale;
        originalFeedbackScale = feedbackText.transform.localScale;
        targetScale = originalHitScale * 1.25f; // Set cap scale to 1.25 times the original size

        UpdateUI(); // Update UI initially
    }

    // Increment hit count and update UI
    public void IncrementHit()
    {
        successfulHits++;
        UpdateUI();
        hitCounterText.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);

        // Scale and shake effects
        StartCoroutine(GrowAndShakeText(hitCounterText, originalHitScale));
        StartCoroutine(GrowAndShakeText(feedbackText, originalFeedbackScale));
    }

    // Reset hits to 0 and update UI
    public void ResetHits()
    {
        successfulHits = 0;
        UpdateUI(); // Update text to reflect 0 hits
        // Reset scales to original values
        hitCounterText.transform.localScale = originalHitScale;
        feedbackText.transform.localScale = originalFeedbackScale;
    }

    // Update UI based on the current hit count
    private void UpdateUI()
    {
        hitCounterText.text = successfulHits.ToString();
        UpdateFeedback();
    }

    // Update the feedback based on the hit count
    private void UpdateFeedback()
    {
        if (successfulHits <= 1)
        {
            SetFeedbackColors(feedbackColors[0]);
            feedbackText.text = feedbackMessages[0];
            feedbackText.color = feedbackColors[0];
        }
        else if (successfulHits <= 3)
        {
            SetFeedbackColors(feedbackColors[1]);
            feedbackText.text = feedbackMessages[1];
            feedbackText.color = feedbackColors[1];
        }
        else if (successfulHits == 4)
        {
            SetFeedbackColors(feedbackColors[2]);
            feedbackText.text = feedbackMessages[2];
            feedbackText.color = feedbackColors[2];
        }
        else
        {
            SetFeedbackColors(feedbackColors[3]);
            feedbackText.text = feedbackMessages[3];
            feedbackText.color = feedbackColors[3];
        }
    }

    // Set the color for hit counter text
    private void SetFeedbackColors(Color color)
    {
        hitCounterText.color = color;
    }

    // Grow the text when space is pressed (visual effect)
    public void OnSpacePressedHack()
    {
        StartCoroutine(GrowText(hitCounterText, targetScale));
        StartCoroutine(GrowText(feedbackText, targetScale));
    }

    // Shrink the text when space is released (visual effect)
    public void OnSpaceReleasedHack()
    {
        StartCoroutine(ShrinkText(hitCounterText, originalHitScale));
        StartCoroutine(ShrinkText(feedbackText, originalFeedbackScale));
    }

    // Coroutine to smoothly grow text to a target scale
    private IEnumerator GrowText(TextMeshProUGUI text, Vector3 target)
    {
        while (text.transform.localScale.x < target.x)
        {
            text.transform.localScale = Vector3.Lerp(text.transform.localScale, target, Time.deltaTime * 10);
            yield return null;
        }
        text.transform.localScale = target; // Ensure it reaches the target scale
    }

    // Coroutine to smoothly shrink text back to the original scale
    private IEnumerator ShrinkText(TextMeshProUGUI text, Vector3 original)
    {
        while (text.transform.localScale.x > original.x)
        {
            text.transform.localScale = Vector3.Lerp(text.transform.localScale, original, Time.deltaTime * 10);
            yield return null;
        }
        text.transform.localScale = original; // Ensure it returns to the original scale
    }

    // Grow the text and apply a shake effect for a short period
    private IEnumerator GrowAndShakeText(TextMeshProUGUI text, Vector3 original)
    {
        Vector3 shakeOffset = new Vector3(0.02f, 0.02f, 0); // Subtle shake offset

        // First grow the text
        yield return GrowText(text, targetScale);

        // Shake effect for a few frames
        for (int i = 0; i < 5; i++)
        {
            text.transform.localPosition += shakeOffset;
            yield return new WaitForSeconds(0.02f);
            text.transform.localPosition -= shakeOffset;
            yield return new WaitForSeconds(0.02f);
        }
    }

    // Call this function to reset both the hit counter and feedback after the skill is complete
    public void ResetAfterSkill()
    {
        ResetHits(); // Resets the hit count and updates the UI to 0 hits
        hitCounterText.gameObject.SetActive(false); // Disables the hit counter text
        feedbackText.gameObject.SetActive(false); // Disables the feedback text

        // Optional: Reset scale if necessary
        hitCounterText.transform.localScale = originalHitScale;
        feedbackText.transform.localScale = originalFeedbackScale;
    }
}
