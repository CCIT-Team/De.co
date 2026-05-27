using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Data/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float hp = 10f;         // 체력
    public float speed = 2f;        // 이동속도
    public float attackPower = 10f; // 공격력
}