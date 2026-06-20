using HeneGames.DialogueSystem;
using UnityEngine;

public class BirdKeyDialogueReward : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private ScenePortal portal;

    private bool rewardGranted;

    private void Awake()
    {
        if (dialogueManager == null)
        {
            dialogueManager = GetComponent<DialogueManager>();
        }
    }

    private void OnEnable()
    {
        if (dialogueManager != null)
        {
            dialogueManager.endDialogueEvent.AddListener(GrantKey);
        }
    }

    private void OnDisable()
    {
        if (dialogueManager != null)
        {
            dialogueManager.endDialogueEvent.RemoveListener(GrantKey);
        }
    }

    private void GrantKey()
    {
        if (rewardGranted || portal == null)
            return;

        rewardGranted = true;
        portal.GrantKey();

        // This event is one-shot, so prevent the dialogue from starting again.
        dialogueManager.enabled = false;
    }
}
