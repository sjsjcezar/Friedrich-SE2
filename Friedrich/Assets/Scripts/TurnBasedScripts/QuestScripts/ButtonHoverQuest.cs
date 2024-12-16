using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverQuest : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image backgroundImage; // The UI Image for the button's background
    public Sprite defaultSprite;  // Sprite for the unselected state
    public Sprite pressedSprite;  // Sprite for the selected state
    public Sprite hoverSprite;    // Sprite for the hover state (optional)

    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    private static ButtonHoverQuest currentlySelectedButton; // Tracks the pressed button
    private bool isPressed = false; // State of the current button

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        ResetButtonState();
    }

    private void OnEnable()
    {
        ResetButtonState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isPressed && hoverSprite != null)
        {
            SetBackgroundImage(hoverSprite);
        }
        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPressed)
        {
            SetBackgroundImage(defaultSprite);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SelectButton();
        PlaySound(clickSound);
    }

    public void SelectButton()
    {
        // Deselect the previously selected button
        if (currentlySelectedButton != null && currentlySelectedButton != this)
        {
            currentlySelectedButton.ResetButtonState();
        }

        // Set this button as selected
        currentlySelectedButton = this;
        isPressed = true;
        SetBackgroundImage(pressedSprite);
    }

    public void ResetButtonState()
    {
        isPressed = false;
        SetBackgroundImage(defaultSprite);
    }

    private void SetBackgroundImage(Sprite sprite)
    {
        if (backgroundImage != null && sprite != null)
        {
            backgroundImage.sprite = sprite;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public static void ForceSelect(ButtonHoverQuest button)
    {
        if (currentlySelectedButton != null && currentlySelectedButton != button)
        {
            currentlySelectedButton.ResetButtonState();
        }
        currentlySelectedButton = button;
        button.isPressed = true;
        button.SetBackgroundImage(button.pressedSprite);
    }
}
