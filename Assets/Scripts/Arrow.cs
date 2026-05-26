using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float flySpeed;
    private Rigidbody2D _rb2;

    private Transform _target;

    // 飞行方向
    private Vector2 _moveDirection;

    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    private void Awake()
    {
        _rb2 = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        FlyToEnemy();
    }

    public void SetTarget(Transform target)
    {
        if (!target)
        {
            Destroy(gameObject);
            return;
        }

        _target = target;
        _moveDirection = (_target.position - transform.position).normalized;
    }

    private void FlyToEnemy()
    {
        if (_target)
        {
            _moveDirection = (_target.position - transform.position).normalized;
        }

        _rb2.velocity = _moveDirection * flySpeed;
        transform.right = _moveDirection;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 是敌人
        if (other.gameObject.TryGetComponent<Enemy>(out var enemy))
        {
            // 有血条
            if (enemy.gameObject.TryGetComponent<HealthSystem>(out var healthSystem))
            {
                healthSystem.TakeDamage(10);
                Destroy(gameObject);
            }
        }
    }
}