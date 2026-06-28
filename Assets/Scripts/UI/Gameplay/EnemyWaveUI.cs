using TMPro;
using UnityEngine;

public class EnemyWaveUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentWaveText;
    [SerializeField] private TextMeshProUGUI currentEnemyCountText;
    [SerializeField] private TextMeshProUGUI nextWaveTimeText;

    private void Start()
    {
        EnemyWaveManager.Instance.OnAliveEnemyCountChanged += UpdateCurrentEnemyCountUI;
        EnemyWaveManager.Instance.OnWaveStarted += UpdateCurrentWaveUI;
    }

    private void OnDisable()
    {
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
        currentEnemyCountText.text = "剩余敌人数量: " + EnemyWaveManager.Instance.aliveEnemyCount;
    }

    private void UpdateCurrentWaveUI()
    {
        currentWaveText.text = "当前波次: " + EnemyWaveManager.Instance.waveIndex;
    }

    private void UpdateWaveStateUI()
    {
        var manager = EnemyWaveManager.Instance;

        switch (manager.State)
        {
            case EnemyWaveManager.WaveState.Preparing:
                nextWaveTimeText.gameObject.SetActive(true);
                nextWaveTimeText.text =
                    $"第一波倒计时: {manager.stateTimer:F1}s";
                break;

            case EnemyWaveManager.WaveState.WaitingNextWave:
                nextWaveTimeText.gameObject.SetActive(true);
                nextWaveTimeText.text =
                    $"下一波倒计时: {manager.stateTimer:F1}s";
                break;

            default:
                nextWaveTimeText.gameObject.SetActive(false);
                break;
        }
    }
}