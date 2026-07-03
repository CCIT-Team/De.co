using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstigatorOnDeath : MonoBehaviour
{
    public float screamRange = 3f; // 비명 범위
    public float speedMultiplier = 1.5f; // 적 이동 속도 증가 비율
    public float duration = 1.5f; // 지속 시간

    public void Scream()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, screamRange);
        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && enemy != GetComponent<Enemy>())
                StartCoroutine(SpeedBoost(enemy));
        }

        // WaveSpawner에서 코루틴 실행 (이 오브젝트가 비활성화돼도 안 끊기게)
        WaveSpawner.Instance.StartCoroutine(ShowScreamEffect(transform.position));
    }
    IEnumerator SpeedBoost(Enemy enemy)
    {
        enemy.ApplySpeedMultiplier(speedMultiplier);
        yield return new WaitForSeconds(duration);
        enemy.ApplySpeedMultiplier(1f / speedMultiplier); // 원래 속도로 복귀
    }

    IEnumerator ShowScreamEffect(Vector3 position)
    {
        GameObject circleObj = new GameObject("ScreamEffect");
        circleObj.transform.position = position;

        LineRenderer lr = circleObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.yellow;
        lr.endColor = Color.yellow;
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 100;

        int segments = 50;
        lr.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 pos = position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * screamRange;
            lr.SetPosition(i, pos);
        }

        float elapsed = 0f;
        float effectDuration = 1.5f;

        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / effectDuration;

            Color fadeColor = Color.Lerp(Color.yellow, new Color(1f, 1f, 0f, 0f), t);
            lr.startColor = fadeColor;
            lr.endColor = fadeColor;

            yield return null;
        }

        Destroy(circleObj);
    }
}