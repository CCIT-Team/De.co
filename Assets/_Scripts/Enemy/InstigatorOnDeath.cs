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
    }
    IEnumerator SpeedBoost(Enemy enemy)
    {
        enemy.ApplySpeedMultiplier(speedMultiplier);
        yield return new WaitForSeconds(duration);
        enemy.ApplySpeedMultiplier(1f /  speedMultiplier); // 원래 속도로 복귀
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, screamRange);
    }

}