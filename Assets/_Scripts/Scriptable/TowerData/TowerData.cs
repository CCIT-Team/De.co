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
    public GameObject attackEffectPrefab;
    
    [Header("Attack Data")]
    public TowerAttackData attackData;

    [Header("Support")]
    public bool isSupportOnly = false;

    public TowerSupportData supportData;

    [Header("Economy")]
    public TowerEconomyData economyData; 
}