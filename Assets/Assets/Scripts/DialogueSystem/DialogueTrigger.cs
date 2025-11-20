using UnityEngine;
using UnityEngine.UI; // Обязательно для Canvas

public class DialogueTrigger : MonoBehaviour
{
    [Header("Настройки Диалога")]
    public DialogueConversation[] conversations;

    [Header("UI Кнопка")]
    public GameObject talkButton; // Сюда перетащена кнопка (или сам Canvas)

    private void Start()
    {
        // 1. Ищем Canvas в дочерних объектах этого NPC
        Canvas myCanvas = GetComponentInChildren<Canvas>();
        
        // --- ЧАСТЬ ДЛЯ ПРИВЯЗКИ КАМЕРЫ ---
        
        // Убедимся, что компонент Canvas найден
        if (myCanvas != null)
        {
            // Привязываем Главную Камеру, если она существует и помечена тегом "MainCamera"
            if (Camera.main != null)
            {
                myCanvas.worldCamera = Camera.main;
                Debug.Log("✔ Камера успешно привязана к Canvas NPC.");
            }
            else
            {
                // Это сработает, если забыли тег MainCamera на объекте камеры
                Debug.LogError("❌ Главная камера не найдена! Убедитесь, что камера на сцене имеет тег 'MainCamera'!");
            }
        } 
        else
        {
            // Это сработает, если забыли создать Canvas внутри префаба NPC
            Debug.LogWarning("⚠️ Не найден Canvas в дочерних объектах NPC! Кнопка работать не будет.");
        }
        // ---------------------------------

        // 2. На старте прячем кнопку
        if (talkButton != null)
            talkButton.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (talkButton != null) talkButton.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (talkButton != null) talkButton.SetActive(false);
        }
    }

    public void OnClickTalk()
    {
        if (talkButton != null) talkButton.SetActive(false);
        TriggerDialogue();
    }

    private void TriggerDialogue()
    {
        // Защита от отсутствия DialogueManager
        if (DialogueManager.Instance == null) 
        {
            Debug.LogError("❌ Нет DialogueManager на сцене!");
            return;
        }

        int runCount = PlayerPrefs.GetInt("CurrentRunCount", 0);
        if (conversations.Length == 0) return;

        DialogueConversation conversationToPlay = conversations[runCount % conversations.Length];
        DialogueManager.Instance.StartDialogue(conversationToPlay);
    }
}