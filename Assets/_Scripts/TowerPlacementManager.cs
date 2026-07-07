using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlacementManager : MonoBehaviour
{
    public static TowerPlacementManager Instance;

    [Header("References")]
    public GameObject[] towerPrefabs = new GameObject[8]; // 소환 바 8슬롯에 대응하는 타워 프리팹 (비어있으면 null)

    private Tower selectedTower;        // 현재 선택된(배치된) 타워
    private int selectedSlotIndex = -1; // 소환 바에서 선택한 슬롯 인덱스 (-1 = 선택 안 함)

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
        // 마우스 클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    // 소환 바 UI의 슬롯을 클릭했을 때 호출됨
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= towerPrefabs.Length || towerPrefabs[index] == null)
        {
            Debug.Log($"[Placement] 슬롯 {index} 선택 실패 (빈 슬롯이거나 프리팹 없음)");
            return;
        }

        selectedSlotIndex = index;
        Debug.Log($"[Placement] 슬롯 {index} 선택됨: {towerPrefabs[index].name}");
    }

    void HandleMouseClick()
    {
        // 소환 바 등 UI를 클릭한 경우에는 배치/선택 로직으로 넘어가지 않게 막음
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 마우스 클릭 위치를 월드(2D) 좌표로 변환
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 클릭한 위치의 2D 콜라이더를 감지하는 레이캐스트
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        // 1. 이미 배치된 타워를 클릭한 경우 -> 해당 타워를 '선택'
        if (hit.collider != null)
        {
            Tower clickedTower = hit.collider.GetComponent<Tower>();
            if (clickedTower != null)
            {
                SelectTower(clickedTower);
                return;
            }
        }

        // 2. 빈 공간을 클릭한 경우 -> 소환 바에서 선택한 타워를 '배치'
        if (selectedSlotIndex < 0)
        {
            Debug.Log("[Placement] 빈 공간 클릭했지만 선택된 슬롯이 없음");
            return;
        }

        if (towerPrefabs[selectedSlotIndex] != null)
        {
            Debug.Log($"[Placement] {towerPrefabs[selectedSlotIndex].name} 배치: {mousePos}");
            GameObject newTowerObj = Instantiate(towerPrefabs[selectedSlotIndex], mousePos, Quaternion.identity);
            Tower newTower = newTowerObj.GetComponent<Tower>();

            // 한 번 배치하면 슬롯 선택이 풀려서, 다시 설치하려면 슬롯을 또 클릭해야 함
            selectedSlotIndex = -1;

            SelectTower(newTower);
        }
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