using UnityEngine;

public class DotAreaAttack : TowerAttack
{
    private readonly Collider2D[] hitBuffer = new Collider2D[32];

    public override void Execute(Tower tower, Enemy target)
    {
        if (tower == null || target == null)
            return;

        DotAreaAttackData dotData = tower.Data.attackData as DotAreaAttackData;

        if (dotData == null)
        {
            Debug.LogWarning($"{tower.name}의 attackData가 DotAreaAttackData가 아닙니다. 단일 공격으로 처리합니다.");
            target.TakeDamage(tower.CurrentDamage);
            return;
        }

        // 1. 가스 살포: 범위 내 모든 적(공중 포함) 즉발 데미지
        int hitCount = Physics2D.OverlapCircleNonAlloc(target.transform.position, dotData.areaRadius, hitBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            Enemy enemy = hitBuffer[i].GetComponent<Enemy>();

            if (enemy == null)
                continue;

            enemy.TakeDamage(dotData.sprayDamage);
        }

        // 2. 독가스 장판 생성: 지상 적만 틱 데미지 (GasCloud가 처리)
        GasCloud.Spawn(target.transform.position, dotData.areaRadius, dotData.dotDamage, dotData.duration, dotData.tickInterval);
    }
}