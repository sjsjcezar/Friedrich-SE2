using UnityEngine;
using System.Collections;

public class TextSceneMusic : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource musicSource;
    [Range(0f, 1f)]
    public float musicVolume = 0.297f;
    public float fadeOutDuration = 2f;

    void Start()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeOutDuration);
            musicSource.volume = newVolume;
            yield return null;
        }

        musicSource.Stop();
    }
}