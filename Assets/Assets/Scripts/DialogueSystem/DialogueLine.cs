using UnityEngine;

// [System.Serializable] значит, что мы сможем видеть это в инспекторе
[System.Serializable]
public class DialogueLine
{
    public string characterName; // Имя персонажа
    [TextArea(3, 10)] // Делает поле для текста большим
    public string text;          // Текст реплики
}