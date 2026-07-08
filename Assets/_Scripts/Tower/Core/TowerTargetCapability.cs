using UnityEngine;

public class TowerTargetCapability : MonoBehaviour
{
    private Tower tower;

    void Awake()
    {
        tower = GetComponent<Tower>();
    }

    public bool CanTarget(Enemy enemy)
    {
        if (enemy == null || tower == null || tower.Data == null)
            return false;

        EnemyTargetAttribute attribute = enemy.GetComponent<EnemyTargetAttribute>();

        bool isFlying = attribute != null && attribute.isFlying;
        bool isStealth = attribute != null && attribute.isStealth;

        if (isFlying && !tower.CanAttackFlying)
            return false;

        if (!isFlying && !tower.Data.canAttackGround)
            return false;

        if (isStealth && !tower.Data.canDetectStealth)
            return false;

        return true;
    }
}