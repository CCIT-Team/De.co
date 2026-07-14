using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour, IStunnable
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
    private TowerEquipment equipment; // 장착된 장비 (없으면 null)
    private Animator animator;         // 있으면 공격/기절 애니메이션 재생 (Blade Squad 등)

    // 기절 상태 (TremorBoss 패턴 등)
    private float stunEndTime;
    private Coroutine stunColorRoutine;
    private Color baseColor = Color.white;

    // 사거리 표시 원 (선택 시에만 보임)
    private LineRenderer rangeRing;
    private bool wasSelected;

    public TowerData Data => data;
    public int UpgradeLevel => upgradeLevel;
    public float CurrentDamage => currentDamage;
    public float CurrentRange => currentRange;
    public float CurrentAttackSpeed => currentAttackSpeed;
    public bool CanAttackFlying => canAttackFlyingOverride ?? (data != null && data.canAttackFlying);

    // 설치 범위(원 반지름). 데이터가 없거나 0 이하면 기본값 사용
    public float PlacementRadius => (data != null && data.placementRadius > 0f) ? data.placementRadius : 1.5f;

    // 설치 비용 (골드). 데이터가 없으면 기본값 사용
    public int BuildCost => data != null ? data.buildCost : 100;

    // 설치 높이 미세 보정 (스프라이트 여백 보정용)
    public float PlacementYOffset => data != null ? data.placementYOffset : 0f;

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
        animator = GetComponentInChildren<Animator>();

        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }

    // 애니메이터에 해당 파라미터가 실제로 있을 때만 값을 넣는다
    // (Animator 없는 타워나, 파라미터가 없는 컨트롤러에서 경고가 뜨지 않도록)
    bool HasAnimatorParam(string name)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == name) return true;
        return false;
    }

    void PlayAttackAnim()
    {
        if (HasAnimatorParam("Attack"))
            animator.SetTrigger("Attack");
    }

    void SetStunnedAnim(bool stunned)
    {
        if (HasAnimatorParam("Stunned"))
            animator.SetBool("Stunned", stunned);
    }

    // 보스 스킬 등으로 기절: 기절이 끝날 때까지 공격 불가 + 빨간색으로 표시
    public void ApplyStun(float duration)
    {
        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);

        SetStunnedAnim(true);

        if (stunColorRoutine != null)
            StopCoroutine(stunColorRoutine);
        stunColorRoutine = StartCoroutine(StunColorRoutine());
    }

    // 기절이 끝날 때까지 빨간색을 유지하다가 원래 색으로 되돌린다
    IEnumerator StunColorRoutine()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 0.3f, 0.3f);

        while (Time.time < stunEndTime)
            yield return null;

        if (spriteRenderer != null)
            spriteRenderer.color = baseColor;

        SetStunnedAnim(false);

        stunColorRoutine = null;
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

        ShowPlacementRing();

        if (data.isSupportOnly)
            return;

        if (attack == null)
        {
            Debug.LogWarning($"{gameObject.name}에 TowerAttack 컴포넌트가 없습니다. 기존 단일 공격 방식으로 임시 처리합니다.");
        }

        StartCoroutine(AttackRoutine());
    }

    // 설치 범위를 노란 원으로 상시 표시한다 (타워 발밑, 바닥 높이)
    void ShowPlacementRing()
    {
        LineRenderer ring = GroundRing.Create(transform, 0.2f);

        float baseY = spriteRenderer != null ? spriteRenderer.bounds.min.y : transform.position.y;
        Vector3 center = new Vector3(transform.position.x, baseY + 0.02f, transform.position.z);

        GroundRing.Draw(ring, center, PlacementRadius, new Color(1f, 0.9f, 0.3f, 0.35f));
    }

    // 공격 사거리를 빨간 원으로 표시 (선택된 타워만)
    void ShowRangeRing()
    {
        if (data != null && data.isSupportOnly) return; // 공격 안 하는 지원 타워는 생략

        if (rangeRing == null)
            rangeRing = GroundRing.Create(transform, 0.2f);

        float baseY = spriteRenderer != null ? spriteRenderer.bounds.min.y : transform.position.y;
        Vector3 center = new Vector3(transform.position.x, baseY + 0.02f, transform.position.z);

        GroundRing.Draw(rangeRing, center, currentRange, new Color(1f, 0.3f, 0.3f, 0.4f));
    }

    void HideRangeRing()
    {
        GroundRing.Hide(rangeRing);
    }

    void Update()
    {
        // 선택 상태가 바뀐 순간에만 사거리 원을 켜거나 끈다
        if (isSelected != wasSelected)
        {
            wasSelected = isSelected;
            if (isSelected) ShowRangeRing();
            else HideRangeRing();
        }

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

        // 장비 스탯 보정 (설치 시 TowerEquipment가 AddComponent되므로 여기서 늦게 찾는다)
        if (equipment == null)
            equipment = GetComponent<TowerEquipment>();

        if (equipment != null)
        {
            currentDamage *= equipment.DamageMultiplier;
            currentAttackSpeed *= equipment.AttackSpeedMultiplier;
            currentRange *= equipment.RangeMultiplier;
        }

        // 선택 중인 타워라면 업그레이드/버프로 바뀐 사거리를 원에 반영
        if (isSelected)
            ShowRangeRing();
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            float delay = currentAttackSpeed > 0f ? 1f / currentAttackSpeed : 1f;
            yield return new WaitForSeconds(delay);

            if (attackLocked)
                continue;

            // 기절 중에는 공격하지 않음
            if (Time.time < stunEndTime)
                continue;

            Enemy target = FindTarget();

            if (target == null)
                continue;

            FaceTarget(target);

            // 공격 애니메이션 재생 (Animator 있는 타워만)
            PlayAttackAnim();

            if (attack != null)
            {
                attack.Execute(this, target);
            }
            else
            {
                target.TakeDamage(currentDamage);
            }
            if (data.attackEffectPrefab != null && target != null)
            {
                // attackEffectAtTower면 타워 위치(칼 휘두르기 등), 아니면 적 위치(탄착 등)
                Vector3 fxPos = (data.attackEffectAtTower ? transform.position : target.transform.position) + Vector3.up * 0.3f;
                GameObject fx = Instantiate(data.attackEffectPrefab, fxPos, Quaternion.identity);
                Destroy(fx, data.attackEffectDuration);
            }
            // 장비 발동 효과 (냉각체 둔화, 강화 탄띠 N타 추가 공격 등)
            if (equipment != null)
                equipment.NotifyHit(this, target);
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

            // 죽었거나 죽는 중인 적, 멈춰서 비활성화된 적은 타겟에서 제외
            if (enemy.IsDead || !enemy.isActiveAndEnabled)
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

        // 설치 범위 (노란 원): 이 원끼리 겹치면 설치 불가
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, PlacementRadius);
    }
}