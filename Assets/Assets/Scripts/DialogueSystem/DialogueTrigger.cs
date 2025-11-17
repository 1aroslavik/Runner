using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    // Сюда мы сложим ВСЕ варианты бесед
    public DialogueConversation[] conversations;

    private bool playerInRange = false;

    void Update()
    {
        // Эта часть не меняется - мы все так же жмем "E"
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TriggerDialogue();
        }
    }

    // !!! ВНИМАНИЕ: ИЗМЕНЕНИЕ ЗДЕСЬ !!!
    // Было: OnTriggerEnter(Collider other)
    // Стало: OnTriggerEnter2D(Collider2D other)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что в триггер вошел Игрок
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Игрок вошел в зону диалога (2D)");
            // Тут можно показать UI-подсказку "[E] Поговорить"
        }
    }

    // !!! И ЗДЕСЬ !!!
    // Было: OnTriggerExit(Collider other)
    // Стало: OnTriggerExit2D(Collider2D other)
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Игрок вышел из зоны диалога (2D)");
            // Тут можно скрыть UI-подсказку
        }
    }

    // Эта часть не меняется - логика та же
    public void TriggerDialogue()
    {
        int runCount = PlayerPrefs.GetInt("CurrentRunCount", 0);
        if (conversations.Length == 0) return;

        DialogueConversation conversationToPlay = conversations[runCount % conversations.Length];
        DialogueManager.Instance.StartDialogue(conversationToPlay);
    }
}