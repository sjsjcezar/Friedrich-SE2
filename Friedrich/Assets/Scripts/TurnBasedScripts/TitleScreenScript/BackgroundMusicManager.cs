using UnityEngine;
using UnityEngine.SceneManagement; // For detecting scene changes
using System.Collections;

public class BackgroundMusicManager : MonoBehaviour
{
    public AudioClip backgroundMusic;         // Assign your audio clip in the Inspector
    private AudioSource audioSource;
    public float fadeDuration = 2.0f;         // Duration of the fade-out effect
    private bool isFadingOut = false;         // To track whether the music is fading out

    private void Awake()
    {
        // Ensure only one instance of this music manager exists
        if (FindObjectsOfType<BackgroundMusicManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        // Set this object to not be destroyed when loading new scenes
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Set up the audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true; // Loop the music
        audioSource.Play();      // Start playing the music

        // Check if we're starting on a scene where the music should play
        HandleMusicForScene(SceneManager.GetActiveScene().name);

        // Subscribe to scene change events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Handle whether the music should play or stop based on the scene
        HandleMusicForScene(scene.name);
    }

    private void HandleMusicForScene(string sceneName)
    {
        // Replace "TitleScreen" with the actual name of your title screen scene
        if (sceneName == "TitleScreen")
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play(); // Play the music on the title screen
            }
        }
        else
        {
            // Stop music when not in the title screen
            audioSource.Stop();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene change events when destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Call this method when the player presses the "Start" button
    public void StartGame()
    {
        if (!isFadingOut)
        {
            StartCoroutine(FadeOutMusic());
        }
    }

    // Coroutine to handle the fade-out effect
    private IEnumerator FadeOutMusic()
    {
        isFadingOut = true;
        float startVolume = audioSource.volume;

        // Gradually reduce the volume over time
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        // Stop the audio and reset the volume for future use
        audioSource.Stop();
        audioSource.volume = startVolume;
        isFadingOut = false;
    }
}
