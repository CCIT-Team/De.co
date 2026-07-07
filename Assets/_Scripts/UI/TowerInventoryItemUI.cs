using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 선택창 하단의 "보유 타워" 한 칸.
// 드래그해서 위쪽 장착 칸에 놓으면 장착된다 (클릭으로 빈 칸에 넣는 것도 가능)
public class TowerInventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;

    private GameObject towerPrefab;
    private LoadoutSceneManager manager;
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void Setup(GameObject prefab, LoadoutSceneManager _manager)
    {
        towerPrefab = prefab;
        manager = _manager;

        SpriteRenderer sr = towerPrefab != null ? towerPrefab.GetComponent<SpriteRenderer>() : null;
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

    // 클릭(드래그 없이 눌렀다 떼기)하면 첫 번째 빈 장착 칸에 장착 - 드래그와 별개로 동작하는 편의 기능
    public void OnClickItem()
    {
        manager.EquipToFirstEmptySlot(towerPrefab);
    }

    // 드래그 시작: 무엇을 끌고 있는지 기록하고 고스트 아이콘 생성
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (towerPrefab == null) return;

        TowerDragState.draggedTower = towerPrefab;
        TowerDragState.fromSlotIndex = -1; // 인벤토리에서 시작했다는 표시
        TowerDragState.dropHandled = false;

        DragGhost.Show(iconImage.sprite, canvas, eventData.position);
    }

    // 드래그 중: 고스트가 마우스를 따라다니게
    public void OnDrag(PointerEventData eventData)
    {
        DragGhost.Move(eventData.position);
    }

    // 드래그 끝: 고스트 제거. 실제 장착 처리는 놓인 칸(LoadoutSlotUI.OnDrop)이 담당
    public void OnEndDrag(PointerEventData eventData)
    {
        DragGhost.Hide();
        TowerDragState.Clear();
    }
}