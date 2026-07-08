using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 인스티게이터가 사망할 때 주변 적들에게 이동속도 증가 버프를 뿌리는 스크립트
public class InstigatorOnDeath : MonoBehaviour
{
    public float screamRange = 3f;       // 버프가 적용되는 반경
    public float speedMultiplier = 1.5f; // 이속 증가 배율 (1.5 = 속도 150%)
    public float duration = 1.5f;        // 버프 지속 시간(초)

    // Enemy.Die()에서 호출됨: 주변 적들에게 버프를 뿌리고 화면에 경고 이펙트를 띄운다
    public void Scream()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, screamRange);
        foreach (Collider hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && enemy != GetComponent<Enemy>())
                // 코루틴은 반드시 버프를 받는 적(enemy) 쪽에서 실행해야 한다.
                // 인스티게이터 자신에서 실행하면, 인스티게이터가 죽은 뒤 곧바로
                // 풀로 반환되며 SetActive(false)되어 자신의 코루틴도 함께 강제 종료되고,
                // 그 결과 버프를 되돌리는 코드가 실행되지 못해 버프가 영구히 남는 문제가 있었다.
                enemy.StartCoroutine(SpeedBoost(enemy));
        }

        // WaveSpawner에서 이펙트 코루틴을 실행 (인스티게이터가 비활성화돼도 이펙트가 끝까지 재생되도록)
        WaveSpawner.Instance.StartCoroutine(ShowScreamEffect(transform.position));
    }

    // duration(초) 동안 적의 이동속도를 올렸다가 원래 속도로 되돌린다
    IEnumerator SpeedBoost(Enemy enemy)
    {
        enemy.ApplySpeedMultiplier(speedMultiplier);
        yield return new WaitForSeconds(duration);
        enemy.ApplySpeedMultiplier(1f / speedMultiplier); // 원래 속도로 복구
    }

    // 사망 지점에 원형 경고 이펙트를 그린 뒤 서서히 페이드 아웃시키는 연출용 코루틴
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

        // 원 둘레를 따라 segments개의 점을 찍어 원형 라인을 만든다
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 pos = position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * screamRange;
            lr.SetPosition(i, pos);
        }

        float elapsed = 0f;
        float effectDuration = 1.5f;

        // 시간이 지날수록 색을 투명하게 만들어 페이드 아웃 효과를 낸다
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