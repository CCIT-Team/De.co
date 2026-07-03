using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance;

    public Transform spawnPoint;

    // [추가됨] 인스펙터에서 GameDataManager(ScriptableObject)를 연결할 수 있게 합니다.
    public GameDataManager gameData;

    private List<SpawnData> spawnList = new List<SpawnData>();
    private int activeEnemyCount = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (gameData == null)
        {
            Debug.LogError("[WaveSpawner] GameDataManager가 할당되지 않았습니다! 인스펙터에서 꼭 넣어주세요.");
            return;
        }

        LoadWaveData();
        StartCoroutine(SpawnRoutine());
    }

    void LoadWaveData()
    {
        TextAsset csv = Resources.Load<TextAsset>("WaveData");
        string[] lines = csv.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] v = lines[i].Split(',');

            spawnList.Add(new SpawnData
            {
                wave = int.Parse(v[0]),
                order = int.Parse(v[1]),
                enemyID = v[2].Trim(),
                delay = float.Parse(v[3].Trim())
            });
        }
        spawnList = spawnList.OrderBy(x => x.wave).ThenBy(x => x.order).ToList();
    }

    IEnumerator SpawnRoutine()
    {
        var waveGroups = spawnList.GroupBy(x => x.wave).OrderBy(g => g.Key);

        foreach (var waveGroup in waveGroups)
        {
            int currentWave = waveGroup.Key;
            Debug.Log($"{currentWave}웨이브 시작");

            foreach (SpawnData spawn in waveGroup)
            {
                GameObject obj = ObjectPool.Instance.GetFromPool(spawn.enemyID, spawnPoint.position, Quaternion.identity);
                if (obj != null)
                {
                    activeEnemyCount++;

                    Enemy enemy = obj.GetComponent<Enemy>();

                    // [수정됨] 하드코딩을 지우고 GameDataManager에서 실제 몬스터 스탯을 꺼내옵니다.
                    MonsterStatus status = gameData.GetMonsterStatus(spawn.enemyID);

                    EnemyData d = new EnemyData
                    {
                        enemyID = spawn.enemyID,
                        hp = status.hp,               // 데이터매니저에 적힌 HP 적용
                        speed = status.speed,         // 데이터매니저에 적힌 Speed 적용!
                        rewardGold = status.rewardGold, // 데이터매니저에 적힌 보상 골드 적용
                        isFlying = status.isFlying,      
                        isStealthed = status.isStealthed
                    };

                    enemy.Initialize(d);
                    Debug.Log(spawn.enemyID + " 생성 (속도: " + status.speed + ")");
                }
                yield return new WaitForSeconds(spawn.delay);
            }

            yield return new WaitForSeconds(3f);
            Debug.Log($"{currentWave}웨이브 끝");
        }

        Debug.Log("웨이브 종료");
    }

    public void OnEnemyDespawn()
    {
        activeEnemyCount--;
        if (activeEnemyCount < 0) activeEnemyCount = 0;
    }
    public void OnEnemySplit()
    {
        activeEnemyCount++;
    }
}