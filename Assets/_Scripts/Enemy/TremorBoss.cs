/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 최종보스 기믹: 등장 후 patternInterval초마다 패턴1 -> 패턴2 -> 패턴3 순서로 자동 발동한다.
// 애니메이션이 아직 없어서 각 패턴 발동 지점에 TODO로 연결 위치만 표시해두었다.
// 패턴1/3(타워 기절)은 Tower.cs가 IStunnable을 구현해야 실제 효과가 적용된다. (IStunnable.cs 참고)
[RequireComponent(typeof(Enemy))]
public class TremorBoss : MonoBehaviour
{
    [Header("패턴 자동 발동 설정")]
    [Tooltip("패턴이 1->2->3 순서로 자동 발동되는 주기(초). 보스 등장 후 첫 발동까지도 이 시간만큼 대기한다")]
    public float patternInterval = 10f;

    [Header("패턴1 : 주먹 내려찍기 (타워 기절)")]
    [Tooltip("타워를 기절시키는 범위(반지름)")]
    public float slamRange = 2.5f;
    [Tooltip("타워가 기절하는 시간(초)")]
    public float slamStunDuration = 2f;
    public Color slamRingColor = new Color(1f, 0.6f, 0f); // 주황색

    [Header("패턴2 : 붉은 액체 (길 회복)")]
    [Tooltip("보스의 현재 웨이포인트를 기준으로 앞으로 몇 개의 웨이포인트까지 액체를 뿌릴지")]
    public int liquidWaypointCount = 8;
    [Tooltip("각 웨이포인트 지점에서 적을 감지하고 표시하는 원의 반지름")]
    public float liquidPuddleRadius = 1f;
    [Tooltip("액체가 길 위에 남아있는 시간(초). 이 시간 동안 지나가는 적이 반복 회복된다")]
    public float liquidDuration = 5f;
    [Tooltip("회복이 발동되는 주기(초)")]
    public float liquidHealInterval = 1f;
    [Tooltip("한 번 발동될 때 회복되는 고정 체력량")]
    public float liquidHealAmount = 10f;
    public Color liquidRingColor = Color.red;
    [Tooltip("공중 유닛용 웨이포인트를 기준으로 뿌릴지 여부 (지상 보스면 체크 해제)")]
    public bool useFlyingWaypoints = false;

    [Header("패턴3 : 비명 (타워 기절)")]
    [Tooltip("타워를 기절시키는 범위(반지름)")]
    public float screamRange = 3.5f;
    [Tooltip("타워가 기절하는 시간(초)")]
    public float screamStunDuration = 3f;
    public Color screamRingColor = Color.magenta;

    [Header("공통 이펙트 설정")]
    [Tooltip("기절 범위 표시 원이 화면에 보이는 시간(초)")]
    public float ringVisibleDuration = 1f;

    private Enemy myEnemy;
    private Coroutine patternRoutine;

    void Awake()
    {
        myEnemy = GetComponent<Enemy>();
    }

    // 오브젝트 풀에서 재사용되어 다시 활성화될 때마다 패턴 루프를 새로 시작한다
    void OnEnable()
    {
        patternRoutine = StartCoroutine(PatternLoop());
    }

    void OnDisable()
    {
        if (patternRoutine != null)
            StopCoroutine(patternRoutine);
    }

    IEnumerator PatternLoop()
    {
        int patternIndex = 0; // 0 = 패턴1, 1 = 패턴2, 2 = 패턴3

        while (true)
        {
            yield return new WaitForSeconds(patternInterval);

            if (myEnemy == null || myEnemy.IsDead)
                yield break;

            switch (patternIndex)
            {
                case 0:
                    Pattern1_Slam();
                    break;
                case 1:
                    Pattern2_RedLiquid();
                    break;
                case 2:
                    Pattern3_Scream();
                    break;
            }

            patternIndex = (patternIndex + 1) % 3;
        }
    }

