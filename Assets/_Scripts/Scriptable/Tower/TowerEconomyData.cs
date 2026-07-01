using UnityEngine;

[CreateAssetMenu(fileName = "TowerEconomyData", menuName = "Data/Tower Economy")]
public class TowerEconomyData : ScriptableObject
{
    public int goldPerInterval = 5;
    public float goldInterval = 3f;
}