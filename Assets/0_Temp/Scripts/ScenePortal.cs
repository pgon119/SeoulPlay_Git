using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition;
using HeneGames.DialogueSystem;
using System.Collections.Generic;

public class ScenePortal : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "SeoulPlay_BossBattle";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private TransitionSettings transition;
    [SerializeField, Min(0f)] private float transitionStartDelay;
    [Header("Key Event")]
    [SerializeField] private DialogueManager lockedDialogue;
    [SerializeField] private GameObject keyItemUI;
    [SerializeField] private GameObject usePromptPrefab;
    [SerializeField] private Transform usePromptParent;

    private bool isLoading;
    private bool hasKey;
    private GameObject usePromptInstance;
    private readonly HashSet<Collider> playerColliders = new HashSet<Collider>();

    public bool HasKey => hasKey;

    private void Awake()
    {
        if (keyItemUI != null)
        {
            keyItemUI.SetActive(false);
        }

        if (usePromptPrefab != null)
        {
            usePromptInstance = Instantiate(usePromptPrefab, usePromptParent);
            usePromptInstance.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isLoading && hasKey && playerColliders.Count > 0 && UseInputDown())
        {
            LoadTargetScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading || !IsPlayer(other))
            return;

        playerColliders.Add(other);

        if (hasKey)
        {
            SetUsePromptVisible(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerColliders.Remove(other);
        if (playerColliders.Count == 0)
        {
            SetUsePromptVisible(false);
        }
    }

    public void GrantKey()
    {
        if (hasKey)
            return;

        hasKey = true;

        if (keyItemUI != null)
        {
            keyItemUI.SetActive(true);
        }

        if (lockedDialogue != null)
        {
            lockedDialogue.enabled = false;
        }

        if (playerColliders.Count > 0)
        {
            SetUsePromptVisible(true);
        }
    }

    private void LoadTargetScene()
    {
        isLoading = true;
        SetUsePromptVisible(false);
        HideDialogueUI();

        var transitionManager = transition != null ? TransitionManager.Instance() : null;
        if (transitionManager != null)
        {
            transitionManager.Transition(targetSceneName, transition, transitionStartDelay);
            return;
        }

        SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
    }

    private static void HideDialogueUI()
    {
        if (DialogueUI.instance == null)
            return;

        DialogueUI.instance.ClearText();
        DialogueUI.instance.ShowInteractionUI(false);
    }

    private void SetUsePromptVisible(bool visible)
    {
        if (usePromptInstance != null && usePromptInstance.activeSelf != visible)
        {
            usePromptInstance.SetActive(visible);
        }
    }

    private static bool UseInputDown()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
            return true;

        try
        {
            return Input.GetButtonDown("A");
        }
        catch (UnityException)
        {
            return false;
        }
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
