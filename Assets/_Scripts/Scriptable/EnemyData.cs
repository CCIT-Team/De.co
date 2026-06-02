using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Data/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float hp = 10f;         // Ã¼·Â
    public float speed = 2f;        // ÀÌµ¿¼Óµµ
    public float attackPower = 10f; // °ø°Ý·Â
    public int Gold = 5; // °ñµå È¹µæ·®
}