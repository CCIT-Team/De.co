using UnityEngine;

[CreateAssetMenu(fileName = "RangerSquadData", menuName = "Data/Tower Extra/Ranger Squad")]
public class RangerSquadData : ScriptableObject
{
    [Header("Melee Mode")]
    public float meleeDamage = 30f;
    public float meleeAttackSpeed = 1f;
    public float meleeRange = 1.5f;

    [Header("Ranged Mode")]
    public float rangedDamage = 15f;
    public float rangedAttackSpeed = 1.2f;
    public float rangedRange = 4.5f;
    public bool rangedCanAttackFlying = true;

    [Header("Mode Switch")]
    public float modeSwitchCooldown = 0.3f;
}