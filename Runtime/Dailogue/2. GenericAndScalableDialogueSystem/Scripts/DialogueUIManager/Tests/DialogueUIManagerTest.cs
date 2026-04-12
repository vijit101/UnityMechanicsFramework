using UnityEngine;
using System.Collections.Generic;

public class DialogueManagerTest : MonoBehaviour
{
    [SerializeField] private DialogueUIManager dialogueUI;
    [SerializeField] private DialogueData dialogueData;

    private DialogueNode currentNode;

    void Start()
    {
        if (dialogueData == null || dialogueData.rootNode == null)
        {
            Debug.LogError("Dialogue Data or Root Node is not assigned!");
            return;
        }

        currentNode = dialogueData.rootNode;
        ShowCurrentNode();
    }

    void ShowCurrentNode()
    {
        List<string> choiceTexts = new List<string>();
        bool isLastDialogue = false;

        if (currentNode.dialogueChoices.Count > 0)
        {
            foreach (DialogueChoice choice in currentNode.dialogueChoices)
            {
                choiceTexts.Add(choice.choiceText);
            }
        }
        else
        {
            isLastDialogue = true;
        }
        

        dialogueUI.ShowDialogue(
            currentNode.speakerName,
            currentNode.dialogueText,
            choiceTexts,
            OnChoiceSelected,
            isLastDialogue
        );
    }

    void OnChoiceSelected(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= currentNode.dialogueChoices.Count)
        {
            Debug.LogError("Invalid choice index");
            return;
        }

        DialogueChoice selectedChoice = currentNode.dialogueChoices[choiceIndex];

        if (selectedChoice.nextNode != null)
        {
            currentNode = selectedChoice.nextNode;
            ShowCurrentNode();
        }
        else
        {
            Debug.Log("Dialogue ended.");
            dialogueUI.HideDialogue();
        }
    }
}
