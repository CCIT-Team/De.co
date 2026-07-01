using UnityEngine;

public class AreaAttack : TowerAttack
{
    public override void Execute(Tower tower, Enemy target)
    {
        if (tower == null || target == null)
            return;

        AreaAttackData areaData = tower.Data.attackData as AreaAttackData;

        if (areaData == null)
        {
            Debug.LogWarning($"{tower.name}의 attackData가 AreaAttackData가 아닙니다. 단일 공격으로 처리합니다.");
            target.TakeDamage(tower.CurrentDamage);
            return;
        }

        float radius = areaData.splashRadius * (1f + areaData.upgradeSplashPercent * tower.UpgradeLevel);

        Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, radius);

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy == null)
                continue;

            enemy.TakeDamage(tower.CurrentDamage);
        }
    }
} 

