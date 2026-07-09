using UnityEngine;

// 바닥(XZ 평면)에 평평한 원을 그리는 LineRenderer 유틸리티.
// 타워 설치 범위 표시 등에 사용한다 (Instigator 경고 이펙트와 같은 방식)
public static class GroundRing
{
    private const int Segments = 50;

    // 링용 LineRenderer를 만든다. parent가 null이면 씬 루트에 생성
    public static LineRenderer Create(Transform parent, float width = 0.05f)
    {
        GameObject obj = new GameObject("GroundRing");
        if (parent != null)
            obj.transform.SetParent(parent, false);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.positionCount = Segments;
        lr.enabled = false;
        return lr;
    }

    // 지정한 중심/반지름/색으로 원을 그린다
    public static void Draw(LineRenderer lr, Vector3 center, float radius, Color color)
    {
        if (lr == null) return;

        lr.enabled = true;
        lr.startColor = color;
        lr.endColor = color;

        for (int i = 0; i < Segments; i++)
        {
            float angle = (float)i / Segments * Mathf.PI * 2f;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            lr.SetPosition(i, pos);
        }
    }

    public static void Hide(LineRenderer lr)
    {
        if (lr != null)
            lr.enabled = false;
    }
}