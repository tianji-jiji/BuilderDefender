using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemySo enemySo;
    [FormerlySerializedAs("buildingLayer")] [SerializeField] private LayerMask detectLayer;
    [SerializeField] private HealthSystem healthSystem;
    public static event Action OnEnemyDead;
    private Rigidbody2D _rb2;
    private Transform _currentTarget;
    // 默认目标
    private Transform _defaultTarget;
    private float _timer;
    private const float DETECTION_INTERVAL = 0.2f;
    private float _randomMoveSpeed;
    private GameObject _enemyDiedParticles;
    
    private void Awake()
    {
        _enemyDiedParticles = Resources.Load<GameObject>("Particles/EnemyDieParticles");
        _rb2 = GetComponent<Rigidbody2D>();
        var home = GameObject.FindGameObjectWithTag("Home");
        _defaultTarget = home.transform.Find("DefaultTargetTransform");
        _currentTarget = _defaultTarget;
    }

    public void Init(EnemySo so)
    {
        _randomMoveSpeed = so.moveSpeed * UnityEngine.Random.Range(0.9f, 1.1f);
        healthSystem.Init(enemySo.maxHealth);
    }
    
    private void Start()
    {
        healthSystem.OnDied += Death;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= DETECTION_INTERVAL)
        {
            FindTarget();
            _timer = 0f;
        }
    }

    private void HandleMovement()
    {
        if (_currentTarget)
        {
            var moveDir = (_currentTarget.position - transform.position).normalized;
            _rb2.velocity = moveDir * _randomMoveSpeed;
        }
    }

    private void FindTarget()
    {
        Collider2D c2D =
            Physics2D.OverlapCircle(
                transform.position,
                enemySo.detectRadius,
                detectLayer);

        _currentTarget = c2D ? c2D.transform : _defaultTarget;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Building"))
        {
            other.gameObject.GetComponent<HealthSystem>().TakeDamage(enemySo.atk);
            Debug.Log($"对{other.gameObject.name}造成{enemySo.atk}点伤害");
            Death();
        }
    }
    
    private void Death()
    {
        Destroy(gameObject);
        Instantiate(_enemyDiedParticles,transform.position,Quaternion.identity);
        OnEnemyDead?.Invoke();
    }

    private void OnDisable()
    {
        healthSystem.OnDied -= Death;
    }
}