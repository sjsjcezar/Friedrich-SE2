using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class BlacksmithInteraction : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject buttonsPanel;
    public Button buyButton;
    public Button sellButton;
    public Button leaveButton;
    public CanvasGroup dialogueCanvasGroup;

    private PlayerMovement playerMovement => PlayerMovement.instance;
    private Animator playerAnimator;

    // Reference to the ShopManager
    public ShopManager shopManager;

    private bool isInteracting = false;
    private bool isPlayerInRange = false;
    private bool isTypewriterEffectActive = false;

    private bool canInteract = true;
    private float interactionCooldown = 1.2f;

    private string[] buyPhrases = {
        "The finest weapons and armor.",
        "Looking to protect yourself? Or deal some damage?",
        "Take a look.",
        "What are you buying?"
    };

    private void Start()
    {
        buyButton.onClick.AddListener(OnBuyClicked);
        sellButton.onClick.AddListener(OnSellClicked);
        leaveButton.onClick.AddListener(OnLeaveClicked);

        dialoguePanel.SetActive(false);
        buttonsPanel.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            StartInteraction();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerAnimator = other.GetComponent<Animator>();
            
            if (playerAnimator == null)
            {
                Debug.LogError("Could not find Animator component on Player!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (isInteracting)
            {
                StartCoroutine(EndInteraction());
            }
        }
    }

    public void StartInteraction()
    {
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement.instance is null!");
            return;
        }

        isInteracting = true;
        StartCoroutine(FadeInDialogueUI());
        
        playerMovement.enabled = false;
        
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0);
            playerAnimator.SetFloat("IdleHorizontal", 0);
            playerAnimator.SetFloat("IdleVertical", 1);
        }
    }

    private IEnumerator FadeInDialogueUI()
    {
        dialoguePanel.SetActive(true);
        buttonsPanel.SetActive(true);
        dialogueText.text = "";
        
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            dialogueCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        dialogueCanvasGroup.alpha = 1;
        StartCoroutine(TypewriterEffect("Hello, Traveler."));
    }

    private IEnumerator FadeOutDialogueUI()
    {
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            dialogueCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        dialogueCanvasGroup.alpha = 0;
        dialoguePanel.SetActive(false);
        buttonsPanel.SetActive(false);
    }

    private void OnBuyClicked()
    {
        if (isTypewriterEffectActive)
        {
            StopAllCoroutines();
        }

        string randomPhrase = buyPhrases[Random.Range(0, buyPhrases.Length)];
        StartCoroutine(TypewriterEffect(randomPhrase));
        Debug.Log("Buy button clicked!");
        buttonsPanel.SetActive(false);
        StartCoroutine(ShowShopAfterDialogue());
    }

    private IEnumerator ShowShopAfterDialogue()
    {
        yield return new WaitUntil(() => !isTypewriterEffectActive);
        yield return new WaitForSeconds(1f);

        dialoguePanel.SetActive(false);
        buttonsPanel.SetActive(false);
        shopManager.OpenShop();
    }

    public void OnShopClosed()
    {
        if (isTypewriterEffectActive)
        {
            StopAllCoroutines();
        }

        dialoguePanel.SetActive(true);
        buttonsPanel.SetActive(true);
        StartCoroutine(TypewriterEffect("Anything else you want?"));
    }

    private void OnSellClicked()
    {
        if (isTypewriterEffectActive)
        {
            StopAllCoroutines();
        }

        dialogueText.text = "";
        StartCoroutine(TypewriterEffect("What are you selling?"));
        Debug.Log("Sell button clicked!");
    }


    private void OnLeaveClicked()
    {
        if (isTypewriterEffectActive)
        {
            StopAllCoroutines();
        }
        buttonsPanel.SetActive(false);
        dialogueText.text = "";
        StartCoroutine(TypewriterEffect("Come back anytime."));
        StartCoroutine(EndInteraction());
    }

    private IEnumerator EndInteraction()
    {
        canInteract = false;
        yield return new WaitForSeconds(1.5f);
        isInteracting = false;
        StartCoroutine(FadeOutDialogueUI());
        
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        StartCoroutine(InteractionCooldown());
    }

    private IEnumerator InteractionCooldown()
    {
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }

    private IEnumerator TypewriterEffect(string text)
    {
        isTypewriterEffectActive = true;
        dialogueText.text = "";
        
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
        
        isTypewriterEffectActive = false;
    }
}
