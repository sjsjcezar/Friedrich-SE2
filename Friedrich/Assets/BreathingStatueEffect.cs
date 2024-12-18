using UnityEngine;

public class BreathingStatueEffect : MonoBehaviour
{
    [Header("Breathing Color Settings")]
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color targetColor = new Color(0.333f, 0.357f, 1f); // Target color (#555BFF)
    [SerializeField] private float duration = 2f;

    [Header("Renderer Settings")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private bool isUIElement = false;
    [SerializeField] private UnityEngine.UI.Graphic targetUIElement;

    private float timer = 0f;
    private bool goingToTarget = true;

    private void Update()
    {
        timer += Time.deltaTime / (duration / 2);

        float t = goingToTarget ? Mathf.SmoothStep(0, 1, timer) : Mathf.SmoothStep(1, 0, timer);
        Color currentColor = Color.Lerp(startColor, targetColor, t);

        if (isUIElement && targetUIElement != null)
        {
            targetUIElement.color = currentColor;
        }
        else if (targetRenderer != null)
        {
            targetRenderer.material.color = currentColor;
        }

        if (timer >= 1f)
        {
            timer = 0f;
            goingToTarget = !goingToTarget;
        }
    }
}
