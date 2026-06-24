using UnityEngine;

public class TowerPlacementManager : MonoBehaviour
{
    [Header("References")]
    public GameObject towerPrefab; // 설치할 타워 프리팹을 인스펙터에서 넣어주세요.

    private Tower selectedTower;   // 현재 키보드 입력을 받을 선택된 타워

    void Update()
    {
        // 마우스 좌클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    void HandleMouseClick()
    {
        // 마우스 좌표를 월드 좌표(2D)로 변환
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 클릭한 위치에 2D 콜라이더가 있는지 레이캐스트 검사
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

        // 2. 빈 공간을 클릭한 경우 -> 타워를 해당 위치에 '설치'
        if (towerPrefab != null)
        {
            // 마우스 위치에 타워 생성
            GameObject newTowerObj = Instantiate(towerPrefab, mousePos, Quaternion.identity);
            Tower newTower = newTowerObj.GetComponent<Tower>();

            // 설치 직후 생성된 타워가 바로 선택되도록 세팅
            SelectTower(newTower);
        }
    }

    void SelectTower(Tower tower)
    {
        // 이전에 선택되어 있던 타워는 선택 해제
        if (selectedTower != null)
        {
            selectedTower.isSelected = false;
        }

        // 새로운 타워를 선택 상태로 변경
        selectedTower = tower;
        if (selectedTower != null)
        {
            selectedTower.isSelected = true;
            Debug.Log($"[{selectedTower.gameObject.name}] 타워 선택됨! 단축키 입력 가능 (U:맨앞 / I:맨뒤 / O:체력많음 / P:체력적음)");
        }
    }
}
