using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject towerPrefab;

    [SerializeField]
    private float checkRadius = 1f;

    [SerializeField]
    private LayerMask towerLayer;

    public void SpawnTower(Vector3 spawnPosition)
    {
        Collider[] colliders = Physics.OverlapSphere(spawnPosition, checkRadius, towerLayer);

        if (colliders.Length > 0)
        {
            return;
        }

        Instantiate(towerPrefab, spawnPosition, Quaternion.identity);
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
