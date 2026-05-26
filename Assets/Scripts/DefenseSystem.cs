using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Serialization;

public class DefenseSystem : MonoBehaviour
{
    [SerializeField] private float detectRadius = 15f;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Transform detectPoint;
    [FormerlySerializedAs("buildingLayer")] [SerializeField] private LayerMask detectLayer;
    [SerializeField] private float arrowGenerateRate = 0.3f;
    [SerializeField] private Arrow arrowPrefab;
    
    // 侦测时间间隔
    private const float DETECT_INTERVAL = 0.2f;
    private float _timer;
    private bool _hasEnemy; 
    private Transform _currentTarget;
    
    private void Start()
    {
        InvokeRepeating(nameof(DetectEnemy),0f,DETECT_INTERVAL);
    }

    /// <summary>
    /// 生成箭头，并且告诉箭头敌人是谁
    /// </summary>
    private void GenerateArrow()
    {
        if (!_currentTarget) return;
        var arrow  = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        arrow.SetTarget(_currentTarget);
    }

    private void Update()
    {
        if(!_hasEnemy) return;
        
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            GenerateArrow();
            _timer = arrowGenerateRate;
        }
    }

    /// <summary>
    /// 监测敌人，设置当前敌人
    /// </summary>
    private void DetectEnemy()
    {
        Collider2D[] results = new Collider2D[10];
        var size = Physics2D.OverlapCircleNonAlloc(detectPoint.position, detectRadius, results, detectLayer);
        _hasEnemy = size > 0;
        
        // 没有敌人
        if (size == 0)
        {
            _currentTarget = null;
            return;
        }

        // 设置最近的敌人
        Transform closestTarget = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < size; i++)
        {
            Transform enemyPositon = results[i].transform;
            float distance = Vector2.SqrMagnitude(enemyPositon.position - transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestTarget = enemyPositon;
            }
        }
        _currentTarget = closestTarget;
    }
    
    private void OnDisable()
    {
        CancelInvoke();
    }
}
