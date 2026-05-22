using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public float speed = 2f;
    private int currentIndex = 0;
    private Transform targetWaypoint;

    void OnEnable()
    {
        currentIndex = 0;
        targetWaypoint = WaypointManager.Instance.GetWaypoint(currentIndex);
    }

    void Update()
    {
        if (targetWaypoint == null) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetWaypoint.position,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentIndex++;

            if (currentIndex >= WaypointManager.Instance.GetWaypointCount())
            {
                ObjectPool.Instance.ReturnToPool(gameObject);
                return;
            }

            targetWaypoint = WaypointManager.Instance.GetWaypoint(currentIndex);
        }
    }
}