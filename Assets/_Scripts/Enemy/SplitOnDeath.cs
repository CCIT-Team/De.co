using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitOnDeath : MonoBehaviour
{
    public string splitEnemyID;  // �п��ؼ� ���� �� ID
    public int splitCount;       // �п� ����
    private bool hasSplit = false; // �̹� �п��ߴ��� ���� (�ߺ� ����)

    public void TrySplit(int currentIndex, float currentHp, float maxHp)
    {
        // �̹� �п��߰ų� ü���� 50% �ʰ��� ���� �� ��
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
                    hp = currentHp / splitCount, // ���� ü���� splitCount�� ������ �й�
                    atk = status.atk,
                    speed = status.speed,
                    rewardGold = status.rewardGold,
                    isFlying = status.isFlying,
                    isStealthed = status.isStealthed
                };

                splitEnemy.Initialize(splitData);
                splitEnemy.CurrentIndex = currentIndex;
            }
        }

        // �п� �� �ڱ� �ڽ��� ����
        GetComponent<Enemy>().ForceDie();
    }

    // Ǯ ��ȯ�� �� �ʱ�ȭ
    public void ResetSplit()
    {
        hasSplit = false;
    }
}