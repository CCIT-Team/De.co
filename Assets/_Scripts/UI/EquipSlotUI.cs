using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 장착 칸(LoadoutSlot) 밑에 조그맣게 뜨는 장비 칸 (타워당 2개).
// - 클릭하면 하단 목록이 장비 목록으로 전환됨
// - 장비를 드래그해서 놓으면 장착, 장착된 장비를 밖으로 드래그하면 해제
public class EquipSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;

    private LoadoutSlotUI parentSlot; // 이 장비 칸이 붙어있는 타워 장착 칸
    private int equipIndex;           // 0 또는 1
    private LoadoutSceneManager manager;
    private Canvas canvas;

    private int TowerSlotIndex => parentSlot.SlotIndex;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void Setup(LoadoutSlotUI _parentSlot, int _equipIndex, LoadoutSceneManager _manager)
    {
        parentSlot = _parentSlot;
        equipIndex = _equipIndex;
        manager = _manager;
        Refresh();
    }

    // TowerLoadout의 현재 상태를 읽어서 아이콘을 갱신
    public void Refresh()
    {
        EquipmentData equip = TowerLoadout.equipSlots[TowerSlotIndex][equipIndex];
        iconImage.color = Color.white;

        if (equip == null || equip.icon == null)
        {
            iconImage.enabled = false;
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = equip.icon;
    }

    // 장비 칸 클릭 -> 하단 목록을 장비 목록으로 전환 (버튼 OnClick에 연결)
    public void OnClickEquipSlot()
    {
        manager.ShowEquipmentList();
    }

    // 드래그하던 것을 이 장비 칸 위에서 놓았을 때
    public void OnDrop(PointerEventData eventData)
    {
        // 타워를 여기에 놓은 경우: 부모 장착 칸이 처리하도록 넘겨줌
        if (TowerDragState.draggedTower != null)
        {
            parentSlot.OnDrop(eventData);
            return;
        }

        if (TowerDragState.draggedEquipment == null) return;

        // 타워가 없는 칸에는 장비를 달 수 없음 (드래그는 받아준 것으로 처리해서 원래 자리 유지)
        if (TowerLoadout.towerSlots[TowerSlotIndex] == null)
        {
            TowerDragState.dropHandled = true;
            Debug.Log("[Loadout] 타워가 없는 칸에는 장비를 장착할 수 없음");
            return;
        }

        TowerDragState.dropHandled = true;

        EquipmentData dragged = TowerDragState.draggedEquipment;
        int fromTower = TowerDragState.fromEquipTowerSlot;
        int fromIndex = TowerDragState.fromEquipIndex;

        if (fromTower < 0)
        {
            // 인벤토리에서 온 경우: 같은 타워에 같은 장비 중복 방지
            if (TowerLoadout.HasEquipment(TowerSlotIndex, dragged))
            {
                Debug.Log($"[Loadout] 이 타워에는 {dragged.equipmentName}이(가) 이미 장착되어 있음");
                return;
            }

            TowerLoadout.equipSlots[TowerSlotIndex][equipIndex] = dragged;
            Debug.Log($"[Loadout] {TowerSlotIndex}번 타워의 장비 칸 {equipIndex}에 {dragged.equipmentName} 장착");
        }
        else
        {
            // 다른 장비 칸에서 온 경우: 자리 이동/교환
            if (fromTower == TowerSlotIndex && fromIndex == equipIndex) return;

            // 다른 타워로 옮길 때 그 타워에 같은 장비가 이미 있으면 거부
            if (fromTower != TowerSlotIndex && TowerLoadout.HasEquipment(TowerSlotIndex, dragged))
            {
                Debug.Log($"[Loadout] 이 타워에는 {dragged.equipmentName}이(가) 이미 장착되어 있음");
                return;
            }

            EquipmentData temp = TowerLoadout.equipSlots[TowerSlotIndex][equipIndex];
            TowerLoadout.equipSlots[TowerSlotIndex][equipIndex] = dragged;
            TowerLoadout.equipSlots[fromTower][fromIndex] = temp;
        }

        manager.RefreshAllSlots();
    }

    // 장착된 장비를 드래그하기 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        EquipmentData equip = TowerLoadout.equipSlots[TowerSlotIndex][equipIndex];
        if (equip == null) return;

        TowerDragState.draggedEquipment = equip;
        TowerDragState.fromEquipTowerSlot = TowerSlotIndex;
        TowerDragState.fromEquipIndex = equipIndex;
        TowerDragState.dropHandled = false;

        DragGhost.Show(iconImage.sprite, canvas, eventData.position, 70f);
        iconImage.color = new Color(1f, 1f, 1f, 0.4f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragGhost.Move(eventData.position);
    }

    // 드래그 끝: 어떤 칸도 받아주지 않았으면(빈 공간에 놓았으면) 장비 해제
    public void OnEndDrag(PointerEventData eventData)
    {
        if (TowerDragState.fromEquipTowerSlot != TowerSlotIndex || TowerDragState.fromEquipIndex != equipIndex) return;

        DragGhost.Hide();

        if (!TowerDragState.dropHandled)
        {
            Debug.Log($"[Loadout] 장비 해제: {TowerLoadout.equipSlots[TowerSlotIndex][equipIndex].equipmentName}");
            TowerLoadout.equipSlots[TowerSlotIndex][equipIndex] = null;
        }

        TowerDragState.Clear();
        manager.RefreshAllSlots();
    }
}