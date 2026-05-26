using UnityEngine;


[CreateAssetMenu(menuName = "Enemy/EnemySo")]
public class EnemySo : ScriptableObject
{
    public GameObject prefab;
    public int maxHealth;
    public float moveSpeed;
    public int atk;
    public float detectRadius;
}