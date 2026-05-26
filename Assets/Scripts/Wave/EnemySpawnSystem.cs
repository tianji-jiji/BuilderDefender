using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnSystem : MonoBehaviour
{
    public List<EnemySpawnPoint> spawnPoints;
    
    public Transform GetRandomPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Count)].transform;
    }
    
    public Transform GetPointByType(EnemySpawnPoint.SpawnType type)
    {
        var list = spawnPoints.FindAll(p => p.type == type);
        if (list.Count == 0)
            return GetRandomPoint();

        return list[Random.Range(0, list.Count)].transform;
    }
}
