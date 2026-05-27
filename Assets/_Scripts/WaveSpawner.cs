using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int enemyCount = 5;
        public float spawnInterval = 1f;
    }

    public Transform spawnPoint;
    public Wave[] waves;

    public float timeBetweenWaves = 5f;

    private int currentWaveIndex = 0;

    void Start()
    {
        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        while (currentWaveIndex < waves.Length)
        {
            Wave wave = waves[currentWaveIndex];

            Debug.Log((currentWaveIndex + 1) + " Wave 시작");

            for (int i = 0; i < wave.enemyCount; i++)
            {
                ObjectPool.Instance.GetFromPool(
                    spawnPoint.position,
                    spawnPoint.rotation
                );

                yield return new WaitForSeconds(wave.spawnInterval);
            }

            Debug.Log((currentWaveIndex + 1) + " Wave 종료");

            currentWaveIndex++;

            yield return new WaitForSeconds(timeBetweenWaves);
        }

        Debug.Log("모든 Wave 종료");
    }
}