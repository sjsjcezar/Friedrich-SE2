using UnityEngine;
using UnityEngine.UI;  // Use this for Unity's built-in Text component
using TMPro;  // Use this if you are using TextMeshPro

public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;
    public TMP_Text fpsText; // For TextMeshPro

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {fps:F2}"; // Update the UI text
    }
}
