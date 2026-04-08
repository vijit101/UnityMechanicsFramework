using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueTree", menuName = "Dialogue System/Dialogue tree")]
public class DialogueData : ScriptableObject
{
    public DialogueNode rootNode;
}
