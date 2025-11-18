using UnityEngine;

// И ВОТ ЭТА СТРОЧКА
[CreateAssetMenu(fileName = "New Conversation", menuName = "Dialogue/Conversation")]
public class DialogueConversation : ScriptableObject
{
    public DialogueNode startNode;
}