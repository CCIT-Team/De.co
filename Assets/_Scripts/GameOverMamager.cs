using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI rewardText;

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDead += GameOver;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDead -= GameOver;
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);

        // 프리팹 안의 버튼은 씬에 있는 매니저를 직접 참조할 수 없으므로 코드로 연결
        foreach (Button button in gameOverPanel.GetComponentsInChildren<Button>(true))
        {
            if (button.name == "Lobby")
                button.onClick.AddListener(OnClickLobby);
        }
    }

    private void GameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        int reward = RewardManager.Instance.CalculateReward(
            WaveSpawner.Instance.CurrentWave,
            false
        );
        rewardText.text = $"Reward Gold : {reward}";
        Debug.Log("Reward Gold : " + reward);
        CurrencyManager.Instance.Add(reward);
    }

    // 로비로 돌아가기 버튼
    public void OnClickLobby()
    {
        Time.timeScale = 1f; // 게임오버로 멈춘 시간 복구 (안 하면 로비까지 멈춘 채로 감)
        GameSceneManager.LoadLobby();
    }
}