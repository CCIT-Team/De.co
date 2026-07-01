using UnityEngine;

[CreateAssetMenu(fileName = "DotAreaAttackData", menuName = "Data/Tower Attack/Dot Area")]
public class DotAreaAttackData : TowerAttackData
{
    public float areaRadius = 1.8f;
    public float duration = 2f;
    public float dotDamage = 5f;
    public float tickInterval = 0.5f;
}