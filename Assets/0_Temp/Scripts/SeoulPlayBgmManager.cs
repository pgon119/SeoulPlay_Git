using System.Collections;
using UnityEngine;

public sealed class SeoulPlayBgmManager : MonoBehaviour
{
    private static SeoulPlayBgmManager instance;

    private AudioSource audioSource;
    private Coroutine fadeRoutine;

    public static void Play(AudioClip clip, float volume, float fadeDuration, bool restartIfSameClip)
    {
        if (clip == null)
        {
            return;
        }

        Instance.PlayInternal(clip, Mathf.Clamp01(volume), Mathf.Max(0f, fadeDuration), restartIfSameClip);
    }

    private static SeoulPlayBgmManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            var managerObject = new GameObject(nameof(SeoulPlayBgmManager));
            instance = managerObject.AddComponent<SeoulPlayBgmManager>();
            DontDestroyOnLoad(managerObject);
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureAudioSource();
    }

    private void PlayInternal(AudioClip clip, float targetVolume, float fadeDuration, bool restartIfSameClip)
    {
        EnsureAudioSource();

        if (audioSource.clip == clip && audioSource.isPlaying && !restartIfSameClip)
        {
            audioSource.volume = targetVolume;
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        if (fadeDuration <= 0f || !audioSource.isPlaying)
        {
            StartClip(clip, targetVolume);
            return;
        }

        fadeRoutine = StartCoroutine(FadeToClip(clip, targetVolume, fadeDuration));
    }

    private void EnsureAudioSource()
    {
        if (audioSource != null)
        {
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void StartClip(AudioClip clip, float volume)
    {
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private IEnumerator FadeToClip(AudioClip clip, float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;

        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.volume = 0f;
        audioSource.Play();

        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        fadeRoutine = null;
    }
}
