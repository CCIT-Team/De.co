using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Data/TowerData")]
public class TowerData : ScriptableObject
{
    public float damage = 10f;       // 공격력
    public float attackSpeed = 1f;   // 공격 속도 (초당 횟수)
    public float range = 3f;         // 사거리

    public int maxUpgradeLevel = 5;        // 최대 업그레이드 단계
    public float upgradePercent = 0.1f;    // 단계당 공격력 증가 퍼센트 (0.1 = 10%)
}