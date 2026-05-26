using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyWaveUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentWaveText;
    [SerializeField] private TextMeshProUGUI currentEnemyCountText;
    [SerializeField] private TextMeshProUGUI nextWaveTimeText;

    private void Start()
    {
        Enemy.OnEnemyDead += UpdateCurrentEnemyCountUI;
        EnemyWaveManager.Instance.OnAliveEnemyCountChanged += UpdateCurrentEnemyCountUI;
        EnemyWaveManager.Instance.OnWaveStarted += UpdateCurrentWaveUI;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDead -= UpdateCurrentEnemyCountUI;

        if (EnemyWaveManager.Instance != null)
        {
            EnemyWaveManager.Instance.OnAliveEnemyCountChanged -= UpdateCurrentEnemyCountUI;
            EnemyWaveManager.Instance.OnWaveStarted -= UpdateCurrentWaveUI;
        }
    }

    private void Update()
    {
        UpdateWaveStateUI();
    }

    private void UpdateCurrentEnemyCountUI()
    {
        currentEnemyCountText.text = "Current Enemy Count: " + EnemyWaveManager.Instance.aliveEnemyCount;
    }

    private void UpdateCurrentWaveUI()
    {
        currentWaveText.text = "Wave: " + EnemyWaveManager.Instance.waveIndex;
    }

    private void UpdateWaveStateUI()
    {
        var manager = EnemyWaveManager.Instance;

        // 没怪显示倒计时
        if (manager.aliveEnemyCount <= 0)
        {
            nextWaveTimeText.gameObject.SetActive(true);

            nextWaveTimeText.text = $"Next Wave: {manager.stateTimer:F1}s";
        }
        else
        {
            nextWaveTimeText.gameObject.SetActive(false);
        }
    }
}