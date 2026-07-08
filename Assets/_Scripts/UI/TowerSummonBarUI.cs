using UnityEngine;

public class TowerSummonBarUI : MonoBehaviour
{
    [SerializeField] private TowerPlacementManager placementManager;
    [SerializeField] private TowerSlotUI slotPrefab;
    [SerializeField] private Transform slotContainer; // Horizontal Layout Group이 붙은 부모

    void Start()
    {
        GameObject[] towerPrefabs = placementManager.towerPrefabs;

        for (int i = 0; i < towerPrefabs.Length; i++)
        {
            TowerSlotUI slot = Instantiate(slotPrefab, slotContainer);
            slot.Setup(i, towerPrefabs[i]);
        }
    }
}