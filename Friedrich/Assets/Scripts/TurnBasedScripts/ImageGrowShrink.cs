using UnityEngine;
using UnityEngine.UI;

public class ImageGrowShrink : MonoBehaviour
{
    public float scaleFactor = 1.2f;  // Factor by which the image will grow
    public float duration = 1.0f;     // Duration for one grow and shrink cycle
    private Vector3 originalScale;    // To store the original scale of the image
    private bool isGrowing = true;    // Flag to determine if the image is growing or shrinking

    private void Start()
    {
        // Store the original scale of the image
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Time elapsed for the current animation cycle
        float timeElapsed = Mathf.PingPong(Time.time / duration, 1.0f);

        // Calculate the scale based on whether we're growing or shrinking
        if (isGrowing)
        {
            // Smoothly grow the image
            transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleFactor, timeElapsed);
        }
        else
        {
            // Smoothly shrink the image
            transform.localScale = Vector3.Lerp(originalScale * scaleFactor, originalScale, timeElapsed);
        }
    }
}
