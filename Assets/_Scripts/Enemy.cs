using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyData data;
    private bool isDead = false;
    public int CurrentIndex { get; set; } = 0;

    public void Initialize(EnemyData _data)
    {
        data = _data;
        isDead = false;
        CurrentIndex = 0;
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
                WaveSpawner.Instance.OnEnemyDespawn(); // 스포너에게 카운트 감소 알림
                ObjectPool.Instance.ReturnToPool(data.enemyID, gameObject);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        data.hp -= damage;
        if (data.hp <= 0) Die();
    }

    void Die()
    {
        isDead = true;

        // [콘솔창] 타워가 몬스터를 죽였을 때만 골드 획득 로그 출력
        Debug.Log($"{data.rewardGold}골드 획득");

        WaveSpawner.Instance.OnEnemyDespawn(); // 스포너에게 카운트 감소 알림
        ObjectPool.Instance.ReturnToPool(data.enemyID, gameObject);
    }
}