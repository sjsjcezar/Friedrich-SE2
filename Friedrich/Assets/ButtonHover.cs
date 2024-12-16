using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text buttonText;
    public Image backgroundImage;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.blue;

    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        ResetButtonState();
    }

    private void OnEnable()
    {
        ResetButtonState();
    }

    private void OnDisable()
    {
        ResetButtonState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = hoverColor;
        SetBackgroundVisibility(true);
        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetButtonState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(clickSound);
        SetBackgroundVisibility(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void SetBackgroundVisibility(bool isVisible)
    {
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(isVisible);
        }
    }

    private void ResetButtonState()
    {
        if (buttonText != null)
        {
            buttonText.color = normalColor;
        }
        SetBackgroundVisibility(false);
    }
}