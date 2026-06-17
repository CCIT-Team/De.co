using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DestroyTower : MonoBehaviour
{
    [SerializeField]
    private LayerMask towerLayer;
    public void DestroyTowerAt(GameObject tower)
    {
        Destroy(tower);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
