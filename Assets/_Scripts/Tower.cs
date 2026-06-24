using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public enum TargetPriority { First, Last, Strong, Weak }

    public TowerData data;

    [Header("Targeting Settings")]
    public TargetPriority currentPriority = TargetPriority.First;
    public bool isSelected = false;

    private int upgradeLevel = 0;
    private float currentDamage;

    void Start()
    {
        Debug.Log("Tower Start");

        if (data == null)
        {
            Debug.LogError("TowerData가 연결되지 않았습니다!");
            return;
        }

        currentDamage = data.damage;
        StartCoroutine(AttackRoutine());
    }

    void Update()
    {
        if (!isSelected) return;

        if (Input.GetKeyDown(KeyCode.U)) SetPriority(TargetPriority.First);
        if (Input.GetKeyDown(KeyCode.I)) SetPriority(TargetPriority.Last);
        if (Input.GetKeyDown(KeyCode.O)) SetPriority(TargetPriority.Strong);
        if (Input.GetKeyDown(KeyCode.P)) SetPriority(TargetPriority.Weak);
    }

    void SetPriority(TargetPriority newPriority)
    {
        currentPriority = newPriority;
        Debug.Log($"[{gameObject.name}] 타겟팅 변경 -> {newPriority}");
    }

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
            {
                Debug.Log("타겟 발견 : " + target.name);

                target.TakeDamage(currentDamage);
                StartCoroutine(FlashColor());
            }
            else
            { 
                
            }
        }
    }

    IEnumerator FlashColor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr == null)
            yield break;

        Color originalColor = sr.color;
        sr.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        sr.color = originalColor;
    }

    Enemy FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            data.range
        );



        if (hits.Length == 0)
            return null;

        Enemy bestTarget = null;

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy == null)
                continue;

            if (bestTarget == null)
            {
                bestTarget = enemy;
                continue;
            }

            switch (currentPriority)
            {
                case TargetPriority.First:
                    if (IsFurtherAhead(enemy, bestTarget))
                        bestTarget = enemy;
                    break;

                case TargetPriority.Last:
                    if (!IsFurtherAhead(enemy, bestTarget))
                        bestTarget = enemy;
                    break;

                case TargetPriority.Strong:
                    if (enemy.CurrentHp > bestTarget.CurrentHp)
                    {
                        bestTarget = enemy;
                    }
                    else if (Mathf.Approximately(enemy.CurrentHp, bestTarget.CurrentHp))
                    {
                        if (IsFurtherAhead(enemy, bestTarget))
                            bestTarget = enemy;
                    }
                    break;

                case TargetPriority.Weak:
                    if (enemy.CurrentHp < bestTarget.CurrentHp)
                    {
                        bestTarget = enemy;
                    }
                    else if (Mathf.Approximately(enemy.CurrentHp, bestTarget.CurrentHp))
                    {
                        if (IsFurtherAhead(enemy, bestTarget))
                            bestTarget = enemy;
                    }
                    break;
            }
        }

        return bestTarget;
    }

    bool IsFurtherAhead(Enemy a, Enemy b)
    {
        if (a.CurrentIndex != b.CurrentIndex)
            return a.CurrentIndex > b.CurrentIndex;

        Transform waypoint = WaypointManager.Instance.GetWaypoint(a.CurrentIndex);

        if (waypoint == null)
            return false;

        Vector3 wpPos = waypoint.position;

        float xDiffA = wpPos.x - a.transform.position.x;
        float yDiffA = wpPos.y - a.transform.position.y;
        float distA = Mathf.Sqrt((xDiffA * xDiffA) + (yDiffA * yDiffA));

        float xDiffB = wpPos.x - b.transform.position.x;
        float yDiffB = wpPos.y - b.transform.position.y;
        float distB = Mathf.Sqrt((xDiffB * xDiffB) + (yDiffB * yDiffB));

        return distA < distB;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            data != null ? data.range : 1f
        );
    }
}