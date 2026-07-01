using UnityEngine;

public abstract class TowerAttack : MonoBehaviour
{
    public abstract void Execute(Tower tower, Enemy target);
}