using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 인게임 왼쪽 아래 소환 바의 슬롯 하나.
// 슬롯을 드래그해서 맵 위에 놓으면 그 자리에 타워가 설치된다
public class TowerSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;

    private int slotIndex;
    private Canvas canvas;
    private bool dragging;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    // TowerSummonBarUI가 슬롯을 생성할 때 호출해서 어떤 타워를 보여줄지 세팅
    public void Setup(int index, GameObject towerPrefab)
    {
        slotIndex = index;

        if (towerPrefab == null)
        {
            // 아직 배정된 타워가 없는 빈 슬롯
            iconImage.enabled = false;
            return;
        }

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

    // 예전 클릭 설치 방식의 흔적. 드래그 설치로 바뀌면서 사용하지 않지만
    // 프리팹의 Button OnClick 연결이 깨지지 않도록 남겨둠
    public void OnClickSlot()
    {
    }

    // 드래그 시작: 고스트 아이콘 생성
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!iconImage.enabled) return; // 빈 슬롯은 드래그할 것이 없음

        dragging = true;
        DragGhost.Show(iconImage.sprite, canvas, eventData.position, 80f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragging)
            DragGhost.Move(eventData.position);
    }

    // 드래그 끝: 맵 위에서 놓았으면 그 자리에 설치 요청
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;

        DragGhost.Hide();

        // 다른 UI(소환 바 등) 위에서 놓았으면 설치 취소
        if (eventData.pointerCurrentRaycast.gameObject != null) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        TowerPlacementManager.Instance.PlaceTower(slotIndex, worldPos);
    }
}