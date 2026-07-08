using UnityEngine;

namespace _Scripts.Tower.Attacks
{
    public class AreaAttack : TowerAttack
    {
        private readonly Collider[] hitBuffer = new Collider[32];

        public override void Execute(global::Tower tower, Enemy target)
        {
            if (tower == null || target 
                == null)
                return;

            AreaAttackData areaData = tower.Data.attackData as AreaAttackData;

            if (areaData == null)
            {
                Debug.LogWarning($"{tower.name}의 attackData가 AreaAttackData가 아닙니다. 단일 공격으로 처리합니다.");
                target.TakeDamage(tower.CurrentDamage);
                return;
            }

            float radius = areaData.splashRadius * (1f + areaData.upgradeSplashPercent * tower.UpgradeLevel);

            int hitCount = Physics.OverlapSphereNonAlloc(target.transform.position, radius, hitBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                Enemy enemy = hitBuffer[i].GetComponent<Enemy>();

                if (enemy == null)
                    continue;

                enemy.TakeDamage(tower.CurrentDamage);
            }
        }
    }
}