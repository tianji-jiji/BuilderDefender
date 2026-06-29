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
        Building.OnBuildingDestroyed += HandleHomeDestroyed;
    }

    // 取消订阅基地建筑摧毁事件。
    private void OnDisable()
    {
        Building.OnBuildingDestroyed -= HandleHomeDestroyed;
    }

    // 根据当前游戏状态处理暂停和恢复输入。
    private void Update()
    {
        HandleHealthBarVisible();

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

    // 处理全局血条显示隐藏快捷键。
    private void HandleHealthBarVisible()
    {
        if (!Input.GetKeyDown(KeyCode.Alpha1) && !Input.GetKeyDown(KeyCode.Keypad1))
        {
            return;
        }

        HealthBar.ToggleAllHealthBarsVisibility();
    }

    // 开始游戏
    public void StartGame()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;

        OnGameStart?.Invoke();

        // 启动波次系统（关键）
        if (WaveManager.Instance)
        {
            WaveManager.Instance.Begin();
        }
    }

    // 游戏结束
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

    // 暂停
    public void PauseGame()
    {
        if (State != GameState.Playing) return;

        State = GameState.Paused;

        Time.timeScale = 0f;

        OnGamePaused?.Invoke();
    }

    // 恢复
    public void ResumeGame()
    {
        if (State != GameState.Paused) return;

        State = GameState.Playing;

        Time.timeScale = 1f;

        OnGameResumed?.Invoke();
    }

    // 检查被摧毁建筑是否是基地
    private void HandleHomeDestroyed(BuildingSo buildingSo, Vector3 position)
    {
        if (!buildingSo || buildingSo.buildingType != BuildingSo.BuildingType.Home)
        {
            return;
        }

        GameOver();
    }
}
