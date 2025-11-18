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
    }

    public void StartDialogue(DialogueConversation conversation)
    {
        if (isDialogueActive) return;

        isDialogueActive = true;
        dialoguePanel.SetActive(true);

        // Тут можно поставить игру на паузу (Time.timeScale = 0)
        // или отключить управление игроком

        ShowNode(conversation.startNode);
    }

    private void ShowNode(DialogueNode node)
    {
        currentNode = node;
        lineQueue.Clear(); // Очищаем очередь

        // Заполняем очередь всеми репликами из этого узла
        foreach (DialogueLine line in node.lines)
        {
            lineQueue.Enqueue(line);
        }

        // Очищаем старые кнопки
        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        // Если в очереди есть реплики, показываем
        if (lineQueue.Count > 0)
        {
            DialogueLine line = lineQueue.Dequeue();
            nameText.text = line.characterName;

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
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null; // Ждем 1 кадр
            // yield return new WaitForSeconds(0.02f); // Для замедления
        }
    }

    private void ShowOptions()
    {
        // Если вариантов ответа нет, просто закрываем диалог
        if (currentNode.options.Length == 0)
        {
            // Добавляем кнопку "Завершить" для наглядности
            GameObject buttonGO = Instantiate(optionButtonPrefab, optionsContainer);
            buttonGO.GetComponentInChildren<TextMeshProUGUI>().text = "Завершить";
            buttonGO.GetComponent<Button>().onClick.AddListener(EndDialogue);
            return;
        }

        // Создаем кнопки для каждого варианта
        foreach (DialogueOption option in currentNode.options)
        {
            GameObject buttonGO = Instantiate(optionButtonPrefab, optionsContainer);
            buttonGO.GetComponentInChildren<TextMeshProUGUI>().text = option.optionText;

            DialogueNode nextNode = option.nextNode;
            buttonGO.GetComponent<Button>().onClick.AddListener(() => SelectOption(nextNode));
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
        dialoguePanel.SetActive(false);

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