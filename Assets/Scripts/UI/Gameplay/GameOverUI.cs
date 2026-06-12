using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    private const string RESULT_WAVE_TEXT_FORMAT = "你存活了 {0} 波";

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultWaveText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    // 初始化游戏结束面板和按钮点击事件。
    private void Awake()
    {
        HideGameOverPanel();
        retryButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    // 订阅游戏结束事件。
    private void Start()
    {
        GameManager.Instance.OnGameOver += ShowGameOverPanel;
    }

    // 取消订阅游戏结束事件并移除按钮点击事件。
    private void OnDestroy()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.OnGameOver -= ShowGameOverPanel;
        }

        retryButton.onClick.RemoveListener(RestartGame);
        quitButton.onClick.RemoveListener(QuitGame);
    }

    // 隐藏游戏结束面板。
    private void HideGameOverPanel()
    {
        gameOverPanel.SetActive(false);
    }

    // 显示游戏结束面板并刷新坚持波数。
    private void ShowGameOverPanel()
    {
        resultWaveText.text = string.Format(RESULT_WAVE_TEXT_FORMAT, GetResultWave());
        gameOverPanel.SetActive(true);
    }

    // 获取玩家本局坚持到的波数。
    private int GetResultWave()
    {
        if (!EnemyWaveManager.Instance)
        {
            return 0;
        }

        return EnemyWaveManager.Instance.waveIndex;
    }

    // 重新加载当前场景开始新一局。
    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 退出游戏或在编辑器中停止播放。
    private void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
