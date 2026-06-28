using System.Collections;
using UnityEngine;

public sealed class SeoulPlayBgmManager : MonoBehaviour
{
    private enum BgmChannel
    {
        Music,
        Ambience
    }

    private static SeoulPlayBgmManager instance;

    private AudioSource musicAudioSource;
    private AudioSource ambienceAudioSource;
    private Coroutine musicFadeRoutine;
    private Coroutine ambienceFadeRoutine;

    public static void Play(AudioClip clip, float volume, float fadeDuration, bool restartIfSameClip)
    {
        if (clip == null)
        {
            return;
        }

        Instance.PlayInternal(BgmChannel.Music, clip, Mathf.Clamp01(volume), Mathf.Max(0f, fadeDuration), restartIfSameClip);
    }

    public static void PlayAmbience(AudioClip clip, float volume, float fadeDuration, bool restartIfSameClip)
    {
        float safeFadeDuration = Mathf.Max(0f, fadeDuration);

        if (clip == null)
        {
            if (instance != null)
            {
                instance.StopInternal(BgmChannel.Ambience, safeFadeDuration);
            }

            return;
        }

        Instance.PlayInternal(BgmChannel.Ambience, clip, Mathf.Clamp01(volume), safeFadeDuration, restartIfSameClip);
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
        EnsureAudioSource(BgmChannel.Music);
        EnsureAudioSource(BgmChannel.Ambience);
    }

    private void PlayInternal(BgmChannel channel, AudioClip clip, float targetVolume, float fadeDuration, bool restartIfSameClip)
    {
        AudioSource audioSource = EnsureAudioSource(channel);

        if (audioSource.clip == clip && audioSource.isPlaying && !restartIfSameClip)
        {
            audioSource.volume = targetVolume;
            return;
        }

        StopFadeRoutine(channel);

        if (fadeDuration <= 0f || !audioSource.isPlaying)
        {
            StartClip(audioSource, clip, targetVolume);
            return;
        }

        SetFadeRoutine(channel, StartCoroutine(FadeToClip(channel, audioSource, clip, targetVolume, fadeDuration)));
    }

    private void StopInternal(BgmChannel channel, float fadeDuration)
    {
        AudioSource audioSource = EnsureAudioSource(channel);

        StopFadeRoutine(channel);

        if (!audioSource.isPlaying)
        {
            return;
        }

        if (fadeDuration <= 0f)
        {
            StopClip(audioSource);
            return;
        }

        SetFadeRoutine(channel, StartCoroutine(FadeOut(channel, audioSource, fadeDuration)));
    }

    private AudioSource EnsureAudioSource(BgmChannel channel)
    {
        AudioSource audioSource = GetAudioSource(channel);

        if (audioSource != null)
        {
            return audioSource;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        SetAudioSource(channel, audioSource);

        return audioSource;
    }

    private AudioSource GetAudioSource(BgmChannel channel)
    {
        return channel == BgmChannel.Music ? musicAudioSource : ambienceAudioSource;
    }

    private void SetAudioSource(BgmChannel channel, AudioSource audioSource)
    {
        if (channel == BgmChannel.Music)
        {
            musicAudioSource = audioSource;
            return;
        }

        ambienceAudioSource = audioSource;
    }

    private Coroutine GetFadeRoutine(BgmChannel channel)
    {
        return channel == BgmChannel.Music ? musicFadeRoutine : ambienceFadeRoutine;
    }

    private void SetFadeRoutine(BgmChannel channel, Coroutine routine)
    {
        if (channel == BgmChannel.Music)
        {
            musicFadeRoutine = routine;
            return;
        }

        ambienceFadeRoutine = routine;
    }

    private void StopFadeRoutine(BgmChannel channel)
    {
        Coroutine routine = GetFadeRoutine(channel);

        if (routine == null)
        {
            return;
        }

        StopCoroutine(routine);
        SetFadeRoutine(channel, null);
    }

    private static void StartClip(AudioSource audioSource, AudioClip clip, float volume)
    {
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    private static void StopClip(AudioSource audioSource)
    {
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.volume = 0f;
    }

    private IEnumerator FadeToClip(BgmChannel channel, AudioSource audioSource, AudioClip clip, float targetVolume, float duration)
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
        SetFadeRoutine(channel, null);
    }

    private IEnumerator FadeOut(BgmChannel channel, AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        StopClip(audioSource);
        SetFadeRoutine(channel, null);
    }
}
