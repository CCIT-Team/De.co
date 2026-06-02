using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MonsterStatus
{
    public string id;          // int -> string으로 변경 (slime, bug 등 글자 ID 대응)
    public string monsterName;
    public float hp;
    public float atk;
    public float speed;
    public int rewardGold;
}

[Serializable]
public struct WaveSpawnData
{
    public int waveIndex;
    public int spawnOrder;
    public string monsterId;   // int -> string으로 변경 (slime, bug 등 글자 ID 대응)
    public float delay;
}

[CreateAssetMenu(fileName = "GameDataManager", menuName = "Scriptable Object/Game Data Manager")]
public class GameDataManager : ScriptableObject
{
    public List<MonsterStatus> monsterDataTable = new List<MonsterStatus>();
    public List<WaveSpawnData> waveDataTable = new List<WaveSpawnData>();

    // ID(string)로 몬스터 능력치 찾기
    public MonsterStatus GetMonsterStatus(string id)
    {
        MonsterStatus status = monsterDataTable.Find(x => x.id == id);
        if (string.IsNullOrEmpty(status.id)) Debug.LogWarning($"[Warning] ID '{id}' 몬스터 데이터를 찾을 수 없습니다.");
        return status;
    }

    // 특정 웨이브의 모든 스폰 데이터 가져오기
    public List<WaveSpawnData> GetWaveDatas(int waveIndex)
    {
        return waveDataTable.FindAll(x => x.waveIndex == waveIndex);
    }
}