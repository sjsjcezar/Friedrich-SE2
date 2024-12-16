using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using static DialogueSO;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }
    [Header("UI References")]
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueOptionsPanel;
    public Button dialogueOptionPrefab;
    public TextMeshProUGUI interactionInstructionsText;
    public float typewriterSpeed = 0.05f;

    [Header("Dialogue Box Settings")]
    public Image dialogueBoxBackground;
    public float dialogueBoxPadding = 20f;
    public Color dialogueBoxColor = new Color(0, 0, 0, 0.8f);
    public Image npcImage;

    private DialogueSO currentDialogue;
    private DialogueNode currentNode;
    private PlayerMovement playerMovement;
    private bool isTyping = false;
    private string currentFullText = "";
    private Queue<DialogueContentSO.DialogueLine> pendingDialogueLines = new Queue<DialogueContentSO.DialogueLine>();
    private DialogueNode returnToNode;
    private List<Button> currentButtons = new List<Button>();

    [Header("Audio Settings")]
    private AudioSource voiceSource;
    public float voiceFadeOutTime = 0.1f;

    private QuestManager questManager;
    public Color highlightColor = new Color(0.596f, 0.984f, 0.596f, 1f);
    private Coroutine currentVoiceFade;

    [Header("UI Colors")] 
    public Color questButtonColor = new Color(1f, 0.7f, 0.7f, 1f); 

    private NPCSO currentNPC;
    private bool isShowingFarewell = false;
    public bool canInteract = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        DisableAllUI();
        playerMovement = PlayerMovement.instance;
        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
    }

    private void Update()
    {
        if (isShowingFarewell && Input.GetKeyDown(KeyCode.E))
        {
            isShowingFarewell = false;
            canInteract = false;  // Prevent immediate re-interaction
            
            // Only stop voice if we're not on the last line
            if (pendingDialogueLines.Count > 0)
            {
                StopCurrentVoice();
            }

            if (dialogueBoxBackground != null)
            {
                StartCoroutine(FadeOut(dialogueBoxBackground));
                StartCoroutine(FadeOut(npcImage));
            }
            
            currentNode?.onDialogueComplete?.Invoke();
            
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
            NPCInteraction.playerIsInConversation = false;
            DisableAllUI();

            // Re-enable interaction after a short delay
            StartCoroutine(EnableInteractionAfterDelay());
        }
            
        if (NPCInteraction.playerIsInConversation && Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                // Complete current text immediately
                StopAllCoroutines();
                dialogueText.text = currentFullText;
                    if (currentNode != null && !string.IsNullOrEmpty(currentNode.mainDialogue?.dialogueLines[0].textHighlight))
                    {
                        string highlightText = currentNode.mainDialogue.dialogueLines[0].textHighlight;
                        string coloredText = currentFullText.Replace(
                            highlightText, 
                            $"<color=#{ColorUtility.ToHtmlStringRGB(highlightColor)}>{highlightText}</color>"
                        );
                        dialogueText.text = coloredText;
                    }
                    else
                    {
                        dialogueText.text = currentFullText;
                    }
                isTyping = false;
                
                if (pendingDialogueLines.Count > 0)
                {
                    StopCurrentVoice();
                    StartCoroutine(PlayNextLineNextFrame());
                }
                else
                {
                    ShowDialogueOptions();
                }
            }
            else if (pendingDialogueLines.Count > 0)
            {
                // Only stop current voice if we're moving to next line
                StopCurrentVoice();
                ShowNextDialogueLine();
            }
            else
            {
                // Don't stop voice on last line
                ShowDialogueOptions();
            }
        }
    }

    private IEnumerator EnableInteractionAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);  // Half second delay
        canInteract = true;
    }

    private IEnumerator PlayNextLineNextFrame()
    {
        yield return null;  // Wait one frame
        if (pendingDialogueLines.Count > 0)
        {
            ShowNextDialogueLine();
        }
        else
        {
            ShowDialogueOptions();
        }
    }

    private DialogueContentSO GetRandomDialogue(List<DialogueContentSO> dialogueList)
    {
        if (dialogueList == null || dialogueList.Count == 0) return null;
        int randomIndex = Random.Range(0, dialogueList.Count);
        return dialogueList[randomIndex];
    }

    public void DisplayDialogue(DialogueSO dialogueSO, DialogueNode node, NPCSO npcSO = null)
    {
        if (dialogueSO == null || node == null)
        {
            Debug.LogError("DialogueSO or DialogueNode is null!");
            return;
        }

        StopAllCoroutines();
        dialogueText.text = "";
        pendingDialogueLines.Clear();
        isTyping = false;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        currentNPC = npcSO;

        if (npcImage != null && currentNPC != null && currentNPC.npcImage != null)
        {
            npcImage.sprite = currentNPC.npcImage;
            npcImage.gameObject.SetActive(true);
        }
        else
        {
            npcImage.gameObject.SetActive(false);
        }


        currentDialogue = dialogueSO;
        currentNode = node;

        dialogueOptionsPanel.gameObject.SetActive(true);
        npcNameText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        interactionInstructionsText.gameObject.SetActive(true);  

        interactionInstructionsText.text = "[E]";  

        // Animate dialogue box appearing
        if (dialogueBoxBackground != null)
        {
            dialogueBoxBackground.gameObject.SetActive(true);
            npcImage.gameObject.SetActive(true);  // Add this
            
            Color bgColor = dialogueBoxBackground.color;
            bgColor.a = 1f;
            dialogueBoxBackground.color = bgColor;
            
            Color npcImageColor = npcImage.color;
            npcImageColor.a = 1f;
            npcImage.color = npcImageColor;
            
            StartCoroutine(FadeIn(dialogueBoxBackground));
            StartCoroutine(FadeIn(npcImage));
        }
        ClearDialogueOptions();
        npcNameText.text = dialogueSO.npcName;

        // Queue up all dialogue lines
        pendingDialogueLines.Clear();

        var greeting = GetRandomDialogue(dialogueSO.greetings);
        if (greeting != null && greeting.dialogueLines != null && greeting.dialogueLines.Count > 0)
        {
            foreach (var line in greeting.dialogueLines)
            {
                if (line != null)
                {
                    pendingDialogueLines.Enqueue(line);
                }
            }
        }

        // Add null check for mainDialogue
        if (node.mainDialogue != null && node.mainDialogue.dialogueLines != null)
        {
            foreach (var line in node.mainDialogue.dialogueLines)
            {
                if (line != null)
                {
                    pendingDialogueLines.Enqueue(line);  // Now enqueueing the whole DialogueLine object
                }
            }
        }
        else
        {
            // If no dialogue lines, add a default one
      //      pendingDialogueLines.Enqueue(new DialogueContentSO.DialogueLine { text = "..." });
        //    Debug.LogWarning("No dialogue lines found in DialogueNode!");
        }

        // Hide options during dialogue
        dialogueOptionsPanel.SetActive(false);

        // Start displaying first line
        ShowNextDialogueLine();
    }

    private void DisableAllUI()
    {
        dialogueBoxBackground.gameObject.SetActive(false);
        dialogueOptionsPanel.gameObject.SetActive(false);
        npcNameText.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        interactionInstructionsText.gameObject.SetActive(false);
        npcImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeIn(Image image)
    {
        float duration = 0.3f;
        float elapsed = 0;
        Color startColor = image.color;
        startColor.a = 0;
        Color targetColor = startColor;
        targetColor.a = 1;
        image.color = startColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            image.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
    }


    private void ShowNextDialogueLine()
    {
        if (pendingDialogueLines.Count > 0)
        {
            var nextLine = pendingDialogueLines.Dequeue();
            // Only hide options panel if not in farewell
            if (!isShowingFarewell)
            {
                dialogueOptionsPanel.SetActive(false);
            }
            StartTypewriterEffect(nextLine);
        }
        else
        {
            ShowDialogueOptions();
        }
    }

    private void StartTypewriterEffect(DialogueContentSO.DialogueLine line)
    {
        if (line == null) return;
        
        isTyping = true;
        currentFullText = line.text;
        dialogueText.text = "";  // Clear text before starting new line

        // Play voice clip if available
        if (line.voiceClip != null)
        {
            voiceSource.clip = line.voiceClip;
            voiceSource.volume = 1f;
            voiceSource.Play();
        }

        // Start typewriter with highlight text
        StartCoroutine(TypeText(line.text, line.textHighlight));
    }

    private IEnumerator TypeText(string text, string highlightText = null)
    {
        dialogueText.text = "";
        isTyping = true;
        StringBuilder displayText = new StringBuilder();
        
        for (int i = 0; i < text.Length; i++)
        {
            displayText.Append(text[i]);
            
            // If we have highlight text, apply color to it in the full string
            if (!string.IsNullOrEmpty(highlightText))
            {
                string currentText = displayText.ToString();
                int highlightIndex = currentText.LastIndexOf(highlightText);
                if (highlightIndex != -1)
                {
                    dialogueText.text = currentText.Substring(0, highlightIndex) + 
                                    $"<color=#{ColorUtility.ToHtmlStringRGB(highlightColor)}>{highlightText}</color>" +
                                    currentText.Substring(highlightIndex + highlightText.Length);
                }
                else
                {
                    dialogueText.text = currentText;
                }
            }
            else
            {
                dialogueText.text = displayText.ToString();
            }
            
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
    }


    private void ShowDialogueOptions()
    {
        // Clear any existing buttons first
        ClearDialogueOptions();

        // Enable the panels
        dialogueOptionsPanel.SetActive(true);

        if (currentNode?.dialogueOptions != null)
        {
            // Create all dialogue options in the same panel
            foreach (var option in currentNode.dialogueOptions)
            {
                if (option != null)
                {
                    CreateDialogueButton(option, dialogueOptionsPanel.transform);
                }
            }
        }
        // Always add Leave button last
        CreateLeaveButton();
    }

    private void CreateDialogueButton(DialogueSO.DialogueOption option, Transform parent)
    {
        // Don't create button if it's been selected and isn't repeatable
        if (option.hasBeenSelected && !option.isRepeatable)
            return;

        Button optionButton = Instantiate(dialogueOptionPrefab, parent);
        TextMeshProUGUI buttonText = optionButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = option.optionText;
        currentButtons.Add(optionButton);

        if (option.isQuestRelated)
        {
            buttonText.text = option.optionText;
            buttonText.color = new Color(0f, 0.5f, 0f, 1f);
        }


        optionButton.onClick.AddListener(() => {
            // Hide options while dialogue is playing
            dialogueOptionsPanel.SetActive(false);

            // Execute quest-related action if any
            option.onSelectAction?.Invoke();
            
            if (option.dialogueContent != null)
            {
                pendingDialogueLines.Clear();
                foreach (var line in option.dialogueContent.dialogueLines)
                {
                    pendingDialogueLines.Enqueue(line);
                }
                ShowNextDialogueLine();

                // Store where to return after dialogue
                returnToNode = option.nextDialogue?.baseDialogue ?? currentNode;

                // Mark as selected
                option.hasBeenSelected = true;

                // If not repeatable, remove the button
                if (!option.isRepeatable)
                {
                    Destroy(optionButton.gameObject);
                    currentButtons.Remove(optionButton);
                }
            }
            else if (option.nextDialogue != null)
            {
                DisplayDialogue(option.nextDialogue, option.nextDialogue.baseDialogue);
            }
            else
            {
                CloseDialogue();
            }
        });
    }

    
    private void CreateLeaveButton()
    {
        Button leaveButton = Instantiate(dialogueOptionPrefab, dialogueOptionsPanel.transform);
        leaveButton.GetComponentInChildren<TextMeshProUGUI>().text = "Leave";
        currentButtons.Add(leaveButton);
        leaveButton.onClick.AddListener(CloseDialogue);
    }

    public void CloseDialogue()
    {
        // Show farewell dialogue if available
        if (currentDialogue != null)
        {
            var farewell = GetRandomDialogue(currentDialogue.farewells);
            if (farewell != null && farewell.dialogueLines != null && farewell.dialogueLines.Count > 0)
            {
                // First disable all UI
                dialogueOptionsPanel.SetActive(false);
                
                // Clear and prepare farewell
                pendingDialogueLines.Clear();
                dialogueText.text = "";  // Clear any existing text
                foreach (var line in farewell.dialogueLines)
                {
                    if (line != null)
                    {
                        pendingDialogueLines.Enqueue(line);
                    }
                }
                isShowingFarewell = true;
                ShowNextDialogueLine();
                return;
            }
        }

        // If no farewell dialogue, do the cleanup immediately
        StopCurrentVoice();
        if (dialogueBoxBackground != null)
        {
            StartCoroutine(FadeOut(dialogueBoxBackground));
            StartCoroutine(FadeOut(npcImage));
        }
        currentNode?.onDialogueComplete?.Invoke();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        NPCInteraction.playerIsInConversation = false;
        DisableAllUI();
    }
    
    private IEnumerator FadeOut(Image image)
    {
        float duration = 0.3f;
        float elapsed = 0;
        Color startColor = image.color;
        Color targetColor = startColor;
        targetColor.a = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            image.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        // Reset the opacity back to 1 before disabling
        Color resetColor = image.color;
        resetColor.a = 1f;
        image.color = resetColor;
        
        image.gameObject.SetActive(false);
    }

    private void StopCurrentVoice()
    {
        if (voiceSource.isPlaying)
        {
            // If there's an ongoing fade, stop it first
            if (currentVoiceFade != null)
            {
                StopCoroutine(currentVoiceFade);
                currentVoiceFade = null;
            }
            
            // Immediately stop and reset volume
            voiceSource.Stop();
            voiceSource.volume = 1f;  // Reset to full volume
        }
    }

    private void ClearDialogueOptions()
    {
        if (currentVoiceFade != null)
        {
            StopCoroutine(currentVoiceFade);
            currentVoiceFade = null;
        }
        
        foreach (var button in currentButtons)
        {
            Destroy(button.gameObject);
        }
        currentButtons.Clear();
    }

    public void RefreshDialogueOptions(DialogueNode node)
    {
        if (node == null) return;

        // Clear existing buttons
        ClearDialogueOptions();

        // Show new dialogue options
        if (node.dialogueOptions != null)
        {
            foreach (var option in node.dialogueOptions)
            {
                if (option != null)
                {
                    CreateDialogueButton(option, dialogueOptionsPanel.transform);
                }
            }
        }

        // Always add Leave button last
        CreateLeaveButton();

        // Show the options panel
        dialogueOptionsPanel.SetActive(true);
    }


}