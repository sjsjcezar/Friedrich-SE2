using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VialController : MonoBehaviour
{
    public static VialController Instance;

    public GameObject vialUIPanel;
    public Image flaskHealImage, flaskManaImage;
    public TMP_Text counterForHeal, counterForMana, counter;
    public Button rightButtonMana, leftButtonMana;
    public Button rightButtonHeal, leftButtonHeal;
    public Button leaveButton;

    public TMP_Text allocateText;

    public int maxVials = 3;
    public int remainingVialCount;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        maxVials = 3;
        vialUIPanel.SetActive(false);

        GlobalVariables.healVialCount = PlayerPrefs.GetInt("HealVialCount", 0);
        GlobalVariables.manaVialCount = PlayerPrefs.GetInt("ManaVialCount", 0);
        remainingVialCount = PlayerPrefs.GetInt("RemainingVialCount", maxVials);

        vialUIPanel.SetActive(false);

        InitializeUI();
        LoadVialCounts();

        rightButtonHeal.onClick.AddListener(() => AdjustVialCount(ref GlobalVariables.healVialCount, 1));
        leftButtonHeal.onClick.AddListener(() => AdjustVialCount(ref GlobalVariables.healVialCount, -1));
        rightButtonMana.onClick.AddListener(() => AdjustVialCount(ref GlobalVariables.manaVialCount, 1));
        leftButtonMana.onClick.AddListener(() => AdjustVialCount(ref GlobalVariables.manaVialCount, -1));
        leaveButton.onClick.AddListener(CloseVialUI);
    }

    private void InitializeUI()
    {
        vialUIPanel.SetActive(false);
        flaskHealImage.gameObject.SetActive(false);
        flaskManaImage.gameObject.SetActive(false);
        counter.gameObject.SetActive(false);
        counterForHeal.gameObject.SetActive(false);
        counterForMana.gameObject.SetActive(false);
        allocateText.gameObject.SetActive(false);
        rightButtonHeal.gameObject.SetActive(false);
        leftButtonHeal.gameObject.SetActive(false);
        rightButtonMana.gameObject.SetActive(false);
        leftButtonMana.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
    }

    public void OpenVialUI()
    {
        vialUIPanel.SetActive(true);
        flaskHealImage.gameObject.SetActive(true);
        flaskManaImage.gameObject.SetActive(true);
        counter.gameObject.SetActive(true);
        counterForHeal.gameObject.SetActive(true);
        counterForMana.gameObject.SetActive(true);
        allocateText.gameObject.SetActive(true);
        rightButtonHeal.gameObject.SetActive(true);
        leftButtonHeal.gameObject.SetActive(true);
        rightButtonMana.gameObject.SetActive(true);
        leftButtonMana.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);

        UpdateCounters();
    }

    public void CloseVialUI()
    {
        vialUIPanel.SetActive(false);
        InitializeUI();
        SaveVialCounts();
        GlobalVariables.currentHealVialCount = GlobalVariables.healVialCount;
        GlobalVariables.currentManaVialCount = GlobalVariables.manaVialCount;
        
        // Return to statue UI
        StatueController.Instance.ShowStatueUI();
    }

    public void AdjustVialCount(ref int vialCount, int amount)
    {
        if (amount > 0 && (GlobalVariables.healVialCount + GlobalVariables.manaVialCount < maxVials))
        {
            vialCount++;
            remainingVialCount--;
        }
        else if (amount < 0 && vialCount > 0)
        {
            vialCount--;
            remainingVialCount++;
        }
        UpdateCounters();
    }

    public void UpdateCounters()
    {
        counterForHeal.text = GlobalVariables.healVialCount.ToString();
        counterForMana.text = GlobalVariables.manaVialCount.ToString();
        counter.text = remainingVialCount.ToString();
    }

    private void LoadVialCounts()
    {
        UpdateCounters();
    }

    private void SaveVialCounts()
    {
        PlayerPrefs.SetInt("HealVialCount", GlobalVariables.healVialCount);
        PlayerPrefs.SetInt("ManaVialCount", GlobalVariables.manaVialCount);
        PlayerPrefs.SetInt("RemainingVialCount", remainingVialCount);
        PlayerPrefs.Save();
    }
}