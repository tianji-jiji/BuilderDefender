using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    public enum SpawnType
    {
        Normal,
        Hard,
        Boss
    }

    public SpawnType type;
}
