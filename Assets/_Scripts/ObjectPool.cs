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
            // 프리팹이 비었거나 깨진 항목이 하나 있어도 나머지 풀은 정상 등록되도록 건너뛴다
            if (item.prefab == null)
            {
                Debug.LogError($"[ObjectPool] '{item.poolKey}' 항목의 프리팹이 비어 있습니다(None/깨진 참조). 이 항목만 건너뜁니다.");
                continue;
            }
            if (poolDictionary.ContainsKey(item.poolKey))
            {
                Debug.LogError($"[ObjectPool] '{item.poolKey}' 키가 중복 등록되어 있습니다. 뒤의 항목은 무시합니다.");
                continue;
            }

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