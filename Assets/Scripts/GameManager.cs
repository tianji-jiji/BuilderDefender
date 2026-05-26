using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        Intro,      // 开局准备
        Playing,    
        Paused,     
        GameOver
    }

    public GameState State { get; private set; }

    [Header("Intro Settings")]
    [SerializeField] private float introDuration = 3f;

    private float _introTimer;

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
        EnterIntro();
    }

    private void Update()
    {
        switch (State)
        {
            case GameState.Intro:
                UpdateIntro();
                break;

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

    private void EnterIntro()
    {
        State = GameState.Intro;
        _introTimer = introDuration;
    }

    private void UpdateIntro()
    {
        _introTimer -= Time.deltaTime;

        if (_introTimer <= 0f)
        {
            StartGame();
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