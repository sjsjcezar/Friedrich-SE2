using UnityEngine;
using TMPro;
using System.Collections;

public class RapidAssaultFeedback : MonoBehaviour
{
    public TextMeshProUGUI comboCounterText; // TMP for combo counter
    public TextMeshProUGUI hypeText;         // TMP for hype text

    private int comboCount = 0;
    private Color[] colorGradients;
    private string[] hypeMessages;

    private Vector3 originalComboScale; // Store the original scale of the combo counter
    private Vector3 originalHypeScale;  // Store the original scale of the hype text
    private Vector3 targetScale;

    private void Start()
    {
        comboCounterText.gameObject.SetActive(false);
        hypeText.gameObject.SetActive(false);
        colorGradients = new Color[]
        {
            Color.white, // Combo 0-13
            new Color32(0, 70, 255, 255), // Brutal! Color
            new Color32(255, 0, 0, 255), // Insane!! Color
            new Color32(255, 70, 255, 70) // GODLY!! Color
        };

        hypeMessages = new string[]
        {
            "Mediocre", 
            "Brutal!", 
            "Insane!!", 
            "GODLY!!!"
        };

        originalComboScale = comboCounterText.transform.localScale;
        originalHypeScale = hypeText.transform.localScale;
        targetScale = originalComboScale * 1.25f; // Set cap scale to 1.12 times the original size

        UpdateUI();
    }

    public void IncrementCombo()
    {
        comboCount++;
        UpdateUI();
        StartCoroutine(GrowAndShakeText(comboCounterText, originalComboScale));
        StartCoroutine(GrowAndShakeText(hypeText, originalHypeScale));
    }

    public void ResetCombo()
    {
        comboCount = 0;
        UpdateUI();
        comboCounterText.transform.localScale = originalComboScale;
        hypeText.transform.localScale = originalHypeScale;
    }

    private void UpdateUI()
    {
        comboCounterText.text = comboCount.ToString();
        UpdateColorsAndHypeText();
    }

    private void UpdateColorsAndHypeText()
    {
        if (comboCount < 14)
        {
            SetColors(Color.white);
            hypeText.text = hypeMessages[0];
            hypeText.color = Color.white;
        }
        else if (comboCount < 25)
        {
            SetColors(colorGradients[1]);
            hypeText.text = hypeMessages[1];
            hypeText.color = colorGradients[1];
        }
        else if (comboCount < 38)
        {
            SetColors(colorGradients[2]);
            hypeText.text = hypeMessages[2];
            hypeText.color = colorGradients[2];
        }
        else
        {
            SetColors(colorGradients[3]);
            hypeText.text = hypeMessages[3];
            hypeText.color = colorGradients[3];
        }
    }

    private void SetColors(Color color)
    {
        comboCounterText.color = color;
    }

    // This function grows the text to a target scale when space is pressed and shrinks back when released
    public void OnSpacePressed()
    {
        StartCoroutine(GrowText(comboCounterText, targetScale));
        StartCoroutine(GrowText(hypeText, targetScale));
    }

    public void OnSpaceReleased()
    {
        StartCoroutine(ShrinkText(comboCounterText, originalComboScale));
        StartCoroutine(ShrinkText(hypeText, originalHypeScale));
    }

    private IEnumerator GrowText(TextMeshProUGUI text, Vector3 target)
    {
        // Scale up towards the target scale
        while (text.transform.localScale.x < target.x)
        {
            text.transform.localScale = Vector3.Lerp(text.transform.localScale, target, Time.deltaTime * 10);
            yield return null;
        }
        text.transform.localScale = target; // Ensure it reaches the target scale
    }

    private IEnumerator ShrinkText(TextMeshProUGUI text, Vector3 original)
    {
        // Scale down towards the original scale
        while (text.transform.localScale.x > original.x)
        {
            text.transform.localScale = Vector3.Lerp(text.transform.localScale, original, Time.deltaTime * 10);
            yield return null;
        }
        text.transform.localScale = original; // Ensure it returns to the original scale
    }

    private IEnumerator GrowAndShakeText(TextMeshProUGUI text, Vector3 original)
    {
        // Grow to target scale
        Vector3 shakeOffset = new Vector3(0.02f, 0.02f, 0); // Subtle shake offset

        yield return GrowText(text, targetScale); // Grow text first

        // Shake effect
        for (int i = 0; i < 5; i++) // Shake for a few frames
        {
            text.transform.localPosition += shakeOffset;
            yield return new WaitForSeconds(0.02f);
            text.transform.localPosition -= shakeOffset;
            yield return new WaitForSeconds(0.02f);
        }
    }
}
