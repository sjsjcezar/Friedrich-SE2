using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SpotlightBlink : MonoBehaviour
{
    public Light2D spotlight; // Reference to the Light2D component

    [Header("Breathing Settings")]
    public float breathCycleDuration = 2.0f; // Duration for a full breath cycle (in and out)
    public float maxIntensity = 1.0f; // Maximum intensity of the spotlight
    public float minIntensity = 0.2f; // Minimum intensity of the spotlight

    private float intensityRange; // Difference between max and min intensity
    private float timeElapsed = 0f; // Time elapsed since the start of the current cycle

    private void Start()
    {
        if (spotlight == null)
        {
            spotlight = GetComponent<Light2D>();
            if (spotlight == null)
            {
                Debug.LogError("No Light2D component found on this GameObject.");
                return;
            }
        }

        // Calculate the range of intensity
        intensityRange = maxIntensity - minIntensity;
    }

    private void Update()
    {
        if (spotlight != null)
        {
            // Update the breathing effect
            timeElapsed += Time.deltaTime;
            float cycleProgress = timeElapsed / breathCycleDuration;
            float sineWave = Mathf.Sin(cycleProgress * 2 * Mathf.PI); // Oscillates between -1 and 1

            // Smoothly transition intensity between min and max
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, (sineWave + 1) / 2);

            spotlight.intensity = intensity;

            // Reset the cycle when it completes
            if (timeElapsed >= breathCycleDuration)
            {
                timeElapsed -= breathCycleDuration;
            }
        }
    }
}
