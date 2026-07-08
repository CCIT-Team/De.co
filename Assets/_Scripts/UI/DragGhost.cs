using UnityEngine;
using UnityEngine.UI;

// 드래그하는 동안 마우스를 따라다니는 반투명 아이콘(고스트)을 만들고 지우는 유틸리티
public static class DragGhost
{
    private static GameObject ghostObj;

    public static void Show(Sprite sprite, Canvas canvas, Vector2 screenPos, float size = 100f)
    {
        Hide();
        if (sprite == null || canvas == null) return;

        ghostObj = new GameObject("DragGhost");
        ghostObj.transform.SetParent(canvas.rootCanvas.transform, false);

        Image img = ghostObj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.color = new Color(1f, 1f, 1f, 0.8f);
        // 고스트가 마우스 클릭을 가로채면 그 아래 장착 칸이 드롭을 못 받으므로 꺼둔다
        img.raycastTarget = false;

        RectTransform rt = ghostObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);
        rt.position = screenPos;

        // 다른 UI들 위에 그려지도록 맨 마지막 순서로
        ghostObj.transform.SetAsLastSibling();
    }

    public static void Move(Vector2 screenPos)
    {
        if (ghostObj != null)
            ghostObj.transform.position = screenPos;
    }

    public static void Hide()
    {
        if (ghostObj != null)
            Object.Destroy(ghostObj);
        ghostObj = null;
    }
}