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
    private float currentAttackSpeed;
    private float currentRange;

    private TowerAttack attack;
    private TowerTargetCapability targetCapability;

    public TowerData Data => data;
    public int UpgradeLevel => upgradeLevel;
    public float CurrentDamage => currentDamage;
    public float CurrentRange => currentRange;
    public float CurrentAttackSpeed => currentAttackSpeed;

    void Awake()
    {
        attack = GetComponent<TowerAttack>();
        targetCapability = GetComponent<TowerTargetCapability>();
    }

    void Start()
    {
        Debug.Log("Tower Start");

        if (data == null)
        {
            Debug.LogError("TowerData가 연결되지 않았습니다!");
            enabled = false;
            return;
        }

        currentDamage = data.damage;
        currentAttackSpeed = data.attackSpeed;
        currentRange = data.range;

        if (attack == null)
        {
            Debug.LogWarning($"{gameObject.name}에 TowerAttack 컴포넌트가 없습니다. 기존 단일 공격 방식으로 임시 처리합니다.");
        }

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

        currentDamage = data.damage * (1f + data.upgradeDamagePercent * upgradeLevel);
        currentAttackSpeed = data.attackSpeed * (1f + data.upgradeAttackSpeedPercent * upgradeLevel);
        currentRange = data.range * (1f + data.upgradeRangePercent * upgradeLevel);

        Debug.Log($"업그레이드 완료! 단계: {upgradeLevel} / 공격력: {currentDamage} / 공속: {currentAttackSpeed} / 사거리: {currentRange}");
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            float delay = currentAttackSpeed > 0f ? 1f / currentAttackSpeed : 1f;
            yield return new WaitForSeconds(delay);

            Enemy target = FindTarget();

            if (target == null)
                continue;

            

            if (attack != null)
            {
                attack.Execute(this, target);
            }
            else
            {
                target.TakeDamage(currentDamage);
            }

            StartCoroutine(FlashColor());
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRange);

        if (hits.Length == 0)
            return null;

        Enemy bestTarget = null;

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy == null)
                continue;

            if (targetCapability != null && !targetCapability.CanTarget(enemy))
                continue;

            if (bestTarget == null)
            {
                bestTarget = enemy;
                continue;
            }

            if (IsBetterTarget(enemy, bestTarget))
                bestTarget = enemy;
        }

        return bestTarget;
    }

    bool IsBetterTarget(Enemy enemy, Enemy bestTarget)
    {
        switch (currentPriority)
        {
            case TargetPriority.First:
                return IsFurtherAhead(enemy, bestTarget);

            case TargetPriority.Last:
                return !IsFurtherAhead(enemy, bestTarget);

            case TargetPriority.Strong:
                if (enemy.CurrentHp > bestTarget.CurrentHp)
                    return true;

                if (Mathf.Approximately(enemy.CurrentHp, bestTarget.CurrentHp))
                    return IsFurtherAhead(enemy, bestTarget);

                return false;

            case TargetPriority.Weak:
                if (enemy.CurrentHp < bestTarget.CurrentHp)
                    return true;

                if (Mathf.Approximately(enemy.CurrentHp, bestTarget.CurrentHp))
                    return IsFurtherAhead(enemy, bestTarget);

                return false;
        }

        return false;
    }

    bool IsFurtherAhead(Enemy a, Enemy b)
    {
        if (a.CurrentIndex != b.CurrentIndex)
            return a.CurrentIndex > b.CurrentIndex;

        Transform waypoint = WaypointManager.Instance.GetWaypoint(a.CurrentIndex);

        if (waypoint == null)
            return false;

        float distA = Vector3.Distance(a.transform.position, waypoint.position);
        float distB = Vector3.Distance(b.transform.position, waypoint.position);

        return distA < distB;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float drawRange = data != null ? data.range : 1f;

        if (Application.isPlaying)
            drawRange = currentRange;

        Gizmos.DrawWireSphere(transform.position, drawRange);
    }
}