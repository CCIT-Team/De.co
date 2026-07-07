using UnityEngine;

// 장비 하나의 데이터. 스탯 보정(%)과 발동 효과를 숫자로 설정하면
// 타워가 알아서 적용한다 (0이면 해당 효과 없음)
[CreateAssetMenu(fileName = "EquipmentData", menuName = "Data/Equipment")]
public class EquipmentData : ScriptableObject
{
    public string equipmentName;
    public Sprite icon;

    [Header("스탯 보정 (%). 예: +150 = 150% 증가, -40 = 40% 감소")]
    public float damagePercent;
    public float attackSpeedPercent;
    public float rangePercent;

    [Header("둔화 효과 (냉각체) : 공격 시 적 이동속도 감소")]
    [Tooltip("공격당한 적의 이동속도 감소량(%). 0이면 효과 없음")]
    public float slowPercent;
    [Tooltip("둔화 지속 시간(초)")]
    public float slowDuration = 2f;

    [Header("N타마다 추가 공격 (강화 탄띠)")]
    [Tooltip("몇 타마다 추가 공격이 발동하는지. 0이면 효과 없음")]
    public int bonusHitInterval;
    [Tooltip("추가 공격의 데미지 (현재 공격력의 %)")]
    public float bonusHitPercent;
}