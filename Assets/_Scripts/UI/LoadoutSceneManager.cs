using UnityEngine;
using UnityEngine.SceneManagement;

// 타워 선택창 씬의 총괄 매니저.
// 장착 칸 8개와 하단 목록(타워/장비)을 자동 생성하고, 게임 시작 버튼을 처리한다
public class LoadoutSceneManager : MonoBehaviour
{
    [Header("보유 타워 목록 (전체 타워 프리팹을 등록)")]
    public GameObject[] allTowerPrefabs;

    [Header("보유 장비 목록 (전체 장비 에셋을 등록)")]
    public EquipmentData[] allEquipments;

    [Header("장착 칸 (4x2)")]
    [SerializeField] private LoadoutSlotUI slotPrefab;
    [SerializeField] private Transform slotContainer;   // Grid Layout Group이 붙은 상단 패널

    [Header("하단 목록")]
    [SerializeField] private TowerInventoryItemUI towerItemPrefab;
    [SerializeField] private EquipmentInventoryItemUI equipItemPrefab;
    [SerializeField] private Transform itemContainer;   // Grid Layout Group이 붙은 하단 패널

    [Header("게임 씬 이름")]
    public string gameSceneName = "tlqkf";

    private LoadoutSlotUI[] slots;

    void Start()
    {
        // 장착 칸 8개 생성
        slots = new LoadoutSlotUI[TowerLoadout.SlotCount];
        for (int i = 0; i < TowerLoadout.SlotCount; i++)
        {
            LoadoutSlotUI slot = Instantiate(slotPrefab, slotContainer);
            slot.Setup(i, this);
            slots[i] = slot;
        }

        // 하단은 타워 목록으로 시작
        ShowTowerList();
    }

    // 하단 목록을 전부 지우기 (타워<->장비 전환할 때 사용)
    private void ClearItems()
    {
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);
    }

    // 하단 목록을 "보유 타워"로 채우기
    public void ShowTowerList()
    {
        ClearItems();
        foreach (GameObject towerPrefab in allTowerPrefabs)
        {
            TowerInventoryItemUI item = Instantiate(towerItemPrefab, itemContainer);
            item.Setup(towerPrefab, this);
        }
    }

    // 하단 목록을 "보유 장비"로 채우기 (장비 칸을 클릭하면 호출됨)
    public void ShowEquipmentList()
    {
        ClearItems();
        foreach (EquipmentData equipment in allEquipments)
        {
            EquipmentInventoryItemUI item = Instantiate(equipItemPrefab, itemContainer);
            item.Setup(equipment);
        }
    }

    // 타워가 있는 장착 칸을 클릭했을 때:
    // 그 칸의 장비 패널만 열고(이미 열려있으면 닫음) 나머지 칸은 전부 닫기. 하단은 타워 목록으로 복귀
    public void OnTowerSlotClicked(LoadoutSlotUI clicked)
    {
        bool willShow = !clicked.IsEquipPanelVisible;

        foreach (LoadoutSlotUI slot in slots)
            slot.SetEquipPanelVisible(false);

        if (willShow)
            clicked.SetEquipPanelVisible(true);

        ShowTowerList();
    }

    // 보유 타워를 클릭했을 때: 첫 번째 빈 칸에 장착 (드래그와 별개의 편의 기능)
    public bool EquipToFirstEmptySlot(GameObject towerPrefab)
    {
        if (towerPrefab == null) return false;

        // 이미 장착된 타워는 중복 장착 불가
        if (TowerLoadout.Contains(towerPrefab))
        {
            Debug.Log($"[Loadout] {towerPrefab.name}은(는) 이미 장착되어 있음");
            return false;
        }

        for (int i = 0; i < TowerLoadout.SlotCount; i++)
        {
            if (TowerLoadout.towerSlots[i] == null)
            {
                TowerLoadout.SetTower(i, towerPrefab);
                slots[i].Refresh();
                Debug.Log($"[Loadout] {i}번 칸에 {towerPrefab.name} 장착");
                return true;
            }
        }

        Debug.Log("[Loadout] 빈 칸이 없음");
        return false;
    }

    // 모든 장착 칸의 아이콘(장비 칸 포함)을 현재 TowerLoadout 상태로 다시 그리기
    public void RefreshAllSlots()
    {
        foreach (LoadoutSlotUI slot in slots)
            slot.Refresh();
    }

    // "게임 시작" 버튼에 연결할 함수
    public void OnClickStartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}