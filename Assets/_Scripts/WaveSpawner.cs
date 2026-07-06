using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance;

    public Transform spawnPoint;

    public GameDataManager gameData;

    // 클리어 시 띄울 패널 (인스펙터에서 ClearPanel 연결)
    public GameObject clearPanel;

    // 클리어 후 넘어갈 씬 이름 (인스펙터에서 입력)
    public string nextSceneName = "WinScene";

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
            Debug.LogError("[WaveSpawner] GameDataManager가 할당되지 않았습니다! 인스펙터에서 넣어주세요.");
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

                    MonsterStatus status = gameData.GetMonsterStatus(spawn.enemyID);

                    EnemyData d = new EnemyData
                    {
                        enemyID = spawn.enemyID,
                        hp = status.hp,
                        atk = status.atk,
                        speed = status.speed,
                        rewardGold = status.rewardGold,
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

        // 마지막 적까지 전부 처치/통과되어 카운트가 0이 될 때까지 대기
        // (적이 풀로 반환되는 시점은 죽는 모션이 끝난 뒤이므로, 여기 도달하면 모션도 끝난 상태)
        while (activeEnemyCount > 0)
        {
            yield return null;
        }

        // 마지막 적이 사라진 뒤 1.5초 더 대기
        yield return new WaitForSeconds(1.5f);

        Debug.Log("웨이브 전체 클리어");

        // 클리어 패널 표시
        if (clearPanel != null)
            clearPanel.SetActive(true);
    }

    // "다음" 버튼에 연결할 함수
    public void OnClickNext()
    {
        SceneManager.LoadScene(nextSceneName);
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