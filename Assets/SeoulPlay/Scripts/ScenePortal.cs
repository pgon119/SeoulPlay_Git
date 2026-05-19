using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "SeoulPlay_BossBattle";
    [SerializeField] private string playerTag = "Player";

    private bool isLoading;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading || !IsPlayer(other))
            return;

        isLoading = true;
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
