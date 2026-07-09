using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance;

    public Transform spawnPoint;
    public int CurrentWave { get; private set; } = 0;
    public int MaxWave { get; private set; } = 0;

    public GameDataManager gameData;

    // 클리어 시 띄울 패널 (인스펙터에서 ClearPanel 연결)
    public GameObject clearPanel;
    [SerializeField] private TextMeshProUGUI rewardText;


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

        SetupClearPanel();
        SetupPortal();
        LoadWaveData();
        StartCoroutine(SpawnRoutine());
    }

    private ParticleSystem portal; // 스폰 지점의 포탈 이펙트

    // 씬에서 'portal' 오브젝트를 찾아 게임 시작과 함께 재생한다 (없으면 조용히 넘어감)
    void SetupPortal()
    {
        GameObject portalObj = FindInScene("portal");
        if (portalObj == null) return;

        portal = portalObj.GetComponent<ParticleSystem>();
        if (portal != null)
            portal.Play(true); // 자식 파티클까지 함께 재생
    }

    // 클리어 패널이 인스펙터에 연결되어 있지 않으면 씬에서 이름으로 찾고,
    // 패널 안의 Next 버튼에 씬 이동 기능을 코드로 연결한다
    // (프리팹 안의 버튼은 씬에 있는 WaveSpawner를 직접 참조할 수 없기 때문)
    void SetupClearPanel()
    {
        if (clearPanel == null)
            clearPanel = FindInScene("ClearPanel");

        if (clearPanel == null)
        {
            Debug.LogWarning("[WaveSpawner] ClearPanel을 찾지 못했습니다. 클리어 시 패널이 뜨지 않습니다.");
            return;
        }

        clearPanel.SetActive(false);

        UnityEngine.UI.Button nextButton = clearPanel.GetComponentInChildren<UnityEngine.UI.Button>(true);
        if (nextButton != null)
            nextButton.onClick.AddListener(OnClickNext);

        // 보상 텍스트가 인스펙터에 연결되어 있지 않으면 패널 안에서 'RewardText' 이름으로 찾는다
        if (rewardText == null)
        {
            foreach (TextMeshProUGUI text in clearPanel.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (text.name == "RewardText")
                {
                    rewardText = text;
                    break;
                }
            }
        }
    }

    // 비활성 오브젝트까지 포함해서 씬 전체에서 이름으로 찾기
    GameObject FindInScene(string name)
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            if (root.name == name)
                return root;

            Transform found = FindRecursive(root.transform, name);
            if (found != null)
                return found.gameObject;
        }
        return null;
    }

    Transform FindRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
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

        MaxWave = spawnList.Max(x => x.wave);
    }

    IEnumerator SpawnRoutine()
    {
        var waveGroups = spawnList.GroupBy(x => x.wave).OrderBy(g => g.Key);

        foreach (var waveGroup in waveGroups)
        {
            CurrentWave = waveGroup.Key;
            Debug.Log($"{CurrentWave}웨이브 시작");

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
            Debug.Log($"{CurrentWave}웨이브 끝");
        }

        // 마지막 적까지 전부 처치/통과되어 카운트가 0이 될 때까지 대기
        // (적이 풀로 반환되는 시점은 죽는 모션이 끝난 뒤이므로, 여기 도달하면 모션도 끝난 상태)
        while (activeEnemyCount > 0)
        {
            yield return null;
        }

        // 마지막 적까지 정리됨: 포탈을 끈다 (남은 입자는 1.5초 대기 동안 자연스럽게 사라짐)
        if (portal != null)
            portal.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        // 마지막 적이 사라진 뒤 1.5초 더 대기
        yield return new WaitForSeconds(1.5f);

        Debug.Log("웨이브 전체 클리어");

        // 보상 계산/지급 (매니저나 텍스트가 없어도 클리어 패널은 반드시 뜨도록 전부 null 방어)
        if (RewardManager.Instance != null && CurrencyManager.Instance != null)
        {
            int reward = RewardManager.Instance.CalculateReward(CurrentWave, true);

            if (rewardText != null)
                rewardText.text = $"Reward Gold : {reward}";

            Debug.Log("Reward Gold : " + reward);
            CurrencyManager.Instance.Add(reward);
        }
        else
        {
            Debug.LogWarning("[WaveSpawner] RewardManager/CurrencyManager가 씬에 없어 보상 지급을 건너뜁니다.");
        }

        // 클리어 패널 표시
        if (clearPanel != null)
            clearPanel.SetActive(true);
    }

    // 클리어 패널의 "다음" 버튼: 로비로 복귀
    public void OnClickNext()
    {
        GameSceneManager.LoadLobby();
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