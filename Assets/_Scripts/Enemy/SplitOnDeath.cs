using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitOnDeath : MonoBehaviour
{
    public string splitEnemyID;
    public int splitCount;
    public void Split(int currentIndex)
    {
        for (int i = 0; i < splitCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(
                Random.Range(-0.6f, 0.6f),
                Random.Range(-0.6f, 0.6f),
                0f
                );
            GameObject obj = ObjectPool.Instance.GetFromPool(splitEnemyID, spawnPos, Quaternion.identity);
            if (obj !=null)
            {
                WaveSpawner.Instance.OnEnemySplit();
                Enemy splitEnemy = obj.GetComponent<Enemy>();
                MonsterStatus status = WaveSpawner.Instance.gameData.GetMonsterStatus(splitEnemyID);
                EnemyData splitData = new EnemyData
                {
                    enemyID = splitEnemyID,
                    hp = status.hp,
                    speed = status.speed,
                    rewardGold = status.rewardGold,
                    isFlying = status.isFlying,
                    isStealthed = status.isStealthed
                };
                splitEnemy.Initialize(splitData);
                splitEnemy.CurrentIndex = currentIndex;
            }
        }
    }
}
