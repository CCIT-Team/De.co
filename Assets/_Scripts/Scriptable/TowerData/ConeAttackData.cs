using UnityEngine;

[CreateAssetMenu(fileName = "ConeAttackData", menuName = "Data/Tower Attack/Cone")]
public class ConeAttackData : TowerAttackData
{
    public float coneAngle = 70f;
    public float attackLength = 3f;
    public float upgradeAttackLengthPercent = 0.05f;
}