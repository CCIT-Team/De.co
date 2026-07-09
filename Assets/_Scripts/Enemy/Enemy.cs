using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Enemy : MonoBehaviour, ISlowable
{
    private EnemyData data;
    private bool isDead = false;
    public int CurrentIndex { get; set; } = 0;
    public float CurrentHp { get; private set; }
    public float MaxHp => data.hp;
    public bool IsDead => isDead;
    public bool IsFlying => data != null && data.isFlying;
    public bool IsStealthed => data != null && data.isStealthed;

    private SphereCollider sphereCollider;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private Coroutine flashCoroutine;

    // 둔화 상태 (냉각체 장비 등). 배율 1 = 정상 속도
    private float slowMultiplier = 1f;
    private float slowEndTime;

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        SetStandardHitbox();
    }

    void SetStandardHitbox()
    {
        if (sphereCollider == null) return;
        sphereCollider.radius = 0.2f;
        sphereCollider.center = Vector3.zero;
        sphereCollider.isTrigger = true;
    }

    public void Initialize(EnemyData _data)
    {
        data = _data;
        isDead = false;
        enabled = true; // 방어 코드로 꺼졌던 경우 대비
        CurrentIndex = 0;
        CurrentHp = data.hp;

        // 풀에서 재사용될 때 이전 둔화가 남지 않도록 초기화
        slowMultiplier = 1f;
        slowEndTime = 0f;

        // 분열 초기화
        SplitOnDeath split = GetComponent<SplitOnDeath>();
        if (split != null)
            split.ResetSplit();

        // 애니메이터 초기화
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == "Die")
                {
                    animator.ResetTrigger("Die");
                    break;
                }
            }
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }

    void Update()
    {
        if (isDead) return;
        Move();
    }

    void Move()
    {
        // 스폰 경로 문제 방어: 오류를 매 프레임 쏟아내는 대신, 원인을 한 번만 정확히 알려주고 멈춘다
        if (data == null)
        {
            Debug.LogError($"[Enemy] '{name}'이(가) Initialize 없이 활성화되어 있습니다 (스폰 경로 확인 필요). 이 로그를 클릭하면 해당 오브젝트가 하이라이트됩니다.", gameObject);
            enabled = false;
            return;
        }
        if (WaypointManager.Instance == null)
        {
            Debug.LogError("[Enemy] 씬에 WayPointManager가 없습니다. 적이 이동할 수 없습니다.", gameObject);
            enabled = false;
            return;
        }

        if (CurrentIndex >= WaypointManager.Instance.GetWaypointCount(data.isFlying)) return;

        Transform target = WaypointManager.Instance.GetWaypoint(CurrentIndex, data.isFlying);

        // 둔화 중이면 감소된 속도로 이동
        float speed = data.speed;
        if (Time.time < slowEndTime)
            speed *= slowMultiplier;

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            CurrentIndex++;
            if (CurrentIndex >= WaypointManager.Instance.GetWaypointCount(data.isFlying))
            {
                // [놓침] 타워에 죽지 않고 마지막 지점까지 살아서 통과했을 때
                if (PlayerHealth.Instance != null)
                    PlayerHealth.Instance.TakeDamage(data.atk);

                WaveSpawner.Instance.OnEnemyDespawn();
                ObjectPool.Instance.ReturnToPool(data.enemyID, gameObject);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        CurrentHp -= damage;
        if (HitEffectManager.Instance != null)
            HitEffectManager.Instance.PlayHit(transform.position + Vector3.up * 0.3f);
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashColor());

        // 체력 50% 이하 분열 체크
        SplitOnDeath split = GetComponent<SplitOnDeath>();
        if (split != null)
            split.TrySplit(CurrentIndex, CurrentHp, data.hp);

        if (CurrentHp <= 0) Die();
    }

    IEnumerator FlashColor()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = baseColor;
    }

    void Die()
    {
        isDead = true;
        // [콘솔창] 타워가 몬스터를 죽였을 때만 골드 획득 로그 출력
        Debug.Log($"{data.rewardGold}골드 획득");

        InstigatorOnDeath scream = GetComponent<InstigatorOnDeath>();
        if (scream != null)
            scream.Scream();

        // 죽는 애니메이션 트리거 실행
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == "Die")
                {
                    animator.SetTrigger("Die");
                    break;
                }
            }
        }

        // ⭐ [추가된 부분] GoldManager에게 골드 지급 요청
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(data.rewardGold);
        }
        

        // 죽는 애니메이션이 끝날 때까지 기다렸다가 풀로 반환
        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(0.417f);

        WaveSpawner.Instance.OnEnemyDespawn();
        isDead = false;
        ObjectPool.Instance.ReturnToPool(data.enemyID, gameObject);
    }

    public void ApplySpeedMultiplier(float multiplier)
    {
        data.speed *= multiplier;
    }

    // 이동속도를 slowPercent(%)만큼 duration(초) 동안 감소시킨다 (냉각체 장비 등)
    // 이미 둔화 중이면: 더 강한 둔화가 우선, 지속시간은 갱신
    public void ApplySlow(float slowPercent, float duration)
    {
        if (isDead) return;

        float multiplier = 1f - slowPercent / 100f;

        if (Time.time >= slowEndTime || multiplier < slowMultiplier)
            slowMultiplier = multiplier;

        slowEndTime = Time.time + duration;
    }

    // 고정량만큼 체력을 회복시킨다. 최대 체력(data.hp)을 넘지 않도록 clamp한다
    public void Heal(float amount)
    {
        if (isDead) return;
        CurrentHp = Mathf.Min(CurrentHp + amount, data.hp);
    }
    public void ForceDie()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"{data.rewardGold}골드 획득");

        if (GoldManager.Instance != null)
            GoldManager.Instance.AddGold(data.rewardGold);

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == "Die")
                {
                    animator.SetTrigger("Die");
                    break;
                }
            }
        }

        StartCoroutine(DieRoutine());
    }
}
