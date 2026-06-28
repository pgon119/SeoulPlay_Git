using UnityEngine;

[DisallowMultipleComponent]
public sealed class SeoulPlaySceneBgm : MonoBehaviour
{
    [Header("Music BGM")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

    [Header("Ambience BGM")]
    [SerializeField] private AudioClip ambienceClip;
    [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.8f;

    [Header("Fade")]
    [SerializeField, Min(0f)] private float fadeDuration = 0.5f;
    [SerializeField] private bool restartIfSameClip;

    private void Start()
    {
        SeoulPlayBgmManager.Play(musicClip, volume, fadeDuration, restartIfSameClip);
        SeoulPlayBgmManager.PlayAmbience(ambienceClip, ambienceVolume, fadeDuration, restartIfSameClip);
    }
}
