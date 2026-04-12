using UnityEngine;
using System.Collections.Generic;

public class DialogueTester : MonoBehaviour
{
    [SerializeField] public DialogueUIManager dialogueUI;

    void Start()
    {
        List<string> choices = new List<string> { "Yes", "No", "Maybe" };

        dialogueUI.ShowDialogue("Guard", "Halt! Who goes there?", choices, OnChoiceSelected, false);
    }

    void OnChoiceSelected(int index)
    {
        Debug.Log("Player selected choice: " + index);
    }
}
