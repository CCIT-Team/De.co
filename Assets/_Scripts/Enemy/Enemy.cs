using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Enemy : MonoBehaviour
{
    private EnemyData data;
    private bool isDead = false;
    public int CurrentIndex { get; set; } = 0;
    public float CurrentHp { get; private set; }
    public float MaxHp => data.hp;
    public bool IsDead => isDead;

    private CircleCollider2D circleCollider;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private Coroutine flashCoroutine;

    void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        SetStandardHitbox();
    }

    void SetStandardHitbox()
    {
        if (circleCollider == null) return;
        circleCollider.radius = 0.2f;
        circleCollider.offset = Vector2.zero;
        circleCollider.isTrigger = true;
    }

    public void Initialize(EnemyData _data)
    {
        data = _data;
        isDead = false;
        CurrentIndex = 0;
        CurrentHp = data.hp;

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
        if (CurrentIndex >= WaypointManager.Instance.GetWaypointCount(data.isFlying)) return;

        Transform target = WaypointManager.Instance.GetWaypoint(CurrentIndex, data.isFlying);
        transform.position = Vector3.MoveTowards(transform.position, target.position, data.speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            CurrentIndex++;
            if (CurrentIndex >= WaypointManager.Instance.GetWaypointCount(data.isFlying))
            {
                // [놓침] 타워에 죽지 않고 마지막 지점까지 살아서 통과했을 때
                WaveSpawner.Instance.OnEnemyDespawn();
                ObjectPool.Instance.ReturnToPool(data.enemyID, gameObject);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        CurrentHp -= damage;
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
