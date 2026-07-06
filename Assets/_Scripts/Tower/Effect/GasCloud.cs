using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasCloud : MonoBehaviour
{
    private readonly Collider2D[] hitBuffer = new Collider2D[32];

    // 적별 장판 체류 시간 누적 (시간 보간형 파셜 틱)
    private readonly Dictionary<Enemy, float> residentTimers = new Dictionary<Enemy, float>();

    private float radius;
    private float tickDamage;
    private float tickInterval;

    public static GasCloud Spawn(Vector3 position, float radius, float tickDamage, float duration, float tickInterval)
    {
        GameObject obj = new GameObject("GasCloud");
        obj.transform.position = position;

        GasCloud cloud = obj.AddComponent<GasCloud>();
        cloud.radius = radius;
        cloud.tickDamage = tickDamage;
        cloud.tickInterval = tickInterval;

        cloud.DrawGasArea(radius);
        cloud.StartCoroutine(cloud.LifetimeRoutine(duration));

        return cloud;
    }

    private void DrawGasArea(float radius)
    {
        LineRenderer lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(0.4f, 1f, 0.2f, 0.6f);
        lr.endColor = new Color(0.4f, 1f, 0.2f, 0.6f);
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 5;

        int segments = 40;
        lr.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            lr.SetPosition(i, pos);
        }
    }

    private void Update()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, radius, hitBuffer);

        HashSet<Enemy> stillInside = new HashSet<Enemy>();

        for (int i = 0; i < hitCount; i++)
        {
            Enemy enemy = hitBuffer[i].GetComponent<Enemy>();

            if (enemy == null)
                continue;

            // 장판은 지상 적만 타격 (공중 적은 제외)
            EnemyTargetAttribute attribute = enemy.GetComponent<EnemyTargetAttribute>();
            if (attribute != null && attribute.isFlying)
                continue;

            stillInside.Add(enemy);

            if (!residentTimers.TryGetValue(enemy, out float accumulated))
                accumulated = 0f;

            accumulated += Time.deltaTime;

            // 누적 시간이 틱 간격을 채우면 정식 틱 데미지, 초과분은 다음 누적으로 이월
            if (accumulated >= tickInterval)
            {
                enemy.TakeDamage(tickDamage);
                accumulated -= tickInterval;
            }

            residentTimers[enemy] = accumulated;
        }

        // 이번 프레임에 장판을 벗어난 적: 남은 누적 시간을 비례 데미지로 즉시 정산
        List<Enemy> exited = null;

        foreach (KeyValuePair<Enemy, float> pair in residentTimers)
        {
            if (stillInside.Contains(pair.Key))
                continue;

            (exited ??= new List<Enemy>()).Add(pair.Key);
        }

        if (exited != null)
        {
            foreach (Enemy enemy in exited)
                SettleAndRemove(enemy);
        }
    }

    private void SettleAndRemove(Enemy enemy)
    {
        if (!residentTimers.TryGetValue(enemy, out float remaining))
            return;

        residentTimers.Remove(enemy);

        // 장판을 '벗어난' 게 아니라 풀로 반환되며 비활성화된 경우 데미지 정산하지 않음
        if (enemy == null || !enemy.isActiveAndEnabled || remaining <= 0f)
            return;

        float partialDamage = remaining / tickInterval * tickDamage;
        enemy.TakeDamage(partialDamage);
    }

    private IEnumerator LifetimeRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        // 장판 소멸 시 아직 안에 남아있는 적들 잔여 데미지 정산
        foreach (Enemy enemy in new List<Enemy>(residentTimers.Keys))
            SettleAndRemove(enemy);

        Destroy(gameObject);
    }
}