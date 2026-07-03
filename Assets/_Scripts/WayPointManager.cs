using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;

    public Transform[] waypoints; // Inspector에서 체크포인트들을 순서대로 연결
    public Transform[] flyingWaypoints;

    void Awake()
    {
        Instance = this;
    }

    public Transform GetWaypoint(int index, bool isFlying = false)
    {
        if (isFlying)
            return flyingWaypoints[index];
        return waypoints[index];
    }

    public int GetWaypointCount(bool isFlying = false)
    {
        if (isFlying)
            return flyingWaypoints.Length;
        return waypoints.Length;
    }
}