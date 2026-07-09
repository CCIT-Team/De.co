using UnityEngine;

// 이 컴포넌트가 붙은 콜라이더 영역에는 타워를 설치할 수 없다 (바위, 건물 같은 장애물용).
// 사용법: 빈 오브젝트에 BoxCollider(Is Trigger 체크) 추가 -> 이 컴포넌트 추가 -> 장애물 위치에 맞게 배치
// (길은 웨이포인트를 따라 자동으로 금지되므로 이걸 깔 필요 없다)
public class NoBuildZone : MonoBehaviour
{
    void OnDrawGizmos()
    {
        // 에디터에서 금지 구역이 잘 보이도록 반투명 빨간 박스 표시
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}