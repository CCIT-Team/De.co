using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 선택창 하단의 "보유 장비" 한 칸. 드래그해서 장비 칸에 놓으면 장착된다
public class EquipmentInventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;

    private EquipmentData equipment;
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void Setup(EquipmentData _equipment)
    {
        equipment = _equipment;

        if (equipment != null && equipment.icon != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = equipment.icon;

            // 이미 어딘가에 장착된 장비는 흐리게 표시
            bool equipped = TowerLoadout.IsEquippedAnywhere(equipment);
            iconImage.color = equipped ? new Color(1f, 1f, 1f, 0.3f) : Color.white;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (equipment == null) return;

        // 이미 장착된 장비는 드래그 불가 (해제 후 다시 장착해야 함)
        if (TowerLoadout.IsEquippedAnywhere(equipment)) return;

        TowerDragState.draggedEquipment = equipment;
        TowerDragState.fromEquipTowerSlot = -1; // 인벤토리에서 시작했다는 표시
        TowerDragState.fromEquipIndex = -1;
        TowerDragState.dropHandled = false;

        DragGhost.Show(iconImage.sprite, canvas, eventData.position, 70f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragGhost.Move(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragGhost.Hide();
        TowerDragState.Clear();
    }
}