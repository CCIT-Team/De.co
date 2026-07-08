using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public enum TargetPriority { First, Last, Strong, Weak }

    private struct SupportBuffValues
    {
        public float damagePercent;
        public float attackSpeedPercent;
        public float rangePercent;
        public float upgradeDiscountPercent;
    }

    public TowerData data;

    [Header("Targeting Settings")]
    public TargetPriority currentPriority = TargetPriority.First;
    public bool isSelected = false;

    private int upgradeLevel = 0;
    private float currentDamage;
    private float currentAttackSpeed;
    private float currentRange;

    private readonly Dictionary<TowerSupportEffect, SupportBuffValues> activeSupportBuffs = new Dictionary<TowerSupportEffect, SupportBuffValues>();

    private float? modeDamage;
    private float? modeAttackSpeed;
    private float? modeRange;
    private bool? canAttackFlyingOverride;
    private bool attackLocked;

    private TowerAttack attack;
    private TowerTargetCapability targetCapability;
    private SpriteRenderer spriteRenderer;

    public TowerData Data => data;
    public int UpgradeLevel => upgradeLevel;
    public float CurrentDamage => currentDamage;
    public float CurrentRange => currentRange;
    public float CurrentAttackSpeed => currentAttackSpeed;
    public bool CanAttackFlying => canAttackFlyingOverride ?? (data != null && data.canAttackFlying);

    public float UpgradeCostMultiplier
    {
        get
        {
            float discount = 0f;

            foreach (SupportBuffValues buff in activeSupportBuffs.Values)
                discount += buff.upgradeDiscountPercent;

            return Mathf.Clamp01(1f - discount);
        }
    }

    void Awake()
    {
        attack = GetComponent<TowerAttack>();
        targetCapability = GetComponent<TowerTargetCapability>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Debug.Log("Tower Start");

        if (data == null)
        {
            Debug.LogError($"[{gameObject.name}] TowerData가 연결되지 않았습니다!", this);
            enabled = false;
            return;
        }

        RecalculateStats();

        if (data.isSupportOnly)
            return;

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

        RecalculateStats();

        Debug.Log($"업그레이드 완료! 단계: {upgradeLevel} / 공격력: {currentDamage} / 공속: {currentAttackSpeed} / 사거리: {currentRange}");
    }

    public void ApplySupportBuff(TowerSupportEffect source, float damagePercent, float attackSpeedPercent, float rangePercent, float upgradeDiscountPercent)
    {
        activeSupportBuffs[source] = new SupportBuffValues
        {
            damagePercent = damagePercent,
            attackSpeedPercent = attackSpeedPercent,
            rangePercent = rangePercent,
            upgradeDiscountPercent = upgradeDiscountPercent
        };

        RecalculateStats();
    }

    public void RemoveSupportBuff(TowerSupportEffect source)
    {
        if (activeSupportBuffs.Remove(source))
            RecalculateStats();
    }

    public void SetModeBaseStats(float damage, float attackSpeed, float range)
    {
        modeDamage = damage;
        modeAttackSpeed = attackSpeed;
        modeRange = range;

        RecalculateStats();
    }

    public void SetCanAttackFlyingOverride(bool? value)
    {
        canAttackFlyingOverride = value;
    }

    public void SetAttackLocked(bool locked)
    {
        attackLocked = locked;
    }

    void RecalculateStats()
    {
        float damageBuff = 0f;
        float attackSpeedBuff = 0f;
        float rangeBuff = 0f;

        foreach (SupportBuffValues buff in activeSupportBuffs.Values)
        {
            damageBuff += buff.damagePercent;
            attackSpeedBuff += buff.attackSpeedPercent;
            rangeBuff += buff.rangePercent;
        }

        float baseDamage = modeDamage ?? data.damage;
        float baseAttackSpeed = modeAttackSpeed ?? data.attackSpeed;
        float baseRange = modeRange ?? data.range;

        currentDamage = baseDamage * (1f + data.upgradeDamagePercent * upgradeLevel) * (1f + damageBuff);
        currentAttackSpeed = baseAttackSpeed * (1f + data.upgradeAttackSpeedPercent * upgradeLevel) * (1f + attackSpeedBuff);
        currentRange = baseRange * (1f + data.upgradeRangePercent * upgradeLevel) * (1f + rangeBuff);
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            float delay = currentAttackSpeed > 0f ? 1f / currentAttackSpeed : 1f;
            yield return new WaitForSeconds(delay);

            if (attackLocked)
                continue;

            Enemy target = FindTarget();

            if (target == null)
                continue;

            FaceTarget(target);

            if (attack != null)
            {
                attack.Execute(this, target);
            }
            else
            {
                target.TakeDamage(currentDamage);
            }
        }
    }

    void FaceTarget(Enemy target)
    {
        if (spriteRenderer == null || target == null)
            return;

        if (Mathf.Approximately(target.transform.position.x, transform.position.x))
            return;

        spriteRenderer.flipX = target.transform.position.x < transform.position.x;
    }

    Enemy FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, currentRange);

        if (hits.Length == 0)
            return null;

        Enemy bestTarget = null;

        foreach (Collider hit in hits)
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