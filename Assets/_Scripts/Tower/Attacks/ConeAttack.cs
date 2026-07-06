using UnityEngine;

public class ConeAttack : TowerAttack
{
    private readonly Collider2D[] hitBuffer = new Collider2D[32];

    public override void Execute(Tower tower, Enemy target)
    {
        if (tower == null || target == null)
            return;

        ConeAttackData coneData = tower.Data.attackData as ConeAttackData;

        if (coneData == null)
        {
            Debug.LogWarning($"{tower.name}의 attackData가 ConeAttackData가 아닙니다. 단일 공격으로 처리합니다.");
            target.TakeDamage(tower.CurrentDamage);
            return;
        }

        float attackLength = coneData.attackLength * (1f + coneData.upgradeAttackLengthPercent * tower.UpgradeLevel);
        Vector2 direction = (Vector2)(target.transform.position - tower.transform.position).normalized;

        ConeEffect.Spawn(tower.transform.position, direction, coneData.coneAngle, attackLength);

        int hitCount = Physics2D.OverlapCircleNonAlloc(tower.transform.position, attackLength, hitBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            Enemy enemy = hitBuffer[i].GetComponent<Enemy>();

            if (enemy == null)
                continue;

            Vector2 toEnemy = enemy.transform.position - tower.transform.position;

            // 부채꼴 각도(coneAngle) 안에 있는 적만 타격
            if (Vector2.Angle(direction, toEnemy) > coneData.coneAngle * 0.5f)
                continue;

            enemy.TakeDamage(tower.CurrentDamage);
        }
    }
}