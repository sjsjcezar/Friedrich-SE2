using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource battleMusicSource;
    public AudioSource skillEffectSource;
    
    [Header("Audio Clips")]
    public AudioClip battleMusic;
    public AudioClip rapidAssaultMusic; 
    public AudioClip slashSound;
    
    [Header("Transition Settings")]
    public float fadeSpeed = 1.0f;
    private bool isBattleMusicPlaying = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create audio sources if not assigned
            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();
            if (battleMusicSource == null)
                battleMusicSource = gameObject.AddComponent<AudioSource>();
            if (skillEffectSource == null)
                skillEffectSource = gameObject.AddComponent<AudioSource>();
            
            // Configure audio sources
            musicSource.loop = true;
            battleMusicSource.loop = true;
            skillEffectSource.loop = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayRapidAssaultMusic()
    {
        if (rapidAssaultMusic != null)
        {
            skillEffectSource.clip = rapidAssaultMusic;
            skillEffectSource.loop = false;
            skillEffectSource.Play();
        }
    }

    public void PlaySlashSound()
    {
        if (slashSound != null)
        {
            // Play the slash sound effect without interrupting the current sound
            skillEffectSource.PlayOneShot(slashSound, 0.7f);
        }
    }

    public void StartBattleMusic()
    {
        if (!isBattleMusicPlaying)
        {
            isBattleMusicPlaying = true;
            StartCoroutine(FadeMusicTransition());
        }
    }

    public void StopBattleMusic()
    {
        if (isBattleMusicPlaying)
        {
            isBattleMusicPlaying = false;
            StartCoroutine(FadeMusicTransition());
        }
    }

    private IEnumerator FadeMusicTransition()
    {
        float musicVolume = musicSource.volume;
        float battleVolume = battleMusicSource.volume;

        if (isBattleMusicPlaying)
        {
            // Start battle music at 0 volume
            battleMusicSource.clip = battleMusic;
            battleMusicSource.volume = 1;
            battleMusicSource.Play();

            // Crossfade
            while (battleMusicSource.volume < 1)
            {
                musicSource.volume = Mathf.MoveTowards(musicSource.volume, 0, fadeSpeed * Time.deltaTime);
                battleMusicSource.volume = Mathf.MoveTowards(battleMusicSource.volume, 1, fadeSpeed * Time.deltaTime);
                yield return null;
            }

            musicSource.Stop();
        }
        else
        {
            // Start normal music at 0 volume if it's not playing
            if (!musicSource.isPlaying)
            {
                musicSource.volume = 0;
                musicSource.Play();
            }

            // Crossfade back
            while (musicSource.volume < 1)
            {
                battleMusicSource.volume = Mathf.MoveTowards(battleMusicSource.volume, 0, fadeSpeed * Time.deltaTime);
                musicSource.volume = Mathf.MoveTowards(musicSource.volume, 1, fadeSpeed * Time.deltaTime);
                yield return null;
            }

            battleMusicSource.Stop();
        }
    }
}