using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人数据库
/// </summary>
public class EnemyDatabase : MonoBehaviour
{
    public List<EnemySo> normalEnemies;
    public List<EnemySo> hardEnemies;
    public EnemySo bossEnemy;

    public EnemySo GetNormal()
    {
        return normalEnemies[Random.Range(0, normalEnemies.Count)];
    }

    public EnemySo GetHard()
    {
        return hardEnemies[Random.Range(0, hardEnemies.Count)];
    }

    public EnemySo GetBoss()
    {
        return bossEnemy;
    }
}
