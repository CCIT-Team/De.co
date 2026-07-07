using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 타워 선택창의 장착 칸(4x2 중 하나).
// - 인벤토리/다른 칸에서 드래그한 타워를 받아서 장착 (OnDrop)
// - 장착된 타워를 드래그해서 칸 밖에 놓으면 장착 해제
// - 타워가 있는 칸을 클릭하면 밑에 장비 칸 2개가 열림
public class LoadoutSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;

    [Header("장비 칸")]
    [SerializeField] private GameObject equipPanel;    // 장비 칸 2개를 담고 있는 패널 (평소엔 숨김)
    [SerializeField] private EquipSlotUI[] equipSlotUIs; // 장비 칸 2개

    private int slotIndex;
    private LoadoutSceneManager manager;
    private Canvas canvas;

    public int SlotIndex => slotIndex;
    public bool IsEquipPanelVisible => equipPanel != null && equipPanel.activeSelf;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void Setup(int index, LoadoutSceneManager _manager)
    {
        slotIndex = index;
        manager = _manager;

        if (equipSlotUIs != null)
        {
            for (int i = 0; i < equipSlotUIs.Length; i++)
                equipSlotUIs[i].Setup(this, i, manager);
        }

        SetEquipPanelVisible(false);
        Refresh();
    }

    // 장비 패널 열기/닫기 (매니저가 "다른 칸은 다 닫기" 할 때도 사용)
    public void SetEquipPanelVisible(bool visible)
    {
        if (equipPanel != null)
            equipPanel.SetActive(visible);
    }

    // TowerLoadout의 현재 상태를 읽어서 아이콘을 갱신
    public void Refresh()
    {
        GameObject towerPrefab = TowerLoadout.towerSlots[slotIndex];
        iconImage.color = Color.white;

        // 타워가 없어졌으면 장비 패널도 닫음
        if (towerPrefab == null)
        {
            iconImage.enabled = false;
            SetEquipPanelVisible(false);
        }
        else
        {
            SpriteRenderer sr = towerPrefab.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = sr.sprite;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        // 장비 칸 아이콘도 같이 갱신
        if (equipSlotUIs != null)
        {
            foreach (EquipSlotUI equipSlot in equipSlotUIs)
                equipSlot.Refresh();
        }
    }

    // 드래그하던 타워를 이 칸 위에서 놓았을 때
    public void OnDrop(PointerEventData eventData)
    {
        if (!TowerDragState.IsDragging) return;

        TowerDragState.dropHandled = true; // "칸이 받아줬다"고 기록 -> 드래그 시작한 쪽에서 장착 해제 안 함

        GameObject dragged = TowerDragState.draggedTower;
        int from = TowerDragState.fromSlotIndex;

        if (from < 0)
        {
            // 인벤토리에서 온 경우: 중복 장착만 막고, 칸에 이미 타워가 있으면 교체 (기존 타워의 장비도 함께 비워짐)
            if (TowerLoadout.Contains(dragged))
            {
                Debug.Log($"[Loadout] {dragged.name}은(는) 이미 장착되어 있음");
                return;
            }

            TowerLoadout.SetTower(slotIndex, dragged);
            Debug.Log($"[Loadout] {slotIndex}번 칸에 {dragged.name} 장착");
        }
        else
        {
            // 다른 장착 칸에서 온 경우: 자리 이동 (장비도 타워를 따라감). 대상 칸에 타워가 있으면 서로 교환
            if (from == slotIndex) return;

            TowerLoadout.SwapSlots(from, slotIndex);
            Debug.Log($"[Loadout] {from}번 칸 -> {slotIndex}번 칸 이동");
        }

        manager.RefreshAllSlots();
    }

    // 장착된 타워를 드래그하기 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (TowerLoadout.towerSlots[slotIndex] == null) return; // 빈 칸은 드래그할 것이 없음

        TowerDragState.draggedTower = TowerLoadout.towerSlots[slotIndex];
        TowerDragState.fromSlotIndex = slotIndex;
        TowerDragState.dropHandled = false;

        DragGhost.Show(iconImage.sprite, canvas, eventData.position);
        iconImage.color = new Color(1f, 1f, 1f, 0.4f); // 원래 자리는 반투명 표시
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragGhost.Move(eventData.position);
    }

    // 드래그 끝: 어떤 칸도 받아주지 않았으면(빈 공간에 놓았으면) 장착 해제
    public void OnEndDrag(PointerEventData eventData)
    {
        // 이 칸에서 시작한 드래그가 아니면 무시 (빈 칸을 끌었을 때 등)
        if (TowerDragState.fromSlotIndex != slotIndex) return;

        DragGhost.Hide();

        if (!TowerDragState.dropHandled)
        {
            Debug.Log($"[Loadout] {slotIndex}번 칸 장착 해제: {TowerLoadout.towerSlots[slotIndex].name}");
            TowerLoadout.ClearSlot(slotIndex); // 타워와 함께 장비도 비움
        }

        TowerDragState.Clear();
        manager.RefreshAllSlots();
    }

    // 칸 클릭 (버튼 OnClick에 연결):
    // 타워가 있으면 장비 칸 2개를 열고(다시 클릭하면 닫힘), 하단 목록을 타워 목록으로 되돌림
    public void OnClickSlot()
    {
        if (TowerLoadout.towerSlots[slotIndex] == null)
        {
            manager.ShowTowerList(); // 빈 칸 클릭도 "타워 목록으로 복귀" 역할
            return;
        }

        manager.OnTowerSlotClicked(this);
    }
}