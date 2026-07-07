using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryTower
{
    public string TowerName;
    public int TowerPrice;
    public bool IsUsed;
}

    public class ShopManager : MonoBehaviour
{
    public List<InventoryTower> allTowerList = new List<InventoryTower>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
