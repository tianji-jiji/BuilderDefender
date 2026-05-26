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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

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

    public void StartGame()
    {
        State = GameState.Playing;

        OnGameStart?.Invoke();

        // 启动波次系统（关键）
        if (EnemyWaveManager.Instance)
        {
            EnemyWaveManager.Instance.Begin();
        }
    }

    public void GameOver()
    {
        State = GameState.GameOver;

        OnGameOver?.Invoke();

        Time.timeScale = 0f;
    }

    public void PauseGame()
    {
        if (State != GameState.Playing) return;

        State = GameState.Paused;

        Time.timeScale = 0f;

        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused) return;

        State = GameState.Playing;

        Time.timeScale = 1f;

        OnGameResumed?.Invoke();
    }
}