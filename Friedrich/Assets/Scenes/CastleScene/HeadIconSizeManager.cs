using UnityEngine;
using UnityEngine.SceneManagement;

public class HeadIconSizeManager : MonoBehaviour
{
    [Header("Head Icon Transform")]
    [SerializeField] private Transform headIcon;

    [Header("Scene-specific Head Sizes")]
    [SerializeField] private Vector3 castleEntranceSize = new Vector3(2.36f, 2.36f, 2.36f);
    [SerializeField] private Vector3 mainCastleSize = new Vector3(2.36f, 2.36f, 2.36f);
    [SerializeField] private Vector3 defaultSize = new Vector3(4.34f, 4.34f, 4.34f);
    [SerializeField] private Vector3 sampleSceneSize = new Vector3(8.559783f, 8.559783f, 8.559783f);

    private void Start()
    {
        UpdateHeadSize(SceneManager.GetActiveScene().name);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateHeadSize(scene.name);
    }

    private void UpdateHeadSize(string sceneName)
    {
        if (sceneName == "CastleEntrance" || sceneName == "MainCastle")
        {
            headIcon.localScale = castleEntranceSize;
        }
        else if (sceneName == "SampleScene")
        {
            headIcon.localScale = sampleSceneSize;
        }
        else
        {
            headIcon.localScale = defaultSize;
        }
    }
}
