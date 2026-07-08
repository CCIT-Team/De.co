using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    [SerializeField] private int rewardPerWave = 10;
    [SerializeField] private int clearBonus = 100;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public int CalculateReward(int clearedWave, bool isClear)
    {
        int reward = clearedWave * rewardPerWave;

        if (isClear)
            reward += clearBonus;

        return reward;
    }
}