using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSupportEffect : MonoBehaviour
{
    [Header("Update")]
    public float refreshInterval = 0.5f;

    private Tower tower;
    private readonly Collider[] hitBuffer = new Collider[32];
    private readonly HashSet<Tower> buffedTowers = new HashSet<Tower>();
    private readonly HashSet<Tower> currentlyInRange = new HashSet<Tower>();

    void Awake()
    {
        tower = GetComponent<Tower>();
    }

    void Start()
    {
        StartCoroutine(BuffRoutine());
    }

    IEnumerator BuffRoutine()
    {
        while (true)
        {
            RefreshBuffs();
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    void RefreshBuffs()
    {
        TowerSupportData supportData = tower != null && tower.Data != null ? tower.Data.supportData : null;

        if (supportData == null)
            return;

        currentlyInRange.Clear();

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, supportData.auraRange, hitBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            Tower other = hitBuffer[i].GetComponent<Tower>();

            if (other == null || other == tower)
                continue;

            currentlyInRange.Add(other);

            if (!buffedTowers.Contains(other))
            {
                Debug.Log($"[{tower.name}] 버프 시작 -> {other.name} " +
                    $"(공격력 +{supportData.damageBuffPercent:P0}, 공속 +{supportData.attackSpeedBuffPercent:P0}, " +
                    $"사거리 +{supportData.rangeBuffPercent:P0}, 업글비용 -{supportData.upgradeDiscountPercent:P0})");
            }

            other.ApplySupportBuff(
                this,
                supportData.damageBuffPercent,
                supportData.attackSpeedBuffPercent,
                supportData.rangeBuffPercent,
                supportData.upgradeDiscountPercent);
        }

        foreach (Tower previous in buffedTowers)
        {
            if (previous != null && !currentlyInRange.Contains(previous))
            {
                previous.RemoveSupportBuff(this);
                Debug.Log($"[{tower.name}] 버프 해제 -> {previous.name}");
            }
        }

        buffedTowers.Clear();
        buffedTowers.UnionWith(currentlyInRange);
    }

    void OnDestroy()
    {
        foreach (Tower buffed in buffedTowers)
        {
            if (buffed != null)
                buffed.RemoveSupportBuff(this);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        float drawRange = tower != null && tower.Data != null && tower.Data.supportData != null
            ? tower.Data.supportData.auraRange
            : 3f;

        Gizmos.DrawWireSphere(transform.position, drawRange);
    }
}