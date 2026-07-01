using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Data/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("Identity")]
    public string towerID;

    public string towerName;

    public string attackType;

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

    [Header("Attack Data")]
    public TowerAttackData attackData;

    [Header("Support")]
    public TowerSupportData supportData;

    [Header("Economy")]
    public TowerEconomyData economyData;
}