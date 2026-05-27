using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public TowerData data;

    private int upgradeLevel = 0;       // 현재 업그레이드 단계
    private float currentDamage;        // 실제 적용되는 공격력
    // test 1


    void Start()
    {
        currentDamage = data.damage;    // 기본 공격력으로 초기화
        StartCoroutine(AttackRoutine());
    }

    // UI에서 이 함수 호출하면 업그레이드
    public void Upgrade()
    {
        if (upgradeLevel >= data.maxUpgradeLevel)
        {
            Debug.Log("최대 업그레이드 단계입니다!");
            return;
        }

        upgradeLevel++;
        currentDamage = data.damage * (1f + data.upgradePercent * upgradeLevel);
        Debug.Log($"업그레이드 완료! 단계: {upgradeLevel} / 공격력: {currentDamage}");
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / data.attackSpeed);

            Enemy target = FindTarget();
            if (target != null)
                target.TakeDamage(currentDamage); // currentDamage 사용
        }
    }

    Enemy FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            data.range
        );

        Enemy bestTarget = null;
        int highestIndex = -1;

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && enemy.CurrentIndex > highestIndex)
            {
                highestIndex = enemy.CurrentIndex;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data != null ? data.range : 1f);
    }
}