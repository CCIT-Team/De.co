using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
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
    }

    private void GameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        int reward = RewardManager.Instance.CalculateReward(
    WaveSpawner.Instance.CurrentWave,
    false
);
        rewardText.text = $"획득 재화 : {reward}";
        Debug.Log("획득 재화 : " + reward);
        CurrencyManager.Instance.Add(reward);
    }
}