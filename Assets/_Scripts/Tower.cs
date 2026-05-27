using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public TowerData data;

    void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / data.attackSpeed);

            Enemy target = FindTarget();
            if (target != null)
                target.TakeDamage(data.damage);
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