using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlacementManager : MonoBehaviour
{
    public static TowerPlacementManager Instance;

    // 타워가 설치되는 바닥 평면의 높이 (3D 맵의 바닥 y좌표)
    private const float GroundHeight = 1.6f;

    [Header("References")]
    public GameObject[] towerPrefabs = new GameObject[8]; // 소환 바 8슬롯에 대응하는 타워 프리팹 (비어있으면 null)

    [Header("Placement Area")]
    [Tooltip("설치 가능한 바닥 영역(BoxCollider). 지정하면 이 영역 안에서만 설치/미리보기가 된다 (배경 뒤로 새는 것 방지)")]
    [SerializeField] private Collider groundArea;

    [Tooltip("길(웨이포인트 경로) 중심선에서 이 거리 안에는 설치 불가. 길 폭의 절반 정도로 설정")]
    [SerializeField] private float pathClearance = 1f;

    [Header("Sell")]
    [Tooltip("타워 판매 시 설치 비용의 몇 %를 돌려줄지 (0.5 = 50%)")]
    [SerializeField] private float sellRefundPercent = 0.5f;

    private Tower selectedTower; // 현재 선택된(배치된) 타워

    private LineRenderer previewRing;     // 드래그 중 설치 가능/불가를 보여주는 미리보기 링
    private SpriteRenderer previewGhost;  // 드래그 중 실제 설치될 크기로 보여주는 반투명 타워 고스트

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
        // 좌클릭: 배치된 타워 '선택' (타겟팅 변경용). 설치는 소환 바 드래그로 함
        if (Input.GetMouseButtonDown(0))
        {
            Tower clicked = GetTowerUnderMouse();
            if (clicked != null)
                SelectTower(clicked);
        }

        // 우클릭: 타워 판매 (설치 비용의 sellRefundPercent만큼 골드로 환급)
        if (Input.GetMouseButtonDown(1))
        {
            Tower clicked = GetTowerUnderMouse();
            if (clicked != null)
                SellTower(clicked);
        }
    }

    // 마우스 위치에서 Ray를 쏴 타워를 찾는다 (UI 위 클릭이면 null)
    Tower GetTowerUnderMouse()
    {
        // 소환 바 등 UI를 클릭한 경우는 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return null;

        // 원근 카메라이므로 Ray를 쏴서 타워 콜라이더를 찾는다
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
            return hit.collider.GetComponent<Tower>();

        return null;
    }

    // 타워를 판매한다: 설치 비용의 일정 비율을 골드로 돌려주고 제거
    void SellTower(Tower tower)
    {
        int refund = Mathf.RoundToInt(tower.BuildCost * sellRefundPercent);

        if (GoldManager.Instance != null)
            GoldManager.Instance.AddGold(refund);

        if (selectedTower == tower)
            selectedTower = null;

        Debug.Log($"[Sell] {tower.gameObject.name} 판매 -> +{refund} 골드 환급");

        Destroy(tower.gameObject);
    }

    // 소환 바 슬롯을 드래그해서 맵에 놓았을 때 호출됨 (TowerSlotUI.OnEndDrag)
    // screenPos: 마우스를 놓은 화면 좌표 (월드 좌표 변환은 여기서 처리)
    public void PlaceTower(int slotIndex, Vector2 screenPos)
    {
        if (slotIndex < 0 || slotIndex >= towerPrefabs.Length || towerPrefabs[slotIndex] == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (!TryGetGroundPoint(ray, out Vector3 worldPos))
            return;

        // 설치 규칙 검사 (타워 겹침 / 길 / 장애물)
        if (!CanPlaceAt(worldPos, GetPlacementRadius(towerPrefabs[slotIndex])))
        {
            Debug.Log("[Placement] 설치 불가: 다른 타워/길/장애물과 겹칩니다");
            return;
        }

        // 골드 지불 (부족하면 설치 취소)
        int cost = GetBuildCost(towerPrefabs[slotIndex]);
        if (GoldManager.Instance != null && !GoldManager.Instance.ConsumeGold(cost))
        {
            Debug.Log($"[Placement] 골드 부족: {cost} 필요");
            return;
        }

        Debug.Log($"[Placement] {towerPrefabs[slotIndex].name} 설치: {worldPos}");
        GameObject newTowerObj = Instantiate(towerPrefabs[slotIndex], worldPos, Quaternion.identity);
        Tower newTower = newTowerObj.GetComponent<Tower>();

        // 스프라이트가 땅에 파묻히지 않게, 그림의 맨 아래가 바닥에 닿도록 높이 보정
        SpriteRenderer towerSprite = newTowerObj.GetComponentInChildren<SpriteRenderer>();
        if (towerSprite != null)
        {
            float sinkDepth = worldPos.y - towerSprite.bounds.min.y;
            newTowerObj.transform.position += Vector3.up * sinkDepth;
        }

        // 타워 선택창에서 이 타워 칸에 장착한 장비를 전달
        TowerEquipment equipment = newTowerObj.GetComponent<TowerEquipment>();
        if (equipment == null)
            equipment = newTowerObj.AddComponent<TowerEquipment>();
        equipment.SetEquipments(TowerLoadout.equipSlots[slotIndex]);

        SelectTower(newTower);
    }

    // 드래그 중 매 프레임 호출: 놓을 위치에 설치 가능(초록)/불가(빨강) 링을 표시 (TowerSlotUI에서 호출)
    public void ShowPlacementPreview(int slotIndex, Vector2 screenPos)
    {
        if (slotIndex < 0 || slotIndex >= towerPrefabs.Length || towerPrefabs[slotIndex] == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (!TryGetGroundPoint(ray, out Vector3 worldPos))
        {
            HidePlacementPreview();
            return;
        }

        if (previewRing == null)
            previewRing = GroundRing.Create(transform, 0.25f);

        float radius = GetPlacementRadius(towerPrefabs[slotIndex]);
        bool canPlace = CanPlaceAt(worldPos, radius);

        // 골드가 부족해도 설치 불가(빨강) 표시
        if (GoldManager.Instance != null && !GoldManager.Instance.HasGold(GetBuildCost(towerPrefabs[slotIndex])))
            canPlace = false;

        Color color = canPlace ? new Color(0.3f, 1f, 0.3f, 0.8f) : new Color(1f, 0.25f, 0.25f, 0.8f);
        GroundRing.Draw(previewRing, worldPos + Vector3.up * 0.03f, radius, color);

        UpdatePreviewGhost(towerPrefabs[slotIndex], worldPos, canPlace);
    }

    // 설치될 자리에 타워를 반투명으로 미리 보여준다.
    // 월드 공간에 그리므로 카메라 확대/원근에 따라 실제 설치 크기 그대로 보인다
    void UpdatePreviewGhost(GameObject towerPrefab, Vector3 worldPos, bool canPlace)
    {
        SpriteRenderer prefabSprite = towerPrefab.GetComponentInChildren<SpriteRenderer>();
        if (prefabSprite == null)
            return;

        if (previewGhost == null)
        {
            GameObject obj = new GameObject("PlacementGhost");
            obj.transform.SetParent(transform);
            previewGhost = obj.AddComponent<SpriteRenderer>();
        }

        previewGhost.enabled = true;
        previewGhost.sprite = prefabSprite.sprite;
        previewGhost.sortingLayerID = prefabSprite.sortingLayerID;
        previewGhost.sortingOrder = prefabSprite.sortingOrder + 1;
        previewGhost.transform.localScale = prefabSprite.transform.lossyScale; // 프리팹 스케일 그대로
        previewGhost.color = canPlace ? new Color(1f, 1f, 1f, 0.55f) : new Color(1f, 0.4f, 0.4f, 0.55f);

        // 설치될 때와 똑같이, 그림의 맨 아래가 바닥에 닿도록 배치
        previewGhost.transform.position = worldPos;
        float sinkDepth = worldPos.y - previewGhost.bounds.min.y;
        previewGhost.transform.position += Vector3.up * sinkDepth;
    }

    public void HidePlacementPreview()
    {
        GroundRing.Hide(previewRing);
        if (previewGhost != null)
            previewGhost.enabled = false;
    }

    // 설치 규칙을 한번에 검사: 타워 겹침, 길 침범, 장애물(NoBuildZone) 침범
    bool CanPlaceAt(Vector3 position, float radius)
    {
        if (IsOverlappingOtherTower(position, radius)) return false;
        if (IsOnPath(position, radius)) return false;
        if (IsInNoBuildZone(position, radius)) return false;
        return true;
    }

    // 웨이포인트 경로(길)와 너무 가까운지 검사: 설치 원의 가장자리가 길 폭에 걸치면 불가
    bool IsOnPath(Vector3 position, float radius)
    {
        WaypointManager wm = WaypointManager.Instance;
        if (wm == null) return false;

        float limit = pathClearance + radius;
        float limitSqr = limit * limit;

        int count = wm.GetWaypointCount(false);
        for (int i = 0; i < count - 1; i++)
        {
            Transform a = wm.GetWaypoint(i, false);
            Transform b = wm.GetWaypoint(i + 1, false);
            if (a == null || b == null) continue;

            if (SqrDistanceToSegmentXZ(position, a.position, b.position) < limitSqr)
                return true;
        }
        return false;
    }

    // 점 p에서 선분 a-b까지의 거리 제곱 (높이 Y는 무시하고 바닥 기준)
    float SqrDistanceToSegmentXZ(Vector3 p, Vector3 a, Vector3 b)
    {
        p.y = 0f; a.y = 0f; b.y = 0f;

        Vector3 ab = b - a;
        float abSqr = ab.sqrMagnitude;
        if (abSqr < 0.0001f) return (p - a).sqrMagnitude;

        float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / abSqr);
        Vector3 closest = a + ab * t;
        return (p - closest).sqrMagnitude;
    }

    // NoBuildZone(장애물) 콜라이더와 겹치는지 검사
    bool IsInNoBuildZone(Vector3 position, float radius)
    {
        foreach (Collider col in Physics.OverlapSphere(position, radius))
        {
            if (col.GetComponentInParent<NoBuildZone>() != null)
                return true;
        }
        return false;
    }

    // 설치하려는 위치가 기존 타워들의 설치 범위와 겹치는지 검사 (높이는 무시하고 바닥 기준 거리로 비교)
    bool IsOverlappingOtherTower(Vector3 position, float myRadius)
    {
        foreach (Tower tower in FindObjectsOfType<Tower>())
        {
            Vector3 diff = tower.transform.position - position;
            diff.y = 0f;

            float minDistance = myRadius + tower.PlacementRadius;
            if (diff.sqrMagnitude < minDistance * minDistance)
                return true;
        }
        return false;
    }

    // 프리팹의 설치 반경을 읽는다 (Tower/데이터가 없으면 기본값)
    float GetPlacementRadius(GameObject towerObj)
    {
        Tower tower = towerObj.GetComponent<Tower>();
        return tower != null ? tower.PlacementRadius : 1.5f;
    }

    // 프리팹의 설치 비용을 읽는다 (Tower/데이터가 없으면 기본값)
    int GetBuildCost(GameObject towerObj)
    {
        Tower tower = towerObj.GetComponent<Tower>();
        return tower != null ? tower.BuildCost : 100;
    }

    // 화면 Ray로 설치 지점을 구한다.
    // groundArea가 지정되어 있으면 그 콜라이더에만 판정해서 영역 밖(배경 뒤 등)은 자동 제외되고,
    // 없으면 기존처럼 무한한 바닥 평면(y = GroundHeight)과 교차시킨다
    bool TryGetGroundPoint(Ray ray, out Vector3 worldPos)
    {
        if (groundArea != null)
        {
            if (groundArea.Raycast(ray, out RaycastHit hit, 2000f))
            {
                worldPos = hit.point;
                return true;
            }
            worldPos = Vector3.zero;
            return false;
        }

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, GroundHeight, 0f));

        if (groundPlane.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
            return true;
        }

        worldPos = Vector3.zero;
        return false;
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