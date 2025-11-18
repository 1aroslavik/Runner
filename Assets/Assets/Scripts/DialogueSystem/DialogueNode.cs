using UnityEngine;

// Эта строка отвечает за появление "Dialogue Node" в меню
[CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    // Убедись, что скрипты DialogueLine и DialogueOption
    // тоже существуют и в них нет ошибок

    public DialogueLine[] lines; 
    public DialogueOption[] options; 
}