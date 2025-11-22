using UnityEngine;

public class PermanentStats : MonoBehaviour
{
    public static PermanentStats Instance;

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
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // —ќ’–јЌя≈ћ статы при смерти
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
    }

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

}
