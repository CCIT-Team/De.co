using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolItem
{
    public string poolKey; // CSV�� EnemyID�� �Ϻ��ϰ� ���� Ű�� (��: slime, bug)
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
            // ������ item.prefab.name ��� �ν����Ϳ��� ������ poolKey�� ����մϴ�.
            poolDictionary.Add(item.poolKey, objectQueue);
        }
    }

    public GameObject GetFromPool(string enemyID, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(enemyID))
        {
            Debug.LogError($"[ObjectPool] '{enemyID}' Ű�� �����ϴ�! �ν����Ϳ��� poolKey�� Ȯ���ϼ���.");
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