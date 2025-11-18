using UnityEngine;
using UnityEngine.UI; // Нужно для работы с UI

public class DialogueTrigger : MonoBehaviour
{
    [Header("Настройки Диалога")]
    public DialogueConversation[] conversations;

    [Header("UI Кнопка")]
    public GameObject talkButton; // Сюда перетащена кнопка (или сам Canvas)

    private void Start()
    {
        // --- НОВАЯ ЧАСТЬ: ЧИНИМ КАМЕРУ ---
        
        // 1. Пытаемся найти Canvas внутри этого NPC (в детях)
        Canvas myCanvas = GetComponentInChildren<Canvas>();
        
        // 2. Если нашли Canvas, ищем главную камеру и привязываем её
        if (myCanvas != null)
        {
            myCanvas.worldCamera = Camera.main;
        }
        // ---------------------------------

        // На старте прячем кнопку
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