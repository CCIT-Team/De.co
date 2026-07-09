using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// 인게임 왼쪽 아래 소환 바의 슬롯 하나.
// 슬롯을 드래그해서 맵 위에 놓으면 그 자리에 타워가 설치된다
public class TowerSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI priceText;

    private int slotIndex;
    private Canvas canvas;
    private bool dragging;
    private int cost;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }
    void OnEnable()
    {
        GoldManager.OnGoldChanged += OnGoldChanged;
    }

    void OnDisable()
    {
        GoldManager.OnGoldChanged -= OnGoldChanged;
    }

    private void OnGoldChanged(int gold)
    {
        if (priceText != null && cost > 0)
            priceText.color = gold >= cost ? Color.green : Color.red;
    }

    // TowerSummonBarUI가 슬롯을 생성할 때 호출해서 어떤 타워를 보여줄지 세팅
    public void Setup(int index, GameObject towerPrefab)
    {
        slotIndex = index;

        if (towerPrefab == null)
        {
            // 아직 배정된 타워가 없는 빈 슬롯
            if (priceText != null) priceText.text = "";
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

        if (priceText != null)
        {
            Tower tower = towerPrefab.GetComponent<Tower>();
            cost = tower != null ? tower.BuildCost : 0;
            priceText.text = cost > 0 ? $"${cost}" : "";
            
            // 시작 시점의 골드 기준으로 색 초기화
            if (GoldManager.Instance != null)
                priceText.color = GoldManager.Instance.HasGold(cost) ? Color.green : Color.red;
        }
    }

    // 예전 클릭 설치 방식의 흔적. 드래그 설치로 바뀌면서 사용하지 않지만
    // 프리팹의 Button OnClick 연결이 깨지지 않도록 남겨둠
    public void OnClickSlot()
    {
    }

    // 드래그 시작 (미리보기는 매니저가 월드 공간에 그려주므로 UI 고스트는 쓰지 않는다)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!iconImage.enabled) return; // 빈 슬롯은 드래그할 것이 없음

        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || TowerPlacementManager.Instance == null) return;

        // 드래그 중에는 항상 설치 미리보기 표시
        // (투명한 UI가 맵을 덮고 있어도 미리보기가 끊기지 않도록 UI 감지에 의존하지 않는다)
        TowerPlacementManager.Instance.ShowPlacementPreview(slotIndex, eventData.position);
    }

    // 드래그 끝: 맵 위에서 놓았으면 그 자리에 설치 요청
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;

        if (TowerPlacementManager.Instance != null)
            TowerPlacementManager.Instance.HidePlacementPreview();

        // 소환 바 위에서 놓았으면 설치 취소 (그 외 UI는 설치를 막지 않는다)
        GameObject hovered = eventData.pointerCurrentRaycast.gameObject;
        if (hovered != null && transform.parent != null && hovered.transform.IsChildOf(transform.parent))
            return;

        // 화면 좌표를 그대로 넘기면 매니저가 바닥 평면 위치로 변환해서 설치한다 (3D 대응)
        TowerPlacementManager.Instance.PlaceTower(slotIndex, eventData.position);
    }
}