using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlacementManager : MonoBehaviour
{
    public static TowerPlacementManager Instance;

    [Header("References")]
    public GameObject[] towerPrefabs = new GameObject[8]; // 소환 바 8슬롯에 대응하는 타워 프리팹 (비어있으면 null)

    private Tower selectedTower; // 현재 선택된(배치된) 타워

    void Awake()
    {
        Instance = this;

        // 타워 선택창에서 장착하고 들어온 경우, 인스펙터 값 대신 로드아웃을 적용
        // (선택창을 거치지 않고 이 씬을 바로 실행하면 기존 인스펙터 값 그대로 사용 -> 테스트 편의)
        if (TowerLoadout.HasAnyTower)
        {
            for (int i = 0; i < towerPrefabs.Length && i < TowerLoadout.SlotCount; i++)
                towerPrefabs[i] = TowerLoadout.towerSlots[i];
        }
    }

    void Update()
    {
        // 마우스 클릭 감지 (설치는 드래그로 하므로, 클릭은 배치된 타워 '선택' 전용)
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    void HandleMouseClick()
    {
        // 소환 바 등 UI를 클릭한 경우는 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 클릭 지점의 콜라이더를 전부 확인해서 타워를 찾음
        // (금지 구역 콜라이더가 겹쳐 있어도 타워 선택이 막히지 않도록 All 버전 사용)
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);
        foreach (Collider2D hit in hits)
        {
            Tower clickedTower = hit.GetComponent<Tower>();
            if (clickedTower != null)
            {
                SelectTower(clickedTower);
                return;
            }
        }
    }

    // 소환 바 슬롯을 드래그해서 맵에 놓았을 때 호출됨 (TowerSlotUI.OnEndDrag)
    public void PlaceTower(int slotIndex, Vector2 worldPos)
    {
        if (slotIndex < 0 || slotIndex >= towerPrefabs.Length || towerPrefabs[slotIndex] == null)
            return;

        Debug.Log($"[Placement] {towerPrefabs[slotIndex].name} 설치: {worldPos}");
        GameObject newTowerObj = Instantiate(towerPrefabs[slotIndex], worldPos, Quaternion.identity);
        Tower newTower = newTowerObj.GetComponent<Tower>();

        SelectTower(newTower);
    }

    void SelectTower(Tower tower)
    {
        // 기존에 선택되어 있던 타워는 선택 해제
        if (selectedTower != null)
        {
            selectedTower.isSelected = false;
        }

        // 새로운 타워를 선택 상태로 변경
        selectedTower = tower;
        if (selectedTower != null)
        {
            selectedTower.isSelected = true;
            Debug.Log($"[{selectedTower.gameObject.name}] 선택됨! 타겟팅 방식 변경 (U:처음 / I:마지막 / O:강함 / P:약함)");
        }
    }
}