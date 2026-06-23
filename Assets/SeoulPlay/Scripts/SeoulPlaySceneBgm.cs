using UnityEngine;

[DisallowMultipleComponent]
public sealed class SeoulPlaySceneBgm : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;
    [SerializeField, Min(0f)] private float fadeDuration = 0.5f;
    [SerializeField] private bool restartIfSameClip;

    private void Start()
    {
        SeoulPlayBgmManager.Play(musicClip, volume, fadeDuration, restartIfSameClip);
    }
}
