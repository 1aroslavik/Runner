using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Enemy Type")]
public class EnemyType : ScriptableObject
{
    public string enemyName;
    public Sprite sprite;
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
    public float damage = 15f;
}
