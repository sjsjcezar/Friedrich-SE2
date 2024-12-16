using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrescentSlashFeedback : MonoBehaviour
{
    public Slider chargeSlider;
    public Image chargeIndicator;
    public TextMeshProUGUI chargeText;
    public ParticleSystem chargeParticles;
    public Animator crescentAnimator;

    private float maxChargeTime = 2f;
    private float currentCharge = 0f;
    private bool isCharging = false;

    private void Start()
    {
        DisableUI();
    }

    public void EnableUI()
    {
        chargeSlider.gameObject.SetActive(true);
        chargeIndicator.gameObject.SetActive(true);
        chargeText.gameObject.SetActive(true);
        ResetCharge();
    }

    public void DisableUI()
    {
        chargeSlider.gameObject.SetActive(false);
        chargeIndicator.gameObject.SetActive(false);
        chargeText.gameObject.SetActive(false);
        if (chargeParticles != null)
        {
            chargeParticles.Stop();
        }
    }

    public void StartCharging()
    {
        isCharging = true;
        if (chargeParticles != null)
        {
            chargeParticles.Play();
        }
    }

    public void UpdateCharge()
    {
        if (!isCharging) return;

        currentCharge += Time.deltaTime;
        chargeSlider.value = currentCharge / maxChargeTime;
        
        UpdateChargeText();
    }

    public void StopCharging()
    {
        isCharging = false;
        if (chargeParticles != null)
        {
            chargeParticles.Stop();
        }
    }

    private void UpdateChargeText()
    {
        float chargePercentage = (currentCharge / maxChargeTime) * 100f;
        chargeText.text = $"Charge: {Mathf.RoundToInt(chargePercentage)}%";
    }

    public void ResetCharge()
    {
        currentCharge = 0f;
        chargeSlider.value = 0f;
        UpdateChargeText();
    }

    public float GetChargeMultiplier()
    {
        return Mathf.Clamp01(currentCharge / maxChargeTime);
    }

    public void PlaySlashAnimation()
    {
        if (crescentAnimator != null)
        {
            crescentAnimator.SetTrigger("CrescentSlash");
        }
    }
}