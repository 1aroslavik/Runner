using UnityEngine;

public class Chest : MonoBehaviour
{
    private Animator _animator;
    private bool _isOpened = false;

    void Start()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Компонент Animator не найден на объекте сундука: " + gameObject.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_isOpened) return;
        if (!col.CompareTag("Player")) return;

        OpenChestSequence();
    }

    private void OpenChestSequence()
    {
        _isOpened = true; 
        
        // 1. Запуск анимации
        if (_animator != null)
        {
            _animator.SetTrigger("Open");
        }
        
        // 2. Логика выдачи лута
        FindObjectOfType<UpgradeManager>().TriggerUpgrade();

        // 3. Удаление будет вызвано функцией CleanUp через Animation Event
    }
    
    // !!! ЭТУ ФУНКЦИЮ ВЫЗЫВАЕТ АНИМАТОР, КОГДА АНИМАЦИЯ ЗАКАНЧИВАЕТСЯ !!!
    public void CleanUp()
    {
        Destroy(gameObject);
    }
}