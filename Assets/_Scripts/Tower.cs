using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public TowerData data;

    private int upgradeLevel = 0;       // ���� ���׷��̵� �ܰ�
    private float currentDamage;        // ���� ����Ǵ� ���ݷ�

    void Start()
    {
        currentDamage = data.damage;    // �⺻ ���ݷ����� �ʱ�ȭ
        StartCoroutine(AttackRoutine());
    }

    // UI���� �� �Լ� ȣ���ϸ� ���׷��̵�
    public void Upgrade()
    {
        if (upgradeLevel >= data.maxUpgradeLevel)
        {
            Debug.Log("�ִ� ���׷��̵� �ܰ��Դϴ�!");
            return;
        }

        upgradeLevel++;
        currentDamage = data.damage * (1f + data.upgradePercent * upgradeLevel);
        Debug.Log($"���׷��̵� �Ϸ�! �ܰ�: {upgradeLevel} / ���ݷ�: {currentDamage}");
    }

    //IEnumerator AttackRoutine()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(1f / data.attackSpeed);

    //        Enemy target = FindTarget();
    //        if (target != null)
    //            target.TakeDamage(currentDamage); // currentDamage ���
    //    }
    //}
    IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / data.attackSpeed);

            Enemy target = FindTarget();
            if (target != null)
            {
                target.TakeDamage(currentDamage);
                StartCoroutine(FlashColor()); // 공격 시 색상 플래시
            }
        }
    }

    IEnumerator FlashColor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color originalColor = sr.color;
        sr.color = Color.red; // 공격 색상 (원하는 색으로 변경 가능)

        yield return new WaitForSeconds(0.1f); // 플래시 지속 시간

        sr.color = originalColor; // 원래 색으로 복구
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