public class SingleTargetAttack : TowerAttack
{
    public override void Execute(Tower tower, Enemy target)
    {
        if (target == null)
            return;

        target.TakeDamage(tower.CurrentDamage);
    }
}