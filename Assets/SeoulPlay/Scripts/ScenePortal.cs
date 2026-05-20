using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition;

public class ScenePortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "SeoulPlay_BossBattle";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private TransitionSettings transition;
    [SerializeField, Min(0f)] private float transitionStartDelay;

    private bool isLoading;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading || !IsPlayer(other))
            return;

        isLoading = true;

        var transitionManager = transition != null ? TransitionManager.Instance() : null;
        if (transitionManager != null)
        {
            transitionManager.Transition(targetSceneName, transition, transitionStartDelay);
            return;
        }

        SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        if (other.CompareTag(playerTag))
            return true;

        Transform current = other.transform.parent;
        while (current != null)
        {
            if (current.CompareTag(playerTag))
                return true;

            current = current.parent;
        }

        return false;
    }
}
