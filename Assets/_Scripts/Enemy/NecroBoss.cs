using System.Collections;
using UnityEngine;

// 네크로보스 기믹: 일정 주기마다 정해진 고정량만큼 체력을 회복한다 (%회복 아님, 발동형 능력)
// 체력/이동속도는 CSV(EnemyData)에서 관리하므로 이 스크립트에서는 건드리지 않는다
[RequireComponent(typeof(Enemy))]
public class NecroBoss : MonoBehaviour
{
    [Header("체력 회복 설정")]
    [Tooltip("한 번 발동될 때 회복되는 고정 체력량")]
    public float healAmount = 20f;
    [Tooltip("회복이 발동되는 주기(초). 기본값 1초 = 초당 발동")]
    public float healInterval = 1f;

    [Header("회복 이펙트 설정 (초록색 테두리)")]
    [Tooltip("테두리 원의 반지름")]
    public float ringRadius = 0.6f;
    [Tooltip("회복이 발동되는 순간 테두리가 보이는 시간(초)")]
    public float ringVisibleDuration = 0.2f;
    public Color ringColor = Color.green;

    private Enemy enemy;
    private LineRenderer ring;
    private Coroutine healRoutine;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        CreateRing();
    }

    // 오브젝트 풀에서 재사용되어 다시 활성화될 때마다 회복 루프를 새로 시작한다
    void OnEnable()
    {
        healRoutine = StartCoroutine(HealLoop());
    }

    // 풀로 반환되어 비활성화될 때 루프를 멈추고 테두리도 꺼둔다
    void OnDisable()
    {
        if (healRoutine != null)
            StopCoroutine(healRoutine);
        if (ring != null)
            ring.enabled = false;
    }

    // healInterval마다 반복해서 고정량 회복 + 테두리 깜빡임 이펙트를 발동시킨다
    IEnumerator HealLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(healInterval);

            if (enemy == null || enemy.IsDead)
                yield break;

            enemy.Heal(healAmount);
            StartCoroutine(BlinkRing());
        }
    }

    // 회복이 발동되는 순간 테두리를 켰다가, ringVisibleDuration 뒤 다시 끈다
    IEnumerator BlinkRing()
    {
        ring.enabled = true;
        yield return new WaitForSeconds(ringVisibleDuration);
        ring.enabled = false;
    }

    // 보스 자식으로 초록색 원형 테두리를 미리 만들어두고, 필요할 때만 켜고 끈다
    void CreateRing()
    {
        GameObject ringObj = new GameObject("HealRing");
        ringObj.transform.SetParent(transform);
        ringObj.transform.localPosition = Vector3.zero;

        ring = ringObj.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.startWidth = 0.05f;
        ring.endWidth = 0.05f;
        ring.material = new Material(Shader.Find("Sprites/Default"));
        ring.startColor = ringColor;
        ring.endColor = ringColor;
        ring.sortingLayerName = "Default";
        ring.sortingOrder = 100;
        ring.enabled = false;

        int segments = 50;
        ring.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * ringRadius;
            ring.SetPosition(i, pos);
        }
    }
}