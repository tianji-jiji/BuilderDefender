using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        Playing,    
        Paused,     
        GameOver
    }

    public GameState State { get; private set; }

    private float _timer;

    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action OnGamePaused;
    public event Action OnGameResumed;

    // 初始化游戏管理器单例。
    private void Awake()
    {
        Instance = this;
    }

    // 游戏启动时进入游玩状态。
    private void Start()
    {
        StartGame();
    }

    // 订阅基地建筑摧毁事件。
    private void OnEnable()
    {
        Building.OnBuildingDestroyed += HandleBuildingDestroyed;
    }

    // 取消订阅基地建筑摧毁事件。
    private void OnDisable()
    {
        Building.OnBuildingDestroyed -= HandleBuildingDestroyed;
    }

    // 根据当前游戏状态处理暂停和恢复输入。
    private void Update()
    {
        switch (State)
        {
            case GameState.Playing:
                if (Input.GetKeyDown(KeyCode.Escape))
                    PauseGame();
                break;

            case GameState.Paused:
                if (Input.GetKeyDown(KeyCode.Escape))
                    ResumeGame();
                break;
        }
    }

    // 开始游戏并启动敌人波次系统。
    public void StartGame()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;

        OnGameStart?.Invoke();

        // 启动波次系统（关键）
        if (EnemyWaveManager.Instance)
        {
            EnemyWaveManager.Instance.Begin();
        }
    }

    // 进入游戏结束状态并暂停游戏时间。
    public void GameOver()
    {
        if (State == GameState.GameOver)
        {
            return;
        }

        State = GameState.GameOver;

        OnGameOver?.Invoke();

        Time.timeScale = 0f;
    }

    // 暂停当前游戏。
    public void PauseGame()
    {
        if (State != GameState.Playing) return;

        State = GameState.Paused;

        Time.timeScale = 0f;

        OnGamePaused?.Invoke();
    }

    // 恢复当前游戏。
    public void ResumeGame()
    {
        if (State != GameState.Paused) return;

        State = GameState.Playing;

        Time.timeScale = 1f;

        OnGameResumed?.Invoke();
    }

    // 检查被摧毁建筑是否是基地，是则触发游戏结束。
    private void HandleBuildingDestroyed(BuildingSo buildingSo, Vector3 position)
    {
        if (!buildingSo || buildingSo.buildingType != BuildingSo.BuildingType.Home)
        {
            return;
        }

        GameOver();
    }
}
