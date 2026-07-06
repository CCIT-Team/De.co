using System.Collections;
using UnityEngine;

public class ConeEffect : MonoBehaviour
{
    public static void Spawn(Vector3 position, Vector2 direction, float angle, float length, float duration = 0.15f)
    {
        GameObject obj = new GameObject("ConeEffect");
        obj.transform.position = position;

        ConeEffect effect = obj.AddComponent<ConeEffect>();
        effect.Draw(direction, angle, length);
        effect.StartCoroutine(effect.FadeRoutine(duration));
    }

    private void Draw(Vector2 direction, float angle, float length)
    {
        LineRenderer lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(1f, 0.6f, 0.1f, 0.6f);
        lr.endColor = new Color(1f, 0.6f, 0.1f, 0.6f);
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 5;

        int arcSegments = 20;
        lr.positionCount = arcSegments + 2; // 꼭짓점(타워 위치) + 호(arc) 위

        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - angle * 0.5f;
        float angleStep = angle / arcSegments;

        lr.SetPosition(0, Vector3.zero);

        for (int i = 0; i <= arcSegments; i++)
        {
            float rad = (startAngle + angleStep * i) * Mathf.Deg2Rad;
            Vector3 point = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * length;
            lr.SetPosition(i + 1, point);
        }
    }

    private IEnumerator FadeRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}