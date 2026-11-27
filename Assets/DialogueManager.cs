using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private List<string> dialogues;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private bool isPreTutorial;
    [SerializeField] private bool isPreBoss;
    [SerializeField] private bool IsBegginingOfScene;
    [SerializeField] private bool pausesScene;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject dialogueMark;

    
    

    [System.Serializable]
    private class DialogueEvent
    {
        public int dialogueIndex;
        public UnityEvent onDialogueReached;
    }

    [Header("Events")]
    [SerializeField] private List<DialogueEvent> dialogueEvents;
    [SerializeField] private UnityEvent onDialogueEnd;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    private int currentIndex = -1;
    private Coroutine typingCoroutine;
    private bool isTyping;
    private bool movementLockedByDialogue;


    #region TriggerLogic

    

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerMovement == null)
                playerMovement = other.GetComponent<PlayerMovement>() ?? other.GetComponentInParent<PlayerMovement>();

            dialogueMark.SetActive(false);
            dialoguePanel.SetActive(true);
            StartDialogue();
        }
    }

    private void OnDisable()
    {
        UnlockPlayerMovement();
    }

    #endregion

    #region PUBLIC API
    private void Start()
    {
        if (IsBegginingOfScene)
        {
            StartDialogue();
        }
          else
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void StartDialogue()
    {
        LockPlayerMovement();

        if(pausesScene)
        {
            Time.timeScale = 0f; // Pausa el juego
        }
        NextDialogue();
    }
    public void NextDialogue()
    {
  
        if (isTyping)
        {
            CompleteTextInstantly();
            return;
        }

        currentIndex++;

        if (currentIndex >= dialogues.Count)
        {
            EndText();
            return;
        }

        TriggerDialogueEvent(currentIndex);

        typingCoroutine = StartCoroutine(TypeText(dialogues[currentIndex]));
    }

    #endregion

    #region TYPEWRITER

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void CompleteTextInstantly()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = dialogues[currentIndex];
        isTyping = false;
    }

    #endregion

    #region EVENTS

    private void TriggerDialogueEvent(int index)
    {
        foreach (var dialogueEvent in dialogueEvents)
        {
            if (dialogueEvent.dialogueIndex == index)
            {
                dialogueEvent.onDialogueReached?.Invoke();
            }
        }
    }

    private void EndText()
    {
        Time.timeScale = 1f; // Reanuda el juego si estaba pausado
        UnlockPlayerMovement();

        if (isPreTutorial)
        {
            SceneManager.LoadScene("Tutorial");
        }
        else if (isPreBoss)
        {
            SceneManager.LoadScene("LVL 4");
        }
     
        onDialogueEnd?.Invoke();
    }

    private void LockPlayerMovement()
    {
        if (movementLockedByDialogue)
            return;

        EnsurePlayerMovementReference();
        if (playerMovement == null)
            return;

        playerMovement.BeginWaterFlow();
        movementLockedByDialogue = true;
    }

    private void UnlockPlayerMovement()
    {
        if (!movementLockedByDialogue)
            return;

        movementLockedByDialogue = false;
        if (playerMovement != null)
            playerMovement.EndWaterFlow();
    }

    private void EnsurePlayerMovementReference()
    {
        if (playerMovement != null)
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerMovement = playerObj.GetComponent<PlayerMovement>();
    }

    #endregion
}