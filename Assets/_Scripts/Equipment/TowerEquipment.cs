using UnityEngine;

// 타워에 붙어서 장비 효과를 실행하는 컴포넌트.
// 타워 쪽에서는 딱 두 가지만 해주면 된다:
//   1) 스탯 계산 시 Damage/AttackSpeed/RangeMultiplier를 곱한다
//   2) 공격이 나갈 때마다 NotifyHit(this, target)를 호출한다
public class TowerEquipment : MonoBehaviour
{
    [Header("장착된 장비 (최대 2개, 빈 칸은 null)")]
    public EquipmentData[] equipments = new EquipmentData[2];

    // 강화 탄띠용: 장비 칸별 누적 타격 수
    private int[] hitCounts = new int[2];

    // 배치 시 로드아웃의 장비 목록을 넘겨받는다 (TowerPlacementManager에서 호출)
    public void SetEquipments(EquipmentData[] source)
    {
        if (source == null) return;

        equipments = new EquipmentData[source.Length];
        hitCounts = new int[source.Length];

        for (int i = 0; i < source.Length; i++)
            equipments[i] = source[i];
    }

    // ===== 스탯 배율 (여러 장비의 %는 합산 후 적용) =====
    // 예: +150%와 -10%가 같이 붙으면 1 + (150 - 10) / 100 = 2.4배

    public float DamageMultiplier => GetMultiplier(StatType.Damage);
    public float AttackSpeedMultiplier => GetMultiplier(StatType.AttackSpeed);
    public float RangeMultiplier => GetMultiplier(StatType.Range);

    private enum StatType { Damage, AttackSpeed, Range }

    private float GetMultiplier(StatType type)
    {
        float percentSum = 0f;

        foreach (EquipmentData equip in equipments)
        {
            if (equip == null) continue;

            switch (type)
            {
                case StatType.Damage: percentSum += equip.damagePercent; break;
                case StatType.AttackSpeed: percentSum += equip.attackSpeedPercent; break;
                case StatType.Range: percentSum += equip.rangePercent; break;
            }
        }

        // 감소가 너무 겹쳐도 스탯이 0 이하로 내려가지 않도록 최소 10% 보장
        return Mathf.Max(0.1f, 1f + percentSum / 100f);
    }

    // ===== 발동 효과 =====
    // 타워가 공격을 실행한 직후 호출. target은 이번 공격의 (주)대상
    public void NotifyHit(Tower tower, Enemy target)
    {
        if (tower == null || target == null) return;

        for (int i = 0; i < equipments.Length; i++)
        {
            EquipmentData equip = equipments[i];
            if (equip == null) continue;

            // 냉각체: 공격당한 적 둔화
            if (equip.slowPercent > 0f)
            {
                ISlowable slowable = target as ISlowable;
                if (slowable != null)
                    slowable.ApplySlow(equip.slowPercent, equip.slowDuration);
            }

            // 강화 탄띠: N타마다 추가 공격
            if (equip.bonusHitInterval > 0)
            {
                hitCounts[i]++;
                if (hitCounts[i] >= equip.bonusHitInterval)
                {
                    hitCounts[i] = 0;
                    float bonusDamage = tower.CurrentDamage * equip.bonusHitPercent / 100f;
                    target.TakeDamage(bonusDamage);
                }
            }
        }
    }
}