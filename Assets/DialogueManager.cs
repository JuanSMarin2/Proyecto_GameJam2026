using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private List<string> dialogues;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private bool isPreBoss;

    [System.Serializable]
    private class DialogueEvent
    {
        public int dialogueIndex;
        public UnityEvent onDialogueReached;
    }

    [Header("Events")]
    [SerializeField] private List<DialogueEvent> dialogueEvents;
    [SerializeField] private UnityEvent onDialogueEnd;

    private int currentIndex = -1;
    private Coroutine typingCoroutine;
    private bool isTyping;

    #region PUBLIC API

    private void Start()
    {
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
        if (!isPreBoss)
        {
            SceneManager.LoadScene("Tutorial");
        }
        else
        {
            SceneManager.LoadScene("LVL 4");
        }
     
        onDialogueEnd?.Invoke();
    }

    #endregion
}