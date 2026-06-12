using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人生成系统，决定敌人的出生点
/// </summary>
public class EnemySpawnSystem : MonoBehaviour
{
    public List<EnemySpawnPoint> spawnPoints;
    
    public Transform GetRandomPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Count)].transform;
    }
    
    // 备用，按按敌人类型选出生点
    public Transform GetPointByType(EnemySpawnPoint.SpawnType type)
    {
        var list = spawnPoints.FindAll(p => p.type == type);
        if (list.Count == 0)
            return GetRandomPoint();

        return list[Random.Range(0, list.Count)].transform;
    }
}