    // 패턴1 : 주먹을 내려쳐 범위의 타워를 기절시킨다
    void Pattern1_Slam()
    {
        // TODO: 애니메이션 연결 예정 (예: animator.SetTrigger("Slam"))
        StunTowersInRange(slamRange, slamStunDuration);
        StartCoroutine(ShowRing(transform.position, slamRange, slamRingColor, ringVisibleDuration));
    }

    // 패턴3 : 비명을 질러 범위의 타워를 기절시킨다
    void Pattern3_Scream()
    {
        // TODO: 애니메이션 연결 예정 (예: animator.SetTrigger("Scream"))
        StunTowersInRange(screamRange, screamStunDuration);
        StartCoroutine(ShowRing(transform.position, screamRange, screamRingColor, ringVisibleDuration));
    }

    // 범위 내 모든 타워를 찾아 기절시킨다.
    // Tower.cs가 IStunnable을 구현하기 전까지는 대상이 없어 아무 효과도 나지 않는다 (IStunnable.cs 참고)
    void StunTowersInRange(float range, float stunDuration)
    {
        Tower[] towers = FindObjectsOfType<Tower>();
        foreach (Tower tower in towers)
        {
            if (Vector3.Distance(transform.position, tower.transform.position) <= range)
            {
                IStunnable stunnable = tower.GetComponent<IStunnable>();
                if (stunnable != null)
                    stunnable.ApplyStun(stunDuration);
            }
        }
    }

    // 패턴2 : 보스의 현재 웨이포인트를 기준으로 앞쪽 liquidWaypointCount개 지점에 붉은 액체를 뿌리고,
    // liquidDuration 동안 그 위를 지나가는 적을 liquidHealInterval초마다 liquidHealAmount만큼 회복시킨다
    void Pattern2_RedLiquid()
    {
        // TODO: 애니메이션 연결 예정 (예: animator.SetTrigger("SpitLiquid"))

        int startIndex = myEnemy != null ? myEnemy.CurrentIndex : 0;
        int totalWaypoints = WaypointManager.Instance.GetWaypointCount(useFlyingWaypoints);

        List<Vector3> puddlePositions = new List<Vector3>();
        for (int i = 0; i < liquidWaypointCount; i++)
        {
            int index = startIndex + i;
            if (index >= totalWaypoints) break;

            Transform wp = WaypointManager.Instance.GetWaypoint(index, useFlyingWaypoints);
            if (wp != null)
                puddlePositions.Add(wp.position);
        }

        if (puddlePositions.Count == 0) return;

        foreach (Vector3 pos in puddlePositions)
            StartCoroutine(ShowRing(pos, liquidPuddleRadius, liquidRingColor, liquidDuration));

        StartCoroutine(HealLoopOverPuddles(puddlePositions));
    }

    IEnumerator HealLoopOverPuddles(List<Vector3> puddlePositions)
    {
        float elapsed = 0f;
        while (elapsed < liquidDuration)
        {
            yield return new WaitForSeconds(liquidHealInterval);
            elapsed += liquidHealInterval;

            HashSet<Enemy> healedEnemies = new HashSet<Enemy>();
            foreach (Vector3 pos in puddlePositions)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(pos, liquidPuddleRadius);
                foreach (Collider2D hit in hits)
                {
                    Enemy enemy = hit.GetComponent<Enemy>();
                    if (enemy != null && enemy != myEnemy && !enemy.IsDead)
                        healedEnemies.Add(enemy);
                }
            }

            foreach (Enemy enemy in healedEnemies)
                enemy.Heal(liquidHealAmount);
        }
    }

    // 지정된 위치에 duration초 동안 보이는 원형 범위 표시를 그린 뒤 제거한다 (Instigator/NecroBoss와 동일한 LineRenderer 방식)
    IEnumerator ShowRing(Vector3 position, float radius, Color color, float duration)
    {
        GameObject ringObj = new GameObject("TremorBossRing");
        ringObj.transform.position = position;

        LineRenderer lr = ringObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 100;

        int segments = 50;
        lr.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 pos = position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            lr.SetPosition(i, pos);
        }

        yield return new WaitForSeconds(duration);

        Destroy(ringObj);
    }
}*/