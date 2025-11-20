using UnityEngine;
using TMPro; // Для работы с TextMeshPro
using UnityEngine.UI; // Для работы с Button
using System.Collections;
using System.Collections.Generic; // Для работы с Queue

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    // --- Ссылки на UI (которые мы создали) ---
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Transform optionsContainer;
    public GameObject optionButtonPrefab; // Наш префаб кнопки

    // --- НОВЫЕ ПОЛЯ ДЛЯ ПОРТРЕТА И СКОРОСТИ ---
    public UnityEngine.UI.Image portraitImage; // Ссылка на новый компонент UI
    public float textAnimationSpeed = 0.02f;   // Скорость печати
    // ------------------------------------------

    // --- Состояние ---
    private DialogueNode currentNode;
    private Queue<DialogueLine> lineQueue; // Очередь реплик
    private bool isDialogueActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        lineQueue = new Queue<DialogueLine>();
        // ВАЖНО: Убедитесь, что панель диалога выключена на старте
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void StartDialogue(DialogueConversation conversation)
    {
        if (isDialogueActive) return;

        // --- ПРОВЕРКА ДАННЫХ ---
        if (conversation == null || conversation.startNode == null)
        {
            Debug.LogError("❌ Ошибка запуска: Conversation или StartNode равен NULL. Проверьте ассеты!");
            return;
        }
        // -----------------------
        
        isDialogueActive = true;
        
        // Проверяем, что панель подключена, прежде чем ее включать
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // Тут можно поставить игру на паузу (Time.timeScale = 0)
        // или отключить управление игроком

        ShowNode(conversation.startNode);
    }

    private void ShowNode(DialogueNode node)
    {
        currentNode = node;
        lineQueue.Clear(); // Очищаем очередь

        // --- ПРОВЕРКА: ЕСЛИ ЛИНИЙ ВООБЩЕ НЕТ, ВЫХОДИМ ---
        if (node.lines == null || node.lines.Length == 0)
        {
            Debug.LogWarning("⚠️ Диалог пустой! Перехожу к вариантам.");
            ShowOptions();
            return;
        }
        // ----------------------------------------------

        // Заполняем очередь всеми репликами из этого узла
        foreach (DialogueLine line in node.lines)
        {
            lineQueue.Enqueue(line);
        }

        // --- ДИАГНОСТИКА (1): СКОЛЬКО ДАННЫХ ПОЛУЧЕНО ---
        Debug.Log($"[DEBUG] Node received. Lines in queue: {lineQueue.Count}.");
        // ------------------------------------------------

        // Очищаем старые кнопки
        if (optionsContainer != null)
        {
            foreach (Transform child in optionsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        // Если в очереди есть реплики, показываем
        if (lineQueue.Count > 0)
        {
            DialogueLine line = lineQueue.Dequeue();
            
            // --- ПРОВЕРКА СУЩЕСТВОВАНИЯ UI ---
            if (nameText != null) nameText.text = line.characterName;

            // ОБНОВЛЕНИЕ: ПОКАЗЫВАЕМ ПОРТРЕТ
            if (portraitImage != null && line.portrait != null)
            {
                portraitImage.sprite = line.portrait;
            } 
            // -------------------------------------

            StopAllCoroutines(); // Останавливаем прошлую "печатающую машинку"
            StartCoroutine(TypeSentence(line.text));
        }
        // Если реплики кончились, показываем кнопки
        else
        {
            ShowOptions();
        }
    }

    // Эффект "пишущей машинки"
    IEnumerator TypeSentence(string sentence)
    {
        // 🚨 ПРОВЕРКА: Если строка текста пустая, не запускаем корутину
        if (string.IsNullOrEmpty(sentence) || dialogueText == null) yield break;
        
        dialogueText.text = "";
        
        // Проверка: скорость не должна быть нулевой
        float speed = textAnimationSpeed > 0 ? textAnimationSpeed : 0.02f;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(speed); 
        }
    }

    private void ShowOptions()
    {
        // Если вариантов ответа нет, просто закрываем диалог
        if (currentNode.options.Length == 0)
        {
            // Убедимся, что есть куда вставлять кнопку
            if (optionButtonPrefab == null || optionsContainer == null)
            {
                EndDialogue();
                return;
            }
            
            // Добавляем кнопку "Завершить" для наглядности
            GameObject buttonGO = Instantiate(optionButtonPrefab, optionsContainer);
            // ПРОВЕРКА: что есть компонент TMP для текста на кнопке
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                 buttonText.text = "Завершить";
            }
            
            Button buttonComponent = buttonGO.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(EndDialogue);
            }
            return;
        }

        // Создаем кнопки для каждого варианта
        foreach (DialogueOption option in currentNode.options)
        {
            if (optionButtonPrefab == null || optionsContainer == null) break;
            
            GameObject buttonGO = Instantiate(optionButtonPrefab, optionsContainer);
            
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                // Тут мы используем TextMeshProUGUI, который установлен в префабе кнопки
                buttonText.text = option.optionText;
            }

            Button buttonComponent = buttonGO.GetComponent<Button>();
            if (buttonComponent != null)
            {
                DialogueNode nextNode = option.nextNode;
                buttonComponent.onClick.AddListener(() => SelectOption(nextNode));
            }
        }
    }

    public void SelectOption(DialogueNode nextNode)
    {
        if (nextNode != null)
        {
            ShowNode(nextNode);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        
        // --- ДИАГНОСТИКА (2): СРАБАТЫВАЕТ ЛИ ЗАВЕРШЕНИЕ СРАЗУ? ---
        Debug.Log($"[DEBUG] Dialogue process ended.");
        // ---------------------------------------------------------
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Тут возвращаем управление игроку (Time.timeScale = 1)
    }

    // Этот метод мы повесим на клик по панели
    public void OnDialogueWindowClick()
    {
        // Если реплики еще есть (т.е. кнопок нет), то по клику показываем следующую
        if (isDialogueActive && lineQueue.Count > 0)
        {
            DisplayNextLine();
        }
        // Если реплики уже кончились (и показаны кнопки), клик ничего не делает
    }
}