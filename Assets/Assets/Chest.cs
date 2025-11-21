using UnityEngine;

public class Chest : MonoBehaviour
{
    private Animator _animator;
    private bool _isOpened = false;

    [Header("Система апгрейдов")]
    public UpgradeManager upgradeManager;

    void Start()
    {
        _animator = GetComponent<Animator>();

        if (_animator == null)
            Debug.LogError("❌ Chest: Animator не найден!");

        if (upgradeManager == null)
            Debug.LogError("❌ Chest: UpgradeManager не назначен!");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_isOpened) return;
        if (!col.CompareTag("Player")) return;

        PlayerStats stats = col.GetComponent<PlayerStats>();

        if (stats == null)
        {
            Debug.LogError("❌ Chest: PlayerStats не найден у игрока!");
            return;
        }

        _isOpened = true;
        OpenChestSequence(stats);
    }

    private void OpenChestSequence(PlayerStats stats)
    {
        // 1. Запуск анимации
        if (_animator != null)
            _animator.SetTrigger("Open");

        // 2. Выдать апгрейды
        upgradeManager.ShowRandom(stats);

        // 3. Удаление произойдет через Animation Event → CleanUp()
    }

    // ВЫЗЫВАЕТСЯ ИЗ АНИМАЦИИ (Animation Event)
    public void CleanUp()
    {
        Destroy(gameObject);
    }
}
