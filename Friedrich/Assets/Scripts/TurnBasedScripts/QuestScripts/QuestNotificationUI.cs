using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class QuestNotificationUI : MonoBehaviour
{
    public static QuestNotificationUI Instance { get; private set; }
    
    [Header("UI References")]
    public CanvasGroup notificationPanel;
    public TextMeshProUGUI notificationText;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float displayDuration = 2.5f;
    public float fadeOutDuration = 0.5f;
    private Queue<string> notificationQueue = new Queue<string>();
    private bool isShowingNotification = false;

    public static QuestManager questManager;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            notificationPanel.alpha = 0f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowNotification(string message)
    {
        if (!ShouldShowNotification()) return;
        
        notificationQueue.Enqueue(message);
        if (!isShowingNotification)
        {
            StartCoroutine(ProcessNotificationQueue());
        }
    }

    private bool ShouldShowNotification()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return currentScene != "BattleScene";
    }

    public void ShowQuestStartNotification(QuestSO quest)
    {
        if (!ShouldShowNotification()) return;
        
        Debug.Log($"[QuestNotificationUI] Showing start notification for quest: {quest.title}");
        notificationQueue.Enqueue($"Started Quest: {quest.title}");
        
        if (!isShowingNotification)
        {
            StartCoroutine(ProcessNotificationQueue());
        }
    }



    public void ShowQuestCompletedNotification(QuestSO quest)
    {
        if (!ShouldShowNotification()) return;
        
        Debug.Log($"[QuestNotificationUI] Showing completion notification for quest: {quest.title}");
        notificationQueue.Enqueue($"Quest Completed: {quest.title}");
        
        if (!isShowingNotification)
        {
            StartCoroutine(ProcessNotificationQueue());
        }
    }

    public void ShowSegmentCompletedNotification(QuestSO quest, QuestSegment segment)
    {
        if (!ShouldShowNotification()) return;
        
        Debug.Log($"[QuestNotificationUI] Showing segment completion for quest: {quest.title}, segment: {segment.segmentName}");
        notificationQueue.Enqueue($"Completed: {segment.segmentName}");
        
        if (!isShowingNotification)
        {
            StartCoroutine(ProcessNotificationQueue());
        }
    }

    public void ShowNewObjectiveNotification(QuestSegment segment)
    {
        if (!ShouldShowNotification()) return;
        if (segment.isCompleted) return; // Don't show notification if segment is already completed
        
        Debug.Log($"[QuestNotificationUI] Showing new objective: {segment.segmentName}");
        notificationQueue.Enqueue($"New Objective: {segment.segmentName}");
        
        if (!isShowingNotification)
        {
            StartCoroutine(ProcessNotificationQueue());
        }
    }


    private IEnumerator ProcessNotificationQueue()
    {
        isShowingNotification = true;
        
        while (notificationQueue.Count > 0)
        {
            string message = notificationQueue.Dequeue();
            yield return StartCoroutine(ShowNotificationCoroutine(message));
            yield return new WaitForSecondsRealtime(0.5f); // Small delay between notifications
        }
        
        isShowingNotification = false;
    }

    private IEnumerator ShowNotificationCoroutine(string message)
    {
        notificationText.text = message;
        
        // Fade in
        notificationPanel.DOFade(1f, fadeInDuration);
        yield return new WaitForSecondsRealtime(fadeInDuration);
        
        // Display duration
        yield return new WaitForSecondsRealtime(displayDuration);
        
        // Fade out
        notificationPanel.DOFade(0f, fadeOutDuration);
        yield return new WaitForSecondsRealtime(fadeOutDuration);
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "BattleScene")
        {
            string lastKilledEnemyType = PlayerPrefs.GetString("LastKilledEnemyType", "");
            if (!string.IsNullOrEmpty(lastKilledEnemyType))
            {
                CharacterType enemyType = (CharacterType)System.Enum.Parse(typeof(CharacterType), lastKilledEnemyType);
                QuestManager.Instance.OnEnemyKilled(enemyType);
                PlayerPrefs.DeleteKey("LastKilledEnemyType");
            }
        }
    }


}