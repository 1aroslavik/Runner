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
        
        // --- ИСПРАВЛЕНИЕ: ИСПОЛЬЗУЕМ GAMEMANAER ДЛЯ ПРОГРЕССА ---
        int runCount = 0;
        
        if (GameStateManager.Instance != null)
        {
            // Получаем счетчик смертей (0 при первом прохождении, 1 при первом респавне и т.д.)
            runCount = GameStateManager.Instance.DeathCount; 
        }
        else
        {
            Debug.LogError("❌ GameStateManager не найден! Прогресс диалогов невозможен.");
            return;
        }
        // --------------------------------------------------------

        if (conversations.Length == 0) return;

        // Выбираем диалог по остатку деления: (0, 1, 2, 0, 1, 2...)
        DialogueConversation conversationToPlay = conversations[runCount % conversations.Length];
        
        // Проверяем, что выбранный Conversation существует
        if (conversationToPlay == null)
        {
            Debug.LogError($"❌ DialogueTrigger: Conversation для индекса {runCount % conversations.Length} (прохождение {runCount + 1}) не назначен в массиве!");
            return;
        }
        
        DialogueManager.Instance.StartDialogue(conversationToPlay);
    }
}