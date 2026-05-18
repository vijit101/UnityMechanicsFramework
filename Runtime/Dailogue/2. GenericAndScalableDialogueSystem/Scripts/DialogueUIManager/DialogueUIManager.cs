using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Transform choicePanel;
    public GameObject choiceButtonPrefab;

    private List<GameObject> currentChoiceButtons = new List<GameObject>();

    private IEnumerator AutoSelectOnlyChoice(string choiceText, System.Action<int> onChoiceSelected)
    {
        yield return new WaitForSeconds(3.0f);

        onChoiceSelected?.Invoke(0);
    }

    void Start()
    {
        dialoguePanel.SetActive(false);
    }


    // Call this method to show a line of dialogue
    public void ShowDialogue(string speaker, string text, List<string> choices, System.Action<int> onChoiceSelected, bool showEndButton)
    {
        dialoguePanel.SetActive(true);

        speakerText.text = speaker;
        dialogueText.text = text;

        ClearChoices();

        // AUTO-ADVANCE if only one choice
        if (choices.Count == 1)
        {
            StartCoroutine(AutoSelectOnlyChoice(choices[0], onChoiceSelected));
            return;
        }

        // Player selects from multiple choices
        for (int i = 0; i < choices.Count; i++)
        {
            int choiceIndex = i;
            GameObject choiceButtonObj = Instantiate(choiceButtonPrefab, choicePanel);
            TextMeshProUGUI choiceText = choiceButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            choiceText.text = choices[i];

            Button button = choiceButtonObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                onChoiceSelected?.Invoke(choiceIndex);
            });

            currentChoiceButtons.Add(choiceButtonObj);
        }

        if (showEndButton)
        {
            displayEndButton();
        }
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        ClearChoices();
    }

    private void ClearChoices()
    {
        foreach (GameObject btn in currentChoiceButtons)
        {
            Destroy(btn);
        }
        currentChoiceButtons.Clear();
    }

    private void displayEndButton()
    {
        GameObject endButtonObj = Instantiate(choiceButtonPrefab, choicePanel);
        TextMeshProUGUI endText = endButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        endText.text = "Leave";

        Button button = endButtonObj.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            HideDialogue();
        });

        currentChoiceButtons.Add(endButtonObj);
    }
}
