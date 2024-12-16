using UnityEngine;

public class ProximityAudio : MonoBehaviour
{
    public AudioClip audioClip;
    public Transform player;

    [Header("Audio Settings")]
    public float maxVolume = 1.0f;
    public float minVolume = 0.0f;
    public float volumeChangeSpeed = 1.0f;

    [Header("Trigger Zones")]
    public BoxCollider2D[] highVolumeZones;
    public BoxCollider2D[] lowVolumeZones;

    private AudioSource audioSource;
    private float targetVolume;
    private bool playerInZone = false;

    private void Start()
    {
        SetupAudioSource();
        Debug.Log($"[ProximityAudio] Started on {gameObject.name}");
    }


    private void SetupAudioSource()
    {
        // Only create a new AudioSource if one doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log($"[ProximityAudio] Created new AudioSource on {gameObject.name}");
            
            // Only set these properties when creating a new AudioSource
            audioSource.clip = audioClip;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f;
            audioSource.volume = 0f;
            audioSource.playOnAwake = false;
            
            audioSource.Play();
            audioSource.Pause();
        }
        Debug.Log($"[ProximityAudio] AudioSource setup completed on {gameObject.name}");
    }
    private void OnEnable()
    {
        SetupAudioSource();
        Debug.Log($"[ProximityAudio] OnEnable called on {gameObject.name}");
    }

    private void Update()
    {
        if (player == null)
        {
            var allObjects = FindObjectsOfType<GameObject>(true);
            foreach (var obj in allObjects)
            {
                if (obj.scene.buildIndex == -1 && obj.name == "warriorPlayer")
                {
                    player = obj.transform;
                    Debug.Log("[ProximityAudio] Found warriorPlayer in DontDestroyOnLoad");
                    break;
                }
            }
        }

        if (audioSource == null)
        {
            Debug.LogWarning("[ProximityAudio] AudioSource was lost, reinitializing...");
            SetupAudioSource();
        }

        if (audioSource != null && player != null)
        {
            playerInZone = false;
            targetVolume = 0f;

            // Check high volume zones
            foreach (var zone in highVolumeZones)
            {
                if (zone != null && zone.bounds.Contains(player.position))
                {
                    targetVolume = maxVolume;
                    playerInZone = true;
                    Debug.Log($"[ProximityAudio] Player in high volume zone of {gameObject.name}, Volume: {targetVolume}");
                    break;
                }
            }

            // Check low volume zones
            foreach (var zone in lowVolumeZones)
            {
                if (zone != null && zone.bounds.Contains(player.position))
                {
                    targetVolume = minVolume;
                    playerInZone = true;
                    Debug.Log($"[ProximityAudio] Player in low volume zone of {gameObject.name}, Volume: {targetVolume}");
                    break;
                }
            }

            // Smoothly adjust volume
            float previousVolume = audioSource.volume;
            audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * volumeChangeSpeed);
            
            if (Mathf.Abs(previousVolume - audioSource.volume) > 0.01f)
            {
                Debug.Log($"[ProximityAudio] Volume changing on {gameObject.name}: {audioSource.volume:F2}");
            }

            // Handle audio playing state
            if (playerInZone && !audioSource.isPlaying)
            {
                audioSource.UnPause();
                Debug.Log($"[ProximityAudio] Unpausing audio on {gameObject.name}");
            }
            else if (!playerInZone && audioSource.isPlaying && audioSource.volume <= 0.01f)
            {
                audioSource.Pause();
                Debug.Log($"[ProximityAudio] Pausing audio on {gameObject.name}");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[ProximityAudio] Trigger Enter with {other.gameObject.name}");
        if (other.gameObject.name == "warriorPlayer")
        {
            playerInZone = true;
            Debug.Log($"[ProximityAudio] Player entered trigger zone of {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[ProximityAudio] Trigger Exit with {other.gameObject.name}");
        if (other.gameObject.name == "warriorPlayer")
        {
            playerInZone = false;
            Debug.Log($"[ProximityAudio] Player exited trigger zone of {gameObject.name}");
        }
    }

    private void OnDisable()
    {
        Debug.Log($"[ProximityAudio] OnDisable called on {gameObject.name}");
    }

    private void OnDestroy()
    {
        Debug.Log($"[ProximityAudio] OnDestroy called on {gameObject.name}");
    }
}