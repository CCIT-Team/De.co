using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Data/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("Identity")]
    public string towerID;

    public string towerName;

    public string attackType;

    [Header("Placement")]
    [Tooltip("설치 범위(원 반지름). 다른 타워의 설치 범위와 겹치면 설치할 수 없다")]
    public float placementRadius = 1.5f;

    [Tooltip("설치 비용 (인게임 골드)")]
    public int buildCost = 100;

    [Tooltip("설치 높이 미세 보정. 스프라이트 여백 때문에 공중에 뜨면 음수로 내리고, 파묻히면 양수로 올린다")]
    public float placementYOffset = 0f;

    [Header("Basic Stats")]
    public float damage = 10f;

    public float attackSpeed = 1f;

    public float range = 3f;

    [Header("Target Capability")]
    public bool canAttackGround = true;

    public bool canAttackFlying = false;

    public bool canDetectStealth = false;

    [Header("Upgrade")]
    public int maxUpgradeLevel = 5;

    public float upgradeDamagePercent = 0.1f;

    public float upgradeAttackSpeedPercent = 0f;

    public float upgradeRangePercent = 0f;

    [Header("Effect")]
    [Tooltip("공격할 때 재생할 이펙트 프리팹 (비워두면 없음)")]
    public GameObject attackEffectPrefab;

    [Tooltip("체크하면 이펙트가 타워 위치에 뜬다 (칼 휘두르기 등). 해제하면 적 위치에 뜬다 (탄착 등)")]
    public bool attackEffectAtTower = false;

    [Tooltip("이펙트가 사라지기까지의 시간(초)")]
    public float attackEffectDuration = 0.5f;

    [Header("Attack Data")]
    public TowerAttackData attackData;

    [Header("Support")]
    public bool isSupportOnly = false;

    public TowerSupportData supportData;

    [Header("Economy")]
    public TowerEconomyData economyData; 
}