using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Enemy : MonoBehaviour
{
    private EnemyData data;
    private bool isDead = false;
    public int CurrentIndex { get; set; } = 0;
    public float CurrentHp { get; private set; }

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
        if (CurrentIndex >= WaypointManager.Instance.GetWaypointCount()) return;

        Transform target = WaypointManager.Instance.GetWaypoint(CurrentIndex);
        transform.position = Vector3.MoveTowards(transform.position, target.position, data.speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            CurrentIndex++;
            if (CurrentIndex >= WaypointManager.Instance.GetWaypointCount())
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
        WaveSpawner.Instance.OnEnemyDespawn();
        ObjectPool.Instance.ReturnToPool(data.enemyID, gameObject);
    }
}