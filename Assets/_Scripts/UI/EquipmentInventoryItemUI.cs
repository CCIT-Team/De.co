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
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (equipment == null) return;

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