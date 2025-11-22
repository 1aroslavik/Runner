using UnityEngine;

public class PermanentStats : MonoBehaviour
{
    public static PermanentStats Instance;

    // Хранимые статы
    public float maxHealth = 100;
    public float currentHealth = 100;

    public float meleeDamage = 25;
    public float arrowDamage = 40;

    public float moveSpeed = 5;
    public float sprintSpeed = 8;
    public float jumpForce = 20;
    public float defence = 0;

    void Awake()
    {
        // Singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Загружаем данные при запуске
        LoadPermanentData();
    }

    // ==========================================================
    //                СОХРАНЕНИЕ СТАТОВ ИГРОКА
    // ==========================================================

    /// <summary>
    /// Копируем статы из PlayerStats и сохраняем в PlayerPrefs.
    /// </summary>
    public void SaveFrom(PlayerStats s)
    {
        maxHealth = s.maxHealth;
        currentHealth = s.currentHealth;

        meleeDamage = s.meleeDamage;
        arrowDamage = s.arrowDamage;

        moveSpeed = s.moveSpeed;
        sprintSpeed = s.sprintSpeed;
        jumpForce = s.jumpForce;
        defence = s.defence;

        SavePermanentData();
    }

    /// <summary>
    /// Записывает сохранённые статы в объект PlayerStats
    /// </summary>
    public void ApplyTo(PlayerStats s)
    {
        s.maxHealth = maxHealth;
        s.currentHealth = currentHealth;

        s.meleeDamage = meleeDamage;
        s.arrowDamage = arrowDamage;

        s.moveSpeed = moveSpeed;
        s.sprintSpeed = sprintSpeed;
        s.jumpForce = jumpForce;
        s.defence = defence;
    }

    // ==========================================================
    //                     PlayerPrefs SAVE / LOAD
    // ==========================================================

    /// <summary>
    /// Сохраняем данные в PlayerPrefs (без аргументов!)
    /// </summary>
    public void SavePermanentData()
    {
        PlayerPrefs.SetFloat("maxHealth", maxHealth);
        PlayerPrefs.SetFloat("currentHealth", currentHealth);

        PlayerPrefs.SetFloat("meleeDamage", meleeDamage);
        PlayerPrefs.SetFloat("arrowDamage", arrowDamage);

        PlayerPrefs.SetFloat("moveSpeed", moveSpeed);
        PlayerPrefs.SetFloat("sprintSpeed", sprintSpeed);
        PlayerPrefs.SetFloat("jumpForce", jumpForce);

        PlayerPrefs.SetFloat("defence", defence);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Загружаем данные при старте игры
    /// </summary>
    private void LoadPermanentData()
    {
        maxHealth = PlayerPrefs.GetFloat("maxHealth", maxHealth);
        currentHealth = PlayerPrefs.GetFloat("currentHealth", currentHealth);

        meleeDamage = PlayerPrefs.GetFloat("meleeDamage", meleeDamage);
        arrowDamage = PlayerPrefs.GetFloat("arrowDamage", arrowDamage);

        moveSpeed = PlayerPrefs.GetFloat("moveSpeed", moveSpeed);
        sprintSpeed = PlayerPrefs.GetFloat("sprintSpeed", sprintSpeed);
        jumpForce = PlayerPrefs.GetFloat("jumpForce", jumpForce);

        defence = PlayerPrefs.GetFloat("defence", defence);
    }

    // ==========================================================
    //                   СБРОС ДЛЯ ТЕСТА
    // ==========================================================
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("🔥 PermanentStats: всё очищено");

        // Обновим значения по умолчанию
        LoadPermanentData();
    }
}
