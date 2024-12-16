using UnityEngine;
using UnityEngine.UI;

public class ColorBlinker : MonoBehaviour
{
    [Header("Colors")]
    [Tooltip("The base color of the object.")]
    [SerializeField] private Color baseColor = new Color(0.459f, 0.765f, 0.463f); // Hex: #75C376
    [Tooltip("The blinking color of the object.")]
    [SerializeField] private Color blinkColor = new Color(0.302f, 0.984f, 0.310f); // Hex: #4DFB4F

    [Header("Blink Settings")]
    [Tooltip("Duration for one blink cycle in seconds (base -> blink -> base).")]
    [SerializeField] private float blinkDuration = 1.0f;

    private Image image;
    private float timer;

    private void Start()
    {
        // Get the Image component
        image = GetComponent<Image>();

        if (image != null)
        {
            image.color = baseColor;
        }
        else
        {
            Debug.LogWarning("No Image component found on this GameObject!");
        }
    }

    private void Update()
    {
        BlinkColor();
    }

    private void BlinkColor()
    {
        timer += Time.deltaTime;
        float phase = (timer % blinkDuration) / (blinkDuration / 2);

        // Determine if we are in the first or second half of the blink cycle
        if (phase < 1)
        {
            // Interpolate towards blinkColor
            image.color = Color.Lerp(baseColor, blinkColor, phase);
        }
        else
        {
            // Interpolate back to baseColor
            image.color = Color.Lerp(blinkColor, baseColor, phase - 1);
        }
    }
}