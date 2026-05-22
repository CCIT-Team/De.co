/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform spawnPoint;
    public float spawnTime = 3.0f;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            ObjectPool.Instance.GetFromPool(spawnPoint.position, spawnPoint.rotation);
            yield return new WaitForSeconds(spawnTime);
        }
    }
}
*/