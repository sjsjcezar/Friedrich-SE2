using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AncillaryLightManager : MonoBehaviour
{
    [Header("Ancillary Lights")]
    public Light2D[] ancillaryLights; // Assign in the Inspector
    public float nightIntensity = 1f; // Intensity when it’s night
    public float dayIntensity = 0f; // Intensity when it’s day

    private void OnEnable()
    {
        if (WorldTime.WorldTime.Instance != null)
        {
            WorldTime.WorldTime.Instance.OnTimeChanged += HandleTimeChanged;

            // Initialize the current state
            bool isNight = GlobalVariables.hours + (GlobalVariables.minutes / 60f) >= WorldTime.WorldTime.Instance.startLightActivation ||
                           GlobalVariables.hours + (GlobalVariables.minutes / 60f) < WorldTime.WorldTime.Instance.stopLightDeactivation;
            HandleTimeChanged(GlobalVariables.hours + (GlobalVariables.minutes / 60f), isNight);
        }
        else
        {
            Debug.LogError("WorldTime.Instance is null. Ensure WorldTime is initialized before AncillaryLightManager.");
        }
    }

    private void OnDisable()
    {
        if (WorldTime.WorldTime.Instance != null)
        {
            WorldTime.WorldTime.Instance.OnTimeChanged -= HandleTimeChanged;
        }
    }

    private void HandleTimeChanged(float currentTime, bool isNight)
    {
        Debug.Log($"HandleTimeChanged called. CurrentTime: {currentTime}, IsNight: {isNight}");

        foreach (var light in ancillaryLights)
        {
            if (light != null)
            {
                light.intensity = isNight ? nightIntensity : dayIntensity;
                light.gameObject.SetActive(isNight);
            }
        }
    }
}