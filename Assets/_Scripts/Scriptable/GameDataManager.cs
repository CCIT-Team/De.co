using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MonsterStatus
{
    public string id;

    public string monsterName;

    public float hp;

    public float atk;

    public float speed;

    public int rewardGold;
    public bool isFlying;
    public bool isStealthed;
}

[Serializable]
public struct WaveSpawnData
{
    public int waveIndex;

    public int spawnOrder;

    public string monsterId;

    public float delay;
}

[CreateAssetMenu(
    fileName = "GameDataManager",
    menuName = "Scriptable Object/Game Data Manager"
)]
public class GameDataManager
    : ScriptableObject
{
    public List<MonsterStatus>
        monsterDataTable
            = new();

    public List<WaveSpawnData>
        waveDataTable
            = new();

    public List<TowerData>
        towerDataTable
            = new();

    public MonsterStatus
        GetMonsterStatus(
            string id
        )
    {
        return monsterDataTable
            .Find(
                x =>
                x.id == id
            );
    }

    public List<WaveSpawnData>
        GetWaveDatas(
            int wave
        )
    {
        return waveDataTable
            .FindAll(
                x =>
                x.waveIndex
                == wave
            );
    }

    public TowerData
        GetTowerData(
            string towerID
        )
    {
        return towerDataTable
            .Find(
                x =>
                x.towerID
                == towerID
            );
    }
}