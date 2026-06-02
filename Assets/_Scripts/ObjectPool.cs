using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolItem
{
    public string poolKey; // CSV의 EnemyID와 완벽하게 맞출 키값 (예: slime, bug)
    public GameObject prefab;
    public int count;
}

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    public List<PoolItem> poolItems;
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        Instance = this;
        foreach (var item in poolItems)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();
            for (int i = 0; i < item.count; i++)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }
            // 기존의 item.prefab.name 대신 인스펙터에서 설정할 poolKey를 사용합니다.
            poolDictionary.Add(item.poolKey, objectQueue);
        }
    }

    public GameObject GetFromPool(string enemyID, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(enemyID))
        {
            Debug.LogError($"[ObjectPool] '{enemyID}' 키가 없습니다! 인스펙터에서 poolKey를 확인하세요.");
            return null;
        }

        GameObject obj = poolDictionary[enemyID].Count > 0 ? poolDictionary[enemyID].Dequeue() : Instantiate(FindPrefabById(enemyID));
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(string enemyID, GameObject obj)
    {
        obj.SetActive(false);
        poolDictionary[enemyID].Enqueue(obj);
    }

    private GameObject FindPrefabById(string id)
    {
        foreach (var item in poolItems)
        {
            if (item.poolKey == id) return item.prefab;
        }
        return null;
    }
}