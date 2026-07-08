using UnityEngine;

[CreateAssetMenu(fileName = "TowerSupportData", menuName = "Data/Tower Support")]
public class TowerSupportData : ScriptableObject
{
    public float auraRange = 4f;
    public float damageBuffPercent = 0.1f;
    public float attackSpeedBuffPercent = 0.15f;
    public float rangeBuffPercent = 0.1f;
    public float upgradeDiscountPercent = 0.1f;
}