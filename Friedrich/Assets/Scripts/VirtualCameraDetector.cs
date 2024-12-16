using UnityEngine;
using Cinemachine;

public class VirtualCameraDetector : MonoBehaviour
{
    private static VirtualCameraDetector instance;
    private CinemachineVirtualCamera virtualCamera;

    void Awake()
    {
        // Singleton pattern to ensure only one camera controller exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    void Start()
    {
        FindAndFollowPlayer();
        AdjustCameraForScene();
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        FindAndFollowPlayer();
        AdjustCameraForScene();
    }

    void FindAndFollowPlayer()
    {
        if (PlayerMovement.instance != null)
        {
            virtualCamera.Follow = PlayerMovement.instance.transform;
            Debug.Log("Camera found and following player");
        }
        else
        {
            Debug.LogWarning("PlayerMovement instance not found!");
        }
    }

    void AdjustCameraForScene()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (currentScene == "CastleEntrance" || currentScene == "MainCastle")
        {
            virtualCamera.m_Lens.OrthographicSize = 4f;
            if (PlayerMovement.instance != null)
            {
                PlayerMovement.instance.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            }
        }
        else
        {
            virtualCamera.m_Lens.OrthographicSize = 8f;
            if (PlayerMovement.instance != null)
            {
                PlayerMovement.instance.transform.localScale = Vector3.one;
            }
        }
    }
}