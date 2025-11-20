using UnityEngine;

public class Chest : MonoBehaviour
{
    private bool opened = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (opened) return;
        if (!col.CompareTag("Player")) return;

        opened = true; // чтобы второй раз не сработало

        // показать окно выбора 3 апгрейдов
        FindObjectOfType<UpgradeManager>().TriggerUpgrade();

        // удалить сундук (или открыть анимацией)
        Destroy(gameObject);
    }
}
