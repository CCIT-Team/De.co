using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyData data;

    private float currentHp;
    public int CurrentIndex { get; private set; } // 타워가 읽는 값

    void OnEnable()
    {
        currentHp = data.hp;
        CurrentIndex = 0; // 초기화
        StartCoroutine(MoveRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            Transform target = WaypointManager.Instance.GetWaypoint(CurrentIndex);

            while (Vector2.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    target.position,
                    data.speed * Time.deltaTime
                );
                yield return null;
            }

            CurrentIndex++; // 로컬변수 대신 CurrentIndex 사용

            if (CurrentIndex >= WaypointManager.Instance.GetWaypointCount())
            {
                ObjectPool.Instance.ReturnToPool(gameObject);
                yield break;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
            Die();
    }

    void Die()
    {
        ObjectPool.Instance.ReturnToPool(gameObject);
    }
}