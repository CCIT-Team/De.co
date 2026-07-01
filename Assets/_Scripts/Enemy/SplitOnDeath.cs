using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitOnDeath : MonoBehaviour
{
    public string splitEnemyID;  // 분열해서 나올 적 ID
    public int splitCount;       // 분열 개수
    private bool hasSplit = false; // 이미 분열했는지 여부 (중복 방지)

    public void TrySplit(int currentIndex, float currentHp, float maxHp)
    {
        // 이미 분열했거나 체력이 50% 초과면 실행 안 함
        if (hasSplit) return;
        if (currentHp > maxHp * 0.5f) return;

        hasSplit = true;

        for (int i = 0; i < splitCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(
                Random.Range(-0.6f, 0.6f),
                Random.Range(-0.6f, 0.6f),
                0f
            );

            GameObject obj = ObjectPool.Instance.GetFromPool(splitEnemyID, spawnPos, Quaternion.identity);
            if (obj != null)
            {
                WaveSpawner.Instance.OnEnemySplit();
                Enemy splitEnemy = obj.GetComponent<Enemy>();
                MonsterStatus status = WaveSpawner.Instance.gameData.GetMonsterStatus(splitEnemyID);

                EnemyData splitData = new EnemyData
                {
                    enemyID = splitEnemyID,
                    hp = currentHp / splitCount, // 남은 체력을 splitCount로 나눠서 분배
                    speed = status.speed,
                    rewardGold = status.rewardGold,
                    isFlying = status.isFlying,
                    isStealthed = status.isStealthed
                };

                splitEnemy.Initialize(splitData);
                splitEnemy.CurrentIndex = currentIndex;
            }
        }

        // 분열 후 자기 자신은 죽음
        GetComponent<Enemy>().ForceDie();
    }

    // 풀 반환될 때 초기화
    public void ResetSplit()
    {
        hasSplit = false;
    }
}